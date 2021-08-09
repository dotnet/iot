// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Device.I2c;
using System.Device.Spi;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Iot.Device.FtCommon;

namespace Iot.Device.Ft232H
{
    /// <summary>
    /// FT232H Device
    /// </summary>
    public class Ft232HDevice : FtDevice, IDisposable
    {
        /// <summary>
        /// Number of pins
        /// </summary>
        internal const int PinCountConst = 16;

        // Commands are available in:
        // - AN_108_Command_Processor_for_MPSSE_and_MCU_Host_Bus_Emulation_Modes.pdf
        // - AN_113_FTDI_Hi_Speed_USB_To_I2C_Example.pdf
        // To understand the signal need for I2C communication: https://training.ti.com/sites/default/files/docs/slides-i2c-protocol.pdf
        private const uint I2cMasterFrequencyKbps = 400;
        private const byte I2cDirSDAoutSCLout = 0x03;
        private const byte I2cDataSDAloSCLhi = 0x01;
        private const byte I2cDataSDAhiSCLhi = 0x03;
        private const byte I2cDataSDAloSCLlo = 0x00;
        private const byte I2cDataSDAhiSCLlo = 0x02;
        private const byte NumberCycles = 5;

        private SafeFtHandle _ftHandle = null!;

        // This is used by FT232H and others to track the GPIO states
        internal byte _gpioLowData = 0;
        internal byte _gpioLowDir = 0;
        internal byte _gpioHighData = 0;
        internal byte _gpioHighDir = 0;

        internal bool[] _PinOpen = new bool[PinCountConst];
        internal PinMode[] _gpioDirections = new PinMode[PinCountConst];

        internal List<SpiConnectionSettings> _connectionSettings = new List<SpiConnectionSettings>();

        /// <summary>
        /// Instantiates a FT232H device object.
        /// </summary>
        /// <param name="flags">Indicates device state.</param>
        /// <param name="type">Indicates the device type.</param>
        /// <param name="id">The Vendor ID and Product ID of the device.</param>
        /// <param name="locId">The physical location identifier of the device.</param>
        /// <param name="serialNumber">The device serial number.</param>
        /// <param name="description">The device description.</param>
        public Ft232HDevice(FtFlag flags, FtDeviceType type, uint id, uint locId, string serialNumber, string description)
        : base(flags, type, id, locId, serialNumber, description)
        {
        }

        /// <summary>
        /// Instantiates a FT232H device object.
        /// </summary>
        /// <param name="ftDevice">a FT Device</param>
        public Ft232HDevice(FtDevice ftDevice)
            : base(ftDevice.Flags, ftDevice.Type, ftDevice.Id, ftDevice.LocId, ftDevice.SerialNumber, ftDevice.Description)
        {
        }

        /// <summary>
        /// Creates I2C bus related to this device
        /// </summary>
        /// <returns>I2cBus instance</returns>
        /// <remarks>You can create either an I2C, either an SPI device.</remarks>
        public I2cBus CreateI2cBus() => new Ft232HI2cBus(this);

        /// <summary>
        /// Creates SPI device related to this device
        /// </summary>
        /// <param name="settings">The SPI settings</param>
        /// <returns>a SPI device</returns>
        /// <remarks>You can create either an I2C, either an SPI device.
        /// You can create multiple SPI devices, the first one will be the one used for the clock frequency.
        /// They all have to have different Chip Select. You can use any of the 3 to 15 pin for this function.</remarks>
        public SpiDevice CreateSpiDevice(SpiConnectionSettings settings) => new Ft232HSpi(settings, this);

        /// <summary>
        /// Creates GPIO driver related to this device
        /// </summary>
        /// <returns>A GPIO Driver</returns>
        public GpioDriver CreateGpioDriver() => new Ft232HGpio(this);

        /// <summary>
        /// Gets the pin number from a string
        /// </summary>
        /// <param name="pin">A string</param>
        /// <returns>The pin number, -1 in case it's not found.</returns>
        /// <remarks>Valid pins are ADBUS0 to 7, D0 to 7, ACBUS0 to 7, C0 to 7,
        /// TCK, SK, CLK, MOSI, MISO, SDA, TDI, DI, TDO, DO,
        /// TMS, CS, GPIOL0 to 3, GPIOH0 to 7</remarks>
        public static int GetPinNumberFromString(string pin)
        {
            pin = pin.ToUpper();
            switch (pin)
            {
                case "ADBUS0":
                case "D0":
                case "TCK":
                case "SK":
                case "CLK":
                case "SDL":
                    return 0;
                case "ADBUS1":
                case "D1":
                case "DO":
                case "TDI":
                case "SDA":
                case "MOSI":
                    return 1;
                case "ADBUS2":
                case "D2":
                case "DI":
                case "TDO":
                case "MISO":
                    return 2;
                case "ADBUS3":
                case "D3":
                case "TMS":
                case "CS":
                    return 3;
                case "ADBUS4":
                case "D4":
                case "GPIOL0":
                    return 4;
                case "ADBUS5":
                case "D5":
                case "GPIOL1":
                    return 5;
                case "ADBUS6":
                case "D6":
                case "GPIOL2":
                    return 6;
                case "ADBUS7":
                case "D7":
                case "GPIOL3":
                    return 7;
                case "ACBUS0":
                case "C0":
                case "GPIOH0":
                    return 8;
                case "ACBUS1":
                case "C1":
                case "GPIOH1":
                    return 9;
                case "ACBUS2":
                case "C2":
                case "GPIOH2":
                    return 10;
                case "ACBUS3":
                case "C3":
                case "GPIOH3":
                    return 11;
                case "ACBUS4":
                case "C4":
                case "GPIOH4":
                    return 12;
                case "ACBUS5":
                case "C5":
                case "GPIOH5":
                    return 13;
                case "ACBUS6":
                case "C6":
                case "GPIOH6":
                    return 14;
                case "ACBUS7":
                case "C7":
                case "GPIOH7":
                    return 15;
                default:
                    return -1;
            }
        }

        internal bool IsI2cMode { get; set; }

        internal bool IsSpiMode { get; set; }

        internal void GetHandle()
        {
            if ((_ftHandle == null) || _ftHandle.IsClosed)
            {
                // Open device if not opened
                var ftStatus = FtFunction.FT_OpenEx(LocId, FtOpenType.OpenByLocation, out _ftHandle);

                if (ftStatus != FtStatus.Ok)
                {
                    throw new IOException($"Failed to open device {Description}, status: {ftStatus}");
                }
            }
        }

        #region I2C

        internal void I2cInitialize()
        {
            GetHandle();
            // check is any of the first 3 GPIO are open
            if (_PinOpen[0] || _PinOpen[1] || _PinOpen[2])
            {
                throw new IOException("Can't open I2C if GPIO 0, 1 or 2 are open");
            }

            if (IsSpiMode)
            {
                throw new IOException("Can't open I2C if SPI mode is used");
            }

            var ftStatus = FtFunction.FT_SetTimeouts(_ftHandle, 5000, 5000);
            ftStatus = FtFunction.FT_SetLatencyTimer(_ftHandle, 16);
            ftStatus = FtFunction.FT_SetFlowControl(_ftHandle, (ushort)FtFlowControl.FT_FLOW_RTS_CTS, 0x00, 0x00);
            ftStatus = FtFunction.FT_SetBitMode(_ftHandle, 0x00, 0x00);
            ftStatus = FtFunction.FT_SetBitMode(_ftHandle, 0x00, 0x02);

            if (ftStatus != FtStatus.Ok)
            {
                throw new IOException($"Failed to setup device {Description}, status: {ftStatus} in MPSSE mode");
            }

            IsI2cMode = true;
            FlushBuffer();
            SetupMpsseMode();

            // Now setup the clock and other elements
            Span<byte> toSend = stackalloc byte[13];
            int idx = 0;
            // Disable clock divide by 5 for 60Mhz master clock
            toSend[idx++] = (byte)FtOpcode.DisableClockDivideBy5;
            // Turn off adaptive clocking
            toSend[idx++] = (byte)FtOpcode.TurnOffAdaptativeClocking;
            // Enable 3 phase data clock, used by I2C to allow data on both clock edges
            toSend[idx++] = (byte)FtOpcode.Enable3PhaseDataClocking;
            // The SK clock frequency can be worked out by below algorithm with divide by 5 set as off
            // TCK period = 60MHz / (( 1 + [ (0xValueH * 256) OR 0xValueL] ) * 2)
            // Command to set clock divisor
            toSend[idx++] = (byte)FtOpcode.SetClockDivisor;
            uint clockDivisor = 60000 / (I2cMasterFrequencyKbps * 2) - 1;
            toSend[idx++] = (byte)(clockDivisor & 0x00FF);
            toSend[idx++] = (byte)((clockDivisor >> 8) & 0x00FF);
            // loopback off
            toSend[idx++] = (byte)FtOpcode.DisconnectTDItoTDOforLoopback;
            // Enable the FT232H's drive-zero mode with the following enable mask
            toSend[idx++] = (byte)FtOpcode.SetIOOnlyDriveOn0AndTristateOn1;
            // Low byte (ADx) enables - bits 0, 1 and 2
            toSend[idx++] = 0x07;
            // High byte (ACx) enables - all off
            toSend[idx++] = 0x00;
            // Command to set directions of lower 8 pins and force value on bits set as output
            toSend[idx++] = (byte)FtOpcode.SetDataBitsLowByte;
            // SDA and SCL both output high (open drain)
            _gpioLowData = (byte)(I2cDataSDAhiSCLhi | (_gpioLowData & 0xF8));
            _gpioLowDir = (byte)(I2cDirSDAoutSCLout | (_gpioLowDir & 0xF8));
            toSend[idx++] = _gpioLowData;
            toSend[idx++] = _gpioLowDir;
            Write(toSend);
        }

        private void SetupMpsseMode()
        {
            // Seems that we have to send a wrong command to get the MPSSE moed working
            // First with 0xAA
            Span<byte> toSend = stackalloc byte[1];
            toSend[0] = 0xAA;
            Write(toSend);
            Span<byte> toRead = stackalloc byte[2];
            Read(toRead);
            if (!((toRead[0] == 0xFA) && (toRead[1] == 0xAA)))
            {
                throw new IOException($"Failed to setup device {Description} in MPSSE mode using magic 0xAA sync");
            }

            // Second with 0xAB
            toSend[0] = 0xAB;
            Write(toSend);
            Read(toRead);
            if (!((toRead[0] == 0xFA) && (toRead[1] == 0xAB)))
            {
                throw new IOException($"Failed to setup device {Description}, status in MPSSE mode using magic 0xAB sync");
            }
        }

        internal void I2cStart()
        {
            int count;
            int idx = 0;
            // SDA high, SCL high
            _gpioLowData = (byte)(I2cDataSDAhiSCLhi | (_gpioLowData & 0xF8));
            _gpioLowDir = (byte)(I2cDirSDAoutSCLout | (_gpioLowDir & 0xF8));
            Span<byte> toSend = stackalloc byte[NumberCycles * 3 * 3 + 3];
            for (count = 0; count < NumberCycles; count++)
            {
                toSend[idx++] = (byte)FtOpcode.SetDataBitsLowByte;
                toSend[idx++] = _gpioLowData;
                toSend[idx++] = _gpioLowDir;
            }

            // SDA lo, SCL high
            _gpioLowData = (byte)(0x00 | I2cDataSDAloSCLhi | (_gpioLowData & 0xF8));
            for (count = 0; count < NumberCycles; count++)
            {
                toSend[idx++] = (byte)FtOpcode.SetDataBitsLowByte;
                toSend[idx++] = _gpioLowData;
                toSend[idx++] = _gpioLowDir;
            }

            // SDA lo, SCL lo
            _gpioLowData = (byte)(0x00 | I2cDataSDAloSCLlo | (_gpioLowData & 0xF8));
            for (count = 0; count < NumberCycles; count++)
            {
                toSend[idx++] = (byte)FtOpcode.SetDataBitsLowByte;
                toSend[idx++] = _gpioLowData;
                toSend[idx++] = _gpioLowDir;
            }

            // Release SDA
            _gpioLowData = (byte)(0x00 | I2cDataSDAhiSCLlo | (_gpioLowData & 0xF8));
            toSend[idx++] = (byte)FtOpcode.SetDataBitsLowByte;
            toSend[idx++] = _gpioLowData;
            toSend[idx++] = _gpioLowDir;

            Write(toSend);
        }

        internal void I2cStop()
        {
            int count;
            int idx = 0;
            // SDA low, SCL low
            _gpioLowData = (byte)(I2cDataSDAloSCLlo | (_gpioLowData & 0xF8));
            _gpioLowDir = (byte)(I2cDirSDAoutSCLout | (_gpioLowDir & 0xF8));
            Span<byte> toSend = stackalloc byte[NumberCycles * 3 * 3];
            for (count = 0; count < NumberCycles; count++)
            {
                toSend[idx++] = (byte)FtOpcode.SetDataBitsLowByte;
                toSend[idx++] = _gpioLowData;
                toSend[idx++] = _gpioLowDir;
            }

            // SDA low, SCL high
            _gpioLowData = (byte)(I2cDataSDAloSCLhi | (_gpioLowData & 0xF8));
            _gpioLowDir = (byte)(I2cDirSDAoutSCLout | (_gpioLowDir & 0xF8));
            for (count = 0; count < NumberCycles; count++)
            {
                toSend[idx++] = (byte)FtOpcode.SetDataBitsLowByte;
                toSend[idx++] = _gpioLowData;
                toSend[idx++] = _gpioLowDir;
            }

            // SDA high, SCL high
            _gpioLowData = (byte)(I2cDataSDAhiSCLhi | (_gpioLowData & 0xF8));
            _gpioLowDir = (byte)(I2cDirSDAoutSCLout | (_gpioLowDir & 0xF8));
            for (count = 0; count < NumberCycles; count++)
            {
                toSend[idx++] = (byte)FtOpcode.SetDataBitsLowByte;
                toSend[idx++] = _gpioLowData;
                toSend[idx++] = _gpioLowDir;
            }

            Write(toSend);
        }

        internal void I2cLineIdle()
        {
            int idx = 0;
            // SDA low, SCL low
            _gpioLowData = (byte)(I2cDataSDAhiSCLhi | (_gpioLowData & 0xF8));
            _gpioLowDir = (byte)(I2cDirSDAoutSCLout | (_gpioLowDir & 0xF8));
            Span<byte> toSend = stackalloc byte[3];
            toSend[idx++] = (byte)FtOpcode.SetDataBitsLowByte;
            toSend[idx++] = _gpioLowData;
            toSend[idx++] = _gpioLowDir;
            Write(toSend);
            IsI2cMode = false;
        }

        internal bool I2cSendByteAndCheckACK(byte data)
        {
            int idx = 0;
            Span<byte> toSend = stackalloc byte[10];
            Span<byte> toRead = stackalloc byte[1];
            // Just clock with one byte (0 = 1 byte)
            toSend[idx++] = (byte)FtOpcode.ClockDataBytesOutOnMinusVeClockMSBFirst;
            toSend[idx++] = 0;
            toSend[idx++] = 0;
            toSend[idx++] = data;
            // Put line back to idle (data released, clock pulled low)
            _gpioLowData = (byte)(I2cDataSDAhiSCLlo | (_gpioLowData & 0xF8));
            _gpioLowDir = (byte)(I2cDirSDAoutSCLout | (_gpioLowDir & 0xF8));
            toSend[idx++] = (byte)FtOpcode.SetDataBitsLowByte;
            toSend[idx++] = _gpioLowData;
            toSend[idx++] = _gpioLowDir;
            // Clock in (0 = 1 byte)
            toSend[idx++] = (byte)FtOpcode.ClockDataBitsInOnPlusVeClockMSBFirst;
            toSend[idx++] = 0;
            // And ask it right away
            toSend[idx++] = (byte)FtOpcode.SendImmediate;
            Write(toSend);
            Read(toRead);
            // Bit 0 equivalent to acknoledge, otherwise nack
            return (toRead[0] & 0x01) == 0;
        }

        internal bool I2cSendDeviceAddrAndCheckACK(byte Address, bool Read)
        {
            // Set address for read or write
            Address <<= 1;
            if (Read == true)
            {
                Address |= 0x01;
            }

            return I2cSendByteAndCheckACK(Address);
        }

        internal byte I2CReadByte(bool ack)
        {
            int idx = 0;
            Span<byte> toSend = stackalloc byte[10];
            Span<byte> toRead = stackalloc byte[1];
            // Read one byte
            toSend[idx++] = (byte)FtOpcode.ClockDataBytesInOnPlusVeClockMSBFirst;
            toSend[idx++] = 0;
            toSend[idx++] = 0;
            // Send out either ack either nak
            toSend[idx++] = (byte)FtOpcode.ClockDataBitsOutOnMinusVeClockMSBFirst;
            toSend[idx++] = 0;
            toSend[idx++] = (byte)(ack ? 0x00 : 0xFF);
            // I2C lines back to idle state
            toSend[idx++] = (byte)FtOpcode.SetDataBitsLowByte;
            _gpioLowDir = (byte)(I2cDirSDAoutSCLout | (_gpioLowDir & 0xF8));
            _gpioLowData = (byte)(I2cDataSDAhiSCLlo | (_gpioLowData & 0xF8));
            toSend[idx++] = _gpioLowDir;
            toSend[idx++] = _gpioLowData;
            // And ask it right away
            toSend[idx++] = (byte)FtOpcode.SendImmediate;
            Write(toSend);
            Read(toRead);
            return toRead[0];
        }

        #endregion

        #region gpio

        internal byte GetGpioValuesLow()
        {
            Span<byte> toSend = stackalloc byte[2];
            Span<byte> toRead = stackalloc byte[1];
            toSend[0] = (byte)FtOpcode.ReadDataBitsLowByte;
            toSend[1] = (byte)FtOpcode.SendImmediate;
            Write(toSend);
            Read(toRead);
            return (byte)(toRead[0] & 0xF8);
        }

        internal void SetGpioValuesLow()
        {
            Span<byte> toSend = stackalloc byte[3];
            toSend[0] = (byte)FtOpcode.SetDataBitsLowByte;
            toSend[1] = _gpioLowData;
            toSend[2] = _gpioLowDir;
            Write(toSend);
        }

        internal byte GetGpioValuesHigh()
        {
            Span<byte> toSend = stackalloc byte[2];
            Span<byte> toRead = stackalloc byte[1];
            toSend[0] = (byte)FtOpcode.ReadDataBitsHighByte;
            toSend[1] = (byte)FtOpcode.SendImmediate;
            Write(toSend);
            Read(toRead);
            return toRead[0];
        }

        internal void SetGpioValuesHigh()
        {
            Span<byte> toSend = stackalloc byte[3];
            toSend[0] = (byte)FtOpcode.SetDataBitsHighByte;
            toSend[1] = _gpioHighData;
            toSend[2] = _gpioHighDir;
            Write(toSend);
        }

        #endregion

        #region SPI

        internal void SpiInitialize()
        {
            // Do we already have SPI setup?
            if (IsSpiMode)
            {
                // No need to initialize everything
                return;
            }

            GetHandle();
            IsSpiMode = true;
            var ftStatus = FtFunction.FT_SetLatencyTimer(_ftHandle, 1);
            ftStatus = FtFunction.FT_SetUSBParameters(_ftHandle, 65535, 65535);
            ftStatus = FtFunction.FT_SetChars(_ftHandle, 0, 0, 0, 0);
            ftStatus = FtFunction.FT_SetTimeouts(_ftHandle, 3000, 3000);
            ftStatus = FtFunction.FT_SetLatencyTimer(_ftHandle, 1);
            // Reset
            ftStatus = FtFunction.FT_SetBitMode(_ftHandle, 0x00, 0x00);
            // Enable MPSSE mode
            ftStatus = FtFunction.FT_SetBitMode(_ftHandle, 0x00, 0x02);
            if (ftStatus != FtStatus.Ok)
            {
                throw new IOException($"Failed to setup device {Description}, status: {ftStatus} in MPSSE mode");
            }

            // 50 ms according to thr doc for all USB to complete
            Thread.Sleep(50);
            FlushBuffer();
            SetupMpsseMode();

            int idx = 0;
            Span<byte> toSend = stackalloc byte[10];
            toSend[idx++] = (byte)FtOpcode.DisableClockDivideBy5;
            toSend[idx++] = (byte)FtOpcode.TurnOffAdaptativeClocking;
            toSend[idx++] = (byte)FtOpcode.Disable3PhaseDataClocking;
            toSend[idx++] = (byte)FtOpcode.SetDataBitsLowByte;
            // Pin clock output, MISO output, MOSI input
            _gpioLowDir = (byte)((_gpioLowDir & 0xF8) | 0x03);
            // clock, MOSI and MISO to 0
            _gpioLowData = (byte)(_gpioLowData & 0xF8);
            toSend[idx++] = _gpioLowDir;
            toSend[idx++] = _gpioLowData;
            // The SK clock frequency can be worked out by below algorithm with divide by 5 set as off
            // TCK period = 60MHz / (( 1 + [ (0xValueH * 256) OR 0xValueL] ) * 2)
            // Command to set clock divisor
            toSend[idx++] = (byte)FtOpcode.SetClockDivisor;
            uint clockDivisor = (uint)(60000 / ((_connectionSettings[0].ClockFrequency / 1000) * 2) - 1);
            toSend[idx++] = (byte)(clockDivisor & 0xFF);
            toSend[idx++] = (byte)(clockDivisor >> 8);
            // loopback off
            toSend[idx++] = (byte)FtOpcode.DisconnectTDItoTDOforLoopback;
            Write(toSend);
            // Delay as in the documentation
            Thread.Sleep(30);
        }

        internal void SpiDeinitialize()
        {
            if (_connectionSettings.Count == 0)
            {
                IsSpiMode = false;
            }
        }

        internal void SpiWrite(SpiConnectionSettings settings, ReadOnlySpan<byte> buffer)
        {
            if (buffer.Length > 65535)
            {
                throw new ArgumentException("Buffer too large, maximum size if 65535");
            }

            byte clock;
            switch (settings.Mode)
            {
                default:
                case SpiMode.Mode3:
                case SpiMode.Mode0:
                    if (settings.DataFlow == DataFlow.MsbFirst)
                    {
                        clock = (byte)FtOpcode.ClockDataBytesOutOnMinusVeClockMSBFirst;
                    }
                    else
                    {
                        clock = (byte)FtOpcode.ClockDataBytesOutOnMinusVeClockLSBFirst;
                    }

                    break;

                case SpiMode.Mode2:
                case SpiMode.Mode1:
                    if (settings.DataFlow == DataFlow.MsbFirst)
                    {
                        clock = (byte)FtOpcode.ClockDataBytesOutOnPlusVeClockMSBFirst;
                    }
                    else
                    {
                        clock = (byte)FtOpcode.ClockDataBytesOutOnPlusVeClockLSBFirst;
                    }

                    break;
            }

            SpiChipSelectEnable((byte)settings.ChipSelectLine, true);
            int idx = 0;
            Span<byte> toSend = stackalloc byte[3 + buffer.Length];
            toSend[idx++] = clock;
            toSend[idx++] = (byte)((buffer.Length - 1) & 0xFF);
            toSend[idx++] = (byte)((buffer.Length - 1) >> 8);
            buffer.CopyTo(toSend.Slice(3));
            Write(toSend);
            SpiChipSelectEnable((byte)settings.ChipSelectLine, false);
        }

        internal void SpiRead(SpiConnectionSettings settings, Span<byte> buffer)
        {
            if (buffer.Length > 65535)
            {
                throw new ArgumentException("Buffer too large, maximum size if 65535");
            }

            byte clock;
            switch (settings.Mode)
            {
                default:
                case SpiMode.Mode3:
                case SpiMode.Mode0:
                    if (settings.DataFlow == DataFlow.MsbFirst)
                    {
                        clock = (byte)FtOpcode.ClockDataBytesInOnPlusVeClockMSBFirst;
                    }
                    else
                    {
                        clock = (byte)FtOpcode.ClockDataBytesInOnPlusVeClockLSBFirst;
                    }

                    break;
                case SpiMode.Mode2:
                case SpiMode.Mode1:
                    if (settings.DataFlow == DataFlow.MsbFirst)
                    {
                        clock = (byte)FtOpcode.ClockDataBytesInOnMinusVeClockMSBFirst;
                    }
                    else
                    {
                        clock = (byte)FtOpcode.ClockDataBytesInOnMinusVeClockLSBFirst;
                    }

                    break;
            }

            SpiChipSelectEnable((byte)settings.ChipSelectLine, true);
            int idx = 0;
            Span<byte> toSend = stackalloc byte[3];
            toSend[idx++] = clock;
            toSend[idx++] = (byte)((buffer.Length - 1) & 0xFF);
            toSend[idx++] = (byte)((buffer.Length - 1) >> 8);
            Write(toSend);
            Read(buffer);
            SpiChipSelectEnable((byte)settings.ChipSelectLine, false);
        }

        internal void SpiWriteRead(SpiConnectionSettings settings, ReadOnlySpan<byte> bufferWrite, Span<byte> bufferRead)
        {
            if ((bufferRead.Length > 65535) || (bufferWrite.Length > 65535))
            {
                throw new ArgumentException("Buffer too large, maximum size if 65535");
            }

            byte clock;
            switch (settings.Mode)
            {
                default:
                case SpiMode.Mode3:
                case SpiMode.Mode0:
                    if (settings.DataFlow == DataFlow.MsbFirst)
                    {
                        clock = (byte)FtOpcode.ClockDataBytesOutOnMinusBytesInOnPlusVeClockMSBFirst;
                    }
                    else
                    {
                        clock = (byte)FtOpcode.ClockDataBytesOutOnMinusBytesInOnPlusVeClockLSBFirst;
                    }

                    break;
                case SpiMode.Mode2:
                case SpiMode.Mode1:
                    if (settings.DataFlow == DataFlow.MsbFirst)
                    {
                        clock = (byte)FtOpcode.ClockDataBytesOutOnPlusBytesInOnMinusVeClockMSBFirst;
                    }
                    else
                    {
                        clock = (byte)FtOpcode.ClockDataBytesOutOnPlusBytesInOnMinusVeClockLSBFirst;
                    }

                    break;
            }

            SpiChipSelectEnable((byte)settings.ChipSelectLine, true);
            int idx = 0;
            Span<byte> toSend = stackalloc byte[3 + bufferWrite.Length];
            toSend[idx++] = clock;
            toSend[idx++] = (byte)((bufferWrite.Length - 1) & 0xFF);
            toSend[idx++] = (byte)((bufferWrite.Length - 1) >> 8);
            bufferWrite.CopyTo(toSend.Slice(3));
            Write(toSend);
            Read(bufferRead);
            SpiChipSelectEnable((byte)settings.ChipSelectLine, false);
        }

        internal void SpiChipSelectEnable(byte chipSelect, bool enable)
        {
            if (chipSelect < 0)
            {
                return;
            }

            var conn = _connectionSettings.Find(m => m.ChipSelectLine == chipSelect);
            if (conn == null)
            {
                throw new ArgumentException($"Can't find specific chip select {chipSelect}");
            }

            var value = conn.ChipSelectLineActiveState;
            // In case of deselect, we just invert what's needed
            if (!enable)
            {
                value = value == PinValue.High ? PinValue.Low : PinValue.High;
            }

            Span<byte> toSend = stackalloc byte[NumberCycles * 3];
            int idx = 0;
            if (chipSelect < 8)
            {
                _gpioLowDir |= (byte)(1 << chipSelect);
                if (value == PinValue.High)
                {
                    _gpioLowData |= (byte)(1 << chipSelect);
                }
                else
                {
                    byte mask = 0xFF;
                    mask &= (byte)(~(1 << chipSelect));
                    _gpioLowData &= mask;
                }

                for (int i = 0; i < NumberCycles; i++)
                {
                    toSend[idx++] = (byte)FtOpcode.SetDataBitsLowByte;
                    toSend[idx++] = _gpioLowData;
                    toSend[idx++] = _gpioLowDir;
                }
            }
            else
            {
                _gpioHighDir |= (byte)(1 << (chipSelect - 8));
                if (value == PinValue.High)
                {
                    _gpioHighData |= (byte)(1 << (chipSelect - 8));
                }
                else
                {
                    byte mask = 0xFF;
                    mask &= (byte)(~(1 << (chipSelect - 8)));
                    _gpioHighData &= mask;
                }

                for (int i = 0; i < NumberCycles; i++)
                {
                    toSend[idx++] = (byte)FtOpcode.SetDataBitsHighByte;
                    toSend[idx++] = _gpioHighData;
                    toSend[idx++] = _gpioHighDir;
                }
            }

            Write(toSend);
        }

        #endregion

        #region Read Write

        internal void Write(ReadOnlySpan<byte> buffer)
        {
            uint numBytesWritten = 0;
            var ftStatus = FtFunction.FT_Write(_ftHandle, in MemoryMarshal.GetReference(buffer), (ushort)buffer.Length, ref numBytesWritten);
            if ((ftStatus != FtStatus.Ok) || (buffer.Length != numBytesWritten))
            {
                throw new IOException($"Can't write to the device");
            }
        }

        internal int Read(Span<byte> buffer)
        {
            CancellationToken token = new CancellationTokenSource(1000).Token;
            int totalBytesRead = 0;
            uint bytesToRead = 0;
            uint numBytesRead = 0;
            FtStatus ftStatus;
            while ((totalBytesRead < buffer.Length) && (!token.IsCancellationRequested))
            {
                bytesToRead = GetAvailableBytes();
                if (bytesToRead > 0)
                {
                    ftStatus = FtFunction.FT_Read(_ftHandle, in buffer[totalBytesRead], bytesToRead, ref numBytesRead);
                    if ((ftStatus != FtStatus.Ok) && (bytesToRead != numBytesRead))
                    {
                        throw new IOException("Can't read device");
                    }

                    totalBytesRead += (int)bytesToRead;
                }
            }

            return totalBytesRead;
        }

        private bool FlushBuffer()
        {
            var availableBytes = GetAvailableBytes();

            if (availableBytes > 0)
            {
                byte[] toRead = new byte[availableBytes];
                uint bytesRead = 0;
                var ftStatus = FtFunction.FT_Read(_ftHandle, in toRead[0], availableBytes, ref bytesRead);
                return ftStatus == FtStatus.Ok;
            }

            return true;
        }

        private uint GetAvailableBytes()
        {
            uint availableBytes = 0;
            var ftStatus = FtFunction.FT_GetQueueStatus(_ftHandle, ref availableBytes);
            if (ftStatus != FtStatus.Ok)
            {
                throw new IOException($"Can't get available bytes");
            }

            return availableBytes;
        }

        #endregion

        /// <summary>
        /// Dispose FT323H
        /// </summary>
        public void Dispose()
        {
            _ftHandle.Dispose();
            _ftHandle = null!;
        }
    }
}
