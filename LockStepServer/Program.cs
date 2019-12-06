#define USING_TCP_SOCKET

using System;
using System.Threading;

namespace LockStepServer
{

    class Program
    {
        static void Main(string[] args)
        {
#if USING_TCP_SOCKET
            NetworkProxy socket = new TcpSocket();
#else
            NetworkProxy socket = new UdpSocket();
#endif
            try
            {
                socket.StartSocket();
                while (true)
                {
                    Thread.Sleep(3);
                    socket.Update();
                }
            }catch(Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
