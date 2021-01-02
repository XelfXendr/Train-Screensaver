using System;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Windows;
using System.Windows.Media;

namespace Train_Screensaver_Client.Logic
{
    //🚂🚃🚃🚃🚃🚃🚃🚃🚃🚃
    public class Train
    {
        private Image[] wagons;
        
        private DispatcherTimer dispatherTimer;
        
        private Path path;
        private bool right = true;
        private double currentDistance = 0;
        private bool firstFinished = false;

        private double speed = 300;

        public EventHandler<FinishedEventArgs> onFirstFinished;
        public EventHandler<FinishedEventArgs> onLastFinished;

        public Train(Canvas canvas, Config config, BitmapImage[] images)
        {
            wagons = new Image[config.trainIndexes.Length];

            //Create WPF Image objects (wagons) from supplied Bitmaps
            double maxHeight = 0;
            for(int i = 0; i < config.trainIndexes.Length; i++)
            {
                BitmapImage bitmap;
                if (config.trainIndexes[i] >= config.wagonSources.Length || config.trainIndexes[i] < 0)
                {
                    //index out of bounds, use "wrong index" texture instead
                    using (var ms = new System.IO.MemoryStream(Properties.Resources.WrongIndex))
                    {
                        bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.StreamSource = ms;
                        bitmap.EndInit();
                    }
                }
                else
                    bitmap = images[config.trainIndexes[i]];

                wagons[i] = new Image() { Source = bitmap, Width = bitmap.Width, Height = bitmap.Height };
                wagons[i].RenderTransformOrigin = new Point(0.5, 0.5);
                canvas.Children.Add(wagons[i]);
                Canvas.SetLeft(wagons[i], -wagons[i].Width);

                if (wagons[i].Height > maxHeight)
                    maxHeight = wagons[i].Height;
            }

            path = new Path(canvas.ActualHeight - maxHeight - 10, 10, canvas.ActualWidth);

            //Animation frame loop
            dispatherTimer = new DispatcherTimer();
            dispatherTimer.Tick += (_, e) =>
            {
                double dist = currentDistance;

                //move each wagon
                foreach (var wagon in wagons)
                {
                    Point pointBack = path.GetPoint(dist);
                    Point pointFront = path.GetPoint(dist + wagon.Width);

                    if (!right)
                    {
                        pointFront.X = path.width - pointFront.X;
                        pointBack.X = path.width - pointBack.X;
                    }

                    Canvas.SetLeft(wagon, pointBack.X);
                    Canvas.SetTop(wagon, pointBack.Y + maxHeight - wagon.Height);

                    double x = pointFront.X - pointBack.X;
                    double y = pointFront.Y - pointBack.Y;
                    double sin = y / Math.Sqrt(x*x + y*y);

                    TransformGroup grp = new TransformGroup();

                    if (!right)
                    {
                        grp.Children.Add(new ScaleTransform(-1, 1));
                        grp.Children.Add(new RotateTransform(-Math.Asin(sin) / Math.PI * 180, wagon.Width / 2, wagon.Height / 2));
                    }
                    else
                        grp.Children.Add(new RotateTransform(Math.Asin(sin) / Math.PI * 180, -wagon.Width / 2, wagon.Height / 2));
                    wagon.RenderTransform = grp;
                    dist -= wagon.Width;
                }

                //check if first wagon has finished it's trip
                if(!firstFinished && currentDistance > path.length)
                {
                    firstFinished = true;
                    if (onFirstFinished != null)
                        onFirstFinished(null, new FinishedEventArgs(path.GetToTop()));
                }

                //check if the last wagon has finished it's trip
                if (dist > path.length)
                {
                    dispatherTimer.Stop();
                    if (onLastFinished != null)
                        onLastFinished(null, new FinishedEventArgs(path.GetToTop()));
                }

                currentDistance += speed / config.framerate;
            };
            dispatherTimer.Interval = TimeSpan.FromMilliseconds( 1000d / config.framerate );
        }

        //send new train
        public void Send(bool right, UInt16 top)
        {
            this.right = right;
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
