using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Ionic.Zip;
using Ionic.Zlib;
using System.IO;

public class ImageServer : MonoBehaviour
{
    [SerializeField]
    int m_Port;

    MultiSocketTcpServer m_TcpServer;

    public delegate void DelegateOnAudioReceived(ImageData id);
    public DelegateOnAudioReceived m_OnAudioReceived;

    System.Object m_Lock;
    LinkedList<ImageData> m_Queue;

    public bool IsAcceptableData = false;

    MyDeq m_Buf;
    const int m_SizeBuf = 1024 * 1024 * 16;
    byte[] m_Zipped;
    byte[] m_BufTemp;

    public class ImageData
    {
        public int id;
        public int w;
        public int h;
        public byte[] data;
    }

    KeyValuePair<ImageData, int> ParseImageData(ref byte[] b, int size)
    {
        int size_header = 13;
        if (size < size_header) return new KeyValuePair<ImageData, int>(null, 0);
        int w = BitConverter.ToInt32(b, 1);
        int h = BitConverter.ToInt32(b, 5);
        int size_data = BitConverter.ToInt32(b, 9);
        if(b[0] != 0)
        {
            m_Buf.Clear();
            return new KeyValuePair<ImageData, int>(null, size);
        }
        //Debug.LogFormat("id:{0}, w:{1}, h:{2}, size_data:{3}, q:{4}", (int)b[0], w, h, size_data, size);

        if(size < size_header + size_data) return new KeyValuePair<ImageData, int>(null, 0);
        ImageData id = new ImageData();
        id.id = (int)b[0];
        id.w = w;
        id.h = h;

        id.data = new byte[size_data];
        Array.Copy(b, size_header, id.data, 0, size_data);
        return new KeyValuePair<ImageData, int>(id, size_header + size_data);
    }

    void OnReceive(ref byte[] b, int size)
    {
        //Debug.Log("OnReceive:" + size);
        m_Buf.Push(ref b, size);
        int num = m_Buf.GetArray(ref m_BufTemp);

        KeyValuePair<ImageData, int> kv = ParseImageData(ref m_BufTemp, num);
        //Debug.Log("value:" + kv.Value);
        if (kv.Value > 0)
        {
            //consume bytes.
            int num_debug = m_Buf.Pop(kv.Value, ref m_BufTemp);
        }
        if (kv.Key != null)
        {
            ImageData id = kv.Key;
            if (IsAcceptableData)
            {
                lock (m_Lock)
                {
                    m_Queue.AddLast(id);
                }
            }
        }
    }

    private void OnConnect()
    {
        Debug.LogFormat("AudioServer : client connected");
    }

    private void OnDisconnect(string s)
    {
        Debug.LogFormat("AudioServer : client disconnected:" + s);
    }

    void OnListenFailed(string s)
    {
        Debug.LogErrorFormat("AudioServer : client listen failed:" + s);
    }

    void StartListening()
    {
        string address = Utils.GetLocalIPAddress();
        string m_IPAddress = Utils.GetLocalIPAddress();
        m_TcpServer.StartListening(address, m_Port, OnReceive, OnConnect, OnDisconnect, OnListenFailed);
    }

    void Start()
    {
        m_Lock = new System.Object();
        m_Queue = new LinkedList<ImageData>();
        m_TcpServer = new MultiSocketTcpServer();

        m_Buf = new MyDeq(m_SizeBuf);
        m_BufTemp = new byte[m_SizeBuf];

        IsAcceptableData = true;

        StartListening();
    }

    void Update()
    {
        lock (m_Lock)
        {
            foreach (var id in m_Queue)
            {
                m_OnAudioReceived(id);
            }
            m_Queue.Clear();
        }

    }

    private void OnDestroy()
    {
        m_TcpServer.Stop();
    }
}
