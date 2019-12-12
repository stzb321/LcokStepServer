using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using LockStepFrameWork.NetMsg;

namespace LockStepServer
{
    abstract class NetworkProxy
    {
        public static int Port = 8488;
        public static float HeartBreathTimeOut = 3.0f;
        abstract public void StartSocket();
        abstract public void StopSocket();

        abstract public void SendTo(string id, MsgType opcode, string data);

        public void Update()
        {
            
        }

        public Action<string, MsgType, string> HandlerReceiveMessage = (id, opcode, recByte) =>
        {
            Console.WriteLine("ReceiveMessage:{0}", recByte);
        };

        public Action<string> HandlerTimeOut = (id) =>
        {
            Console.WriteLine("On client time out:{0}", id);
        };

        public string GenId(IPEndPoint endPoint)
        {
            return string.Format("{0}:{1}", endPoint.Address, endPoint.Port);
        }

    }
}
