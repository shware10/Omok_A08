using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManager : MonoBehaviour
{
    private Socket client_socket;

    /// <summary> Update 실행 큐 ( Recv의 시점을 Update 프레임 시점에 동기화 시키기 위한 큐인듯 ) </summary>
    private readonly Queue<Action> main_thread_actions = new Queue<Action>();


    private byte[] receive_buffer = new byte[4096];
    private List<byte> incomplete_packet_buffer = new List<byte>();
    private readonly object packetBufferLock = new object();

    Action<byte[]>[] recv_act_lookup_arr;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        this.recv_act_lookup_arr = new Action<byte[]>[]{
            Handle_Room_Response,
            Handle_Room_Create,
            Handle_Room_Join,
            Handle_Room_Exit,
            Handle_Game_DO,
            Handle_Game_Result,
            Handle_Game_Start
        };


        ConnectServer();
    }

    void Update()
    {
        lock (main_thread_actions)
        {
            while (main_thread_actions.Count > 0)
            {
                Action act = main_thread_actions.Dequeue();
                act?.Invoke();
            }
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            this.Send_Get_Room();
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            this.Send_Room_Crate();
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            this.Send_Room_Join(1000);
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            this.Send_Room_Exit();
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            this.Send_Game_Do(2, 5);
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            this.Send_Game_Result(GameResultState.Win);
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            Disconnect();
        }


    }

    private void ConnectServer()
    {
        try
        {
            this.client_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IPAddress ip_addr = IPAddress.Parse(Defines.IP);
            IPEndPoint remote_ep = new IPEndPoint(ip_addr, Defines.port);

            Debug.Log("서버에 연결중..");

            this.client_socket.BeginConnect(remote_ep, new AsyncCallback(ConnectCallback), null);
        }
        catch (Exception e)
        {
            Debug.LogError($"Connection failed: {e.Message}");
        }
    }

    private void ConnectCallback(IAsyncResult AR)
    {
        try
        {
            client_socket.EndConnect(AR);

            lock (main_thread_actions)
            {
                main_thread_actions.Enqueue(() =>
                {
                    Debug.Log("Connected successfully!");
                });
            }

            client_socket.BeginReceive(this.receive_buffer, 0, this.receive_buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), null);
        }
        catch (Exception e)
        {
            lock (main_thread_actions)
            {
                main_thread_actions.Enqueue(() =>
                {
                    Debug.LogError($"Connection callback failed: {e.Message}");
                });
            }
        }
    }

    private void ReceiveCallback(IAsyncResult AR)
    {
        try
        {
            int bytesRead = client_socket.EndReceive(AR);
            if (bytesRead > 0)  // 패킷의 크기가 0 이상일 시
            {
                lock (packetBufferLock)
                {
                    for (int i = 0; i < bytesRead; i++)
                    {
                        incomplete_packet_buffer.Add(receive_buffer[i]);    //  수신 버퍼 복사
                    }
                    ProcessReceivedData();
                }
                client_socket.BeginReceive(this.receive_buffer, 0, this.receive_buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), null);
            }
            else  // 상대가 접속을 끊었을 시 ( 패킷 size 0 )
            {
                lock (main_thread_actions)
                {
                    main_thread_actions.Enqueue(() =>
                    {
                        Debug.Log("Server disconnected.");
                        Disconnect();
                    });
                }
            }
        }
        catch (Exception e)
        {
            lock (main_thread_actions)
            {
                main_thread_actions.Enqueue(() =>
                {
                    Debug.LogError($"Receive failed: {e.Message}");
                    Disconnect();
                });
            }
        }
    }

    private void ProcessReceivedData()
    {
        while (true)
        {
            if (incomplete_packet_buffer.Count < Defines.HEADERSIZE) return;                                 // 헤더 조차 버퍼에 없을 때 return

            ushort protocol = (ushort)((incomplete_packet_buffer[0]) | incomplete_packet_buffer[1] << 8);    // 첫 1~2 바이트 프로토콜

            ushort body_size = (ushort)((incomplete_packet_buffer[2]) | incomplete_packet_buffer[3] << 8);   // 3~4 바이트 body size

            if (incomplete_packet_buffer.Count < Defines.HEADERSIZE + body_size) return;                     // 현재 퍼올린 버퍼의 남은 데이터가 header사이즈 + body 사이즈 이하면 return

            byte[] completed_message_body = new byte[body_size];  // body Data

            incomplete_packet_buffer.CopyTo(Defines.HEADERSIZE, completed_message_body, 0, body_size);      // body Data 복사
            incomplete_packet_buffer.RemoveRange(0, Defines.HEADERSIZE + body_size);                        // 버퍼에 퍼올린 패킷 만큼 제거 

            lock (main_thread_actions)
            {
                main_thread_actions.Enqueue(() =>
                {
                    // 룩업 배열 함수 실행
                    this.recv_act_lookup_arr[protocol]?.Invoke(completed_message_body);
                });
            }
        }
    }

    public void Disconnect()
    {
        if (client_socket != null && client_socket.Connected)
        {
            client_socket.Shutdown(SocketShutdown.Both);
            client_socket.Close();
        }
        client_socket = null;
        Debug.Log("Disconnected from server.");
    }

    void OnApplicationQuit()
    {
        Disconnect();
    }

    private void send_packet(Packet packet)
    {
        if (client_socket == null || !client_socket.Connected)
            return;

        byte[] data_to_send = new byte[Defines.HEADERSIZE + packet.body_size];

        packet.Serialize(data_to_send);

        client_socket.BeginSend(data_to_send, 0, data_to_send.Length, SocketFlags.None, new AsyncCallback(SendCallback), null);
        Debug.Log("패킷 보냄");
    }

    private void SendCallback(IAsyncResult AR)
    {
        try
        {
            int bytesSent = client_socket.EndSend(AR);
            lock (main_thread_actions)
            {
                main_thread_actions.Enqueue(() =>
                {
                    Debug.Log($"Sent {bytesSent} bytes to server.");
                });
            }
        }
        catch (Exception e)
        {
            lock (main_thread_actions)
            {
                main_thread_actions.Enqueue(() =>
                {
                    Debug.LogError($"Send failed: {e.Message}");
                });
            }
        }
    }

    // 
    public void Send_Get_Room()
    {
        Packet packet = new Packet(PROTOCOL.ROOM_REQUEST);
        send_packet(packet);
    }

    public void Send_Room_Crate()
    {
        Packet packet = new Packet(PROTOCOL.ROOM_CREATE);
        send_packet(packet);
    }
    public void Send_Room_Join(int room_id)
    {
        Packet packet = new Packet(PROTOCOL.ROOM_JOIN, room_id);
        send_packet(packet);
    }

    public void Send_Room_Exit()
    {
        Packet packet = new Packet(PROTOCOL.ROOM_EXIT);
        send_packet(packet);
    }

    public void Send_Game_Do(int row, int col)
    {
        Packet packet = new Packet(PROTOCOL.GAME_DO);
        send_packet(packet);
    }

    public void Send_Game_Result(GameResultState state)
    {
        Packet packet = new Packet(PROTOCOL.GAME_WIN, (byte)state);
        send_packet(packet);
    }

    private void Handle_Room_Response(byte[] data)
    {
        Debug.Log("방 목록 받음");
    }

    private void Handle_Room_Create(byte[] data)
    {
        Debug.Log("방 생성 요청 ACK 받음");
    }


    private void Handle_Room_Join(byte[] data)
    {
        Debug.Log("방 참가 요청 ACK 받음");
    }

    private void Handle_Room_Exit(byte[] data)
    {
        Debug.Log("방 퇴장 요청 ACK 받음");
    }
    private void Handle_Game_Start(byte[] data)
    {
        Debug.Log("방 게임 시작 요청 받음");
    }

    private void Handle_Game_DO(byte[] data)
    {
        Debug.Log("상대방 턴 완료 Alert 받음");
    }

    private void Handle_Game_Result(byte[] data)
    {
        Debug.Log("게임종료 Alert 받음");
    }

}
