// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.InteropServices;

namespace Iot.Device.FtCommon
{
    /// <summary>
    /// Represents the native FT_EEPROM_X_SERIES layout, including its separately aligned common header.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    internal struct FtXSeriesEeprom
    {
        /// <summary>
        /// The device-independent EEPROM configuration fields.
        /// </summary>
        public FtEepromHeader Common;

        /// <summary>
        /// Nonzero to enable slow slew rates on AC bus pins.
        /// </summary>
        public byte AcSlowSlew;

        /// <summary>
        /// Nonzero to enable Schmitt inputs on AC bus pins.
        /// </summary>
        public byte AcSchmittInput;

        /// <summary>
        /// The AC bus drive current in milliamperes.
        /// </summary>
        public byte AcDriveCurrent;

        /// <summary>
        /// Nonzero to enable slow slew rates on AD bus pins.
        /// </summary>
        public byte AdSlowSlew;

        /// <summary>
        /// Nonzero to enable Schmitt inputs on AD bus pins.
        /// </summary>
        public byte AdSchmittInput;

        /// <summary>
        /// The AD bus drive current in milliamperes.
        /// </summary>
        public byte AdDriveCurrent;

        /// <summary>
        /// The native function selector for CBUS0.
        /// </summary>
        public byte Cbus0;

        /// <summary>
        /// The native function selector for CBUS1.
        /// </summary>
        public byte Cbus1;

        /// <summary>
        /// The native function selector for CBUS2.
        /// </summary>
        public byte Cbus2;

        /// <summary>
        /// The native function selector for CBUS3.
        /// </summary>
        public byte Cbus3;

        /// <summary>
        /// The native function selector for CBUS4.
        /// </summary>
        public byte Cbus4;

        /// <summary>
        /// The native function selector for CBUS5.
        /// </summary>
        public byte Cbus5;

        /// <summary>
        /// The native function selector for CBUS6.
        /// </summary>
        public byte Cbus6;

        /// <summary>
        /// Nonzero to invert the UART TXD signal on applicable FT-X devices.
        /// </summary>
        public byte InvertTxd;

        /// <summary>
        /// Nonzero to invert the UART RXD signal on applicable FT-X devices.
        /// </summary>
        public byte InvertRxd;

        /// <summary>
        /// Nonzero to invert the UART RTS signal on applicable FT-X devices.
        /// </summary>
        public byte InvertRts;

        /// <summary>
        /// Nonzero to invert the UART CTS signal on applicable FT-X devices.
        /// </summary>
        public byte InvertCts;

        /// <summary>
        /// Nonzero to invert the UART DTR signal on applicable FT-X devices.
        /// </summary>
        public byte InvertDtr;

        /// <summary>
        /// Nonzero to invert the UART DSR signal on applicable FT-X devices.
        /// </summary>
        public byte InvertDsr;

        /// <summary>
        /// Nonzero to invert the UART DCD signal on applicable FT-X devices.
        /// </summary>
        public byte InvertDcd;

        /// <summary>
        /// Nonzero to invert the UART RI signal on applicable FT-X devices.
        /// </summary>
        public byte InvertRi;

        /// <summary>
        /// Nonzero to enable battery charger detection.
        /// </summary>
        public byte BcdEnable;

        /// <summary>
        /// Nonzero to assert the CBUS power-enable signal when a charging port is detected.
        /// </summary>
        public byte BcdForceCbusPwren;

        /// <summary>
        /// Nonzero to disable sleep for battery charging.
        /// </summary>
        public byte BcdDisableSleep;

        /// <summary>
        /// The stored, unshifted I2C slave address.
        /// </summary>
        public ushort I2cSlaveAddress;

        /// <summary>
        /// The stored I2C device identifier, distinct from the slave address.
        /// </summary>
        public uint I2cDeviceId;

        /// <summary>
        /// Nonzero to disable Schmitt triggering on the I2C pins.
        /// </summary>
        public byte I2cDisableSchmitt;

        /// <summary>
        /// The FT1248 clock polarity on applicable FT-X devices.
        /// </summary>
        public byte Ft1248Cpol;

        /// <summary>
        /// Nonzero to transfer FT1248 data least-significant bit first on applicable devices.
        /// </summary>
        public byte Ft1248Lsb;

        /// <summary>
        /// Nonzero to enable FT1248 flow control on applicable devices.
        /// </summary>
        public byte Ft1248FlowControl;

        /// <summary>
        /// Nonzero to suppress RS485 echo on applicable FT-X devices.
        /// </summary>
        public byte Rs485EchoSuppress;

        /// <summary>
        /// The native power-saving configuration flag.
        /// </summary>
        public byte PowerSaveEnable;

        /// <summary>
        /// The native driver selection value for D2XX or virtual COM port operation.
        /// </summary>
        public byte DriverType;

    }
}
