// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using UnitsNet;

namespace Iot.Device.Usb
{
    /// <summary>
    /// Base class for all the different power delivery objects (=PDO).
    /// </summary>
    public abstract class PowerDeliveryObject
    {
        /// <summary>Gets or sets the value which encodes all properties of this PDO.</summary>
        public uint Value { get; protected set; }

        /// <summary>Gets the power of this PDO.</summary>
        public abstract Power Power { get; }

        /// <summary>Initializes a new instance of the <see cref="PowerDeliveryObject"/> class.</summary>
        /// <param name="value">The value.</param>
        protected PowerDeliveryObject(uint value) => Value = value;

        /// <summary>Creates a new PDO from the given value.</summary>
        /// <param name="value">The value.</param>
        /// <returns>A PDO of the type defined by the value.</returns>
        public static PowerDeliveryObject CreateFromValue(uint value)
        {
            uint pdoType = value >> 30;
            switch (pdoType)
            {
                case 0:
                    return new FixedSupplyObject(value);
                case 1:
                    return new VariableSupplyObject(value);
                case 2:
                    return new BatteryObject(value);
                default:
                    throw new ArgumentOutOfRangeException(nameof(value));
            }
        }
    }
}
