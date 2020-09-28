// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.CompilerServices;

namespace Iot.Device.Usb.Objects
{
    /// <summary>
    /// Base class for shared object logic.
    /// </summary>
    public abstract class ObjectBase
    {
        /// <summary>Gets or sets the value.</summary>
        public uint Value { get; protected set; }

        /// <summary>Gets the value of a bit.</summary>
        /// <param name="bit">The bit.</param>
        /// <returns>Value of the bit.</returns>
        protected bool GetBit(int bit) => ((Value >> bit) & 0b1) == 1;

        /// <summary>Updates the value of a bit.</summary>
        /// <param name="bit">The bit.</param>
        /// <param name="value">The new value.</param>
        protected void UpdateBit(int bit, bool value)
        {
            if (value)
            {
                Value |= (uint)(0b1 << bit);
            }
            else
            {
                Value &= ~(uint)(0b1 << bit);
            }
        }

        /// <summary>Checks it an argument is in range.</summary>
        /// <param name="value">The value.</param>
        /// <param name="maxValue">The maximum value.</param>
        /// <param name="minValue">The minimum value.</param>
        /// <param name="propertyName">Name of the property.</param>
        protected void CheckArgumentInRange(int value, int maxValue, int minValue = 0, [CallerMemberName] string propertyName = null)
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
        protected void CheckArgumentInRange(double value, double maxValue, double minValue = 0, [CallerMemberName] string propertyName = null)
        {
            if (value < minValue || value > maxValue)
            {
                throw new ArgumentOutOfRangeException(propertyName);
            }
        }
    }
}
