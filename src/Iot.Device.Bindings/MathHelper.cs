// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;

namespace System
{
    public static class MathHelper
    {
#if NETSTANDARD2_0
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte Clamp(byte value, byte min, byte max)
        {
            if (min > max)
            {
                ThrowMinMaxException(min, max);
            }

            if (value < min)
            {
                return min;
            }
            else if (value > max)
            {
                return max;
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Clamp(double value, double min, double max)
        {
            if (min > max)
            {
                ThrowMinMaxException(min, max);
            }

            if (value < min)
            {
                return min;
            }
            else if (value > max)
            {
                return max;
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Clamp(int value, int min, int max)
        {
            if (min > max)
            {
                ThrowMinMaxException(min, max);
            }

            if (value < min)
            {
                return min;
            }
            else if (value > max)
            {
                return max;
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Clamp(uint value, uint min, uint max)
        {
            if (min > max)
            {
                ThrowMinMaxException(min, max);
            }

            if (value < min)
            {
                return min;
            }
            else if (value > max)
            {
                return max;
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long Clamp(long value, long min, long max)
        {
            if (min > max)
            {
                ThrowMinMaxException(min, max);
            }

            if (value < min)
            {
                return min;
            }
            else if (value > max)
            {
                return max;
            }

            return value;
        }


        private static void ThrowMinMaxException<T>(T min, T max)
        {
            throw new ArgumentException($"Min {min} should be less than max {max}.");
        }
#else
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte Clamp(byte value, byte min, byte max)
        {
            return Math.Clamp(value, min, max);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Clamp(double value, double min, double max)
        {
            return Math.Clamp(value, min, max);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Clamp(int value, int min, int max)
        {
            return Math.Clamp(value, min, max);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Clamp(uint value, uint min, uint max)
        {
            return Math.Clamp(value, min, max);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long Clamp(long value, long min, long max)
        {
            return Math.Clamp(value, min, max);
        }
#endif
    }
}
