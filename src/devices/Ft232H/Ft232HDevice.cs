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
using Iot.Device.Board;
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
        internal byte GpioLowData = 0;
        internal byte GpioLowDir = 0;
        internal byte GpioHighData = 0;
        internal byte GpioHighDir = 0;

        internal bool[] PinOpen = new bool[PinCountConst];
        internal PinMode[] GpioDirections = new PinMode[PinCountConst];

        internal List<SpiConnectionSettings> ConnectionSettings = new List<SpiConnectionSettings>();

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

        /// <summary>
        /// Gets all the FT232H connected
        /// </summary>
        /// <returns>A list of FT232H</returns>
        public static List<Ft232HDevice> GetFt232H()
        {
            List<Ft232HDevice> ft232s = new List<Ft232HDevice>();
            var devices = FtCommon.FtCommon.GetDevices(new FtDeviceType[] { FtDeviceType.Ft232H });
            foreach (var device in devices)
            {
                ft232s.Add(new Ft232HDevice(device));
            }

            return ft232s;
        }

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

        /// <inheritdoc />
        protected override I2cBusManager CreateI2cBusCore(int busNumber, int[]? pins)
        {
            if (busNumber != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(busNumber));
            }

            return new I2cBusManager(this, busNumber, pins, new Ft232HI2cBus(this));
        }

        /// <summary>
        /// Creates SPI device related to this device
        /// </summary>
        /// <param name="settings">The SPI settings</param>
        /// <param name="pins">The pins to use</param>
        /// <returns>a SPI device</returns>
        /// <remarks>You can create either an I2C, either an SPI device.
        /// You can create multiple SPI devices, the first one will be the one used for the clock frequency.
        /// They all have to have different Chip Select. You can use any of the 3 to 15 pin for this function.</remarks>
        protected override SpiDevice CreateSimpleSpiDevice(SpiConnectionSettings settings, int[] pins) => new Ft232HSpi(settings, this);

        /// <summary>
        /// Creates the <see cref="Ft232HGpio"/> controller
        /// </summary>
        /// <returns>A new GPIO driver</returns>
        protected override GpioDriver? TryCreateBestGpioDriver()
        {
            return new Ft232HGpio(this);
        }

        /// <inheritdoc />
        public override int GetDefaultI2cBusNumber()
        {
            return 0;
        }

        /// <inheritdoc />
        public override int[] GetDefaultPinAssignmentForI2c(int busId)
        {
            return new[] { 0, 1 };
        }

        /// <inheritdoc />
        public override int[] GetDefaultPinAssignmentForSpi(SpiConnectionSettings connectionSettings)
        {
            return new[] { 0, 1, 2, 3 };
        }

        internal bool IsI2cModeEnabled { get; set; }

        internal bool IsSpiModeEnabled { get; set; }

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
            if (PinOpen[0] || PinOpen[1] || PinOpen[2])
            {
                throw new IOException("Can't open I2C if GPIO 0, 1 or 2 are open");
            }

            if (IsSpiModeEnabled)
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

            IsI2cModeEnabled = true;
            DiscardInput();
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
            GpioLowData = (byte)(I2cDataSDAhiSCLhi | (GpioLowData & 0xF8));
            GpioLowDir = (byte)(I2cDirSDAoutSCLout | (GpioLowDir & 0xF8));
            toSend[idx++] = GpioLowData;
            toSend[idx++] = GpioLowDir;
            Write(toSend);
        }

        /// <summary>
        /// Multi-Protocol Synchronous Serial Engine (MPSSE). The purpose of the MPSSE command processor is to
        /// communicate with devices which use synchronous protocols (such as JTAG or SPI) in an efficient manner.
        /// </summary>
        internal void SetupMpsseMode()
        {
            // Seems that we have to send a wrong command to get the MPSSE mode working
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
            GpioLowData = (byte)(I2cDataSDAhiSCLhi | (GpioLowData & 0xF8));
            GpioLowDir = (byte)(I2cDirSDAoutSCLout | (GpioLowDir & 0xF8));
            Span<byte> toSend = stackalloc byte[NumberCycles * 3 * 3 + 3];
            for (count = 0; count < NumberCycles; count++)
            {
                toSend[idx++] = (byte)FtOpcode.SetDataBitsLowByte;
                toSend[idx++] = GpioLowData;
                toSend[idx++] = GpioLowDir;
            }

            // SDA lo, SCL high
            GpioLowData = (byte)(0x00 | I2cDataSDAloSCLhi | (GpioLowData & 0xF8));
            for (count = 0; count < NumberCycles; count++)
            {
                toSend[idx++] = (byte)FtOpcode.SetDataBitsLowByte;
                toSend[idx++] = GpioLowData;
                toSend[idx++] = GpioLowDir;
            }

            // SDA lo, SCL lo
            GpioLowData = (byte)(0x00 | I2cDataSDAloSCLlo | (GpioLowData & 0xF8));
            for (count = 0; count < NumberCycles; count++)
            {
                toSend[idx++] = (byte)FtOpcode.SetDataBitsLowByte;
                toSend[idx++] = GpioLowData;
                toSend[idx++] = GpioLowDir;
            }

            // Release SDA
            GpioLowData = (byte)(0x00 | I2cDataSDAhiSCLlo | (GpioLowData & 0xF8));
            toSend[idx++] = (byte)FtOpcode.SetDataBitsLowByte;
            toSend[idx++] = GpioLowData;
            toSend[idx++] = GpioLowDir;

            Write(toSend);
        }

        internal void I2cStop()
        {
            int count;
            int idx = 0;
            // SDA low, SCL low
            GpioLowData = (byte)(I2cDataSDAloSCLlo | (GpioLowData & 0xF8));
            GpioLowDir = (byte)(I2cDirSDAoutSCLout | (GpioLowDir & 0xF8));
            Span<byte> toSend = stackalloc byte[NumberCycles * 3 * 3];
            for (count = 0; count < NumberCycles; count++)
            {
                toSend[idx++] = (byte)FtOpcode.SetDataBitsLowByte;
                toSend[idx++] = GpioLowData;
                toSend[idx++] = GpioLowDir;
            }

            // SDA low, SCL high
            GpioLowData = (byte)(I2cDataSDAloSCLhi | (GpioLowData & 0xF8));
            GpioLowDir = (byte)(I2cDirSDAoutSCLout | (GpioLowDir & 0xF8));
            for (count = 0; count < NumberCycles; count++)
            {
                toSend[idx++] = (byte)FtOpcode.SetDataBitsLowByte;
                toSend[idx++] = GpioLowData;
                toSend[idx++] = GpioLowDir;
            }

            // SDA high, SCL high
            GpioLowData = (byte)(I2cDataSDAhiSCLhi | (GpioLowData & 0xF8));
            GpioLowDir = (byte)(I2cDirSDAoutSCLout | (GpioLowDir & 0xF8));
            for (count = 0; count < NumberCycles; count++)
            {
                toSend[idx++] = (byte)FtOpcode.SetDataBitsLowByte;
                toSend[idx++] = GpioLowData;
                toSend[idx++] = GpioLowDir;
            }

            Write(toSend);
        }

        internal void I2cLineIdle()
        {
            int idx = 0;
            // SDA low, SCL low
            GpioLowData = (byte)(I2cDataSDAhiSCLhi | (GpioLowData & 0xF8));
            GpioLowDir = (byte)(I2cDirSDAoutSCLout | (GpioLowDir & 0xF8));
            Span<byte> toSend = stackalloc byte[3];
            toSend[idx++] = (byte)FtOpcode.SetDataBitsLowByte;
            toSend[idx++] = GpioLowData;
            toSend[idx++] = GpioLowDir;
            Write(toSend);
            IsI2cModeEnabled = false;
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
            GpioLowData = (byte)(I2cDataSDAhiSCLlo | (GpioLowData & 0xF8));
            GpioLowDir = (byte)(I2cDirSDAoutSCLout | (GpioLowDir & 0xF8));
            toSend[idx++] = (byte)FtOpcode.SetDataBitsLowByte;
            toSend[idx++] = GpioLowData;
            toSend[idx++] = GpioLowDir;
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
            GpioLowDir = (byte)(I2cDirSDAoutSCLout | (GpioLowDir & 0xF8));
            GpioLowData = (byte)(I2cDataSDAhiSCLlo | (GpioLowData & 0xF8));
            toSend[idx++] = GpioLowDir;
            toSend[idx++] = GpioLowData;
            // And ask it right away
            toSend[idx++] = (byte)FtOpcode.SendImmediate;
            Write(toSend);
            Read(toRead);
            return toRead[0];
        }

        #endregion

        #region gpio

        internal void InitializeGpio()
        {
            if (IsI2cModeEnabled || IsSpiModeEnabled)
            {
                return;
            }

            // Reset
            var ftStatus = FtFunction.FT_SetBitMode(_ftHandle, 0x00, 0x00);
            // Enable MPSSE mode
            ftStatus = FtFunction.FT_SetBitMode(_ftHandle, 0x00, 0x02);
            if (ftStatus != FtStatus.Ok)
            {
                throw new IOException($"Failed to setup device {Description}, status: {ftStatus} in MPSSE mode");
            }

            // 50 ms according to thr doc for all USB to complete
            Thread.Sleep(50);
            DiscardInput();
            SetupMpsseMode();
        }

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
            toSend[1] = GpioLowData;
            toSend[2] = GpioLowDir;
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
            toSend[1] = GpioHighData;
            toSend[2] = GpioHighDir;
            Write(toSend);
        }

        #endregion

        #region SPI

        internal void SpiInitialize()
        {
            // Do we already have SPI setup?
            if (IsSpiModeEnabled)
            {
                // No need to initialize everything
                return;
            }

            GetHandle();
            IsSpiModeEnabled = true;
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
            DiscardInput();
            SetupMpsseMode();

            int idx = 0;
            Span<byte> toSend = stackalloc byte[10];
            toSend[idx++] = (byte)FtOpcode.DisableClockDivideBy5;
            toSend[idx++] = (byte)FtOpcode.TurnOffAdaptativeClocking;
            toSend[idx++] = (byte)FtOpcode.Disable3PhaseDataClocking;
            toSend[idx++] = (byte)FtOpcode.SetDataBitsLowByte;
            // Pin clock output, MISO output, MOSI input
            GpioLowDir = (byte)((GpioLowDir & 0xF8) | 0x03);
            // clock, MOSI and MISO to 0
            GpioLowData = (byte)(GpioLowData & 0xF8);
            toSend[idx++] = GpioLowDir;
            toSend[idx++] = GpioLowData;
            // The SK clock frequency can be worked out by below algorithm with divide by 5 set as off
            // TCK period = 60MHz / (( 1 + [ (0xValueH * 256) OR 0xValueL] ) * 2)
            // Command to set clock divisor
            toSend[idx++] = (byte)FtOpcode.SetClockDivisor;
            uint clockDivisor = (uint)(60000 / ((ConnectionSettings[0].ClockFrequency / 1000) * 2) - 1);
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
            if (ConnectionSettings.Count == 0)
            {
                IsSpiModeEnabled = false;
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

            SpiChipSelectEnable(settings.ChipSelectLine, settings.ChipSelectLineActiveState, true);
            int idx = 0;
            Span<byte> toSend = stackalloc byte[3 + buffer.Length];
            toSend[idx++] = clock;
            toSend[idx++] = (byte)((buffer.Length - 1) & 0xFF);
            toSend[idx++] = (byte)((buffer.Length - 1) >> 8);
            buffer.CopyTo(toSend.Slice(3));
            Write(toSend);
            SpiChipSelectEnable(settings.ChipSelectLine, settings.ChipSelectLineActiveState, false);
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

            SpiChipSelectEnable((byte)settings.ChipSelectLine, settings.ChipSelectLineActiveState, true);
            int idx = 0;
            Span<byte> toSend = stackalloc byte[3];
            toSend[idx++] = clock;
            toSend[idx++] = (byte)((buffer.Length - 1) & 0xFF);
            toSend[idx++] = (byte)((buffer.Length - 1) >> 8);
            Write(toSend);
            Read(buffer);
            SpiChipSelectEnable((byte)settings.ChipSelectLine, settings.ChipSelectLineActiveState, false);
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

            SpiChipSelectEnable(settings.ChipSelectLine, settings.ChipSelectLineActiveState, true);
            int idx = 0;
            Span<byte> toSend = stackalloc byte[3 + bufferWrite.Length];
            toSend[idx++] = clock;
            toSend[idx++] = (byte)((bufferWrite.Length - 1) & 0xFF);
            toSend[idx++] = (byte)((bufferWrite.Length - 1) >> 8);
            bufferWrite.CopyTo(toSend.Slice(3));
            Write(toSend);
            Read(bufferRead);
            SpiChipSelectEnable(settings.ChipSelectLine, settings.ChipSelectLineActiveState, false);
        }

        internal void SpiChipSelectEnable(int chipSelect, PinValue csPinValue, bool enable)
        {
            if (chipSelect < 0)
            {
                return;
            }

            var value = enable ? csPinValue : !csPinValue;

            Span<byte> toSend = stackalloc byte[NumberCycles * 3];
            int idx = 0;
            if (chipSelect < 8)
            {
                GpioLowDir |= (byte)(1 << chipSelect);
                if (value == PinValue.High)
                {
                    GpioLowData |= (byte)(1 << chipSelect);
                }
                else
                {
                    byte mask = 0xFF;
                    mask &= (byte)(~(1 << chipSelect));
                    GpioLowData &= mask;
                }

                for (int i = 0; i < NumberCycles; i++)
                {
                    toSend[idx++] = (byte)FtOpcode.SetDataBitsLowByte;
                    toSend[idx++] = GpioLowData;
                    toSend[idx++] = GpioLowDir;
                }
            }
            else
            {
                GpioHighDir |= (byte)(1 << (chipSelect - 8));
                if (value == PinValue.High)
                {
                    GpioHighData |= (byte)(1 << (chipSelect - 8));
                }
                else
                {
                    byte mask = 0xFF;
                    mask &= (byte)(~(1 << (chipSelect - 8)));
                    GpioHighData &= mask;
                }

                for (int i = 0; i < NumberCycles; i++)
                {
                    toSend[idx++] = (byte)FtOpcode.SetDataBitsHighByte;
                    toSend[idx++] = GpioHighData;
                    toSend[idx++] = GpioHighDir;
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

                    totalBytesRead += (int)numBytesRead;
                }
            }

            return totalBytesRead;
        }

        /// <summary>
        /// Clears all the input data to get an empty buffer.
        /// </summary>
        /// <returns>True for success</returns>
        private bool DiscardInput()
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
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _ftHandle.Dispose();
                _ftHandle = null!;
            }

            base.Dispose(disposing);
        }
    }
}
