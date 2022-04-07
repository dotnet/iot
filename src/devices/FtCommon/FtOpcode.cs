// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.FtCommon
{
    /// <summary>
    /// Form Application note:
    /// AN_108_Command_Processor_for_MPSSE_and_MCU_Host_Bus_Emulation_Modes.pdf
    /// </summary>
    internal enum FtOpcode
    {
        // MSB First

        /// <summary>
        /// 0x10,
        /// LengthL,
        /// LengthH,
        /// Byte1
        /// ..
        /// Byte65536 (max)
        /// </summary>
        ClockDataBytesOutOnPlusVeClockMSBFirst = 0x10,

        /// <summary>
        /// 0x11,
        /// LengthL,
        /// LengthH,
        /// Byte1
        /// ..
        /// Byte65536 (max)
        /// </summary>
        ClockDataBytesOutOnMinusVeClockMSBFirst = 0x11,

        /// <summary>
        /// 0x12,
        /// Length,
        /// Byte1
        /// </summary>
        ClockDataBitsOutOnPlusVeClockMSBFirst = 0x12,

        /// <summary>
        /// 0x13,
        /// Length,
        /// Byte1
        /// </summary>
        ClockDataBitsOutOnMinusVeClockMSBFirst = 0x13,

        /// <summary>
        /// 0x20,
        /// LengthL,
        /// LengthH
        /// </summary>
        ClockDataBytesInOnPlusVeClockMSBFirst = 0x20,

        /// <summary>
        /// 0x24,
        /// LengthL,
        /// LengthH
        /// </summary>
        ClockDataBytesInOnMinusVeClockMSBFirst = 0x24,

        /// <summary>
        /// 0x22,
        /// Length
        /// </summary>
        ClockDataBitsInOnPlusVeClockMSBFirst = 0x22,

        /// <summary>
        /// 0x26,
        /// Length
        /// </summary>
        ClockDataBitsInOnMinusVeClockMSBFirst = 0x26,

        /// <summary>
        /// 0x31,
        /// LengthL,
        /// LengthH,
        /// Byte1
        /// ..
        /// Byte65536 (max)
        /// </summary>
        ClockDataBytesOutOnMinusBytesInOnPlusVeClockMSBFirst = 0x31,

        /// <summary>
        /// 0x34,
        /// LengthL,
        /// LengthH,
        /// Byte1
        /// ..
        /// Byte65536 (max)
        /// </summary>
        ClockDataBytesOutOnPlusBytesInOnMinusVeClockMSBFirst = 0x34,

        /// <summary>
        /// 0x33,
        /// Length,
        /// Byte1
        /// </summary>
        ClockDataBitsOutOnMinusBitsInOnPlusVeClockMSBFirst = 0x33,

        /// <summary>
        /// 0x36,
        /// Length,
        /// Byte1
        /// </summary>
        ClockDataBitsOutOnPlusBitsInOnMinusVeClockMSBFirst = 0x36,

        // LSB First

        /// <summary>
        /// 0x18,
        /// LengthL,
        /// LengthH,
        /// Byte1
        /// ..
        /// Byte65536 (max)
        /// </summary>
        ClockDataBytesOutOnPlusVeClockLSBFirst = 0x18,

        /// <summary>
        /// 0x19,
        /// LengthL,
        /// LengthH,
        /// Byte1
        /// ..
        /// Byte65536 (max)
        /// </summary>
        ClockDataBytesOutOnMinusVeClockLSBFirst = 0x19,

        /// <summary>
        /// 0x1A,
        /// Length,
        /// Byte1
        /// </summary>
        ClockDataBitsOutOnPlusVeClockLSBFirst = 0x1A,

        /// <summary>
        /// 0x1B,
        /// Length,
        /// Byte1
        /// </summary>
        ClockDataBitsOutOnMinusVeClockLSBFirst = 0x1B,

        /// <summary>
        /// 0x28,
        /// LengthL,
        /// LengthH
        /// </summary>
        ClockDataBytesInOnPlusVeClockLSBFirst = 0x28,

        /// <summary>
        /// 0x2C,
        /// LengthL,
        /// LengthH
        /// </summary>
        ClockDataBytesInOnMinusVeClockLSBFirst = 0x2C,

        /// <summary>
        /// 0x2A,
        /// Length
        /// </summary>
        ClockDataBitsInOnPlusVeClockLSBFirst = 0x2A,

        /// <summary>
        /// 0x2E,
        /// Length
        /// </summary>
        ClockDataBitsInOnMinusVeClockSBFirst = 0x2E,

        /// <summary>
        /// 0x39,
        /// LengthL,
        /// LengthH,
        /// Byte1
        /// ..
        /// Byte65536 (max)
        /// </summary>
        ClockDataBytesOutOnMinusBytesInOnPlusVeClockLSBFirst = 0x39,

        /// <summary>
        /// 0x3C,
        /// LengthL,
        /// LengthH,
        /// Byte1
        /// ..
        /// Byte65536 (max)
        /// </summary>
        ClockDataBytesOutOnPlusBytesInOnMinusVeClockLSBFirst = 0x3C,

        /// <summary>
        /// 0x3B,
        /// Length,
        /// Byte1
        /// </summary>
        ClockDataBitsOutOnMinusBitsInOnPlusVeClockLSBFirst = 0x3B,

        /// <summary>
        /// 0x3E,
        /// Length,
        /// Byte1
        /// </summary>
        ClockDataBitsOutOnPlusBitsInOnMinusVeClockLSBFirst = 0x3E,

        // TMS Commands

        /// <summary>
        /// 0x4A
        /// Length,
        /// Byte1
        /// </summary>
        ClockDataBytesOutOnPlusVeClockTMSPinLSBFirst = 0x4A,

        /// <summary>
        /// 0x4B
        /// Length,
        /// Byte1
        /// </summary>
        ClockDataBytesOutOnMinusVeClockTMSPinSBFirst = 0x4B,

        /// <summary>
        /// 0x6A
        /// Length,
        /// Byte1
        /// </summary>
        ClockDataBytesOutOnPlusDataInOnPlusVeClockTMSPinSBFirst = 0x6A,

        /// <summary>
        /// 0x6B
        /// Length,
        /// Byte1
        /// </summary>
        ClockDataBytesOutOnMinusDataInOnPlusVeClockTMSPinSBFirst = 0x6B,

        /// <summary>
        /// 0x6E
        /// Length,
        /// Byte1
        /// </summary>
        ClockDataBytesOutOnPlusDataInOnMinusVeClockTMSPinSBFirst = 0x6E,

        /// <summary>
        /// 0x6F
        /// Length,
        /// Byte1
        /// </summary>
        ClockDataBytesOutOnMinusDataInOnMinusVeClockTMSPinSBFirst = 0x6F,

        // Set / Read Data Bits High / Low Bytes

        /// <summary>
        /// 0x80,
        /// 0xValue,
        /// 0xDirection
        /// </summary>
        SetDataBitsLowByte = 0x80,

        /// <summary>
        /// 0x82,
        /// 0xValue,
        /// 0xDirection
        /// </summary>
        SetDataBitsHighByte = 0x82,

        /// <summary>
        /// 0x81,
        /// </summary>
        ReadDataBitsLowByte = 0x81,

        /// <summary>
        /// 0x83,
        /// </summary>
        ReadDataBitsHighByte = 0x83,

        // Loopback Commands

        /// <summary>
        /// 0x84,
        /// This will connect the TDI/DO output to the TDO/DI input for loopback testing.
        /// </summary>
        ConnectTDItoTDOforLoopback = 0x84,

        /// <summary>
        /// 0x85,
        /// This will disconnect the TDI output from the TDO input for loopback testing.
        /// </summary>
        DisconnectTDItoTDOforLoopback = 0x85,

        // Clock Divisor

        /// <summary>
        /// Set TCK/SK Divisor (FT2232D)
        /// 0x86,
        /// 0xValueL,
        /// 0xValueH
        /// TCK/SK period = 12MHz / (( 1 +[(0xValueH * 256) OR 0xValueL] ) * 2)
        /// </summary>
        SetTCKSKDivisor = 0x86,

        /// <summary>
        /// 0x86,
        /// 0xValueL,
        /// 0xValueH,
        /// or example with the divide by 5 set as on:
        /// The TCK frequency can be worked out by the following algorithm :
        /// TCK period = 12MHz / (( 1 +[ (0xValueH * 256) OR 0xValueL] ) * 2)
        /// 0x0000 6 MHz
        /// 0xFFFF 91.553 Hz
        /// For example with the divide by 5 set as off:
        /// The TCK frequency can be worked out by the following algorithm :
        /// TCK period = 60MHz / (( 1 +[ (0xValueH * 256) OR 0xValueL] ) * 2)
        /// 0x0000 30 MHz
        /// 0xFFFF 457.763 Hz
        /// </summary>
        SetClockDivisor = 0x86,

        // CPU mode

        /// <summary>
        /// 0x90,
        /// 0xAddrLow
        /// This will read 1 byte from the target device.
        /// </summary>
        CPUModeReadShortAddress = 0x90,

        /// <summary>
        /// 0x91,
        /// 0xAddrHigh
        /// 0xAddrLow
        /// This will read 1 byte from the target device.
        /// </summary>
        CPUModeReadExtendedAddress = 0x91,

        /// <summary>
        /// 0x92,
        /// 0xAddrLow,
        /// 0xData
        /// This will write 1 byte from the target device.
        /// </summary>
        CPUModeWriteShortAddress = 0x92,

        /// <summary>
        /// 0x93,
        /// 0xAddrHigh,
        /// 0xAddrLow,
        /// 0xData
        /// This will write 1 byte from the target device.
        /// </summary>
        CPUModeWriteExtendedAddress = 0x93,

        // MPSSE and MCU Host Emulation Modes

        /// <summary>
        /// 0x87,
        /// This will make the chip flush its buffer back to the PC.
        /// </summary>
        SendImmediate = 0x87,

        /// <summary>
        /// 0x88,
        /// This will cause the MPSSE controller to wait until GPIOL1 (JTAG) or I/O1 (CPU) is high. Once it is detected
        /// as high, it will move on to process the next instruction. The only way out of this will be to disable the
        /// controller if the I/O line never goes high.
        /// </summary>
        WaitOnIOHigh = 0x88,

        /// <summary>
        /// 0x89,
        /// This will cause the controller to wait until GPIOL1 (JTAG) or I/O1 (CPU) is low. Once it is detected as low,
        /// it will move on to process the next instruction. The only way out of this will be to disable the controller if
        /// the I/O line never goes low.
        /// </summary>
        WaitOnIOLow = 0x89,

        // FT232H, FT2232H & FT4232H ONLY

        /// <summary>
        /// 0x8A
        /// This will turn off the divide by 5 from the 60 MHz clock.
        /// </summary>
        DisableClockDivideBy5 = 0x8A,

        /// <summary>
        /// 0x8B
        /// This will turn on the divide by 5 from the 60 MHz clock to give a 12MHz master clock for backward
        /// compatibility with FT2232D designs
        /// </summary>
        EnableClockDivideBy5 = 0x8B,

        /// <summary>
        /// 0x8C
        /// This will give a 3 stage data shift for the purposes of supporting interfaces such as I2C which need the
        /// data to be valid on both edges of the clk. So it will appear as
        /// Data setup for ½ clock period -> pulse clock for ½ clock period -> Data hold for ½ clock period.
        /// </summary>
        Enable3PhaseDataClocking = 0x8C,

        /// <summary>
        /// 0x8D
        /// This will give a 2 stage data shift which is the default state. So it will appear as
        /// Data setup for ½ clock period -> Pulse clock for ½ clock period
        /// </summary>
        Disable3PhaseDataClocking = 0x8D,

        /// <summary>
        /// 0x8E
        /// Length,
        /// </summary>
        ClockForNBitsWithNoDataTransfer = 0x8E,

        /// <summary>
        /// 0x8F
        /// LengthL,
        /// LengthH,
        /// A length of 0x0000 will do 8 clocks
        /// and a length of 0xFFFF will do 524288 clocks
        /// </summary>
        ClockForNx8BitsWithNoDataTransfer = 0x8F,

        /// <summary>
        /// 0x94
        /// </summary>
        ClockContinuouslyAndWaitOnIOHigh = 0x94,

        /// <summary>
        /// 0x95
        /// </summary>
        ClockContinuouslyAndWaitOnIOLow = 0x95,

        /// <summary>
        /// 0x96,
        /// Adaptive clocking is required when using the JTAG interface on an ARM processor.
        /// </summary>
        TurnOnAdaptativeClocking = 0x96,

        /// <summary>
        /// 0x97,
        /// </summary>
        TurnOffAdaptativeClocking = 0x97,

        /// <summary>
        /// 0x9C
        /// LengthL,
        /// LengthH,
        /// A length of 0x0000 will do 8 clocks
        /// and a length of 0xFFFF will do 524288 clocks or until GPIOL1 is high.
        /// </summary>
        ClockForNx8BitsWithNoDataTransferOrUntilGPIOL1IsHigh = 0x9C,

        /// <summary>
        /// 0x9D
        /// LengthL,
        /// LengthH,
        /// A length of 0x0000 will do 8 clocks
        /// and a length of 0xFFFF will do 524288 clocks or until GPIOL1 is low.
        /// </summary>
        ClockForNx8BitsWithNoDataTransferOrUntilGPIOL1IsLow = 0x9D,

        // FT232H ONLY

        /// <summary>
        /// 0x9E
        /// LowByteEnablesForOnlyDrive0
        /// HighByteEnablesForOnlyDrive0
        /// This will make the I/Os only drive when the data is ‘0’ and tristate on the data being ‘1’ when the
        /// appropriate bit is set. Use this op-code when configuring the MPSSE for I2C use.
        /// </summary>
        SetIOOnlyDriveOn0AndTristateOn1 = 0x9E,
    }
}
