// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using UnitsNet;

namespace Iot.Device.Usb
{
    /// <summary>
    /// Representation of a request data object (=RDO).
    /// </summary>
    public class RequestDataObject
    {
        private const ushort MaximalCurrentMask = 0b0011_1111_1111;
        private const uint OperatingCurrentMask = 0b1111_1111_1100_0000_0000;
        private const byte ObjectPositionMask = 0b0111;

        /// <summary>Gets the raw value received from the USB-PD controller which encodes all properties of this RDO. See USB-PD specification for details.</summary>
        public uint Value { get; private set; }

        /// <summary>Gets or sets the maximal current.</summary>
        /// <remarks>This is stored with the factor 100 as a 10-bit value (range 0 - 1023) => 0...10.23A.</remarks>
        public ElectricCurrent MaximalCurrent
        {
            get => ElectricCurrent.FromAmperes((ushort)(Value & MaximalCurrentMask) / 100.0);
            set
            {
                value.Amperes.CheckArgumentInRange(10.23);
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
                value.Amperes.CheckArgumentInRange(10.23);
                Value = (Value & OperatingCurrentMask) | (ushort)(Convert.ToUInt16(value.Amperes * 100) << 10 & OperatingCurrentMask);
            }
        }

        /// <summary>Gets or sets a value indicating whether unchunked extended messages are supported.</summary>
        public bool UnchunkedExtendedMessagesSupported
        {
            get => Value.GetBit(23);
            set => Value = Value.UpdateBit(23, value);
        }

        /// <summary>Gets or sets a value indicating whether the USB is not suspended.</summary>
        public bool NoUsbSuspend
        {
            get => Value.GetBit(24);
            set => Value = Value.UpdateBit(24, value);
        }

        /// <summary>Gets or sets a value indicating whether the port is capable of USB communications.</summary>
        public bool UsbCommunicationsCapable
        {
            get => Value.GetBit(25);
            set => Value = Value.UpdateBit(25, value);
        }

        /// <summary>Gets or sets a value indicating whether there is a capability mismatch.</summary>
        public bool CapabilityMismatch
        {
            get => Value.GetBit(26);
            set => Value = Value.UpdateBit(26, value);
        }

        /// <summary>Gets or sets a value indicating whether the give back flag is set.</summary>
        public bool GiveBackFlag
        {
            get => Value.GetBit(27);
            set => Value = Value.UpdateBit(27, value);
        }

        /// <summary>Gets or sets the object position.</summary>
        /// <remarks>This is stored as a 3-bit value (range 0 - 7). 000b is Reserved and shall not be used.</remarks>
        public byte ObjectPosition
        {
            get => (byte)(Value >> 28 & ObjectPositionMask);
            set
            {
                value.CheckArgumentInRange(7);
                Value = (Value & ObjectPositionMask) | (uint)((value & ObjectPositionMask) << 28);
            }
        }

        /// <summary>Initializes a new instance of the <see cref="RequestDataObject"/> class.</summary>
        /// <param name="rawValue">The raw value received from the USB-PD controller which encodes all properties of this RDO. See USB-PD specification for details.</param>
        public RequestDataObject(uint rawValue) => Value = rawValue;
    }
}
