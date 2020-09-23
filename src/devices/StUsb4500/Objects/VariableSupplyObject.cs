// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using UnitsNet;

namespace Iot.Device.StUsb4500.Objects
{
    /// <summary>
    /// PDO of a variable supply source.
    /// </summary>
    public class VariableSupplyObject : PowerDeliveryObject
    {
        private const uint OperationalCurrentMask = 0b0011_1111_1111;
        private const uint MinVoltageMask = 0b1111_1111_1100_0000_0000;
        private const uint MaxVoltageMask = 0b0011_1111_1111_0000_0000_0000_0000_0000;

        /// <summary>Gets or sets the operational current.</summary>
        /// <remarks>This is stored with the factor 100 as a 10-bit value (range 0 - 1023) => 0...10.23A.</remarks>
        public ElectricCurrent OperationalCurrent
        {
            get => ElectricCurrent.FromAmperes((ushort)(Value & OperationalCurrentMask) / 100.0);
            set
            {
                CheckArgumentInRange(value.Value, 10.23);
                Value = (Value & ~OperationalCurrentMask) | (Convert.ToUInt32(value * 100) & OperationalCurrentMask);
            }
        }

        /// <summary>Gets or sets the minimal voltage.</summary>
        /// <remarks>This is stored with the factor 20 as a 10-bit value (range 0 - 1023) => 0...51.15V.</remarks>
        public ElectricPotentialDc MinimalVoltage
        {
            get => ElectricPotentialDc.FromVoltsDc((ushort)((Value & MinVoltageMask) >> 10) / 20.0);
            set
            {
                CheckArgumentInRange(value.Value, 51.15);
                Value = (Value & ~MinVoltageMask) | (Convert.ToUInt32(value * 20) << 10 & MinVoltageMask);
            }
        }

        /// <summary>Gets or sets the maximal voltage.</summary>
        /// <remarks>This is stored with the factor 20 as a 10-bit value (range 0 - 1023) => 0...51.15V.</remarks>
        public ElectricPotentialDc MaximalVoltage
        {
            get => ElectricPotentialDc.FromVoltsDc((ushort)((Value & MaxVoltageMask) >> 20) / 20.0);
            set
            {
                CheckArgumentInRange(value.Value, 51.15);
                Value = (Value & ~MaxVoltageMask) | (Convert.ToUInt32(value * 20) << 20 & MaxVoltageMask);
            }
        }

        /// <summary>Gets the power of this PDO.</summary>
        public override Power Power => Power.Zero; // not clearly defined for variable source

        /// <summary>Initializes a new instance of the <see cref="VariableSupplyObject"/> class.</summary>
        /// <param name="value">The value.</param>
        public VariableSupplyObject(uint value)
            : base(value)
        {
        }

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString() => $"{nameof(VariableSupplyObject)}: {MinimalVoltage:0.##} - {MaximalVoltage:0.##} @ {OperationalCurrent:0.##}";
    }
}
