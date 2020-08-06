using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PZ2.Repository
{
    public static class CoordToIndexConverter
    {
        public static void CoordToIndex(double latitude, double longitude, out int indexI, out int indexJ)
        {
            indexI = 1000 - (int)((latitude - 45) * 1000);
            indexJ = (int)((longitude - 19) * 1000);
        }
    }
}
