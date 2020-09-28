// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Iot.Device.Usb.Helper;
using UnitsNet;

namespace Iot.Device.Usb.Objects
{
    /// <summary>
    /// PDO of a fixed supply sources.
    /// </summary>
    public class FixedSupplyObject : PowerDeliveryObject
    {
        private const uint OperationalCurrentMask = 0b0011_1111_1111;
        private const uint VoltageMask = 0b1111_1111_1100_0000_0000;

        /// <summary>Gets or sets the operational current.</summary>
        /// <remarks>This is stored with the factor 100 as a 10-bit value (range 0 - 1023) => 0...10.23A.</remarks>
        public ElectricCurrent OperationalCurrent
        {
            get => ElectricCurrent.FromAmperes((ushort)(Value & OperationalCurrentMask) / 100.0);
            set
            {
                value.Amperes.CheckArgumentInRange(10.23);
                Value = (Value & ~OperationalCurrentMask) | (Convert.ToUInt32(value.Amperes * 100) & OperationalCurrentMask);
            }
        }

        /// <summary>Gets or sets the voltage.</summary>
        /// <remarks>This is stored with the factor 20 as a 10-bit value (range 0 - 1023) => 0...51.15V.</remarks>
        public ElectricPotentialDc Voltage
        {
            get => ElectricPotentialDc.FromVoltsDc((ushort)((Value & VoltageMask) >> 10) / 20.0);
            set
            {
                value.VoltsDc.CheckArgumentInRange(51.15);
                Value = (Value & ~VoltageMask) | (Convert.ToUInt32(value.VoltsDc * 20) << 10 & VoltageMask);
            }
        }

        /// <summary>Gets or sets a value indicating whether the PDO supports dual role data.</summary>
        public bool DualRoleData
        {
            get => Value.GetBit(25);
            set => Value = Value.UpdateBit(25, value);
        }

        /// <summary>Gets or sets a value indicating whether this PDO is USB communications capable.</summary>
        public bool UsbCommunicationsCapable
        {
            get => Value.GetBit(26);
            set => Value = Value.UpdateBit(26, value);
        }

        /// <summary>Gets or sets a value indicating whether this PDO has unconstrained power.</summary>
        public bool UnconstrainedPower
        {
            get => Value.GetBit(27);
            set => Value = Value.UpdateBit(27, value);
        }

        /// <summary>Gets or sets a value indicating whether this PDO has a higher capability.</summary>
        public bool HigherCapability
        {
            get => Value.GetBit(28);
            set => Value = Value.UpdateBit(28, value);
        }

        /// <summary>Gets or sets a value indicating whether the PDO supports dual role power.</summary>
        public bool DualRolePower
        {
            get => Value.GetBit(29);
            set => Value = Value.UpdateBit(29, value);
        }

        /// <summary>Gets the power of this PDO.</summary>
        public override Power Power => Power.FromWatts(Voltage.VoltsDc * OperationalCurrent.Amperes);

        /// <summary>Initializes a new instance of the <see cref="FixedSupplyObject"/> class.</summary>
        /// <param name="value">The value.</param>
        public FixedSupplyObject(uint value)
            : base(value)
        {
        }

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString() => $"{nameof(FixedSupplyObject)}: {Voltage:0.##} * {OperationalCurrent:0.##} = {Power:0.##}";
    }
}
