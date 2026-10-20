// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.InteropServices;

namespace Iot.Device.FtCommon
{
    /// <summary>
    /// Represents the native FT_EEPROM_HEADER shared by FTDI EEPROM configuration structures.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    internal struct FtEepromHeader
    {
        /// <summary>
        /// The FTDI chip type identifying the device-specific configuration layout.
        /// </summary>
        public FtDeviceType DeviceType;

        /// <summary>
        /// The USB vendor identifier.
        /// </summary>
        public ushort VendorId;

        /// <summary>
        /// The USB product identifier.
        /// </summary>
        public ushort ProductId;

        /// <summary>
        /// Nonzero to include the serial number in the USB descriptors.
        /// </summary>
        public byte SerialNumberEnabled;

        /// <summary>
        /// The maximum USB current consumption in milliamperes as represented by D2XX.
        /// </summary>
        public ushort MaxPower;

        /// <summary>
        /// Nonzero when the device is self-powered rather than USB bus-powered.
        /// </summary>
        public byte SelfPowered;

        /// <summary>
        /// Nonzero when the USB device supports remote wakeup.
        /// </summary>
        public byte RemoteWakeup;

        /// <summary>
        /// Nonzero to enable I/O pull-downs during USB suspend.
        /// </summary>
        public byte PullDownEnable;
    }
}
