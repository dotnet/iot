// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace IoT.Device.Pn532
{
    /// <summary>
    /// All errors that can be returned by the PN532
    /// </summary>
    public enum ErrorCode
    {
        None = 0x00,
        Timeout = 0x01,
        CRCError = 0x02,
        ParityError = 0x03,
        ErroneuousBitCount = 0x04,
        FramingError = 0x05,
        AbnormalCollision = 0x06,
        BufferSizeInsufficient = 0x07,
        RFBufferOverflow = 0x09,
        RFFieldNotSwitched = 0x0A,
        RFProtocolError = 0x0B,
        TemperatureError = 0x0D,
        InternalBufferOverflow = 0x0E,
        InvalidParameter = 0x10,
        DEPProtocolTargetModeNotSupport = 0x12,
        DEPProtocolDataFormatNotMatch = 0x13,
        MifareAuthenticationError = 0x14,
        CheckByteWrong = 0x23,
        DEPProtocolInvalidDeviceState = 0x25,
        OperationNotAllowed = 0x26,
        CommandNotAcceptable = 0x27,
        ConfiguredTargetBeenReleased = 0x29,
        IDCardDoesNotMatch = 0x2A,
        CardDisappeared = 0x2B,
        MismatchDEPPassive = 0x2C,
        OverCurrentDetected = 0x2D,
        NADMissing = 0x2E,
        Unknown = 0xFF,
    }
}
