// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Mcp25xxx.Register
{
    /// <summary>
    /// Control Register Summary.
    /// </summary>
    public enum Register : byte
    {
        //
        // Message Transmit Registers.
        //
        /// <summary>
        /// TxnRTS Pin Control and Status Register.
        /// </summary>
        TxRtsCtrl = 0x0D,
        /// <summary>
        /// Transmit Buffer 0 Control Register.
        /// </summary>
        TxB0Ctrl = 0x30,
        /// <summary>
        /// Transmit Buffer 0 Standard Identifier High Register.
        /// </summary>
        TxB0Sidh = 0x31,
        /// <summary>
        /// Transmit Buffer 0 Standard Identifier Low Register.
        /// </summary>
        TxB0Sidl = 0x32,
        /// <summary>
        /// Transmit Buffer 0 Extended Identifier High Register.
        /// </summary>
        TxB0Eid8 = 0x33,
        /// <summary>
        /// Transmit Buffer 0 Extended Identifier Low Register.
        /// </summary>
        TxB0Eid0 = 0x34,
        /// <summary>
        /// Transmit Buffer 0 Data Length Code Register.
        /// </summary>
        TxB0Dlc = 0x35,
        /// <summary>
        /// Transmit Buffer 0 Data Byte 0 Register.
        /// </summary>
        TxB0D0 = 0x36,
        /// <summary>
        /// Transmit Buffer 0 Data Byte 1 Register.
        /// </summary>
        TxB0D1 = 0x37,
        /// <summary>
        /// Transmit Buffer 0 Data Byte 2 Register.
        /// </summary>
        TxB0D2 = 0x38,
        /// <summary>
        /// Transmit Buffer 0 Data Byte 3 Register.
        /// </summary>
        TxB0D3 = 0x39,
        /// <summary>
        /// Transmit Buffer 0 Data Byte 4 Register.
        /// </summary>
        TxB0D4 = 0x3A,
        /// <summary>
        /// Transmit Buffer 0 Data Byte 5 Register.
        /// </summary>
        TxB0D5 = 0x3B,
        /// <summary>
        /// Transmit Buffer 0 Data Byte 6 Register.
        /// </summary>
        TxB0D6 = 0x3C,
        /// <summary>
        /// Transmit Buffer 0 Data Byte 7 Register.
        /// </summary>
        TxB0D7 = 0x3D,
        /// <summary>
        /// Transmit Buffer 1 Control Register.
        /// </summary>
        TxB1Ctrl = 0x40,
        /// <summary>
        /// Transmit Buffer 1 Standard Identifier High Register.
        /// </summary>
        TxB1Sidh = 0x41,
        /// <summary>
        /// Transmit Buffer 1 Standard Identifier Low Register.
        /// </summary>
        TxB1Sidl = 0x42,
        /// <summary>
        /// Transmit Buffer 1 Extended Identifier High Register.
        /// </summary>
        TxB1Eid8 = 0x43,
        /// <summary>
        /// Transmit Buffer 1 Extended Identifier Low Register.
        /// </summary>
        TxB1Eid0 = 0x44,
        /// <summary>
        /// Transmit Buffer 1 Data Length Code Register.
        /// </summary>
        TxB1Dlc = 0x45,
        /// <summary>
        /// Transmit Buffer 1 Data Byte 0 Register.
        /// </summary>
        TxB1D0 = 0x46,
        /// <summary>
        /// Transmit Buffer 1 Data Byte 1 Register.
        /// </summary>
        TxB1D1 = 0x47,
        /// <summary>
        /// Transmit Buffer 1 Data Byte 2 Register.
        /// </summary>
        TxB1D2 = 0x48,
        /// <summary>
        /// Transmit Buffer 1 Data Byte 3 Register.
        /// </summary>
        TxB1D3 = 0x49,
        /// <summary>
        /// Transmit Buffer 1 Data Byte 4 Register.
        /// </summary>
        TxB1D4 = 0x4A,
        /// <summary>
        /// Transmit Buffer 1 Data Byte 5 Register.
        /// </summary>
        TxB1D5 = 0x4B,
        /// <summary>
        /// Transmit Buffer 1 Data Byte 6 Register.
        /// </summary>
        TxB1D6 = 0x4C,
        /// <summary>
        /// Transmit Buffer 1 Data Byte 7 Register.
        /// </summary>
        TxB1D7 = 0x4D,
        /// <summary>
        /// Transmit Buffer 2 Control Register.
        /// </summary>
        TxB2Ctrl = 0x50,
        /// <summary>
        /// Transmit Buffer 2 Standard Identifier High Register.
        /// </summary>
        TxB2Sidh = 0x51,
        /// <summary>
        /// Transmit Buffer 2 Standard Identifier Low Register.
        /// </summary>
        TxB2Sidl = 0x52,
        /// <summary>
        /// Transmit Buffer 2 Extended Identifier High Register.
        /// </summary>
        TxB2Eid8 = 0x53,
        /// <summary>
        /// Transmit Buffer 2 Extended Identifier Low Register.
        /// </summary>
        TxB2Eid0 = 0x54,
        /// <summary>
        /// Transmit Buffer 2 Data Length Code Register.
        /// </summary>
        TxB2Dlc = 0x55,
        /// <summary>
        /// Transmit Buffer 2 Data Byte 0 Register.
        /// </summary>
        TxB2D0 = 0x56,
        /// <summary>
        /// Transmit Buffer 2 Data Byte 1 Register.
        /// </summary>
        TxB2D1 = 0x57,
        /// <summary>
        /// Transmit Buffer 2 Data Byte 2 Register.
        /// </summary>
        TxB2D2 = 0x58,
        /// <summary>
        /// Transmit Buffer 2 Data Byte 3 Register.
        /// </summary>
        TxB2D3 = 0x59,
        /// <summary>
        /// Transmit Buffer 2 Data Byte 4 Register.
        /// </summary>
        TxB2D4 = 0x5A,
        /// <summary>
        /// Transmit Buffer 2 Data Byte 5 Register.
        /// </summary>
        TxB2D5 = 0x5B,
        /// <summary>
        /// Transmit Buffer 2 Data Byte 6 Register.
        /// </summary>
        TxB2D6 = 0x5C,
        /// <summary>
        /// Transmit Buffer 2 Data Byte 7 Register.
        /// </summary>
        TxB2D7 = 0x5D,

        //
        // Message Receive Registers.
        //
        /// <summary>
        /// Receive Buffer 0 Control Register.
        /// </summary>
        RxB0Ctrl = 0x60,
        /// <summary>
        /// Receive Buffer 0 Standard Identifier High Register.
        /// </summary>
        RxB0Sidh = 0x61,
        /// <summary>
        /// Receive Buffer 0 Standard Identifier Low Register.
        /// </summary>
        RxB0Sidl = 0x62,
        /// <summary>
        /// Receive Buffer 0 Extended Identifier High Register.
        /// </summary>
        RxB0Eid8 = 0x63,
        /// <summary>
        /// Receive Buffer 0 Extended Identifier Low Register.
        /// </summary>
        RxB0Eid0 = 0x64,
        /// <summary>
        /// Receive Buffer 0 Data Length Code Register.
        /// </summary>
        RxB0Dlc = 0x65,
        /// <summary>
        /// Receive Buffer 1 Control Register.
        /// </summary>
        RxB1Ctrl = 0x70,
        /// <summary>
        /// Receive Buffer 1 Standard Identifier High Register.
        /// </summary>
        RxB1Sidh = 0x71,
        /// <summary>
        /// Receive Buffer 1 Standard Identifier Low Register.
        /// </summary>
        RxB1Sidl = 0x72,
        /// <summary>
        /// Receive Buffer 1 Extended Identifier High Register.
        /// </summary>
        RxB1Eid8 = 0x73,
        /// <summary>
        /// Receive Buffer 1 Extended Identifier Low Register.
        /// </summary>
        RxB2Eid0 = 0x74,
        /// <summary>
        /// Receive Buffer 1 Data Length Code Register.
        /// </summary>
        RxB1Dlc = 0x75,
        /// <summary>
        /// RxnBF Pin Control and Status Register.
        /// </summary>
        BfbCtrl = 0x0C,

        //
        // Acceptance Filter Registers.
        //
        /// <summary>
        /// Filter 0 Standard Identifier High Register.
        /// </summary>
        RxF0Sidh = 0x00,
        /// <summary>
        /// Filter 0 Standard Identifier Low Register.
        /// </summary>
        RxF0Sidl = 0x01,
        /// <summary>
        /// Filter 0 Extended Identifier High Register.
        /// </summary>
        RxF0Eid8 = 0x02,
        /// <summary>
        /// Filter 0 Extended Identifier Low Register.
        /// </summary>
        RxF0Eid0 = 0x03,
        /// <summary>
        /// Filter 1 Standard Identifier High Register.
        /// </summary>
        RxF1Sidh = 0x04,
        /// <summary>
        /// Filter 1 Standard Identifier Low Register.
        /// </summary>
        RxF1Sidl = 0x05,
        /// <summary>
        /// Filter 1 Extended Identifier High Register.
        /// </summary>
        RxF1Eid8 = 0x06,
        /// <summary>
        /// Filter 1 Extended Identifier Low Register.
        /// </summary>
        RxF1Eid0 = 0x07,
        /// <summary>
        /// Filter 2 Standard Identifier High Register.
        /// </summary>
        RxF2Sidh = 0x08,
        /// <summary>
        /// Filter 2 Standard Identifier Low Register.
        /// </summary>
        RxF2Sidl = 0x09,
        /// <summary>
        /// Filter 2 Extended Identifier High Register.
        /// </summary>
        RxF2Eid8 = 0x0A,
        /// <summary>
        /// Filter 2 Extended Identifier Low Register.
        /// </summary>
        RxF2Eid0 = 0x0B,
        /// <summary>
        /// Filter 3 Standard Identifier High Register.
        /// </summary>
        RxF3Sidh = 0x10,
        /// <summary>
        /// Filter 3 Standard Identifier Low Register.
        /// </summary>
        RxF3Sidl = 0x11,
        /// <summary>
        /// Filter 3 Extended Identifier High Register.
        /// </summary>
        RxF3Eid8 = 0x12,
        /// <summary>
        /// Filter 3 Extended Identifier Low Register.
        /// </summary>
        RxF3Eid0 = 0x13,
        /// <summary>
        /// Filter 4 Standard Identifier High Register.
        /// </summary>
        RxF4Sidh = 0x14,
        /// <summary>
        /// Filter 4 Standard Identifier Low Register.
        /// </summary>
        RxF4Sidl = 0x15,
        /// <summary>
        /// Filter 4 Extended Identifier High Register.
        /// </summary>
        RxF4Eid8 = 0x16,
        /// <summary>
        /// Filter 4 Extended Identifier Low Register.
        /// </summary>
        RxF4Eid0 = 0x17,
        /// <summary>
        /// Filter 5 Standard Identifier High Register.
        /// </summary>
        RxF5Sidh = 0x18,
        /// <summary>
        /// Filter 5 Standard Identifier Low Register.
        /// </summary>
        RxF5Sidl = 0x19,
        /// <summary>
        /// Filter 5 Extended Identifier High Register.
        /// </summary>
        RxF5Eid8 = 0x1A,
        /// <summary>
        /// Filter 5 Extended Identifier Low Register.
        /// </summary>
        RxF5Eid0 = 0x1B,
        /// <summary>
        /// Mask 0 Standard Identifier High Register.
        /// </summary>
        RxM0Sidh = 0x20,
        /// <summary>
        /// Mask 0 Standard Identifier Low Register.
        /// </summary>
        RxM0Sidl = 0x21,
        /// <summary>
        /// Mask 0 Extended Identifier High Register.
        /// </summary>
        RxM0Eid8 = 0x22,
        /// <summary>
        /// Mask 0 Extended Identifier Low Register.
        /// </summary>
        RxM0Eid0 = 0x23,
        /// <summary>
        /// Mask 1 Standard Identifier High Register.
        /// </summary>
        RxM1Sidh = 0x24,
        /// <summary>
        /// Mask 1 Standard Identifier Low Register.
        /// </summary>
        RxM1Sidl = 0x25,
        /// <summary>
        /// Mask 1 Extended Identifier High Register.
        /// </summary>
        RxM1Eid8 = 0x26,
        /// <summary>
        /// Mask 1 Extended Identifier Low Register.
        /// </summary>
        RxM1Eid0 = 0x27,

        //
        // Bit Time Configuration Registers.
        // 
        /// <summary>
        /// Configuration 1 Register.
        /// </summary>
        Cnf1 = 0x2A,
        /// <summary>
        /// Configuration 2 Register.
        /// </summary>
        Cnf2 = 0x29,
        /// <summary>
        /// Configuration 3 Register.
        /// </summary>
        Cnf3 = 0x28,

        //
        // Error Detection Registers.
        // 
        /// <summary>
        /// Transmit Error Counter Register.
        /// </summary>
        Tec = 0x1C,
        /// <summary>
        /// Receiver Error Counter Register.
        /// </summary>
        Rec = 0x1D,
        /// <summary>
        /// Error Flag Register.
        /// </summary>
        Eflg = 0x2D,

        //
        // CAN Control Registers.
        //
        /// <summary>
        /// CAN Status Register.
        /// </summary>
        CanStat = 0x0E,
        /// <summary>
        /// CAN Control Register.
        /// </summary>
        CanCtrl = 0x0F,
    }
}
