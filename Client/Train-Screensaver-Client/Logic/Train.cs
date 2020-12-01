using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Linq;
using System.Windows.Threading;

namespace Train_Screensaver_Client.Logic
{
    //🚂🚃🚃🚃🚃🚃🚃🚃🚃🚃
    class Train
    {
        private Canvas canvas;
        private Image[] wagons;

        public Train(Canvas canvas, string[] imageSources, int[] wagonIndexes)
        {
            this.canvas = canvas;

            BitmapImage[] bitmaps = imageSources.Select((s) => new BitmapImage(new Uri(s))).ToArray();

            wagons = new Image[wagonIndexes.Length];

            for(int i = 0; i < wagonIndexes.Length; i++)
            {
                wagons[i] = new Image() { Source = bitmaps[wagonIndexes[i]], Width = bitmaps[wagonIndexes[i]].Width, };
                canvas.Children.Add(wagons[i]);
                Canvas.SetLeft(wagons[i], -wagons[i].Width);
            }


            /*
            //To be removed:
            double left = 1500;
            foreach (var wagon in wagons)
            {
                Canvas.SetLeft(wagon, left);
                left -= wagon.Width;
                Canvas.SetTop(wagon, 500);
            }*/
            /*
            foreach( var source in imageSources )
            {
                var bitmap = new BitmapImage(new Uri(Environment.CurrentDirectory + "/Trains/front.png"));
                Image image = new Image
                {
                    Source = bitmap,
                };
                screensaverCanvas.Children.Add(image);
                RotateTransform rotateTransform = new RotateTransform(45);
                image.RenderTransform = rotateTransform;
                Canvas.SetLeft(image, 960);
                Canvas.SetTop(image, 540);
            }*/
        }

        public void Send(float top)
        {
            DispatcherTimer dispatherTimer = new DispatcherTimer();

            double left = 0;
            dispatherTimer.Tick += (_, e) => 
            {
                double currentLeft = left;

                foreach(var wagon in wagons)
                {
                    Canvas.SetLeft(wagon, currentLeft);
                    Canvas.SetTop(wagon, top * canvas.ActualHeight);
                    currentLeft -= wagon.Width;
                }
                
                left += 5;
            };
            dispatherTimer.Interval = TimeSpan.FromMilliseconds(50 / 3);
            dispatherTimer.Start();
        }
    }
}
