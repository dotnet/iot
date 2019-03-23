// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Il03xx
{
    // Commands are marked with the chipsets that support them. Note
    // that data sent for the various commands does vary from chipset
    // to chipset.

    public enum Commands : byte
    {
        // PSR 0x00 (IL0326, 371, 373, 376, 389)
        PanelSetting = 0b_0000_0000,
        // PWR 0x01 (IL0326, 371, 373, 376, 389)
        PowerSetting = 0b_0000_0001,
        // POF 0x02 (IL0326, 371, 373, 376, 389)
        PowerOff = 0b_0000_0010,
        // PFS 0x03 (IL0326, 371, 373, 376, 389)
        PowerOffSequenceSetting = 0b_0000_0011,

        /// <summary>
        /// Powers on the dislpay. (PON)
        /// </summary>
        /// <remarks>
        /// Supported on the (IL0326, 371, 373, 376, 389).
        /// </remarks>
        PowerOn = 0b_0000_0100,

        // PMES 0x05 (IL0326, 373, 376, 389)
        PowerOnMeasure = 0b_0000_0101,
        // BTST 0x06 (IL0326, 373, 376, 389)
        BoosterSoftStart = 0b_0000_0110,
        // DSLP 0x07 (IL0326, 373, 389)
        DeepSleep = 0b_0000_0111,
        // DTM1 0x10 (IL0326, 371, 373, 376, 389)
        DisplayStartTransmission1 = 0b_0001_0000,
        // DSP 0x11 (IL0326, 371, 373, 376, 389)
        DataStop = 0b_0001_0001,
        // DRF 0x12 (IL0326, 371, 373, 376, 389)
        DisplayRefresh = 0b_0001_0010,
        // DTM2 0x13 (IL0326, 373, 376, 389)
        DisplayStartTransmission2 = 0b_0001_0011,
        // DualSPI 0x15 (IL0326)
        DualSpi = 0b_0001_0101,
        // AUTO 0x17 (IL0326)
        AutoSequence = 0b_0001_0111,
        // LUTC1 0x20 (IL373, 376)
        VCom1LookupTable = 0b_0010_0000,
        // LUTW/LUTWW 0x21 (IL373, 376)
        WhiteLookupTable = 0b_0010_0001,
        // LUTB/LUTBW 0x22 (IL373, 376)
        BlackLookupTable = 0b_0010_0010,
        // LUTG1/LUTWB 0x23 (IL373, 376)
        Gray1LookupTable = 0b_0010_0011,
        // LUTG2/LUTBB 0x24 (IL373, 376)
        Gray2LookupTable = 0b_0010_0100,
        // LUTC2 0x25 (IL376)
        VCom2LookupTable = 0b_0010_0101,
        // LUTR0 0x26 (IL376)
        Red0LookupTable = 0b_0010_0110,
        // LUTR1 0x27 (IL376)
        Red1LookupTable = 0b_0010_0111,
        // LUTOPT 0x2a (IL0326)
        LookupTableOption = 0b_0010_1010,
        // KWLUT 0x2b (IL0326)
        KwLookupTableOption = 0b_0010_1011,
        // PLL 0x30 (IL0326, 371, 373, 376, 389)
        PllControl = 0b_0011_0000,
        // TSC 0x40 (IL0326, 371, 373, 376, 389)
        TemperatureSensorCalibration = 0b_0100_0000,
        // TSE 0x41 (IL0326, 371, 373, 376, 389)
        TemperatureSensorSelection = 0b_0100_0001,
        // TSW 0x42 (IL0326, 373, 376, 389)
        TemperatureSensorWrite = 0b_0100_0010,
        // TSR 0x43 (IL0326, 373, 376, 389)
        TemperatureSensorRead = 0b_0100_0011,
        // PBC 0x44 (IL0326)
        PanelBreakCheck = 0b_0100_0100,
        // CDI 0x50 (IL0326, 371, 373, 376, 389)
        VComAndDataIntervalSetting = 0b_0101_0000,
        // LPD 0x51 (IL0326, 371, 376, 389)
        LowPowerDetection = 0b_0101_0001,
        // EVS 0x52 (IL0326)
        EndVoltageSetting = 0b_0101_0010,
        // TCON 0x60 (IL0326, 371, 373, 376, 389)
        TconSetting = 0b_0110_0000,
        // TRES 0x61 (IL0326, 371, 373, 376, 389)
        ResolutionSetting = 0b_0110_0001,
        // GSST 0x65 (IL0326, 371, 389)
        GateSourceStartSetting = 0b_0110_0101,
        // REV 0x70 (IL0326, 371, 373, 376, 389)
        Revision = 0b_0111_0000,
        // FLG 0x71 (IL0326, 371, 373, 376, 389)
        GetStatus = 0b_0111_0001,
        // AMV 0x80 (IL0326, 371, 373, 376, 389)
        AutoMeasurementVcom = 0b_1000_0000,
        // VV 0x81 (IL0326, 371, 373, 376, 389)
        ReadVcomValue = 0b_1000_0001,

        /// <summary>
        /// Sets VCOM (common voltage). (CDI/VDCS)
        /// </summary>
        /// // <remarks>
        /// Supported on the (IL0326, 371, 373, 376, 389).
        /// </remarks>
        VComDcSetting = 0b_1000_0010,

        // PTL 0x90 (IL0326, 373, 389)
        PartialWindow = 0b_1001_0000,
        // PTIN 0x91 (IL0326, 373, 389)
        PartialIn = 0b_1001_0001,
        // PTOUT 0x92 (IL0326, 373, 389)
        PartialOut = 0b_1001_0010,
        // PGM 0xa0 (IL0326, 373, 389)
        ProgramMode = 0b_1010_0000,
        // APG 0xa1 (IL0326, 373, 389)
        ActiveProgram = 0b_1010_0001,
        // ROTP 0xa2 (IL0326, 373, 389)
        ReadOtp = 0b_1010_0010,
        // CCSET 0xe0 (IL0326, 373, 389)
        CascadeSetting = 0b_1110_0000,
        // PWS 0xe3 (IL0326, 389)
        PowerSaving = 0b_1110_0011,
        // LVSEL 0xe4 (IL0326)
        LvdVoltageSelect = 0b_1110_0100,
        // TSSSET 0xe5 (IL0326, 373, 389)
        ForceTemperature = 0b_1110_0101,
        // TSBDRY 0xe7 (IL0326)
        TemperatureBoundaryPhase = 0b_1110_0111
    }
}
