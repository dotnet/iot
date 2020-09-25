// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using UnitsNet;

namespace Iot.Device.StUsb4500.Objects
{
    /// <summary>
    /// Representation of a request data object (=RDO).
    /// </summary>
    public class RequestDataObject : ObjectBase
    {
        private const ushort MaximalCurrentMask = 0b0011_1111_1111;
        private const uint OperatingCurrentMask = 0b1111_1111_1100_0000_0000;
        private const byte ObjectPositionMask = 0b0111;

        /// <summary>Gets or sets the maximal current.</summary>
        /// <remarks>This is stored with the factor 100 as a 10-bit value (range 0 - 1023) => 0...10.23A.</remarks>
        public ElectricCurrent MaximalCurrent
        {
            get => ElectricCurrent.FromAmperes((ushort)(Value & MaximalCurrentMask) / 100.0);
            set
            {
                CheckArgumentInRange(value.Amperes, 10.23);
                Value = (Value & MaximalCurrentMask) | (uint)(Convert.ToUInt16(value.Amperes * 100) & MaximalCurrentMask);
            }
        }

        /// <summary>Gets or sets the operational current.</summary>
        /// <remarks>This is stored with the factor 100 as a 10-bit value (range 0 - 1023) => 0...10.23A.</remarks>
        public ElectricCurrent OperatingCurrent
        {
            get => ElectricCurrent.FromAmperes(((ushort)(Value & OperatingCurrentMask) >> 10) / 100.0);
            set
            {
                CheckArgumentInRange(value.Amperes, 10.23);
                Value = (Value & OperatingCurrentMask) | (ushort)(Convert.ToUInt16(value.Amperes * 100) << 10 & OperatingCurrentMask);
            }
        }

        /// <summary>Gets or sets a value indicating whether unchunked extended messages are supported.</summary>
        public bool UnchunkedExtendedMessagesSupported
        {
            get => GetBit(23);
            set => UpdateBit(23, value);
        }

        /// <summary>Gets or sets a value indicating whether the USB is not suspended.</summary>
        public bool NoUsbSuspend
        {
            get => GetBit(24);
            set => UpdateBit(24, value);
        }

        /// <summary>Gets or sets a value indicating whether the port is capable of USB communications.</summary>
        public bool UsbCommunicationsCapable
        {
            get => GetBit(25);
            set => UpdateBit(25, value);
        }

        /// <summary>Gets or sets a value indicating whether there is a capability mismatch.</summary>
        public bool CapabilityMismatch
        {
            get => GetBit(26);
            set => UpdateBit(26, value);
        }

        /// <summary>Gets or sets a value indicating whether the give back flag is set.</summary>
        public bool GiveBackFlag
        {
            get => GetBit(27);
            set => UpdateBit(27, value);
        }

        /// <summary>Gets or sets the object position.</summary>
        /// <remarks>This is stored as a 3-bit value (range 0 - 7). 000b is Reserved and shall not be used.</remarks>
        public byte ObjectPosition
        {
            get => (byte)(Value >> 28 & ObjectPositionMask);
            set
            {
                CheckArgumentInRange(value, 7);
                Value = (Value & ObjectPositionMask) | (uint)((value & ObjectPositionMask) << 28);
            }
        }

        /// <summary>Initializes a new instance of the <see cref="RequestDataObject"/> class.</summary>
        /// <param name="value">The value.</param>
        public RequestDataObject(uint value) => Value = value;
    }
}
