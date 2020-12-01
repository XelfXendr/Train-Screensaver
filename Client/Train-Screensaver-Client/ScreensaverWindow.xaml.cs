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

namespace Train_Screensaver_Client
{
    /// <summary>
    /// Interakční logika pro ScreensaverWindow.xaml
    /// </summary>
    public partial class ScreensaverWindow : Window
    {
        private Point mousePos = new Point(-1, -1);

        public ScreensaverWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.None;
            Topmost = true;
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
            Application.Current.Shutdown();
        }
    }
}
