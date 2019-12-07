using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace LockStepServer
{
    abstract class NetworkProxy
    {
        public static int Port = 8488;
        abstract public void StartSocket();
        abstract public void StopSocket();

        abstract public void SendTo(string id, string data);

        public void Update()
        {
            
        }

        public Action<string, string> HandlerReceiveMessage = (id, recByte) =>
        {
            Console.WriteLine("ReceiveMessage:{0}", recByte);
        };

        public string GenId(IPEndPoint endPoint)
        {
            return string.Format("{0}:{1}", endPoint.Address, endPoint.Port);
        }

    }
}
