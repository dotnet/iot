// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Apds9930
{
    public enum Register : byte
    {
        //Command register modes
        REPEATED_BYTE  = 0x80,
        AUTO_INCREMENT = 0xA0,
        SPECIAL_FN = 0xE0,
    
        //Register addresses
        ENABLE = 0x00,
        ATIME = 0x01,
        PTIME = 0x02,
        WTIME = 0x03,
        AILTL = 0x04,
        AILTH = 0x05,
        AIHTL = 0x06,
        AIHTH = 0x07,
        PILTL = 0x08,
        PILTH = 0x09,
        PIHTL = 0x0A,
        PIHTH = 0x0B,
        PERS  = 0x0C,
        CONFIG = 0x0D,
        PPULSE = 0x0E,
        CONTROL = 0x0F,
        ID = 0x12,
        STATUS = 0x13,
        Ch0DATAL = 0x14,
        Ch0DATAH = 0x15,
        Ch1DATAL = 0x16,
        Ch1DATAH = 0x17,
        PDATAL = 0x18,
        PDATAH  = 0x19,
        POFFSET = 0x1E,

        //Acceptable parameters for setMode
        POWER = 0,
        AMBIENT_LIGHT = 1,
        PROXIMITY = 2,
        WAIT = 3,
        AMBIENT_LIGHT_INT = 4,
        PROXIMITY_INT = 5,
        SLEEP_AFTER_INT = 6,
        ALL = 7,

        //LED Drive values
        LED_DRIVE_100MA = 0,
        LED_DRIVE_50MA = 1,
        LED_DRIVE_25MA = 2,
        LED_DRIVE_12_5MA = 3,

        //Proximity Gain (PGAIN) values
        PGAIN_1X = 0,
        PGAIN_2X = 1,
        PGAIN_4X = 2,
        PGAIN_8X = 3,

        //ALS Gain (AGAIN) values
        AGAIN_1X = 0,
        AGAIN_8X = 1,
        AGAIN_16X = 2,
        AGAIN_120X = 3,

        //Interrupt clear values
        CLEAR_PROX_INT = 0xE5,
        CLEAR_ALS_INT = 0xE6,
        CLEAR_ALL_INTS = 0xE7,

        //Default values
        DEFAULT_ATIME = 0xFF,
        DEFAULT_WTIME = 0xFF,
        DEFAULT_PTIME = 0xFF,
        DEFAULT_PPULSE = 0x08,
        DEFAULT_POFFSET = 0,
        DEFAULT_CONFIG = 0,
        DEFAULT_PDRIVE = LED_DRIVE_100MA,
        DEFAULT_PDIODE = 2,
        DEFAULT_PGAIN = PGAIN_8X,
        DEFAULT_AGAIN = AGAIN_16X,
        DEFAULT_PILT = 0,
        DEFAULT_PIHT = 50,
        DEFAULT_AILT_MSB = 0xFF,
        DEFAULT_AILT_LSB = 0xFF,
        DEFAULT_AIHT = 0,
        DEFAULT_PERS = 0x22        
    }
}