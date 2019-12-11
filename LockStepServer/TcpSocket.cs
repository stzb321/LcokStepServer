using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace LockStepServer
{
    class TcpSocket : NetworkProxy
    {
        public static string IP = "0.0.0.0";
        public TcpListener listener;
        public Dictionary<string, TcpClient> Connections = new Dictionary<string, TcpClient>();

        
        public Action<TcpClient> HandlerClientConnected = (socket) =>
        {
            Console.WriteLine("client connect...");
        };
        public Action<TcpClient> HandlerClientDisConnected = (socket) =>
        {
            Console.WriteLine("client disconnect...");
        };
        private bool IsSocketConnected(TcpClient client)
        {
            return client.Connected;
        }

        public override void StartSocket()
        {
            IPAddress iPAddress = IPAddress.Parse(IP);
            IPEndPoint localEndPoint = new IPEndPoint(iPAddress, Port);

            listener = new TcpListener(localEndPoint);
            listener.Start();

            StartListening();
        }

        private void StartListening()
        {
            try
            {
                listener.BeginAcceptTcpClient(async asyncResult =>
                {
                    StartListening();

                    TcpClient client = listener.EndAcceptTcpClient(asyncResult);
                    IPEndPoint endPoint = (IPEndPoint)client.Client.RemoteEndPoint;
                    string id = GenId(endPoint);
                    Connections.Add(id, client);
                    HandlerClientConnected?.Invoke(client);

                    NetworkStream stream = client.GetStream();
                    int length = 1024 * 1024 * 4;

                    while (true)
                    {
                        byte[] buff = new byte[length];
                        int recLength = await stream.ReadAsync(buff, 0, length);
                        byte[] recByte = new byte[recLength];
                        Array.Copy(buff, recByte, recLength);

                        HandlerReceiveMessage?.Invoke(id, Encoding.UTF8.GetString(recByte));
                    }

                }, listener);
            }catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public override void SendTo(string id, string data)
        {
            if (!Connections.ContainsKey(id))
            {
                return;
            }

            TcpClient client = Connections.GetValueOrDefault(id);
            NetworkStream stream = client.GetStream();

            byte[] buffer = Encoding.UTF8.GetBytes(data);
            stream.Write(buffer, 0, buffer.Length);
        }

        public override void StopSocket()
        {
            listener.Stop();
            foreach (var item in Connections)
            {
                item.Value.Close();
                item.Value.Dispose();
            }
        }
    }
}
