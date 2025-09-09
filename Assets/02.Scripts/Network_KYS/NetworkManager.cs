using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    private Socket client_socket;

    private readonly Queue<Action> main_thread_actions = new Queue<Action>();

    private string IP = "127.0.0.1";
    private int port = 7000;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
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
    }

    private void ConnectServer()
    {
        try
        {
            this.client_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IPAddress ip_addr = IPAddress.Parse(this.IP);
            IPEndPoint remote_ep = new IPEndPoint(ip_addr, this.port);

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
        client_socket.EndConnect(AR);

        lock (main_thread_actions)
        {
            main_thread_actions.Enqueue(() =>
            {
                Debug.Log("Connected successfully!");
            });
        }
    }
}
