using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PZ2.Repository
{
    public static class MinMaxCoordCheck
    {
        public static double maxX = 0, minX = 100, maxY = 0, minY = 100;

        public static void Check(double X, double Y)
        {
            if (X > maxX)
                maxX = X;
            if (X < minX)
                minX = X;
            if (Y > maxY)
                maxY = Y;
            if (Y < minY)
                minY = Y;
        }
    }
}
