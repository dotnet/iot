// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;

namespace Iot.Device.Ili934x
{
    internal enum Ili9341Command : byte
    {
        Nop = 0x00,
        SoftwareReset = 0x01,
        ReadDisplayIdentificationInformation = 0x04,
        ReadDisplayStatus = 0x09,

        ReadDisplayPowerMode = 0x0A,
        ReadDisplayMadControl = 0x0B,
        ReadDisplayPixelFormat = 0x0C,
        ReadDisplayImageFormat = 0x0D, // Python: 0x0A
        ReadDisplaySignalMode = 0x0E,
        ReadDisplaySelfDiagnosticResult = 0x0F,

        EnterSleepMode = 0x10,
        SleepOut = 0x11,
        PartialModeOn = 0x12,
        NormalDisplayModeOn = 0x13,

        DisplayInversionOff = 0x20,
        DisplayInversionOn = 0x21,
        GammaSet = 0x26,
        DisplayOff = 0x28,
        DisplayOn = 0x29,

        ColumnAddressSet = 0x2A,
        PageAddressSet = 0x2B,
        MemoryWrite = 0x2C,
        ColorSet = 0x2D,
        MemoryRead = 0x2E,

        PartialArea = 0x30,
        VerticalScrollingDefinition = 0x33,
        TearingEffectLineOff = 0x34,
        TearingEffectLineOn = 0x35,
        MemoryAccessControl = 0x36,
        VerticalScrollingStartAccess = 0x37,
        IdleModeOff = 0x38,
        IdleModeOn = 0x39,
        ColModPixelFormatSet = 0x3A,
        WriteMemoryContinue = 0x3C,
        ReadMemoryContinue = 0x3E,
        ReadTearScanline = 0x44,
        GetScanline = 0x45,
        WriteDisplayBrightness = 0x51,
        ReadDisplayBrightness = 0x52,
        WriteControlDisplay = 0x53,
        ReadControlDisplay = 0x54,
        WriteContentAdaptiveBrightnessControl = 0x55,
        ReadContentAdaptiveBrightnessControl = 0x56,
        WriteCABCMinimumBrightness = 0x5E,
        ReadCABCMinimumBrighness = 0x5F,
        ReadId1 = 0xDA,
        ReadId2 = 0xDB,
        ReadId3 = 0xDC,
        // ILI9341_RDID4 = 0xDD,
        RgbInterfaceSignalControl = 0xB0,
        FrameRateControlInNormalMode = 0xB1,
        FrameRateControlInIdleMode = 0xB2,
        FrameRateControlInPartialMode = 0xB3,
        DisplayInversionControl = 0xB4,
        BlankingPorchControl = 0xB5,
        DisplayFunctionControl = 0xB6,
        EntryModeSet = 0xB7,
        BacklightControl1 = 0xB8,
        BacklightControl2 = 0xB9,
        BacklightControl3 = 0xBA,
        BacklightControl4 = 0xBB,
        BacklightControl5 = 0xBC,
        BacklightControl7 = 0xBE,
        BacklightControl8 = 0xBF,

        PowerControl1 = 0xC0,
        PowerControl2 = 0xC1,
        // ILI9341_PWCTR3  = 0xC2,
        // ILI9341_PWCTR4  = 0xC3,
        // ILI9341_PWCTR5  = 0xC4,
        VcomControl1 = 0xC5,
        VcomControl2 = 0xC7,

        NvMemoryWrite = 0xD0,
        NvMemoryProtectionKey = 0xD1,
        NvMemoryStatusRead = 0xD2,
        ReadId4 = 0xD3,

        PositiveGammaCorrection = 0xE0,
        NegativeGammaCorrection = 0xE1,
        DigitalGammaControl1 = 0xE2,
        DigitalGammaControl2 = 0xE3,

        InterfaceControl = 0xF6,
        // ILI9341_PWCTR6 = 0xFC,
    }
}
