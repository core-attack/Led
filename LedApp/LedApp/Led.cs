using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using Windows.UI.Xaml;

namespace LedApp
{
    public class Led
    {
        /// <summary>
        /// порядковый номер пина светодиода
        /// </summary>
        private const int LED_PIN = 5;

        /// <summary>
        /// Значение пина по умолчанию
        /// </summary>
        private const bool DEFAULT_PIN_VALUE = true;

        /// <summary>
        /// количество раз, которое будем моргать
        /// </summary>
        private const int COUNT_TICKS = 5;

        /// <summary>
        /// Светодиод
        /// </summary>
        private GpioPin Pin { get; set; }

        /// <summary>
        /// Значение светодиода
        /// </summary>
        private GpioPinValue PinValue { get; set; }

        /// <summary>
        /// Таймер моргания
        /// </summary>
        private DispatcherTimer Timer { get; set; }

        /// <summary>
        /// Текущее количество морганий
        /// </summary>
        private int CountTicks { get; set; }

        public Led()
        {
            var gpio = GpioController.GetDefault();

            // если произошла какая-то ошибка при получении контроллера по умолчанию, уведомляем об этом
            if (gpio == null)
            {
                Pin = null;
                System.Diagnostics.Debug.WriteLine("There is no GPIO controller on this device.");
                return;
            }

            //открываем определенный пин для работы с ним
            Pin = gpio.OpenPin(LED_PIN);

            //опять же, если что-то пошло не так, выводим сообщение
            if (Pin == null)
            {
                System.Diagnostics.Debug.WriteLine("Pin was no found.");
                return;
            }

            //устанавливаем значение пина по умолчанию
            SetPinValue(DEFAULT_PIN_VALUE);
            //устанавливаем пин как средство вывода
            Pin.SetDriveMode(GpioPinDriveMode.Output);

            System.Diagnostics.Debug.WriteLine("GPIO pin initialized correctly.");

            //моргаем о готовности работать
            ImReady();
        }

        /// <summary>
        /// Установить значение светодиода
        /// </summary>
        /// <param name="on">вкл/выкл</param>
        public void SetPinValue(bool on)
        {
            System.Diagnostics.Debug.WriteLine(string.Format("Pin is o{0}", on ? "n": "ff"));
            PinValue = on ? GpioPinValue.High : GpioPinValue.Low;
            Pin.Write(PinValue);
        }

        /// <summary>
        /// Мограние в течение некоторого времени
        /// </summary>
        private void ImReady()
        {
            Timer = new DispatcherTimer();
            Timer.Interval = TimeSpan.FromMilliseconds(500);
            Timer.Tick += Timer_Tick;

            Timer.Start();
        }

        /// <summary>
        /// Событие, возникающие на каждый тик таймера
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer_Tick(object sender, object e)
        {
            if (CountTicks > COUNT_TICKS)
            {
                PinValue = GpioPinValue.High;
                Pin.Write(PinValue);
                Timer.Stop();
                return;
            }

            PinValue = PinValue == GpioPinValue.High ? GpioPinValue.Low : GpioPinValue.High;

            Pin.Write(PinValue);

            CountTicks++;
        }
    }
}
