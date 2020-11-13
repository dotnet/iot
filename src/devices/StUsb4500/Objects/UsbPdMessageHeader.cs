// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Iot.Device.Usb.Enumerations;
using Iot.Device.Usb.Helper;

namespace Iot.Device.Usb.Objects
{
    /// <summary>
    /// Represents the header of a USB PD message.
    /// </summary>
    internal class UsbPdMessageHeader
    {
        private const uint MessageTypeMask = 0b0001_1111;
        private const uint SpecificationRevisionMask = 0b1100_0000;
        private const uint MessageIdMask = 0b1110_0000_0000;
        private const uint NumberOfDataObjectsMask = 0b0111_0000_0000_0000;

        /// <summary>Gets the value which encodes all properties of this message header.</summary>
        public uint Value { get; private set; }

        /// <summary>Gets or sets the type of the message, if this is a control message (NumberOfDataObjects = 0).</summary>
        /// <remarks>This is stored as a 5-bit value (range 0 - 31).</remarks>
        public UsbPdControlMessageType ControlMessageType
        {
            get => ((UsbPdControlMessageType)(Value & MessageTypeMask));
            set
            {
                byte type = (byte)value;
                type.CheckArgumentInRange(31);
                Value = (Value & ~MessageTypeMask) | (type & MessageTypeMask);
            }
        }

        /// <summary>Gets or sets the type of the message, if this is a data message (NumberOfDataObjects > 0).</summary>
        /// <remarks>This is stored as a 5-bit value (range 0 - 31).</remarks>
        public UsbPdDataMessageType DataMessageType
        {
            get => ((UsbPdDataMessageType)(Value & MessageTypeMask));
            set
            {
                byte type = (byte)value;
                type.CheckArgumentInRange(31);
                Value = (Value & ~MessageTypeMask) | (type & MessageTypeMask);
            }
        }

        /// <summary>Gets or sets a value indicating whether the port is in a data role.</summary>
        public bool PortDataRole
        {
            get => Value.GetBit(5);
            set => Value = Value.UpdateBit(5, value);
        }

        /// <summary>Gets or sets the specification revision.</summary>
        /// <remarks>This is stored as a 2-bit value (range 0 - 3).</remarks>
        public Version SpecificationRevision
        {
            get => new Version((int)(Value & SpecificationRevisionMask) >> 6, 0);
            set
            {
                value.Major.CheckArgumentInRange(3);
                Value = (Value & ~SpecificationRevisionMask) | (uint)(value.Major << 6 & SpecificationRevisionMask);
            }
        }

        /// <summary>Gets or sets a value indicating whether the port is in power role / cable plugged.</summary>
        public bool PortPowerRoleCablePlug
        {
            get => Value.GetBit(8);
            set => Value = Value.UpdateBit(8, value);
        }

        /// <summary>Gets or sets the Message Id.</summary>
        /// <remarks>This is stored as a 3-bit value (range 0 - 7).</remarks>
        public byte MessageId
        {
            get => (byte)((Value & MessageIdMask) >> 9);
            set
            {
                value.CheckArgumentInRange(7);
                Value = (Value & ~MessageIdMask) | (uint)(value << 9 & MessageIdMask);
            }
        }

        /// <summary>Gets or sets the number of data objects.</summary>
        /// <remarks>This is stored as a 3-bit value (range 0 - 7).</remarks>
        public byte NumberOfDataObjects
        {
            get => (byte)((Value & NumberOfDataObjectsMask) >> 12);
            set
            {
                value.CheckArgumentInRange(7);
                Value = (Value & ~NumberOfDataObjectsMask) | (uint)(value << 12 & NumberOfDataObjectsMask);
            }
        }

        /// <summary>Initializes a new instance of the <see cref="UsbPdMessageHeader"/> class.</summary>
        /// <param name="value">The value.</param>
        public UsbPdMessageHeader(ushort value) => Value = value;
    }
}
