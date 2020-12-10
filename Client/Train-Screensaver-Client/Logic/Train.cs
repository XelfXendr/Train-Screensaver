using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Linq;
using System.Windows.Threading;
using System.Windows;
using System.Windows.Media;

namespace Train_Screensaver_Client.Logic
{
    //🚂🚃🚃🚃🚃🚃🚃🚃🚃🚃
    public class Train
    {
        private Canvas canvas;
        private Image[] wagons;
        
        private DispatcherTimer dispatherTimer;
        
        private Path path;
        private double currentDistance = 0;
        private bool firstFinished = false;

        public EventHandler<FinishedEventArgs> onFirstFinished;
        public EventHandler<FinishedEventArgs> onLastFinished;

        public Train(Canvas canvas, string[] imageSources, int[] wagonIndexes)
        {
            this.canvas = canvas;
            dispatherTimer = new DispatcherTimer();
            path = new Path(canvas.ActualHeight, 100, canvas.ActualWidth);

            BitmapImage[] bitmaps = imageSources.Select((s) => new BitmapImage(new Uri(s))).ToArray();
            
            wagons = new Image[wagonIndexes.Length];

            for(int i = 0; i < wagonIndexes.Length; i++)
            {
                wagons[i] = new Image() { Source = bitmaps[wagonIndexes[i]], Width = bitmaps[wagonIndexes[i]].Width, };

                canvas.Children.Add(wagons[i]);
                Canvas.SetLeft(wagons[i], -wagons[i].Width);
            }


            dispatherTimer.Tick += (_, e) =>
            {
                double dist = currentDistance;

                foreach (var wagon in wagons)
                {
                    Point pointBack = path.GetPoint(dist);
                    Point pointFront = path.GetPoint(dist + wagon.Width);

                    Canvas.SetLeft(wagon, pointBack.X);
                    Canvas.SetTop(wagon, pointBack.Y);

                    double x = pointFront.X - pointBack.X;
                    double y = pointFront.Y - pointBack.Y;
                    double sin = y / Math.Sqrt(x*x + y*y);

                    wagon.RenderTransform = new RotateTransform(Math.Asin(sin) / Math.PI * 180);
                    dist -= wagon.Width;
                }

                if(!firstFinished && currentDistance > path.length)
                {
                    firstFinished = true;
                    if (onFirstFinished != null)
                        onFirstFinished(null, new FinishedEventArgs(path.GetToTop()));
                }

                if (dist > path.length)
                {
                    dispatherTimer.Stop();
                    if (onLastFinished != null)
                        onLastFinished(null, new FinishedEventArgs(path.GetToTop()));
                }

                currentDistance += 5;
            };
            dispatherTimer.Interval = TimeSpan.FromMilliseconds(50 / 3);
        }

        public void Send(UInt16 top)
        {
            firstFinished = false;
            currentDistance = -100;
            path.GeneratePath(top);
            dispatherTimer.Start();
        }
    }

    public class FinishedEventArgs : EventArgs
    {
        public UInt16 position;

        public FinishedEventArgs(UInt16 position)
        {
            this.position = position;
        }

    }
}
