using System;
using System.Windows;

namespace Train_Screensaver_Client.Logic
{
    public class Path
    {
        public double maxTop, minTop, toTop, width; //screen bounds, where the train can appear
        public Point[] pathPoints; //points of the Path
        public double length = 0; //length of the path

        public Path(double maxTop, double minTop, double screenWidth)
        {
            this.maxTop = maxTop;
            this.minTop = minTop;
            this.width = screenWidth;
        }

        //get point on Path from distance from the start
        public Point GetPoint(double distance)
        {
            if(distance < 0)
                return new Point(distance, pathPoints[0].Y);
            if (distance >= length)
                return new Point(distance - length + width, toTop);

            double index = (pathPoints.Length - 1) * distance / length;
            if (index >= pathPoints.Length - 1)
                return new Point(distance - length + width, toTop);
            double part = index % 1;

            return new Point(
                pathPoints[(int)index].X * (1 - part) + pathPoints[(int)index + 1].X * part,
                pathPoints[(int)index].Y * (1 - part) + pathPoints[(int)index + 1].Y * part
                );
        }

        //Converts the Y position of the end point of the Path to a UInt16 value used to send to the server
        public UInt16 GetToTop()
        {
            return (UInt16)((toTop - minTop) / (maxTop - minTop) * UInt16.MaxValue);
        }

        //Generates a new path
        public void GeneratePath(UInt16 top)
        {
            //Path generation works by generating 2 cubic Bézier curves and then turning them into equally-spaced points

            //Computes a starting point Y position from a UInt16 value sent by the server
            double fromTop = ((double)top / UInt16.MaxValue) * (maxTop - minTop) + minTop;

            int halfSegCount = 20; //how many segments the final Path consists of divided by 2
            int pointCount = 2 * halfSegCount + 1;

            Random random = new Random();
            toTop = random.NextDouble() * (maxTop - minTop) + minTop;

            //7 main points: start point, finish point, and 5 more in between
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

            //Making path from two cubic Bézier curves
            Point[] path = new Point[pointCount];

            for (int i = 0; i < halfSegCount; i++) //first curve
            {
                double t = (double)i / halfSegCount;
                double s = 1 - t;
                path[i] = new Point(
                    ((points[0].X * s + points[1].X * t) * s + (points[1].X * s + points[2].X * t) * t) * s + ((points[1].X * s + points[2].X * t) * s + (points[2].X * s + points[3].X * t) * t) * t,
                    ((points[0].Y * s + points[1].Y * t) * s + (points[1].Y * s + points[2].Y * t) * t) * s + ((points[1].Y * s + points[2].Y * t) * s + (points[2].Y * s + points[3].Y * t) * t) * t
                    );
            }
            for (int i = 0; i <= halfSegCount; i++) //second curve
            {
                double t = (double)i / halfSegCount;
                double s = 1 - t;
                path[i + halfSegCount] = new Point(
                    ((points[3].X * s + points[4].X * t) * s + (points[4].X * s + points[5].X * t) * t) * s + ((points[4].X * s + points[5].X * t) * s + (points[5].X * s + points[6].X * t) * t) * t,
                    ((points[3].Y * s + points[4].Y * t) * s + (points[4].Y * s + points[5].Y * t) * t) * s + ((points[4].Y * s + points[5].Y * t) * s + (points[5].Y * s + points[6].Y * t) * t) * t
                    );
            }

            //turning curves into equally-spaced points
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
