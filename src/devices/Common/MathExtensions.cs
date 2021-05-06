using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device
{
    /// <summary>
    /// Implementations of some functions missing in older .NET versions
    /// </summary>
    public static class MathExtensions
    {
        /// <summary>
        /// Returns val, limited to the range min-max (inclusive)
        /// </summary>
        public static double Clamp(double val, double min, double max)
        {
#if NETSTANDARD2_0
            if (val < min)
            {
                return min;
            }

            if (val > max)
            {
                return max;
            }

            return val;
#else
            return Math.Clamp(val, min, max);
#endif
        }

        /// <summary>
        /// Returns val, limited to the range min-max (inclusive)
        /// </summary>
        public static int Clamp(int val, int min, int max)
        {
#if NETSTANDARD2_0
            if (val < min)
            {
                return min;
            }

            if (val > max)
            {
                return max;
            }

            return val;
#else
            return Math.Clamp(val, min, max);
#endif
        }
    }
}
