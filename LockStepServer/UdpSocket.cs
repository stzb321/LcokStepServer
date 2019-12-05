using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace LockStepServer
{
    class UdpSocket : NetworkProxy
    {
        public Dictionary<string, IPEndPoint> User2IpMap = new Dictionary<string, IPEndPoint>();
        private UdpClient udpClient;

        public override void StartSocket()
        {
            try
            {
                udpClient = new UdpClient(Port);
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, Port);
                udpClient.Connect(endPoint);
                StartReceive();
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public void StartReceive()
        {
            udpClient.BeginReceive(async asyncResult =>
            {
                StartReceive();

                IPEndPoint endpoint = new IPEndPoint(IPAddress.Any, Port);
                byte[] buffer = udpClient.EndReceive(asyncResult, ref endpoint);
                string str = Encoding.UTF8.GetString(buffer);

                string id = GenId(endpoint);
                User2IpMap.Add(id, endpoint);
                HandlerReceiveMessage(id, str);
            }, udpClient);
        }

        public override void SendTo(string id, string data)
        {
            if (!User2IpMap.ContainsKey(id))
            {
                return;
            }
            IPEndPoint endPoint = User2IpMap.GetValueOrDefault(id);
            byte[] buffer = Encoding.UTF8.GetBytes(data);
            udpClient.Send(buffer, buffer.Length, endPoint);
        }

        public override void StopSocket()
        {
            udpClient?.Close();
            udpClient?.Dispose();
        }

    }
}
