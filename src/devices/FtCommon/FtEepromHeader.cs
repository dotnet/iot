// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.InteropServices;

namespace Iot.Device.FtCommon
{
    /// <summary>
    /// Represents the native FT_EEPROM_HEADER shared by FTDI EEPROM configuration structures.
    /// See the <see href="https://docs.rs/libftd2xx-ffi/latest/libftd2xx_ffi/struct.ft_eeprom_header.html">native structure documentation</see>.
    /// </summary>
    /// <remarks>
    /// The native structure is 16 bytes with 4-byte packing. Field offsets are byte offsets
    /// from the start of this structure, not addresses in MTP memory.
    /// Padding occupies offsets 9 and 15.
    /// </remarks>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    internal struct FtEepromHeader
    {
        /// <summary>
        /// Offset 0 (0x00): The FTDI chip type identifying the device-specific configuration layout.
        /// </summary>
        public FtDeviceType DeviceType;

        /// <summary>
        /// Offset 4 (0x04): The USB vendor identifier.
        /// </summary>
        public ushort VendorId;

        /// <summary>
        /// Offset 6 (0x06): The USB product identifier.
        /// </summary>
        public ushort ProductId;

        /// <summary>
        /// Offset 8 (0x08): Nonzero to include the serial number in the USB descriptors.
        /// </summary>
        public byte SerialNumberEnabled;

        /// <summary>
        /// Offset 10 (0x0A): The maximum USB current consumption in milliamperes as represented by D2XX.
        /// </summary>
        public ushort MaxPower;

        /// <summary>
        /// Offset 12 (0x0C): Nonzero when the device is self-powered rather than USB bus-powered.
        /// </summary>
        public byte SelfPowered;

        /// <summary>
        /// Offset 13 (0x0D): Nonzero when the USB device supports remote wakeup.
        /// </summary>
        public byte RemoteWakeup;

        /// <summary>
        /// Offset 14 (0x0E): Nonzero to enable I/O pull-downs during USB suspend.
        /// </summary>
        public byte PullDownEnable;
    }
}
