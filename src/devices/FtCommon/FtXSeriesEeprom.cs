// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.InteropServices;

namespace Iot.Device.FtCommon
{
    /// <summary>
    /// Represents the native FT_EEPROM_X_SERIES layout, including its separately aligned common header.
    /// See the <see href="https://docs.rs/libftd2xx-ffi/latest/libftd2xx_ffi/struct.ft_eeprom_x_series.html">native structure documentation</see>.
    /// </summary>
    /// <remarks>
    /// The native structure is 56 bytes with 4-byte packing. Field offsets are byte offsets
    /// from the start of this structure, not addresses in MTP memory.
    /// The common header occupies offsets 0 through 15; additional padding occupies offsets 42, 43, and 55.
    /// </remarks>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    internal struct FtXSeriesEeprom
    {
        /// <summary>
        /// Offset 0 (0x00): The device-independent EEPROM configuration fields.
        /// </summary>
        public FtEepromHeader Common;

        /// <summary>
        /// Offset 16 (0x10): Nonzero to enable slow slew rates on AC bus pins.
        /// </summary>
        public byte AcSlowSlew;

        /// <summary>
        /// Offset 17 (0x11): Nonzero to enable Schmitt inputs on AC bus pins.
        /// </summary>
        public byte AcSchmittInput;

        /// <summary>
        /// Offset 18 (0x12): The AC bus drive current in milliamperes.
        /// </summary>
        public byte AcDriveCurrent;

        /// <summary>
        /// Offset 19 (0x13): Nonzero to enable slow slew rates on AD bus pins.
        /// </summary>
        public byte AdSlowSlew;

        /// <summary>
        /// Offset 20 (0x14): Nonzero to enable Schmitt inputs on AD bus pins.
        /// </summary>
        public byte AdSchmittInput;

        /// <summary>
        /// Offset 21 (0x15): The AD bus drive current in milliamperes.
        /// </summary>
        public byte AdDriveCurrent;

        /// <summary>
        /// Offset 22 (0x16): The native function selector for CBUS0.
        /// </summary>
        public byte Cbus0;

        /// <summary>
        /// Offset 23 (0x17): The native function selector for CBUS1.
        /// </summary>
        public byte Cbus1;

        /// <summary>
        /// Offset 24 (0x18): The native function selector for CBUS2.
        /// </summary>
        public byte Cbus2;

        /// <summary>
        /// Offset 25 (0x19): The native function selector for CBUS3.
        /// </summary>
        public byte Cbus3;

        /// <summary>
        /// Offset 26 (0x1A): The native function selector for CBUS4.
        /// </summary>
        public byte Cbus4;

        /// <summary>
        /// Offset 27 (0x1B): The native function selector for CBUS5.
        /// </summary>
        public byte Cbus5;

        /// <summary>
        /// Offset 28 (0x1C): The native function selector for CBUS6.
        /// </summary>
        public byte Cbus6;

        /// <summary>
        /// Offset 29 (0x1D): Nonzero to invert the UART TXD signal on applicable FT-X devices.
        /// </summary>
        public byte InvertTxd;

        /// <summary>
        /// Offset 30 (0x1E): Nonzero to invert the UART RXD signal on applicable FT-X devices.
        /// </summary>
        public byte InvertRxd;

        /// <summary>
        /// Offset 31 (0x1F): Nonzero to invert the UART RTS signal on applicable FT-X devices.
        /// </summary>
        public byte InvertRts;

        /// <summary>
        /// Offset 32 (0x20): Nonzero to invert the UART CTS signal on applicable FT-X devices.
        /// </summary>
        public byte InvertCts;

        /// <summary>
        /// Offset 33 (0x21): Nonzero to invert the UART DTR signal on applicable FT-X devices.
        /// </summary>
        public byte InvertDtr;

        /// <summary>
        /// Offset 34 (0x22): Nonzero to invert the UART DSR signal on applicable FT-X devices.
        /// </summary>
        public byte InvertDsr;

        /// <summary>
        /// Offset 35 (0x23): Nonzero to invert the UART DCD signal on applicable FT-X devices.
        /// </summary>
        public byte InvertDcd;

        /// <summary>
        /// Offset 36 (0x24): Nonzero to invert the UART RI signal on applicable FT-X devices.
        /// </summary>
        public byte InvertRi;

        /// <summary>
        /// Offset 37 (0x25): Nonzero to enable battery charger detection.
        /// </summary>
        public byte BcdEnable;

        /// <summary>
        /// Offset 38 (0x26): Nonzero to assert the CBUS power-enable signal when a charging port is detected.
        /// </summary>
        public byte BcdForceCbusPwren;

        /// <summary>
        /// Offset 39 (0x27): Nonzero to disable sleep for battery charging.
        /// </summary>
        public byte BcdDisableSleep;

        /// <summary>
        /// Offset 40 (0x28): The stored, unshifted I2C slave address.
        /// </summary>
        public ushort I2cSlaveAddress;

        /// <summary>
        /// Offset 44 (0x2C): The stored I2C device identifier, distinct from the slave address.
        /// </summary>
        public uint I2cDeviceId;

        /// <summary>
        /// Offset 48 (0x30): Nonzero to disable Schmitt triggering on the I2C pins.
        /// </summary>
        public byte I2cDisableSchmitt;

        /// <summary>
        /// Offset 49 (0x31): The FT1248 clock polarity on applicable FT-X devices.
        /// </summary>
        public byte Ft1248Cpol;

        /// <summary>
        /// Offset 50 (0x32): Nonzero to transfer FT1248 data least-significant bit first on applicable devices.
        /// </summary>
        public byte Ft1248Lsb;

        /// <summary>
        /// Offset 51 (0x33): Nonzero to enable FT1248 flow control on applicable devices.
        /// </summary>
        public byte Ft1248FlowControl;

        /// <summary>
        /// Offset 52 (0x34): Nonzero to suppress RS485 echo on applicable FT-X devices.
        /// </summary>
        public byte Rs485EchoSuppress;

        /// <summary>
        /// Offset 53 (0x35): The native power-saving configuration flag.
        /// </summary>
        public byte PowerSaveEnable;

        /// <summary>
        /// Offset 54 (0x36): The native driver selection value for D2XX or virtual COM port operation.
        /// </summary>
        public byte DriverType;

    }
}
