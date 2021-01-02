using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

using Train_Screensaver_Client.Logic;
using Train_Screensaver_Client.Networking;
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
            
            //Make sure the window fills the whole screen(s)
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

            //Certain task like reading from a file and communicating with a server must be done asynchronously, otherwise they would block the application from being killed (a screensaver musts be killed as soon as the user triggers any event)
            BackgroundWorker loader = new BackgroundWorker();
            loader.DoWork += (_, e) =>
            {
                //load configuration and wagon images
                var config = Configurator.LoadConfig();
                var loadedImages = config.LoadImages();
                var connection = new Connection(config.server, config.port);

                e.Result = (config, loadedImages, connection); //pass result onto synchronous task

                connection.Reconnect(); //Begin connection
            };
            loader.RunWorkerCompleted += (_, e) =>
            {
                (Config config, BitmapImage[] loadedImages, Connection connection) = ((Config, BitmapImage[], Connection))e.Result;

                train = new Train(screensaverCanvas, config, loadedImages);

                train.onFirstFinished += (_, e) =>
                {
                    //once the first wagon finishes it's trip, notify the server
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
                    //once the last wagon finishes it's trip, wait for the server to send a new train
                    BackgroundWorker reader = new BackgroundWorker();
                    reader.DoWork += (_, e) =>
                    {
                        byte[] data;
                        while (!connection.Read(out data))
                        {
                            connection.Reconnect();
                        }
                        e.Result = (data[0] == 0x90, (UInt16)((data[1] << 8) | data[2]));
                    };
                    reader.RunWorkerCompleted += (_, e) =>
                    {
                        (bool goRight, UInt16 from) = ((bool, UInt16))e.Result;
                        train.Send(goRight, from);
                    };
                    reader.RunWorkerAsync();
                };

                //Once connected, begin communication
                train.onLastFinished(null, new FinishedEventArgs(0));
            };
            loader.RunWorkerAsync();
        }

        //Stop screensaver if some event happens
        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            //stop only if the mouse moved a lot
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
            => Application.Current.Shutdown();
    }
}
