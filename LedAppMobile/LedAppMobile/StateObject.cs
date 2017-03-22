using System.Net.Sockets;

namespace LedAppMobile
{
    class StateObject
    {
        public byte[] sBuffer { get; set; }

        public Socket sSocket { get; set; }

        public StateObject(int size, Socket sock)
        {
            sBuffer = new byte[size];
            sSocket = sock;
        }
    }
}