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
        /// <summary>
        /// Gets or sets the value which encodes all properties of this PDO and can be used to send this PDO to the USB-PD controller.
        /// See USB-PD specification for details.
        /// </summary>
        public uint Value { get; protected set; }

        /// <summary>Gets the power of this PDO.</summary>
        public abstract Power Power { get; }

        /// <summary>Initializes a new instance of the <see cref="PowerDeliveryObject"/> class.</summary>
        /// <param name="rawValue">The raw value which encodes all properties of this PDO. See USB-PD specification for details.</param>
        protected PowerDeliveryObject(uint rawValue) => Value = rawValue;

        /// <summary>Creates a new PDO from the given value.</summary>
        /// <param name="rawValue">
        /// The raw value received from or sent to the USB-PD controller which encodes all properties of this PDO. See USB-PD specification for details.
        /// </param>
        /// <returns>
        /// A PDO of the type defined by the value,
        /// which can be a <see cref="FixedSupplyObject"/>, <see cref="VariableSupplyObject"/> or <see cref="BatteryObject"/>.
        /// </returns>
        public static PowerDeliveryObject CreateFromValue(uint rawValue)
        {
            uint pdoType = rawValue >> 30;
            switch (pdoType)
            {
                case 0:
                    return new FixedSupplyObject(rawValue);
                case 1:
                    return new VariableSupplyObject(rawValue);
                case 2:
                    return new BatteryObject(rawValue);
                default:
                    throw new ArgumentOutOfRangeException(nameof(rawValue));
            }
        }
    }
}
