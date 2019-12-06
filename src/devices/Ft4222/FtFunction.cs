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
        public static extern FtStatus FT_CreateDeviceInfoList(ref uint numdevs);

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
        public static extern FtStatus FT_GetDeviceInfoDetail(uint index, ref uint flags, ref FtDevice chiptype, ref uint id, ref uint locid, byte[] serialnumber, byte[] description, ref IntPtr ftHandle);

        /// <summary>
        /// Open a device
        /// </summary>
        /// <param name="pvArg1">The device element identifying the device, depends on the flag</param>
        /// <param name="dwFlags">The flag how to open the device</param>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <returns>The status</returns>
        [DllImport("ftd2xx.dll")]
        public static extern FtStatus FT_OpenEx(uint pvArg1, FtOpenType dwFlags, ref IntPtr ftHandle);

        /// <summary>
        /// Close the device
        /// </summary>
        /// <param name="ftHandle">The device handle</param>
        /// <returns>The status</returns>
        [DllImport("ftd2xx.dll")]
        public static extern FtStatus FT_Close(IntPtr ftHandle);

        #endregion

        #region LibFT4222.dll common functions

        /// <summary>
        /// Uninitialize a device, call before closing the device
        /// </summary>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <returns>The status</returns>
        [DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FtStatus FT4222_UnInitialize(IntPtr ftHandle);

        /// <summary>
        /// Set the device system clock
        /// </summary>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <param name="clk">The system clock rate</param>
        /// <returns>The status</returns>
        [DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FtStatus FT4222_SetClock(IntPtr ftHandle, FtClockRate clk);

        /// <summary>
        /// Get the system clock
        /// </summary>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <param name="clk">The system clock rate</param>
        /// <returns>The status</returns>
        [DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FtStatus FT4222_GetClock(IntPtr ftHandle, ref FtClockRate clk);

        /// <summary>
        /// Set the Wake Up Interrupt
        /// </summary>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <param name="enable">True to enable, false to disable</param>
        /// <returns>The status</returns>
        [DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FtStatus FT4222_SetWakeUpInterrupt(IntPtr ftHandle, bool enable);

        /// <summary>
        /// Set Interrupt Trigger
        /// </summary>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <param name="trigger">The trigger type</param>
        /// <returns>The status</returns>
        [DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FtStatus FT4222_SetInterruptTrigger(IntPtr ftHandle, GpioTrigger trigger);

        /// <summary>
        /// Set Suspend Out
        /// </summary>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <param name="enable">True to enable, false to disable</param>
        /// <returns>The status</returns>
        [DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FtStatus FT4222_SetSuspendOut(IntPtr ftHandle, bool enable);

        /// <summary>
        /// Get the maximum transfer buffer size thru USB
        /// </summary>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <param name="pMaxSize">the maximum size in bytes</param>
        /// <returns>The status</returns>
        [DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FtStatus FT4222_GetMaxTransferSize(IntPtr ftHandle, ref ushort pMaxSize);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <param name="mask"></param>
        /// <param name="param"></param>
        /// <returns>The status</returns>
        [DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FtStatus FT4222_SetEventNotification(IntPtr ftHandle, ulong mask, IntPtr param);

        /// <summary>
        /// Get the version of the chip and dll
        /// </summary>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <param name="pVersion">A version structure</param>
        /// <returns>The status</returns>
        [DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FtStatus FT4222_GetVersion(IntPtr ftHandle, ref FtVersion pVersion);

        /// <summary>
        /// Reset the chipset
        /// </summary>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <returns>The status</returns>
        [DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FtStatus FT4222_ChipReset(IntPtr ftHandle);

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
        public static extern FtStatus FT4222_SPIMaster_Init(IntPtr ftHandle, SpiOperatingMode ioLine, SpiClock clock, SpiClockPolarity cpol, SpiClockPhase cpha, byte ssoMap);

        /// <summary>
        /// Set the operation mode for SPI as a master
        /// </summary>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <param name="spiMode">The operation mode, none, single, dual or quad</param>
        /// <returns>The status</returns>
        [DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FtStatus FT4222_SPIMaster_SetLines(IntPtr ftHandle, SpiOperatingMode spiMode);

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
        public static extern FtStatus FT4222_SPIMaster_SingleRead(IntPtr ftHandle, byte[] buffer, ushort bufferSize, ref ushort sizeOfRead, bool isEndTransaction);

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
        public static extern FtStatus FT4222_SPIMaster_SingleWrite(IntPtr ftHandle, byte[] buffer, ushort bufferSize, ref ushort sizeTransferred, bool isEndTransaction);

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
        public static extern FtStatus FT4222_SPIMaster_SingleReadWrite(IntPtr ftHandle, byte[] readBuffer, byte[] writeBuffer, ushort bufferSize, ref ushort sizeTransferred, bool isEndTransaction);

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
        public static extern FtStatus FT4222_SPIMaster_MultiReadWrite(IntPtr ftHandle, byte[] readBuffer, byte[] writeBuffer, byte singleWriteBytes, ushort multiWriteBytes, ushort multiReadBytes, ref uint sizeOfRead);

        /// <summary>
        /// Initialize the chipset as a SPI slave
        /// </summary>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <returns>The status</returns>
        [DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FtStatus FT4222_SPISlave_Init(IntPtr ftHandle);

        /// <summary>
        /// Initialize the chipset as a SPI slave
        /// </summary>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <param name="protocolOpt">Initialize with, without protocol or never send the hacknoledge</param>
        /// <returns>The status</returns>
        [DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FtStatus FT4222_SPISlave_InitEx(IntPtr ftHandle, SpiSlaveProtocol protocolOpt);

        /// <summary>
        /// Set SPI as slave clock modes
        /// </summary>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <param name="cpol">The clock polarity</param>
        /// <param name="cpha">The clock phase</param>
        /// <returns>The status</returns>
        [DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FtStatus FT4222_SPISlave_SetMode(IntPtr ftHandle, SpiClockPolarity cpol, SpiClockPhase cpha);

        /// <summary>
        /// Get the SPI as salve RX status
        /// </summary>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <param name="pRxSize">The RX size</param>
        /// <returns>The status</returns>
        [DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FtStatus FT4222_SPISlave_GetRxStatus(IntPtr ftHandle, ref ushort pRxSize);

        /// <summary>
        /// Operate a SPI read as slave
        /// </summary>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <param name="buffer">The output read buffer</param>
        /// <param name="bufferSize">The size of the buffer</param>
        /// <param name="sizeOfRead">The size of the read buffer</param>
        /// <returns>The status</returns>
        [DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FtStatus FT4222_SPISlave_Read(IntPtr ftHandle, byte[] buffer, ushort bufferSize, ref ushort sizeOfRead);

        /// <summary>
        /// Operate a SPI write as a slave
        /// </summary>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <param name="buffer">The buffer to write</param>
        /// <param name="bufferSize">The size of the buffer</param>
        /// <param name="sizeTransferred">The size what has been sent</param>
        /// <returns>The status</returns>
        [DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FtStatus FT4222_SPISlave_Write(IntPtr ftHandle, byte[] buffer, ushort bufferSize, ref ushort sizeTransferred);

        /// <summary>
        /// Get or set the SPI as slave Rx quick response
        /// </summary>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <param name="enable">True to enable it, false to disable it</param>
        /// <returns>The status</returns>
        [DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FtStatus FT4222_SPISlave_RxQuickResponse(IntPtr ftHandle, bool enable);

        /// <summary>
        /// Reset the SPI
        /// </summary>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <returns>The status</returns>
        [DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FtStatus FT4222_SPI_Reset(IntPtr ftHandle);

        /// <summary>
        /// Reset a specific SPI transaction ID
        /// </summary>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <param name="spiIdx">The SPI ID</param>
        /// <returns>The status</returns>
        [DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FtStatus FT4222_SPI_ResetTransaction(IntPtr ftHandle, byte spiIdx);

        /// <summary>
        /// Set the intensity of the pin out on SPI
        /// </summary>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <param name="clkStrength">The intensity of the clock pin</param>
        /// <param name="ioStrength">The intensity of the MOSI and MISO pins</param>
        /// <param name="ssoStrength">The intensity of the chip select pin</param>
        /// <returns>The status</returns>
        [DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FtStatus FT4222_SPI_SetDrivingStrength(IntPtr ftHandle, PinDrivingStrength clkStrength, PinDrivingStrength ioStrength, PinDrivingStrength ssoStrength);

        #endregion

        #region I2C

        /// <summary>
        /// Initialize the chip as an I2C master
        /// </summary>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <param name="kbps">ency in kilo Hertz</param>
        /// <returns>The status</returns>
        [DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FtStatus FT4222_I2CMaster_Init(IntPtr ftHandle, uint kbps);

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
        public static extern FtStatus FT4222_I2CMaster_Read(IntPtr ftHandle, ushort deviceAddress, byte[] buffer, ushort bufferSize, ref ushort sizeTransferred);

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
        public static extern FtStatus FT4222_I2CMaster_Write(IntPtr ftHandle, ushort deviceAddress, byte[] buffer, ushort bufferSize, ref ushort sizeTransferred);

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
        public static extern FtStatus FT4222_I2CMaster_ReadEx(IntPtr ftHandle, ushort deviceAddress, byte flag, byte[] buffer, ushort bufferSize, ref ushort sizeTransferred);

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
        public static extern FtStatus FT4222_I2CMaster_WriteEx(IntPtr ftHandle, ushort deviceAddress, byte flag, byte[] buffer, ushort bufferSize, ref ushort sizeTransferred);

        /// <summary>
        /// Reset I2C as a master
        /// </summary>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <returns>The status</returns>
        [DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FtStatus FT4222_I2CMaster_Reset(IntPtr ftHandle);

        /// <summary>
        /// Get the I2C status as a master
        /// </summary>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <param name="controllerStatus"></param>
        /// <returns>The status</returns>
        [DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FtStatus FT4222_I2CMaster_GetStatus(IntPtr ftHandle, ref byte controllerStatus);

        /// <summary>
        /// Initialize the chip as an I2C slave
        /// </summary>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <returns>The status</returns>
        [DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FtStatus FT4222_I2CSlave_Init(IntPtr ftHandle);

        /// <summary>
        /// REset the I2C as a slave
        /// </summary>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <returns>The status</returns>
        [DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FtStatus FT4222_I2CSlave_Reset(IntPtr ftHandle);

        /// <summary>
        /// Get the I2C address as a slave
        /// </summary>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <param name="addr">The I2C device address</param>
        /// <returns>The status</returns>
        [DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FtStatus FT4222_I2CSlave_GetAddress(IntPtr ftHandle, ref byte addr);

        /// <summary>
        /// Get the I2C address as a slave
        /// </summary>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <param name="addr">The I2C device address</param>
        /// <returns>The status</returns>
        [DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FtStatus FT4222_I2CSlave_SetAddress(IntPtr ftHandle, byte addr);

        /// <summary>
        /// Get the I2C as a slave RX status
        /// </summary>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <param name="pRxSize">the RX size</param>
        /// <returns>The status</returns>
        [DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FtStatus FT4222_I2CSlave_GetRxStatus(IntPtr ftHandle, ref ushort pRxSize);

        /// <summary>
        /// Operate an I2C read as a slave
        /// </summary>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <param name="buffer">The output read buffer</param>
        /// <param name="bufferSize">The size of the buffer</param>
        /// <param name="sizeTransferred">The size of the transfer</param>
        /// <returns>The status</returns>
        [DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FtStatus FT4222_I2CSlave_Read(IntPtr ftHandle, byte[] buffer, ushort bufferSize, ref ushort sizeTransferred);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <param name="buffer">The buffer to write</param>
        /// <param name="bufferSize">The size of the buffer</param>
        /// <param name="sizeTransferred">The size of the transfer</param>
        /// <returns>The status</returns>
        [DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FtStatus FT4222_I2CSlave_Write(IntPtr ftHandle, byte[] buffer, ushort bufferSize, ref ushort sizeTransferred);

        /// <summary>
        /// Set I2C as a slave clock stretch
        /// </summary>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <param name="enable">True to enable, false to disable</param>
        /// <returns>The status</returns>
        [DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FtStatus FT4222_I2CSlave_SetClockStretch(IntPtr ftHandle, bool enable);

        /// <summary>
        /// Set I2C as a slave response word
        /// </summary>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <param name="responseWord">The response word</param>
        /// <returns>The status</returns>
        [DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FtStatus FT4222_I2CSlave_SetRespWord(IntPtr ftHandle, byte responseWord);

        #endregion

        #region GPIO

        /// <summary>
        /// Initialize the chi as GPIO
        /// </summary>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <param name="gpioDir">Array of pin configuration</param>
        /// <returns>The status</returns>
        [DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FtStatus FT4222_GPIO_Init(IntPtr ftHandle, [MarshalAs(UnmanagedType.LPArray, SizeConst = 4)] GpioPinMode[] gpioDir);

        /// <summary>
        /// Operate a GPIO read
        /// </summary>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <param name="portNum">The pin port</param>
        /// <param name="value">True if high, false if low</param>
        /// <returns>The status</returns>
        [DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FtStatus FT4222_GPIO_Read(IntPtr ftHandle, GpioPort portNum, ref GpioPinValue value);

        /// <summary>
        /// Operate a GPIO write
        /// </summary>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <param name="portNum">The pin port</param>
        /// <param name="bValue">True if high, false if low</param>
        /// <returns>The status</returns>
        [DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FtStatus FT4222_GPIO_Write(IntPtr ftHandle, GpioPort portNum, GpioPinValue bValue);

        /// <summary>
        /// Set the GPIO input trigger
        /// </summary>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <param name="portNum">The pin port</param>
        /// <param name="trigger">The trigger type</param>
        /// <returns>The status</returns>
        [DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FtStatus FT4222_GPIO_SetInputTrigger(IntPtr ftHandle, GpioPort portNum, GpioTrigger trigger);

        /// <summary>
        /// Get the GPIO trigger status
        /// </summary>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <param name="portNum">The pin port</param>
        /// <param name="queueSize">The queue size</param>
        /// <returns>The status</returns>
        [DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FtStatus FT4222_GPIO_GetTriggerStatus(IntPtr ftHandle, GpioPort portNum, ref ushort queueSize);

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
        public static extern FtStatus FT4222_GPIO_ReadTriggerQueue(IntPtr ftHandle, GpioPort portNum, GpioTrigger[] events, ushort readSize, ref ushort sizeofRead);

        /// <summary>
        /// Set the GPIO in wave form
        /// </summary>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <param name="enable">True to enable, false to disable</param>
        /// <returns>The status</returns>
        [DllImport("LibFT4222.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern FtStatus FT4222_GPIO_SetWaveFormMode(IntPtr ftHandle, bool enable);

        #endregion
    }
}
