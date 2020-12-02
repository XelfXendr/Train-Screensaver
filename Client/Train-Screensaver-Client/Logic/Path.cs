using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace Train_Screensaver_Client.Logic
{
    public class Path
    {
        private double maxTop, minTop, toTop, width;
        
        public Point[] pathPoints;

        public double length = 0;

        public Path(double maxTop, double minTop, double screenWidth)
        {
            this.maxTop = maxTop;
            this.minTop = minTop;
            this.width = screenWidth;
        }

        public Point GetPoint(double distance)
        {
            if (distance >= length)
                return new Point(distance - length, toTop);

            double index = pathPoints.Length * distance / length;
            if (index >= pathPoints.Length - 1)
                return new Point(distance - length, toTop);
            double part = index % 1;

            return new Point(
                pathPoints[(int)index].X * (1 - part) + pathPoints[(int)index + 1].X * part,
                pathPoints[(int)index].Y * (1 - part) + pathPoints[(int)index + 1].Y * part
                );

        }

        public void GeneratePath(double fromTop)
        {
            int halfSegCount = 20; //how many segments the final Path consists of divided by 2
            int pointCount = 2 * halfSegCount + 1;

            Random random = new Random();
            toTop = random.NextDouble() * (maxTop - minTop) + minTop;

            Point[] points = new Point[7];
            points[0] = new Point(0, fromTop);
            points[3] = new Point((random.NextDouble() + 1) * (width / 3), random.NextDouble() * (maxTop - minTop) + minTop);
            points[6] = new Point(width, toTop);

            double a = (points[3].X - points[0].X) / 2;
            points[1] = new Point(a, points[0].Y);
            points[2] = new Point(a, points[3].Y);
            a = (points[6].X - points[3].X) / 2 + points[3].X;
            points[4] = new Point(a, points[3].Y);
            points[5] = new Point(a, points[6].Y);

            //Making path from two Bézier curves
            Point[] path = new Point[pointCount];

            for (int i = 0; i < halfSegCount; i++)
            {
                double t = (double)i / halfSegCount;
                double s = 1 - t;
                path[i] = new Point(
                    ((points[0].X * s + points[1].X * t) * s + (points[1].X * s + points[2].X * t) * t) * s + ((points[1].X * s + points[2].X * t) * s + (points[2].X * s + points[3].X * t) * t) * t,
                    ((points[0].Y * s + points[1].Y * t) * s + (points[1].Y * s + points[2].Y * t) * t) * s + ((points[1].Y * s + points[2].Y * t) * s + (points[2].Y * s + points[3].Y * t) * t) * t
                    );
            }
            for (int i = 0; i <= halfSegCount; i++)
            {
                double t = (double)i / halfSegCount;
                double s = 1 - t;
                path[i + halfSegCount] = new Point(
                    ((points[3].X * s + points[4].X * t) * s + (points[4].X * s + points[5].X * t) * t) * s + ((points[4].X * s + points[5].X * t) * s + (points[5].X * s + points[6].X * t) * t) * t,
                    ((points[3].Y * s + points[4].Y * t) * s + (points[4].Y * s + points[5].Y * t) * t) * s + ((points[4].Y * s + points[5].Y * t) * s + (points[5].Y * s + points[6].Y * t) * t) * t
                    );
            }

            double[] accLen = new double[pointCount];

            accLen[0] = 0;
            for (int i = 1; i < pointCount; i++)
            {
                double r = path[i - 1].X - path[i].X;
                double s = path[i - 1].Y - path[i].Y;
                accLen[i] = accLen[i - 1] + Math.Sqrt(r * r + s * s);
            }

            length = accLen[pointCount - 1];
            double step = length / (pointCount - 1);
            Point[] betterPath = new Point[pointCount];

            for (int i = 0, j = 0; i < pointCount - 1; i++)
            {
                while (accLen[j + 1] < i * step)
                    j++;

                double part = (i * step - accLen[j]) / (accLen[j + 1] - accLen[j]);

                betterPath[i] = new Point(
                    path[j].X * (1 - part) + path[j + 1].X * part,
                    path[j].Y * (1 - part) + path[j + 1].Y * part
                    );
            }
            betterPath[pointCount - 1] = path[pointCount - 1];

            pathPoints = betterPath;
        }
    }
}
