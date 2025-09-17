using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkManager : Singleton<NetworkManager>
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

        // #region 테스트 인풋
        // if (Input.GetKeyDown(KeyCode.Q))
        // {
        //     SignupData temp = new SignupData("test04", "test1234");
        //     StartCoroutine(Signup(temp,
        //     () =>
        //     {
        //         Debug.Log("회원 가입 성공");
        //     },
        //     (response) =>
        //     {
        //         if (response == (int)ResponseType.INVALID_USERNAME)
        //         {
        //             Debug.Log($"회원 가입 실패 : 이미 존재하는 사용자입니다.");
        //         }
        //     }));

        // }
        // if (Input.GetKeyDown(KeyCode.W))
        // {
        //     SigninData temp = new SigninData("test05", "test1234");
        //     StartCoroutine(Signin(temp,
        //     () =>
        //     {
        //         Debug.Log("로그인 성공");
        //     },
        //      (response) =>
        //      {
        //          string log = "";

        //          if (response == (int)ResponseType.INVALID_USERNAME)
        //          {
        //              log = "ID가 유효하지 않습니다.";
        //          }
        //          else if (response == (int)ResponseType.INVALID_PASSWORD)
        //          {
        //              log = "비밀번호가 유효하지 않습니다.";
        //          }

        //          Debug.Log(log);
        //      }));

        // }
        // if (Input.GetKeyDown(KeyCode.E))
        // {
        //     this.Send_Room_Join(1000);
        // }
        // if (Input.GetKeyDown(KeyCode.R))
        // {
        //     this.Send_Room_Exit();
        // }
        // if (Input.GetKeyDown(KeyCode.A))
        // {
        //     this.Send_Game_Do(2, 5);
        // }
        // if (Input.GetKeyDown(KeyCode.S))
        // {
        //     this.Send_Game_Result(GameResultState.Win);
        // }
        // if (Input.GetKeyDown(KeyCode.D))
        // {
        //     Disconnect();
        // }

        // #endregion

    }

    #region 회원가입 및 로그인

    /// <param name="signup_data">데이터</param>
    /// <param name="success">성공 시 호출할 콜백 함수</param>
    /// <param name="failure">실패 시 호출할 콜백 함수</param>
    /// <summary> 회원가입</summary>
    public IEnumerator Signup(SignupData signup_data, Action success, Action<int> failure)
    {
        string jsonString = JsonUtility.ToJson(signup_data);
        byte[] byteRaw = System.Text.Encoding.UTF8.GetBytes(jsonString);

        using (UnityWebRequest www = new UnityWebRequest(Defines.LOGIN_SERVER_URL + "/users/signup",
                   UnityWebRequest.kHttpVerbPOST))
        {
            www.uploadHandler = new UploadHandlerRaw(byteRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError)
            {
                // 서버 연결 오류에 대해 알림
                Debug.Log("로그인 서버 연결 에러");
            }
            else
            {
                var resultString = www.downloadHandler.text;
                var result = JsonUtility.FromJson<SigninResult>(resultString);

                if (result.result == 2)
                {
                    success?.Invoke();
                }
                else
                {
                    failure?.Invoke(result.result);
                }
            }
        }
    }


    /// <param name="signin_data">데이터</param>
    /// <param name="success">성공 시 호출할 콜백 함수</param>
    /// <param name="failure">실패 시 호출할 콜백 함수</param>
    /// <summary> 로그인</summary>
    public IEnumerator Signin(SigninData signin_data, Action success, Action<int> failure)
    {
        string jsonString = JsonUtility.ToJson(signin_data);
        byte[] byteRaw = System.Text.Encoding.UTF8.GetBytes(jsonString);

        using (UnityWebRequest www = new UnityWebRequest(Defines.LOGIN_SERVER_URL + "/users/signin",
                   UnityWebRequest.kHttpVerbPOST))
        {
            www.uploadHandler = new UploadHandlerRaw(byteRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError)
            {
                // 서버 연결 오류에 대해 알림
                Debug.Log("로그인 서버 연결 에러");
            }
            else
            {
                var resultString = www.downloadHandler.text;
                var result = JsonUtility.FromJson<SigninResult>(resultString);

                if (result.result == (int)ResponseType.SUCCESS)
                {
                    // 로그인 성공
                    var cookie = www.GetResponseHeader("set-cookie");
                    if (!string.IsNullOrEmpty(cookie))
                    {
                        int lastIndex = cookie.LastIndexOf(';');
                        string sid = cookie.Substring(0, lastIndex);

                        // 저장
                        PlayerPrefs.SetString("sid", sid);
                    }
                    ConnectServer();
                    success?.Invoke();
                }
                else
                {
                    // 로그인 실패
                    failure?.Invoke(result.result);
                }
            }
        }
        ;
    }

    #endregion


    #region TCP 서버 연결 관리 및 Send & Recv 콜백 구조
    /// <summary> TCP 서버 연결 </summary>
    private void ConnectServer()
    {
        try
        {
            // 소켓 생성
            this.client_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // 엔드 포인트 설정
            IPAddress ip_addr = IPAddress.Parse(Defines.TCP_SERVER_IP);
            IPEndPoint remote_ep = new IPEndPoint(ip_addr, Defines.TCP_SERVER_port);

            Debug.Log("서버에 연결중..");

            // 통신 시작
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

    /// <summary> Recv 성공 시 버퍼 데이터 파싱 및 처리 </summary>
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

    #endregion


    #region Send & Recv API

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
    
    #endregion

}
