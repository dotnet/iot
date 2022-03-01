// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Pn532
{
    /// <summary>
    /// All errors that can be returned by the PN532
    /// </summary>
    public enum ErrorCode
    {
        /// <summary>
        /// None
        /// </summary>
        None = 0x00,

        /// <summary>
        /// Timeout
        /// </summary>
        Timeout = 0x01,

        /// <summary>
        /// CRC Error
        /// </summary>
        CRCError = 0x02,

        /// <summary>
        /// Parity Error
        /// </summary>
        ParityError = 0x03,

        /// <summary>
        /// Erroneous Bit Count
        /// </summary>
        ErroneousBitCount = 0x04,

        /// <summary>
        /// Framing Error
        /// </summary>
        FramingError = 0x05,

        /// <summary>
        /// Abnormal Collision
        /// </summary>
        AbnormalCollision = 0x06,

        /// <summary>
        /// Buffer Size Insufficient
        /// </summary>
        BufferSizeInsufficient = 0x07,

        /// <summary>
        /// RF Buffer Overflow
        /// </summary>
        RFBufferOverflow = 0x09,

        /// <summary>
        /// RF Field Not Switched
        /// </summary>
        RFFieldNotSwitched = 0x0A,

        /// <summary>
        /// RF Protocol Error
        /// </summary>
        RFProtocolError = 0x0B,

        /// <summary>
        /// Temperature Error
        /// </summary>
        TemperatureError = 0x0D,

        /// <summary>
        /// Internal Buffer Overflow
        /// </summary>
        InternalBufferOverflow = 0x0E,

        /// <summary>
        /// Invalid Parameter
        /// </summary>
        InvalidParameter = 0x10,

        /// <summary>
        /// DEP Protocol Target Mode Not Supported
        /// </summary>
        DEPProtocolTargetModeNotSupport = 0x12,

        /// <summary>
        /// DEP Protocol Data Format Not Match
        /// </summary>
        DEPProtocolDataFormatNotMatch = 0x13,

        /// <summary>
        /// Mifare Authentication Error
        /// </summary>
        MifareAuthenticationError = 0x14,

        /// <summary>
        /// Check Byte Wrong
        /// </summary>
        CheckByteWrong = 0x23,

        /// <summary>
        /// DEP Protocol Invalid Device State
        /// </summary>
        DEPProtocolInvalidDeviceState = 0x25,

        /// <summary>
        /// Operation Not Allowed
        /// </summary>
        OperationNotAllowed = 0x26,

        /// <summary>
        /// Command Not Acceptable
        /// </summary>
        CommandNotAcceptable = 0x27,

        /// <summary>
        /// Configured Target Been Released
        /// </summary>
        ConfiguredTargetBeenReleased = 0x29,

        /// <summary>
        /// ID Card Does Not Match
        /// </summary>
        IDCardDoesNotMatch = 0x2A,

        /// <summary>
        /// Card Disappeared
        /// </summary>
        CardDisappeared = 0x2B,

        /// <summary>
        /// Mismatch DEP Passive
        /// </summary>
        MismatchDEPPassive = 0x2C,

        /// <summary>
        /// Over Current Detected
        /// </summary>
        OverCurrentDetected = 0x2D,

        /// <summary>
        /// NA DMissing
        /// </summary>
        NADMissing = 0x2E,

        /// <summary>
        /// Unknown
        /// </summary>
        Unknown = 0xFF,
    }
}
