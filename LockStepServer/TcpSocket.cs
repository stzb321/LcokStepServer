using LockStepFrameWork.NetMsg;
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

        
        public Action<string, TcpClient> HandlerClientConnected = (id, socket) =>
        {
            Console.WriteLine(string.Format("client {0} connect...", id));
        };
        public Action<string, TcpClient> HandlerClientDisConnected = (id, socket) =>
        {
            Console.WriteLine(string.Format("client {0} disconnect...", id));
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
                    TcpClient oldClient;
                    if (Connections.TryGetValue(id, out oldClient))
                    {
                        oldClient.GetStream().Close();
                        oldClient.Close();
                        oldClient.Dispose();
                        HandlerClientDisConnected?.Invoke(id, oldClient);
                    }
                    HandlerClientConnected?.Invoke(id, client);

                    NetworkStream stream = client.GetStream();
                    int length = 1024 * 1024 * 4;

                    while (true)
                    {
                        byte[] buff = new byte[length];
                        int recLength = await stream.ReadAsync(buff, 0, length);
                        byte[] recByte = new byte[recLength];
                        Array.Copy(buff, recByte, recLength);

                        HandlerReceiveMessage?.Invoke(id, buff);
                    }

                }, listener);
            }catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public override void SendTo(string id, MsgType opcode, string data)
        {
            if (!Connections.ContainsKey(id))
            {
                return;
            }

            TcpClient client = Connections.GetValueOrDefault(id);
            NetworkStream stream = client.GetStream();
            stream.Write(BitConverter.GetBytes((ushort)opcode));

            byte[] buffer = Encoding.UTF8.GetBytes(data);
            stream.Write(buffer, 1, buffer.Length);
        }

        public override void StopSocket()
        {
            listener.Stop();
            foreach (var item in Connections)
            {
                item.Value.GetStream().Close();
                item.Value.Close();
                item.Value.Dispose();
            }
        }
    }
}
