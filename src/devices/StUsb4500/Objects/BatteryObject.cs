// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.StUsb4500.Objects
{
    /// <summary>
    /// PDO of a battery sources.
    /// </summary>
    public class BatteryObject : PowerDeliveryObject
    {
        private const uint OperatingPowerMask = 0b0011_1111_1111;
        private const uint MinVoltageMask = 0b1111_1111_1100_0000_0000;
        private const uint MaxVoltageMask = 0b0011_1111_1111_0000_0000_0000_0000_0000;

        /// <summary>Gets or sets the operating power.</summary>
        /// <remarks>This is stored with the factor 4 as a 10-bit value (range 0 - 1023) => 0...255.75W.</remarks>
        public double OperatingPower
        {
            get => ((ushort)(Value & OperatingPowerMask)) / 4.0;
            set => Value = (Value & ~OperatingPowerMask) | (Convert.ToUInt32(value * 4) & OperatingPowerMask);
        }

        /// <summary>Gets or sets the minimal voltage.</summary>
        /// <remarks>This is stored with the factor 20 as a 10-bit value (range 0 - 1023) => 0...51.15V.</remarks>
        public double MinimalVoltage
        {
            get => ((ushort)((Value & MinVoltageMask) >> 10)) / 20.0;
            set => Value = (Value & ~MinVoltageMask) | (Convert.ToUInt32(value * 20) << 10 & MinVoltageMask);
        }

        /// <summary>Gets or sets the maximal voltage.</summary>
        /// <remarks>This is stored with the factor 20 as a 10-bit value (range 0 - 1023) => 0...51.15V.</remarks>
        public double MaximalVoltage
        {
            get => ((ushort)((Value & MaxVoltageMask) >> 20)) / 20.0;
            set => Value = (Value & ~MaxVoltageMask) | (Convert.ToUInt32(value * 20) << 20 & MaxVoltageMask);
        }

        /// <summary>Gets the power of this PDO.</summary>
        public override double Power => OperatingPower;

        /// <summary>Initializes a new instance of the <see cref="BatteryObject"/> class.</summary>
        /// <param name="value">The value.</param>
        public BatteryObject(uint value)
            : base(value)
        {
        }

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString() => $"{nameof(BatteryObject)}: {MinimalVoltage:0.##}V - {MaximalVoltage:0.##}V @ {OperatingPower:0.##}W";
    }
}
