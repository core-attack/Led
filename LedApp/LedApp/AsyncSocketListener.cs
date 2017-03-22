using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using System.Diagnostics;

namespace LedApp
{
    /// <summary>
    /// Сокет, асинхронно слушающий определенный порт 
    /// </summary>
    class AsyncSocketListener
    {
        private const string HOST = "localhost"; 
        private const string PORT = "443";

        public AsyncSocketListener(Led Led)
        {
            this.Led = Led;
        }

        /// <summary>
        /// Светодиод
        /// </summary>
        private Led Led { get; set; }

        /// <summary>
        /// Состояние светодиода
        /// </summary>
        private bool State { get; set; }

        public async void StartToListen()
        {
            try
            {

                StreamSocketListener socketListener = new StreamSocketListener();
                socketListener.ConnectionReceived += SocketListener_ConnectionReceived;
                Debug.WriteLine("Starting listen remove requests...");
                await socketListener.BindServiceNameAsync(PORT);
            }
            catch (UnauthorizedAccessException exc)
            {
                Debug.WriteLine("ERROR: " + exc.Message);
            }
            //HostName serverHost = new HostName(HOST);
            //var endpointPairs = await DatagramSocket.GetEndpointPairsAsync(serverHost, PORT);

            //foreach (EndpointPair ep in endpointPairs)
            //{
            //    Debug.WriteLine("EP {0} {1}", new object[] { ep.LocalHostName, ep.RemoteHostName });
            //}
        }

        public async void SocketListener_ConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            State = !State;
            this.Led.SetPinValue(State);
            //читаем строку с клиента
            //Stream inStream = args.Socket.InputStream.AsStreamForRead();
            //StreamReader reader = new StreamReader(inStream);
            //string result = await reader.ReadLineAsync();
            //Debug.WriteLine(result);
        }
    }
}
