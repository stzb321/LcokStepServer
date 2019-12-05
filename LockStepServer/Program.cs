#define USING_TCP_SOCKET

using System;

namespace LockStepServer
{

    class Program
    {
        static void Main(string[] args)
        {
#if USING_TCP_SOCKET
            NetworkProxy socket = new TcpSocket();
#else
            SocketProxy socket = new UdpSocket();
#endif
            socket.StartSocket();
        }
    }
}
