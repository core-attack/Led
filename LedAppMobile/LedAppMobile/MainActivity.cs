using Android.App;
using Android.Widget;
using Android.OS;
using System;
using System.Threading.Tasks;
using Sockets.Plugin;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace LedAppMobile
{
    [Activity(Label = "LedAppMobile", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        private string data = null;


        /// <summary>
        /// Возвращает переключатель
        /// </summary>
        protected Switch GetSwitch
        {
            get
            {
                return FindViewById<Switch>(Resource.Id.switcher);
            }
        }

        /// <summary>
        /// Состояние переключателя
        /// </summary>
        protected bool SwitchStatus { get; set; }

        /// <summary>
        /// Событие, возникающее при запуске MainActivity
        /// </summary>
        /// <param name="bundle"></param>
        protected override void OnCreate(Bundle bundle)
        {
            try
            {
                base.OnCreate(bundle);

                SetContentView(Resource.Layout.Main);

                var switcher = GetSwitch;
                switcher.Checked = true;
                switcher.CheckedChange += onCheckedChange;

            } catch(Exception e)
            {
                AlertDialog.Builder alert = new AlertDialog.Builder(this);
                alert.SetTitle("Error");
                alert.SetMessage(e.Message);
                alert.Show();
            }
        }

        /// <summary>
        /// Событие, возникающее при изменения состояния переключателя
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void onCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            if (sender is Switch)
            {
                SwitchStatus = ((Switch)sender).Checked;

                System.Diagnostics.Debug.WriteLine(string.Format("Switcher is {0}", SwitchStatus ? "on" : "off"));

                SetLedStatus(SwitchStatus);
            }
        }
        
        /// <summary>
        /// Устанавливает статус светодиоду
        /// </summary>
        /// <param name="on"></param>
        private void SetLedStatus(bool on)
        {
            try
            {
                //получение IP адреса и точки подключения
                IPAddress ip = Dns.GetHostEntry(Rpi3.IP).AddressList[0];
                IPEndPoint ipEndpoint = new IPEndPoint(ip, Convert.ToInt32(Rpi3.PORT));

                Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                //попытка соединения с указанной точкой подключения
                IAsyncResult asyncConnect = clientSocket.BeginConnect(ipEndpoint, new AsyncCallback(connectCallback), clientSocket);

                System.Diagnostics.Debug.WriteLine("Connecting...");

                //ожидание коллбеков
                if (writeDot(asyncConnect) == true)
                {
                    Thread.Sleep(100);
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// Метод, вызываемый при успешном соединении с указанным IP
        /// </summary>
        /// <param name="asyncConnect"></param>
        private void connectCallback(IAsyncResult asyncConnect)
        {
            try
            {
                Socket clientSocket = (Socket)asyncConnect.AsyncState;
                clientSocket.EndConnect(asyncConnect);
                
                if (clientSocket.Connected == false)
                {
                    throw new Exception("Сlient is not connected.");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("client is connected.");
                }

                byte[] sendBuffer = new byte[] { Convert.ToByte(SwitchStatus) };
                IAsyncResult asyncSend = clientSocket.BeginSend(
                  sendBuffer,
                  0,
                  sendBuffer.Length,
                  SocketFlags.None,
                  new AsyncCallback(sendCallback),
                  clientSocket);

                System.Diagnostics.Debug.Write("Sending data.");
                writeDot(asyncSend);
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// Событие, возникающее при успешной отправке данных на Rpi3
        /// </summary>
        /// <param name="asyncSend"></param>
        public void sendCallback(IAsyncResult asyncSend)
        {
            try
            {
                Socket clientSocket = (Socket)asyncSend.AsyncState;
                int bytesSent = clientSocket.EndSend(asyncSend);
                System.Diagnostics.Debug.WriteLine(string.Format("{0} bytes sent.", bytesSent.ToString()));

                StateObject stateObject = new StateObject(16, clientSocket);

                //необходимо передать stateObject и сокет обратно
                IAsyncResult asyncReceive = clientSocket.BeginReceive(
                    stateObject.sBuffer,
                    0,
                    stateObject.sBuffer.Length,
                    SocketFlags.None,
                    new AsyncCallback(receiveCallback),
                    stateObject);


                System.Diagnostics.Debug.Write("Receiving response.");
                writeDot(asyncReceive);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// Событие, возникающее после успешной отправки данных обратно на мобильное устройство
        /// </summary>
        /// <param name="asyncReceive"></param>
        private void receiveCallback(IAsyncResult asyncReceive)
        {
            try {
                StateObject stateObject = (StateObject)asyncReceive.AsyncState;

                int bytesReceived = stateObject.sSocket.EndReceive(asyncReceive);

                stateObject.sSocket.Shutdown(SocketShutdown.Both);
                stateObject.sSocket.Close();
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// Ожидание результата
        /// </summary>
        /// <param name="ar"></param>
        /// <returns></returns>
        internal bool writeDot(IAsyncResult ar)
        {
            int i = 0;
            while (ar.IsCompleted == false)
            {
                if (i++ > 20)
                {
                    System.Diagnostics.Debug.WriteLine("Timed out.");
                    return false;
                }
                System.Diagnostics.Debug.Write(".");
                Thread.Sleep(100);
            }
            return true;
        }
    }
}

