// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Device.Ft4222
{
    /// <summary>
    /// Errors for FT4222
    /// </summary>
    internal enum FtStatus
    {
        /// <summary>
        /// Status OK
        /// </summary>
        Ok = 0,
        /// <summary>
        /// The device handle is invalid
        /// </summary>
        InvalidHandle,
        /// <summary>
        /// Device not found
        /// </summary>
        DeviceNotFound,
        /// <summary>
        /// Device is not open
        /// </summary>
        DeviceNotOpen,
        /// <summary>
        /// IO error
        /// </summary>
        IoError,
        /// <summary>
        /// Insufficient resources
        /// </summary>
        InsufficientResources,
        /// <summary>
        /// A parameter was invalid
        /// </summary>
        InvalidParameter,
        /// <summary>
        /// The requested baud rate is invalid
        /// </summary>
        InvalidBaudRate,
        /// <summary>
        /// Device not opened for erase
        /// </summary>
        DeviceNotOpenForErase,
        /// <summary>
        /// Device not opened for write
        /// </summary>
        DeviceNotOpenForWrite,
        /// <summary>
        /// Failed to write to device
        /// </summary>
        FailedToWriteToDevice,
        /// <summary>
        /// Failed to read the device EEPROM
        /// </summary>
        EepromFailedToRead,
        /// <summary>
        /// Failed to write the device EEPROM
        /// </summary>
        EepromFailedToWrite,
        /// <summary>
        /// Failed to erase the device EEPROM
        /// </summary>
        EepromFailedToErase,
        /// <summary>
        /// An EEPROM is not fitted to the device
        /// </summary>
        EepromNotPresent,
        /// <summary>
        /// Device EEPROM is blank
        /// </summary>
        EepromNotProgrammed,
        /// <summary>
        /// Invalid arguments
        /// </summary>
        InvalidArguments,
        /// <summary>
        /// An other error has occurred
        /// </summary>
        OtherError,
        /// <summary>
        /// The device list is not ready
        /// </summary>
        DeviceListNotReady,

        // Below are the specific error messages for the FT4222

        /// <summary>
        /// Device not supported
        /// </summary>
        DeviceNotSupported = 1000,
        /// <summary>
        /// Spi master do not support 80MHz/CLK_2
        /// </summary>
        SpiMasterClockNotSupported,
        /// <summary>
        /// Vender command not supported
        /// </summary>
        VenderCommandNotSupported,
        /// <summary>
        /// FT4222 is not in SPI mode
        /// </summary>
        Ft4222IsNotSpiMode,
        /// <summary>
        /// FT4222 is not in I2C mode
        /// </summary>
        Ft4222IsNotI2cMode,
        /// <summary>
        /// FT4222 is not in SPI single mode
        /// </summary>
        Ft4222IsNotSpiSingleMode,
        /// <summary>
        /// FT4222 is not in SPI multi mode
        /// </summary>
        Ft4222IsNotSpiMultiMode,
        /// <summary>
        /// Wrong I2C address
        /// </summary>
        WrongI2cAddress,
        /// <summary>
        /// Invalid function
        /// </summary>
        InvalidFunction,
        /// <summary>
        /// Invalid pointer
        /// </summary>
        InvalidPointer,
        /// <summary>
        /// Exceeded maximum transfer size
        /// </summary>
        ExceededMaximumTransferSize,
        /// <summary>
        /// Failed to read device
        /// </summary>
        FailedToReadDevice,
        /// <summary>
        /// I2C is not supported in this mode
        /// </summary>
        I2cNotSupportedInThisMode,
        /// <summary>
        /// GPIO is not supported in this mode
        /// </summary>
        GpioNotSupportedInThisMode,
        /// <summary>
        /// GPIO exceeded maximum port number
        /// </summary>
        GpioExceededMaximumPortNumber,
        /// <summary>
        /// GPIO write not supported
        /// </summary>
        GpioWriteNotSupported,
        /// <summary>
        /// GPIO pull up invalid in input mode
        /// </summary>
        GpioPullUpInvalidInInputMode,
        /// <summary>
        /// GPIO pull down invalid in input mode
        /// </summary>
        GpioPullDownInvalidInInputMode,
        /// <summary>
        /// GPIO open drain invalid in output mode
        /// </summary>
        GpioOpenDrainInvalidInOutputMode,
        /// <summary>
        /// Interrupt not supported
        /// </summary>
        InterruptNotSupported,
        /// <summary>
        /// GPIO input not supported
        /// </summary>
        GpioInputNotSupported,
        /// <summary>
        /// Event not supported
        /// </summary>
        EventNotSupported,
    };
}
