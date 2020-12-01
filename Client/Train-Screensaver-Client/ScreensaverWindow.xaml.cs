﻿using System;
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

using Train_Screensaver_Client.Logic;

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
            int[] indexes = new int[] { 0, 1, 1, 1, 5, 8, 6, 7, 2, 2, 4 };
            Train train = new Train(screensaverCanvas, sources, indexes);

            Logic.Path path = new Logic.Path((new Random()).NextDouble() * 1080, 980, 100, 1920);
            var points = path.path;

            for(int i = 0; i < path.path.Length - 1; i++)
            {
                var line = new Line()
                {
                    Stroke = Brushes.Yellow,
                    StrokeThickness = 2,
                    X1 = points[i].X,
                    X2 = points[i + 1].X,
                    Y1 = points[i].Y,
                    Y2 = points[i + 1].Y,
                };
                screensaverCanvas.Children.Add(line);
            }

            train.Send((float)(new Random()).NextDouble());
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
