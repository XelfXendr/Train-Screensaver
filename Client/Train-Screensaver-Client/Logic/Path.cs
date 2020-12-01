using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace Train_Screensaver_Client.Logic
{
    class Path
    {
        public Point[] path;

        public Path(double fromTop, double maxTop, double minTop, double screenWidth)
        {
            Random random = new Random();
            double toTop = random.NextDouble() * (maxTop - minTop) + minTop;

            Point[] points = new Point[7];
            points[0] = new Point(0, fromTop);
            points[3] = new Point((random.NextDouble() + 1) * (screenWidth / 3), random.NextDouble() * (maxTop - minTop) + minTop);
            points[6] = new Point(screenWidth, toTop);

            double a = (points[3].X - points[0].X) / 2;
            points[1] = new Point(a, points[0].Y);
            points[2] = new Point(a, points[3].Y);
            a = (points[6].X - points[3].X) / 2 + points[3].X;
            points[4] = new Point(a, points[3].Y);
            points[5] = new Point(a, points[6].Y);

            //Making path from two Bézier curves
            path = new Point[41];

            for(int i = 0; i < 20; i++)
            {
                double t = (double)i / 20;
                double s = 1 - t;
                path[i] = new Point(
                    ((points[0].X * s + points[1].X * t) * s + (points[1].X * s + points[2].X * t) * t) * s + ((points[1].X * s + points[2].X * t) * s + (points[2].X * s + points[3].X * t) * t) * t,
                    ((points[0].Y * s + points[1].Y * t) * s + (points[1].Y * s + points[2].Y * t) * t) * s + ((points[1].Y * s + points[2].Y * t) * s + (points[2].Y * s + points[3].Y * t) * t) * t
                    );
            }
            for (int i = 0; i <= 20; i++)
            {
                double t = (double)i / 20;
                double s = 1 - t;
                path[i + 20] = new Point(
                    ((points[3].X * s + points[4].X * t) * s + (points[4].X * s + points[5].X * t) * t) * s + ((points[4].X * s + points[5].X * t) * s + (points[5].X * s + points[6].X * t) * t) * t,
                    ((points[3].Y * s + points[4].Y * t) * s + (points[4].Y * s + points[5].Y * t) * t) * s + ((points[4].Y * s + points[5].Y * t) * s + (points[5].Y * s + points[6].Y * t) * t) * t
                    );
            }

        }
    }
}
