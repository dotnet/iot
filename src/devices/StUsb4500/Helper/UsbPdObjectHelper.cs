using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Iot.Device.Usb.Helper
{
    internal static class UsbPdObjectHelper
    {
        /// <summary>Gets the value of a bit.</summary>
        /// <param name="value">The value.</param>
        /// <param name="bit">The bit.</param>
        /// <returns>Value of the bit.</returns>
        public static bool GetBit(this uint value, int bit) => ((value >> bit) & 0b1) == 1;

        /// <summary>Updates the value of a bit.</summary>
        /// <param name="value">The value.</param>
        /// <param name="bit">The bit.</param>
        /// <param name="newValue">The new value.</param>
        public static uint UpdateBit(this uint value, int bit, bool newValue)
        {
            if (newValue)
            {
                value |= (uint)(0b1 << bit);
            }
            else
            {
                value &= ~(uint)(0b1 << bit);
            }

            return value;
        }

        /// <summary>Checks it an argument is in range.</summary>
        /// <param name="value">The value.</param>
        /// <param name="maxValue">The maximum value.</param>
        /// <param name="minValue">The minimum value.</param>
        /// <param name="propertyName">Name of the property.</param>
        public static void CheckArgumentInRange(this byte value, byte maxValue, byte minValue = 0, [CallerMemberName] string propertyName = null)
        {
            if (value < minValue || value > maxValue)
            {
                throw new ArgumentOutOfRangeException(propertyName);
            }
        }

        /// <summary>Checks it an argument is in range.</summary>
        /// <param name="value">The value.</param>
        /// <param name="maxValue">The maximum value.</param>
        /// <param name="minValue">The minimum value.</param>
        /// <param name="propertyName">Name of the property.</param>
        public static void CheckArgumentInRange(this int value, int maxValue, int minValue = 0, [CallerMemberName] string propertyName = null)
        {
            if (value < minValue || value > maxValue)
            {
                throw new ArgumentOutOfRangeException(propertyName);
            }
        }

        /// <summary>Checks it an argument is in range.</summary>
        /// <param name="value">The value.</param>
        /// <param name="maxValue">The maximum value.</param>
        /// <param name="minValue">The minimum value.</param>
        /// <param name="propertyName">Name of the property.</param>
        public static void CheckArgumentInRange(this double value, double maxValue, double minValue = 0, [CallerMemberName] string propertyName = null)
        {
            if (value < minValue || value > maxValue)
            {
                throw new ArgumentOutOfRangeException(propertyName);
            }
        }
    }
}
