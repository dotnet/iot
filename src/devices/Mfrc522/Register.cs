// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Mfrc522
{
    /// <summary>
    /// Address of register moving all by 1 to the lest as
    /// they are defined like this in the documentation. From documentation, all
    /// registers' name finishes by Reg, removed from the name for clarity.
    /// Some of those registers are not used in the current implementation. They can
    /// be used to enhance it.
    /// As most implementations are using SPI and it is left by 1 bit to the left, we're doing it
    /// upfront
    /// </summary>
    internal enum Register
    {
        // Command and status
        Command = 0x01 << 1,
        ComIEn = 0x02 << 1,
        DivlEn = 0x03 << 1,
        ComIrq = 0x04 << 1,
        DivIrq = 0x05 << 1,
        Error = 0x06 << 1,
        Status1 = 0x07 << 1,
        Status2 = 0x08 << 1,
        FifoData = 0x09 << 1,
        FifoLevel = 0x0A << 1,
        WaterLevel = 0x0B << 1,
        Control = 0x0C << 1,
        BitFraming = 0x0D << 1,
        Coll = 0x0E << 1,
        // Communication
        Mode = 0x11 << 1,
        TxMode = 0x12 << 1,
        RxMode = 0x13 << 1,
        TxControl = 0x14 << 1,
        TxAsk = 0x15 << 1,
        TxSel = 0x16 << 1,
        RxSel = 0x17 << 1,
        RxThreshold = 0x18 << 1,
        Demod = 0x19 << 1,
        MfTx = 0x1C << 1,
        MfRx = 0x1D << 1,
        SerialSpeed = 0x1F << 1,
        // Configuration
        CrcResultHigh = 0x21 << 1,
        CrcResultLow = 0x22 << 1,
        ModeWith = 0x24 << 1,
        RFCfg = 0x26 << 1,
        GsN = 0x27 << 1,
        CWGsP = 0x28 << 1,
        ModGsP = 0x29 << 1,
        TMode = 0x2A << 1,
        TPrescaler = 0x2B << 1,
        TReloadHigh = 0x2C << 1,
        TReloadLow = 0x2D << 1,
        TCounterValueHigh = 0x2E << 1,
        TCounterValueLow = 0x2F << 1,
        // Test
        TestSel1 = 0x31 << 1,
        TestSel2 = 0x32 << 1,
        TestPinEn = 0x33 << 1,
        TestPinValue = 0x34 << 1,
        TestBus = 0x35 << 1,
        AutoTest = 0x36 << 1,
        Version = 0x37 << 1,
        AnalogTest = 0x38 << 1,
        TestDAC1 = 0x39 << 1,
        TestDAC2 = 0x3A << 1,
        TestADC = 0x3B << 1,
    }
}
