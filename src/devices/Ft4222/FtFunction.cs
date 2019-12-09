// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.InteropServices;

namespace System.Device.Ft4222
{
    /// <summary>
    /// Imports for the ftd2xx.dll as well as LibFT4222.dll
    /// </summary>
    internal class FtFunction
    {
        #region ftd2xx.dll

        /// <summary>
        /// Create Device Information List
        /// </summary>
        /// <param name="numdevs">number of devices</param>
        /// <returns>The status</returns>
        [DllImport("ftd2xx.dll")]
        public static extern FtStatus FT_CreateDeviceInfoList(out uint numdevs);

        /// <summary>
        /// Get Device Information Detail
        /// </summary>
        /// <param name="index">Index of the device</param>
        /// <param name="flags">Flags</param>
        /// <param name="chiptype">Device type</param>
        /// <param name="id">ID</param>
        /// <param name="locid">Location ID</param>
        /// <param name="serialnumber">Serial Number</param>
        /// <param name="description">Description</param>
        /// <param name="ftHandle">Handle</param>
        /// <returns>The status</returns>
        [DllImport("ftd2xx.dll")]
        public static extern FtStatus FT_GetDeviceInfoDetail(uint index, out uint flags, out FtDevice chiptype, out uint id, out uint locid, out byte serialnumber, out byte description, out IntPtr ftHandle);

        /// <summary>
        /// Open a device
        /// </summary>
        /// <param name="pvArg1">The device element identifying the device, depends on the flag</param>
        /// <param name="dwFlags">The flag how to open the device</param>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <returns>The status</returns>
        [DllImport("ftd2xx.dll")]
        public static extern FtStatus FT_OpenEx(uint pvArg1, FtOpenType dwFlags, out SafeFtHandle ftHandle);

        /// <summary>
        /// Close the device
        /// </summary>
        /// <param name="ftHandle">The device handle</param>
        /// <returns>The status</returns>
        [DllImport("ftd2xx.dll")]
        public static extern FtStatus FT_Close(SafeFtHandle ftHandle);

        #endregion

        #region LibFT4222.dll common functions

        /// <summary>
        /// Uninitialize a device, call before closing the device
        /// </summary>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <returns>The status</returns>
        [DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FtStatus FT4222_UnInitialize(SafeFtHandle ftHandle);

        /// <summary>
        /// Set the device system clock
        /// </summary>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <param name="clk">The system clock rate</param>
        /// <returns>The status</returns>
        [DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FtStatus FT4222_SetClock(SafeFtHandle ftHandle, FtClockRate clk);

        /// <summary>
        /// Get the system clock
        /// </summary>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <param name="clk">The system clock rate</param>
        /// <returns>The status</returns>
        [DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FtStatus FT4222_GetClock(SafeFtHandle ftHandle, out FtClockRate clk);

        /// <summary>
        /// Set the Wake Up Interrupt
        /// </summary>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <param name="enable">True to enable, false to disable</param>
        /// <returns>The status</returns>
        [DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FtStatus FT4222_SetWakeUpInterrupt(SafeFtHandle ftHandle, bool enable);

        /// <summary>
        /// Set Interrupt Trigger
        /// </summary>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <param name="trigger">The trigger type</param>
        /// <returns>The status</returns>
        [DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FtStatus FT4222_SetInterruptTrigger(SafeFtHandle ftHandle, GpioTrigger trigger);

        /// <summary>
        /// Set Suspend Out
        /// </summary>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <param name="enable">True to enable, false to disable</param>
        /// <returns>The status</returns>
        [DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FtStatus FT4222_SetSuspendOut(SafeFtHandle ftHandle, bool enable);

        /// <summary>
        /// Get the maximum transfer buffer size thru USB
        /// </summary>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <param name="pMaxSize">the maximum size in bytes</param>
        /// <returns>The status</returns>
        [DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FtStatus FT4222_GetMaxTransferSize(SafeFtHandle ftHandle, out ushort pMaxSize);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <param name="mask"></param>
        /// <param name="param"></param>
        /// <returns>The status</returns>
        [DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FtStatus FT4222_SetEventNotification(SafeFtHandle ftHandle, ulong mask, IntPtr param);

        /// <summary>
        /// Get the version of the chip and dll
        /// </summary>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <param name="pVersion">A version structure</param>
        /// <returns>The status</returns>
        [DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FtStatus FT4222_GetVersion(SafeFtHandle ftHandle, out FtVersion pVersion);

        /// <summary>
        /// Reset the chipset
        /// </summary>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <returns>The status</returns>
        [DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FtStatus FT4222_ChipReset(SafeFtHandle ftHandle);

        #endregion

        #region SPI

        /// <summary>
        /// Initialize the chip SPI as a master
        /// </summary>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <param name="ioLine">The operation mode, none, single, dual or quad</param>
        /// <param name="clock">The SPI clock divider of the system clock</param>
        /// <param name="cpol">The clock polarity</param>
        /// <param name="cpha">The clock phase</param>
        /// <param name="ssoMap">The chip select starting by 0x01</param>
        /// <returns>The status</returns>
        [DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FtStatus FT4222_SPIMaster_Init(SafeFtHandle ftHandle, SpiOperatingMode ioLine, SpiClock clock, SpiClockPolarity cpol, SpiClockPhase cpha, byte ssoMap);

        /// <summary>
        /// Set the operation mode for SPI as a master
        /// </summary>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <param name="spiMode">The operation mode, none, single, dual or quad</param>
        /// <returns>The status</returns>
        [DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FtStatus FT4222_SPIMaster_SetLines(SafeFtHandle ftHandle, SpiOperatingMode spiMode);

        /// <summary>
        /// Operate a single SPI read as a master
        /// </summary>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <param name="buffer">The output read buffer</param>
        /// <param name="bufferSize">The size of the buffer</param>
        /// <param name="sizeOfRead">Number of bytes read</param>
        /// <param name="isEndTransaction">True if this is the final SPI transaction</param>
        /// <returns>The status</returns>
        [DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FtStatus FT4222_SPIMaster_SingleRead(SafeFtHandle ftHandle, out byte buffer, ushort bufferSize, out ushort sizeOfRead, bool isEndTransaction);

        /// <summary>
        /// Operate a single SPI write as a master
        /// </summary>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <param name="buffer">The buffer to write</param>
        /// <param name="bufferSize">The size of the buffer</param>
        /// <param name="sizeTransferred">The size of the buffer</param>
        /// <param name="isEndTransaction">True if this is the final SPI transaction</param>
        /// <returns>The status</returns>
        [DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FtStatus FT4222_SPIMaster_SingleWrite(SafeFtHandle ftHandle, in byte buffer, ushort bufferSize, out ushort sizeTransferred, bool isEndTransaction);

        /// <summary>
        /// Operate a single read and write SPI operation as a master
        /// </summary>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <param name="readBuffer">The output read buffer</param>
        /// <param name="writeBuffer">The buffer to write</param>
        /// <param name="bufferSize">The size of the buffer</param>
        /// <param name="sizeTransferred">The size of buffer to transfer</param>
        /// <param name="isEndTransaction">True if this is the final SPI transaction</param>
        /// <returns>The status</returns>
        [DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FtStatus FT4222_SPIMaster_SingleReadWrite(SafeFtHandle ftHandle, out byte readBuffer, in byte writeBuffer, ushort bufferSize, out ushort sizeTransferred, bool isEndTransaction);

        /// <summary>
        /// Operate multiple read and write SPI operations as a master
        /// </summary>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <param name="readBuffer">The output read buffer</param>
        /// <param name="writeBuffer">The buffer to write</param>
        /// <param name="singleWriteBytes">singleWriteBytes</param>
        /// <param name="multiWriteBytes">multiWriteBytes</param>
        /// <param name="multiReadBytes">multiReadBytes</param>
        /// <param name="sizeOfRead">The size of the read buffer</param>
        /// <returns>The status</returns>
        [DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FtStatus FT4222_SPIMaster_MultiReadWrite(SafeFtHandle ftHandle, out byte readBuffer, in byte writeBuffer, byte singleWriteBytes, ushort multiWriteBytes, ushort multiReadBytes, out uint sizeOfRead);

        /// <summary>
        /// Initialize the chipset as a SPI slave
        /// </summary>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <returns>The status</returns>
        [DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FtStatus FT4222_SPISlave_Init(SafeFtHandle ftHandle);

        /// <summary>
        /// Initialize the chipset as a SPI slave
        /// </summary>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <param name="protocolOpt">Initialize with, without protocol or never send the acknowledge</param>
        /// <returns>The status</returns>
        [DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FtStatus FT4222_SPISlave_InitEx(SafeFtHandle ftHandle, SpiSlaveProtocol protocolOpt);

        /// <summary>
        /// Set SPI as slave clock modes
        /// </summary>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <param name="cpol">The clock polarity</param>
        /// <param name="cpha">The clock phase</param>
        /// <returns>The status</returns>
        [DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FtStatus FT4222_SPISlave_SetMode(SafeFtHandle ftHandle, SpiClockPolarity cpol, SpiClockPhase cpha);

        /// <summary>
        /// Get the SPI as salve RX status
        /// </summary>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <param name="pRxSize">The RX size</param>
        /// <returns>The status</returns>
        [DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FtStatus FT4222_SPISlave_GetRxStatus(SafeFtHandle ftHandle, out ushort pRxSize);

        /// <summary>
        /// Operate a SPI read as slave
        /// </summary>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <param name="buffer">The output read buffer</param>
        /// <param name="bufferSize">The size of the buffer</param>
        /// <param name="sizeOfRead">The size of the read buffer</param>
        /// <returns>The status</returns>
        [DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FtStatus FT4222_SPISlave_Read(SafeFtHandle ftHandle, out byte buffer, ushort bufferSize, out ushort sizeOfRead);

        /// <summary>
        /// Operate a SPI write as a slave
        /// </summary>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <param name="buffer">The buffer to write</param>
        /// <param name="bufferSize">The size of the buffer</param>
        /// <param name="sizeTransferred">The size what has been sent</param>
        /// <returns>The status</returns>
        [DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FtStatus FT4222_SPISlave_Write(SafeFtHandle ftHandle, in byte buffer, ushort bufferSize, out ushort sizeTransferred);

        /// <summary>
        /// Get or set the SPI as slave Rx quick response
        /// </summary>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <param name="enable">True to enable it, false to disable it</param>
        /// <returns>The status</returns>
        [DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FtStatus FT4222_SPISlave_RxQuickResponse(SafeFtHandle ftHandle, bool enable);

        /// <summary>
        /// Reset the SPI
        /// </summary>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <returns>The status</returns>
        [DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FtStatus FT4222_SPI_Reset(SafeFtHandle ftHandle);

        /// <summary>
        /// Reset a specific SPI transaction ID
        /// </summary>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <param name="spiIdx">The SPI ID</param>
        /// <returns>The status</returns>
        [DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FtStatus FT4222_SPI_ResetTransaction(SafeFtHandle ftHandle, byte spiIdx);

        /// <summary>
        /// Set the intensity of the pin out on SPI
        /// </summary>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <param name="clkStrength">The intensity of the clock pin</param>
        /// <param name="ioStrength">The intensity of the MOSI and MISO pins</param>
        /// <param name="ssoStrength">The intensity of the chip select pin</param>
        /// <returns>The status</returns>
        [DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FtStatus FT4222_SPI_SetDrivingStrength(SafeFtHandle ftHandle, PinDrivingStrength clkStrength, PinDrivingStrength ioStrength, PinDrivingStrength ssoStrength);

        #endregion

        #region I2C

        /// <summary>
        /// Initialize the chip as an I2C master
        /// </summary>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <param name="kbps">ency in kilo Hertz</param>
        /// <returns>The status</returns>
        [DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FtStatus FT4222_I2CMaster_Init(SafeFtHandle ftHandle, uint kbps);

        /// <summary>
        /// Operate an I2C read as a master
        /// </summary>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <param name="deviceAddress">The device address</param>
        /// <param name="buffer">The output read buffer</param>
        /// <param name="bufferSize">The size of the buffer</param>
        /// <param name="sizeTransferred">The size of the transfer</param>
        /// <returns>The status</returns>
        [DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FtStatus FT4222_I2CMaster_Read(SafeFtHandle ftHandle, ushort deviceAddress, out byte buffer, ushort bufferSize, out ushort sizeTransferred);

        /// <summary>
        /// Operate an I2C write as a master
        /// </summary>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <param name="deviceAddress">The device address</param>
        /// <param name="buffer">The buffer to write</param>
        /// <param name="bufferSize">The size of the buffer</param>
        /// <param name="sizeTransferred">The size of the transfer</param>
        /// <returns>The status</returns>
        [DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FtStatus FT4222_I2CMaster_Write(SafeFtHandle ftHandle, ushort deviceAddress, in byte buffer, ushort bufferSize, out ushort sizeTransferred);

        /// <summary>
        /// Operate an I2C read as a master
        /// </summary>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <param name="deviceAddress">The device address to read</param>
        /// <param name="flag">flag</param>
        /// <param name="buffer">The output read buffer</param>
        /// <param name="bufferSize">The size of the buffer</param>
        /// <param name="sizeTransferred">The size of the transfer</param>
        /// <returns>The status</returns>
        [DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FtStatus FT4222_I2CMaster_ReadEx(SafeFtHandle ftHandle, ushort deviceAddress, byte flag, out byte buffer, ushort bufferSize, out ushort sizeTransferred);

        /// <summary>
        /// Operate an I2C write as a master
        /// </summary>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <param name="deviceAddress">The device address to read</param>
        /// <param name="flag">flag</param>
        /// <param name="buffer">The buffer to write</param>
        /// <param name="bufferSize">The size of the buffer</param>
        /// <param name="sizeTransferred">The size of the transfer</param>
        /// <returns>The status</returns>
        [DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FtStatus FT4222_I2CMaster_WriteEx(SafeFtHandle ftHandle, ushort deviceAddress, byte flag, in byte buffer, ushort bufferSize, out ushort sizeTransferred);

        /// <summary>
        /// Reset I2C as a master
        /// </summary>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <returns>The status</returns>
        [DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FtStatus FT4222_I2CMaster_Reset(SafeFtHandle ftHandle);

        /// <summary>
        /// Get the I2C status as a master
        /// </summary>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <param name="controllerStatus"></param>
        /// <returns>The status</returns>
        [DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FtStatus FT4222_I2CMaster_GetStatus(SafeFtHandle ftHandle, out byte controllerStatus);

        /// <summary>
        /// Initialize the chip as an I2C slave
        /// </summary>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <returns>The status</returns>
        [DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FtStatus FT4222_I2CSlave_Init(SafeFtHandle ftHandle);

        /// <summary>
        /// REset the I2C as a slave
        /// </summary>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <returns>The status</returns>
        [DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FtStatus FT4222_I2CSlave_Reset(SafeFtHandle ftHandle);

        /// <summary>
        /// Get the I2C address as a slave
        /// </summary>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <param name="addr">The I2C device address</param>
        /// <returns>The status</returns>
        [DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FtStatus FT4222_I2CSlave_GetAddress(SafeFtHandle ftHandle, out byte addr);

        /// <summary>
        /// Get the I2C address as a slave
        /// </summary>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <param name="addr">The I2C device address</param>
        /// <returns>The status</returns>
        [DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FtStatus FT4222_I2CSlave_SetAddress(SafeFtHandle ftHandle, byte addr);

        /// <summary>
        /// Get the I2C as a slave RX status
        /// </summary>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <param name="pRxSize">the RX size</param>
        /// <returns>The status</returns>
        [DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FtStatus FT4222_I2CSlave_GetRxStatus(SafeFtHandle ftHandle, out ushort pRxSize);

        /// <summary>
        /// Operate an I2C read as a slave
        /// </summary>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <param name="buffer">The output read buffer</param>
        /// <param name="bufferSize">The size of the buffer</param>
        /// <param name="sizeTransferred">The size of the transfer</param>
        /// <returns>The status</returns>
        [DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FtStatus FT4222_I2CSlave_Read(SafeFtHandle ftHandle, out byte buffer, ushort bufferSize, out ushort sizeTransferred);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <param name="buffer">The buffer to write</param>
        /// <param name="bufferSize">The size of the buffer</param>
        /// <param name="sizeTransferred">The size of the transfer</param>
        /// <returns>The status</returns>
        [DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FtStatus FT4222_I2CSlave_Write(SafeFtHandle ftHandle, in byte buffer, ushort bufferSize, out ushort sizeTransferred);

        /// <summary>
        /// Set I2C as a slave clock stretch
        /// </summary>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <param name="enable">True to enable, false to disable</param>
        /// <returns>The status</returns>
        [DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FtStatus FT4222_I2CSlave_SetClockStretch(SafeFtHandle ftHandle, bool enable);

        /// <summary>
        /// Set I2C as a slave response word
        /// </summary>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <param name="responseWord">The response word</param>
        /// <returns>The status</returns>
        [DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FtStatus FT4222_I2CSlave_SetRespWord(SafeFtHandle ftHandle, byte responseWord);

        #endregion

        #region GPIO

        /// <summary>
        /// Initialize the chip as GPIO
        /// </summary>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <param name="gpioDir">Array of pin configuration</param>
        /// <returns>The status</returns>
        [DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FtStatus FT4222_GPIO_Init(SafeFtHandle ftHandle, GpioPinMode[] gpioDir);

        /// <summary>
        /// Operate a GPIO read
        /// </summary>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <param name="portNum">The pin port</param>
        /// <param name="value">True if high, false if low</param>
        /// <returns>The status</returns>
        [DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FtStatus FT4222_GPIO_Read(SafeFtHandle ftHandle, GpioPort portNum, out GpioPinValue value);

        /// <summary>
        /// Operate a GPIO write
        /// </summary>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <param name="portNum">The pin port</param>
        /// <param name="bValue">True if high, false if low</param>
        /// <returns>The status</returns>
        [DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FtStatus FT4222_GPIO_Write(SafeFtHandle ftHandle, GpioPort portNum, GpioPinValue bValue);

        /// <summary>
        /// Set the GPIO input trigger
        /// </summary>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <param name="portNum">The pin port</param>
        /// <param name="trigger">The trigger type</param>
        /// <returns>The status</returns>
        [DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FtStatus FT4222_GPIO_SetInputTrigger(SafeFtHandle ftHandle, GpioPort portNum, GpioTrigger trigger);

        /// <summary>
        /// Get the GPIO trigger status
        /// </summary>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <param name="portNum">The pin port</param>
        /// <param name="queueSize">The queue size</param>
        /// <returns>The status</returns>
        [DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FtStatus FT4222_GPIO_GetTriggerStatus(SafeFtHandle ftHandle, GpioPort portNum, out ushort queueSize);

        /// <summary>
        /// Read the GPIO Trigger queue
        /// </summary>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <param name="portNum">The pin port</param>
        /// <param name="events">Type of event</param>
        /// <param name="readSize">The number of events read</param>
        /// <param name="sizeofRead">The size of the read buffer</param>
        /// <returns>The status</returns>
        [DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FtStatus FT4222_GPIO_ReadTriggerQueue(SafeFtHandle ftHandle, GpioPort portNum, out GpioTrigger events, ushort readSize, out ushort sizeofRead);

        /// <summary>
        /// Set the GPIO in wave form
        /// </summary>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <param name="enable">True to enable, false to disable</param>
        /// <returns>The status</returns>
        [DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FtStatus FT4222_GPIO_SetWaveFormMode(SafeFtHandle ftHandle, bool enable);

        #endregion
    }
}
