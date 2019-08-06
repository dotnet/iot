// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Pn532
{
    /// <summary>
    /// All supported commands for the PN532
    /// </summary>
    internal enum CommandSet
    {
        // Miscellaneous
        Diagnose = 0x00,
        GetFirmwareVersion = 0x02,
        GetGeneralStatus = 0x04,
        ReadRegister = 0x06,
        WriteRegister = 0x08,
        ReadGPIO = 0x0C,
        WriteGPIO = 0x0E,
        SetSerialBaudRate = 0x10,
        SetParameters = 0x12,
        SAMConfiguration = 0x14,
        PowerDown = 0x16,
        // RF communication
        RFConfiguration = 0x32,
        RFRegulationTest = 0x58,
        // Initiator 
        InJumpForDEP = 0x56,
        InJumpForPSL = 0x46,
        InListPassiveTarget = 0x4A,
        InATR = 0x50,
        InPSL = 0x4E,
        InDataExchange = 0x40,
        InCommunicateThru = 0x42,
        InDeselect = 0x44,
        InRelease = 0x52,
        InSelect = 0x54,
        InAutoPoll = 0x60,
        // Target 
        TgInitAsTarget  = 0x8C,
        TgSetGeneralBytes = 0x92,
        TgGetData = 0x86,
        TgSetData = 0x8E,
        TgSetMetaData = 0x94,
        TgGetInitiatorCommand = 0x88,
        TgResponseToInitiator = 0x90,
        TgGetTargetStatus = 0x8A,
    }
}
