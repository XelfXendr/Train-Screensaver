using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Threading;

using Train_Screensaver_Client.Logic;
using Train_Screensaver_Client.Networking;
using System.Threading.Tasks;
using System.ComponentModel;

namespace Train_Screensaver_Client
{
    /// <summary>
    /// Interakční logika pro ScreensaverWindow.xaml
    /// </summary>
    public partial class ScreensaverWindow : Window
    {
        private Point mousePos = new Point(-1, -1);

        private Train train;

        private Connection connection;

        public ScreensaverWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.None;
            Topmost = true;

            //Train testing
            string[] sources = new string[]
            {
                Environment.CurrentDirectory + "/Trains/front.png",
                Environment.CurrentDirectory + "/Trains/wagon01.png",
                Environment.CurrentDirectory + "/Trains/wagon02.png",
                Environment.CurrentDirectory + "/Trains/wagon03.png",
                Environment.CurrentDirectory + "/Trains/wagon04.png",
                Environment.CurrentDirectory + "/Trains/wagon05.png",
                Environment.CurrentDirectory + "/Trains/wagon06.png",
                Environment.CurrentDirectory + "/Trains/wagon07.png",
                Environment.CurrentDirectory + "/Trains/wagon08.png",
            };
            int[] indexes = new int[] { 0, 1, 1, 1, 5, 1, 1, 8, 6, 7, 2, 2, 4, 3, 3, 8, 3, 3, 6, 8, 6, 7 };

            train = new Train(screensaverCanvas, sources, indexes);


            connection = new Connection("192.168.1.35", 25308);

            train.onFirstFinished += (_, e) =>
            {
                byte[] data = new byte[] { 0x09, (byte)(e.position >> 8), (byte)e.position };

                BackgroundWorker sender = new BackgroundWorker();
                sender.DoWork += (_, e) =>
                {
                    connection.Send(data);
                };

                sender.RunWorkerAsync();
            };

            train.onLastFinished += (_, e) =>
            {
                BackgroundWorker reader = new BackgroundWorker();
                reader.DoWork += (_, e) =>
                {
                    byte[] data;

                    while (!connection.Read(out data))
                    {
                        Reconnect();
                    }

                    e.Result = (UInt16)((data[1] << 8) | data[2]);
                };

                reader.RunWorkerCompleted += (_, e) =>
                {
                    train.Send((UInt16)e.Result);
                };

                reader.RunWorkerAsync();
            };

            BackgroundWorker waiter = new BackgroundWorker();
            waiter.DoWork += (_, e) =>
            {
                Reconnect();
            };

            waiter.RunWorkerCompleted += (_, e) =>
            {
                train.onLastFinished(null, new FinishedEventArgs(0));
            };

            waiter.RunWorkerAsync();
        }

        private void Reconnect()
        {
            connection.Close();
            while (!connection.Open())
                Thread.Sleep(10000);
        }

        //Stop screensaver if some event happens
        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            var pos = e.GetPosition(this);
            if (mousePos.X != -1 || mousePos.Y != -1)
            {
                if (Math.Abs(pos.X - mousePos.X) > 5 || Math.Abs(pos.Y - mousePos.Y) > 5)
                    StopScreenSaver();
            }
            mousePos = pos;
        }
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
            => StopScreenSaver();
        private void Window_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
            => StopScreenSaver();
        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
            => StopScreenSaver();
        private void Window_KeyDown(object sender, KeyEventArgs e)
            => StopScreenSaver();

        public void StopScreenSaver()
        {
            //connectionThread.Abort();
            Application.Current.Shutdown();
        }
    }
}
