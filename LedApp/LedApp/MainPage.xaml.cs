using Windows.Devices.Gpio;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Devices.Bluetooth;
using Windows.Devices.Enumeration;
using Windows.Devices.Bluetooth.Rfcomm;
using System.Collections.ObjectModel;
using System;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using System.Threading;
using Sockets.Plugin;
using System.IO;

namespace LedApp
{
    public sealed partial class MainPage : Page
    {
        /// <summary>
        /// Cветодиод
        /// </summary>
        private Led Led;

        public MainPage()
        {
            InitializeComponent();

            Led = new Led();
            new AsyncSocketListener(Led).StartToListen();
        }
    }
}
