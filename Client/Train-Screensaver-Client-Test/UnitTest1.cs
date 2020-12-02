using System;
using Xunit;
using Train_Screensaver_Client;
using Train_Screensaver_Client.Logic;

namespace Train_Screensaver_Client_Test
{
    public class UnitTest1
    {
        [Theory] //Check that the path doesn't go backwards at any point, 
        [InlineData(500, 1000, 200, 2000, 100)]
        
        public void RightGoingNoBack(double fromTop, double maxTop, double minTop, double width, int numberOfPaths)
        {
            var path = new Path(maxTop, minTop, width);
            for (int p = 0; p < numberOfPaths; p++)
            {
                path.GeneratePath(fromTop);
                var points = path.pathPoints;

                for (int i = 1; i < points.Length; i++)
                    Assert.True(points[i - 1].X < points[i].X);
            }
        }

        [Theory] //Check that the path is inside the specified bounds
        [InlineData(500, 1000, 200, 2000, 100)]
        [InlineData(200, 1000, 200, 2000, 5)]
        [InlineData(1000, 1000, 200, 2000, 5)]
        public void RightGoingInBounds(double fromTop, double maxTop, double minTop, double width, int numberOfPaths)
        {
            var path = new Path(maxTop, minTop, width);
            for (int p = 0; p < numberOfPaths; p++)
            {
                path.GeneratePath(fromTop);
                var points = path.pathPoints;

                for (int i = 0; i < points.Length; i++)
                    Assert.True(points[i].X >= 0 && points[i].X <= width && points[i].Y >= minTop && points[i].Y <= maxTop);
            }
        }

        [Theory] //Check that the ends are where they should be
        [InlineData(500, 1000, 200, 2000, 10)]
        public void RightGoingEnds(double fromTop, double maxTop, double minTop, double width, int numberOfPaths)
        {
            var path = new Path(maxTop, minTop, width);
            for (int p = 0; p < numberOfPaths; p++)
            {
                path.GeneratePath(fromTop);
                var points = path.pathPoints;

                Assert.True(AproxEqual(points[0].X, 0) && AproxEqual(points[0].Y, fromTop) && AproxEqual(points[points.Length - 1].X, width));
            }
        }

        private bool AproxEqual(double a, double b)
            => a * 0.999 <= b && a * 1.001 >= b;
    }
}
