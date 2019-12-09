using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

class MultiSocketTcpServer
{
    private int size_receiving_;
    private Socket handler_;
    public delegate void OnDisconnect(string s);
    public delegate void OnConnect();
    public delegate void OnListenError(string s);
    public delegate void OnReceive(ref byte[] b, int size);
    OnDisconnect on_disconnect_;
    OnConnect on_connect_;
    OnListenError on_listen_error_;
    OnReceive on_receive_;
    Socket listener_;

    // State object for reading client data asynchronously
    public class StateObject
    {
        public Socket workSocket = null;
        public static readonly int BufferSize = 1024 * 1024 * 16;
        public byte[] buffer = new byte[BufferSize];
    }

    List<StateObject> activeConnections = new List<StateObject>();

    public void StartListening(String host, int port, OnReceive on_receive, OnConnect on_connect, OnDisconnect on_disconnect, OnListenError on_listen_error)
    {
        on_receive_ = on_receive;
        on_connect_ = on_connect;
        on_disconnect_ = on_disconnect;
        on_listen_error_ = on_listen_error;
        size_receiving_ = 0;
        IPAddress ipAddress = IPAddress.Parse(GetIPAddress(host));

        IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);

        // Create a TCP/IP socket.
        listener_ = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        listener_.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

        // Bind the socket to the local endpoint and listen for incoming connections.
        try
        {
            listener_.Bind(localEndPoint);
            listener_.Listen(0);

            // Start an asynchronous socket to listen for connections.
            listener_.BeginAccept(new AsyncCallback(AcceptCallback), listener_);
        }
        catch (Exception e)
        {
            on_listen_error_(e.ToString());
        }

    }

    public void AcceptCallback(IAsyncResult ar)
    {
        // Get the socket that handles the client request.
        Socket listener = (Socket)ar.AsyncState;
        Socket handler = listener.EndAccept(ar);
        handler_ = handler;

        on_connect_();

        // Create the state object.
        StateObject state = new StateObject();
        state.workSocket = handler;
        handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
            new AsyncCallback(ReadCallback), state);

        //確立した接続のオブジェクトをリストに追加
        activeConnections.Add(state);

        System.Console.WriteLine("there is {0} connections", activeConnections.Count);

        listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);
    }

    public void ReadCallback(IAsyncResult ar)
    {
        String content = String.Empty;

        // Retrieve the state object and the handler socket
        // from the asynchronous state object.
        StateObject state = (StateObject)ar.AsyncState;
        Socket handler = state.workSocket;

        // Read data from the client socket. 
        int bytesRead = handler.EndReceive(ar);

        if (bytesRead > 0)
        {
            // There  might be more data, so store the data received so far.
            //state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));
            // Check for end-of-file tag. If it is not there, read 
            // more data.
            on_receive_(ref state.buffer, bytesRead);

            // Not all data received. Get more.
            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReadCallback), state);
        }
        else
        {
            on_disconnect_("stop receiving");
            if (activeConnections.Contains(state))
            {
                activeConnections.Remove(state);
            }

            listener_.BeginAccept(new AsyncCallback(AcceptCallback), listener_);
        }
    }

    public void Send(String data)
    {
        if (handler_ == null) return;

        // Convert the string data to byte data using ASCII encoding.
        byte[] byteData = Encoding.ASCII.GetBytes(data);

        // Begin sending the data to the remote device.
        handler_.BeginSend(byteData, 0, byteData.Length, 0,
            new AsyncCallback(SendCallback), handler_);

    }

    private void SendCallback(IAsyncResult ar)
    {
        try
        {
            // Retrieve the socket from the state object.
            Socket handler = (Socket)ar.AsyncState;

            // Complete sending the data to the remote device.
            int bytesSent = handler.EndSend(ar);
        }
        catch (Exception e)
        {
            System.Console.WriteLine(e.ToString());
        }
    }

    private string GetIPAddress(string hostname)
    {
        IPHostEntry host;
        host = Dns.GetHostEntry(hostname);

        foreach (IPAddress ip in host.AddressList)
        {
            if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        return string.Empty;
    }

    public void Stop()
    {
        //一応。なくてもいける？
        if (listener_ != null)
        {
            foreach (StateObject so in activeConnections)
            {
                so.workSocket.Close();
            }
            listener_.BeginDisconnect(true, null, null);
            listener_.Close();
        }
    }
}
