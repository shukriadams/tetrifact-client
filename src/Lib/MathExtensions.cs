using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TetrifactClient
{
    public static class MathHelper
    {
        /// <summary>
        /// Calculates percent from two longs.
        /// </summary>
        /// <param name="progress"></param>
        /// <param name="total"></param>
        /// <returns></returns>
        public static int Percent(long progress, long total)
        {
            if (total == 0)
                return 0;

            decimal p = (decimal)progress / (decimal)total;
            return (int)System.Math.Round((decimal)(p * 100), 0);
        }

    }
}
