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

        public ScreensaverWindow()
        {
            InitializeComponent();
            
            this.Left = SystemParameters.VirtualScreenLeft - 10;
            this.Top = SystemParameters.VirtualScreenTop - 10;

            this.Height = SystemParameters.VirtualScreenHeight + 20;
            this.Width = SystemParameters.VirtualScreenWidth + 20;
            
            WindowState = WindowState.Normal;
            WindowStyle = WindowStyle.None;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.None;
            Topmost = true;

            BackgroundWorker loader = new BackgroundWorker();

            loader.DoWork += (_, e) =>
            {
                //load configuration and wagon images
                var config = Configurator.LoadConfig();
                var loadedImages = config.LoadImages();
                var connection = new Connection(config.server, config.port);

                e.Result = (config, loadedImages, connection);

                //Begin connection
                Reconnect(connection);        
            };
            loader.RunWorkerCompleted += (_, e) =>
            {
                (Config config, BitmapImage[] loadedImages, Connection connection) = ((Config, BitmapImage[], Connection))e.Result;

                train = new Train(screensaverCanvas, config, loadedImages);

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
                            Reconnect(connection);
                        }

                        e.Result = (UInt16)((data[1] << 8) | data[2]);
                    };

                    reader.RunWorkerCompleted += (_, e) =>
                    {
                        train.Send((UInt16)e.Result);
                    };

                    reader.RunWorkerAsync();
                };

                //Once connected, begin communication
                train.onLastFinished(null, new FinishedEventArgs(0));
            };


            loader.RunWorkerAsync();
        }

        private void Reconnect(Connection connection)
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
