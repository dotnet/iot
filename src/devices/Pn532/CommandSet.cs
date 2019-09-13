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
        // Miscellaneous section
        /// <summary>
        /// Diagnose
        /// </summary>
        Diagnose = 0x00,
        /// <summary>
        /// GetFirmwareVersion
        /// </summary>
        GetFirmwareVersion = 0x02,
        /// <summary>
        /// GetGeneralStatus
        /// </summary>
        GetGeneralStatus = 0x04,
        /// <summary>
        /// ReadRegister
        /// </summary>
        ReadRegister = 0x06,
        /// <summary>
        /// WriteRegister
        /// </summary>
        WriteRegister = 0x08,
        /// <summary>
        /// ReadGPIO
        /// </summary>
        ReadGPIO = 0x0C,
        /// <summary>
        /// WriteGPIO
        /// </summary>
        WriteGPIO = 0x0E,
        /// <summary>
        /// SetSerialBaudRate
        /// </summary>
        SetSerialBaudRate = 0x10,
        /// <summary>
        /// SetParameters
        /// </summary>
        SetParameters = 0x12,
        /// <summary>
        /// SAMConfiguration
        /// </summary>
        SAMConfiguration = 0x14,
        /// <summary>
        /// PowerDown
        /// </summary>
        PowerDown = 0x16,
        // RF communication section
        /// <summary>
        /// RFConfiguration
        /// </summary>
        RFConfiguration = 0x32,
        /// <summary>
        /// RFRegulationTest
        /// </summary>
        RFRegulationTest = 0x58,
        // Initiator section
        /// <summary>
        /// InJumpForDEP
        /// </summary>        
        InJumpForDEP = 0x56,
        /// <summary>
        /// InJumpForPSL
        /// </summary>
        InJumpForPSL = 0x46,
        /// <summary>
        /// InListPassiveTarget
        /// </summary>
        InListPassiveTarget = 0x4A,
        /// <summary>
        /// InATR
        /// </summary>
        InATR = 0x50,
        /// <summary>
        /// InPSL
        /// </summary>
        InPSL = 0x4E,
        /// <summary>
        /// InDataExchange
        /// </summary>
        InDataExchange = 0x40,
        /// <summary>
        /// InCommunicateThru
        /// </summary>
        InCommunicateThru = 0x42,
        /// <summary>
        /// InDeselect
        /// </summary>
        InDeselect = 0x44,
        /// <summary>
        /// InRelease
        /// </summary>
        InRelease = 0x52,
        /// <summary>
        /// InSelect
        /// </summary>
        InSelect = 0x54,
        /// <summary>
        /// InAutoPoll
        /// </summary>
        InAutoPoll = 0x60,
        // Target 
        /// <summary>
        /// TgInitAsTarget
        /// </summary>
        TgInitAsTarget = 0x8C,
        /// <summary>
        /// TgSetGeneralBytes
        /// </summary>
        TgSetGeneralBytes = 0x92,
        /// <summary>
        /// TgGetData
        /// </summary>
        TgGetData = 0x86,
        /// <summary>
        /// TgSetData
        /// </summary>
        TgSetData = 0x8E,
        /// <summary>
        /// TgSetMetaData
        /// </summary>
        TgSetMetaData = 0x94,
        /// <summary>
        /// TgGetInitiatorCommand
        /// </summary>
        TgGetInitiatorCommand = 0x88,
        /// <summary>
        /// TgResponseToInitiator
        /// </summary>
        TgResponseToInitiator = 0x90,
        /// <summary>
        /// TgGetTargetStatus
        /// </summary>
        TgGetTargetStatus = 0x8A,
    }
}
