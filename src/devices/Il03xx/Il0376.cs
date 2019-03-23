// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Gpio;
using System.Device.Spi;
using System.Drawing;

namespace Iot.Device.Il03xx
{
    public class Il0376 : Il03xx
    {
        // Lookup tables have five phases. The first two bytes are two bits for VCOM level select and
        // 6 bits for number of frames. The last byte in each group is a repeat group for the phase.
        //
        //      | 7 | 6 | 5 | 4 | 3 | 2 | 1 | 0 |
        //      | Level |    Number of frames   |
        //      | Level |    Number of frames   |
        //      |        Times to repeat        |

        // VCOM can be set by VComDcSetting command [VDCS].
        // If not explicitly set, VCOM defaults to 0V.

        //  VCOM Level:
        //
        //   00 = VCOM
        //   01 = VCOMH (VCOM + VDPS [15V])
        //   10 = VCOML (VCOM + VDNS [-15V])
        //   11 = Floating

        private static byte[] s_vCom1LookupTable = new byte[]
        {
            // VCOM x14 (0x0e), VCOM x20 (0x14), x1
            0b_00_001110, 0b_00_010100, 0x01,
            // VCOM x10 (0x0a), VCOM x6 (0x06),  x4
            0b_00_001010, 0b_00_000110, 0x04,
            // VCOM x10 (0x0a), VCOM x10 (0x0a), x15
            0b_00_001010, 0b_00_001010, 0x0f,
            // VCOM x3 (0x03),  VCOM x3 (0x03),  x12
            0b_00_000011, 0b_00_000011, 0x0c,
            // VCOM x6 (0x06),  VCOM x10 (0x0a), x0
            0b_10_000110, 0b_00_001010, 0x00
        };

        //  White/black/gray lookup table level:
        //
        //   00 = 0V
        //   01 = VSH (15V)
        //   10 = VSL (-15V)
        //   11 = Floating

        private static byte[] s_whiteLookupTable = new byte[]
        {
            // VCOM x14 (0x0e),  VCOM x20 (0x14),  x1
            0b_00_001110, 0b_00_010100, 0x01,
            // VCOM x10 (0x0a),  VCOMH x6 (0x46),  x4
            0b_00_001010, 0b_01_0000110, 0x04,
            // VCOML x10 (0x8a), VCOMH x10 (0x4a), x15
            0b_10_001010, 0b_01_001010, 0x0f,
            // VCOML x3 (0x83),  VCOMH x3 (0x43),  x12
            0b_10_000011, 0b_01_000011, 0x0c,
            // VCOML x6 (0x86),  VCOM x10 (0x0a),  x4
            0b_10_000110, 0b_00_001010, 0x04
        };

        private static byte[] s_blackLookupTable = new byte[]
        {
            // VCOM x14 (0x0e),  VCOM x20 (0x14),  x1
            0b_00_001110, 0b_00_010100, 0x01,
            // VCOML x10 (0x8a), VCOM x6 (0x06),   x4
            0b_10_001010, 0b_00_000110, 0x04,
            // VCOML x10 (0x8a), VCOMH x10 (0x4a), x15
            0b_10_001010, 0b_01_001010, 0x0f,
            // VCOML x3 (0x83),  VCOMH x3 (0x43),  x12
            0b_10_000011, 0b_01_000011, 0x0c,
            // VCOM x6 (0x06),   VCOMH x10 (0x4a), x4
            0b_00_000110, 0b_01_001010, 0x04
        };

        // Same for Gray1 and Gray2
        private static byte[] s_grayLookupTable = new byte[]
        {
            // VCOML x14 (0x8e), VCOM x20 (0x94),  x1
            0b_10_001110, 0b_10_010100, 0x01,
            // VCOML x10 (0x8a), VCOM x6 (0x06),   x4
            0b_10_001010, 0b_00_000110, 0x04,
            // VCOML x10 (0x8a), VCOMH x10 (0x4a), x15
            0b_10_001010, 0b_01_001010, 0x0f,
            // VCOML x3 (0x83),  VCOMH x3 (0x43),  x12
            0b_10_000011, 0b_01_000011, 0x0c,
            // VCOM x6 (0x06),  VCOM x10 (0x0a),   x4
            0b_00_000110, 0b_00_001010, 0x04
        };

        //  Red lookup table level:
        //
        //   00 = 0V
        //   01 = Red VSH (VDPS_LV as set in PowerSetting command [PWR])
        //   10 = Red VSL (VDNS_LV as set in PowerSetting command [PWR])
        //   11 = Floating

        //  VCom2 Level:
        //
        //   00 = VCOM
        //   01 = Red VSH + VCOM (VDPS_LV as set in PowerSetting command [PWR])
        //   10 = Red VSL + VCOM (VDNS_LV as set in PowerSetting command [PWR])
        //   11 = Floating

        private static byte[] s_vCom2AndRed2LookupTable = new byte[]
        {
            // VCOM x3 (0x03),  VCOM x29 (0x1d), x1
            0b_00_000011, 0b_00_011101, 0x01,
            // VCOM x1 (0x01),  VCOM x8 (0x08),  x35
            0b_00_000001, 0b_00_001000, 0x23,
            // VCOM x55 (0x37), VCOM x55 (0x37), x1
            0b_00_110111, 0b_00_110111, 0x01,
            0x00, 0x00, 0x00,
            0x00, 0x00, 0x00
        };

        private static byte[] s_red0LookupTable = new byte[]
        {
            // VCOML x3 (0x83), VCOMH x29 (0x5d),  x1
            0b_10_000011, 0b_01_011101, 0x01,
            // VCOML x1 (0x81), VCOMH x8 (0x48),  x35
            0b_10_000001, 0b_01_001000, 0x23,
            // VCOMH x55 (0x77),  VCOMH x55 (0x77),  x1
            0b_01_110111, 0b_01_110111, 0x01,
            0x00, 0x00, 0x00,
            0x00, 0x00, 0x00
        };

        public Il0376(SpiDevice spiDevice, int busyPin, int resetPin, int dataCommandPin, Size resolution, IGpioController gpioController = null)
            : base(spiDevice, busyPin, resetPin, dataCommandPin, resolution, gpioController)
        {
            if (resolution.Width > 200 || (resolution.Width & 0x01) != 0)
                throw new ArgumentOutOfRangeException(nameof(resolution), "Width must be 200 or less and even.");

            if (resolution.Height > 300)
                throw new ArgumentOutOfRangeException(nameof(resolution), "Height must be 300 or less.");
        }

        // Pixels are sent as two bit values based on the DDX setting
        // in VComAndDataIntervalSetting. The values specify which
        // lookup table to use when displaying the pixel. The byte
        // format is:
        //
        //   |  px  | px+1 | px+2 | px+3 |
        //   | px+4 | px+5 | px+6 | px+7 | ...
        //
        //   00 = Black lookup table (if DDX == 1, white otherwise)
        //   01 = Gray1 lookup table
        //   10 = Gray2 lookup table
        //   11 = White lookup table (if DDX == 1, black otherwise)
        //
        //
        // For red there is only one bit per pixel, again based off of
        // the DDX setting. If DDX == 1 then a value of 1 means to use
        // the Red1 lookup table, 0 means use Red0. If DDX = 0 this is
        // simply reversed.

        protected override byte BitsPerBlackPixel => 2;

        public override void PowerOn()
        {
            // Same settings for GooDisplay GDEW0154Z04 and WaveShare 1.54" 200x200

            Span<byte> data = stackalloc byte[4];
            Reset();

            // RVSHLS = 01 : VSH 2.4V ~ 8.0V  VSL -15V
            // VDS_EN = 1  : Internal generation of VDH/VDL (drain)
            // VDG_EN = 1  : Internal generation of VGH/VGL (gate)
            data[0] = 0b_0000_01_1_1;

            // VGHL_LV = 000 : VGH = 20V, VGL = -19.3V
            // data[1] = 0b_00000_000

            // VDPS_LV = 01000 : VDPS for red lookup table is 4.0V
            data[2] = 0b_000_01000;

            // VDNS_LV = 00000 : VDNS for red lookup table is -2.4V
            // data[3] = 0b_000_00000;

            Send(Commands.PowerSetting, data);

            // Phase A start 10mS, strength 1, 6.58us min off time
            data[0] = 0b_0_00_00_111;
            // Phase B start 10mS, strength 1, 6.58us min off time
            data[1] = 0b_0_00_00_111;
            // Phase C start 10mS, strength 1, 6.58us min off time
            data[2] = 0b_0_00_00_111;
            Send(Commands.BoosterSoftStart, data.Slice(0, 3));

            Send(Commands.PowerOn, Span<byte>.Empty);

            // Busy pin frees after VGH, VDH, VDL, and VGL get initialized.
            // Specified to take about 80ms.
            WaitForIdle();

            // RES (Not on IL0326)
            // RES    IL0371    IL0373    IL0376    IL0389
            // 00     640x480   96x230    94x230    400x300      (default)
            // 01     600x450   96x252    94x252    320x300
            // 10     640x448   128x296   128x296   320x240
            // 11     600x448   160x296   200x300   200x300

            // RES = 11  : 200 x 300
            // KWR = 0   : Three color
            // UD = 1    : Scan up
            // SHL = 1   : Shift right
            // SHD_N = 1 : DC-DC converter on
            // RST_N = 1 : No effect
            data[0] = 0b_11_0_0_1_1_1_1; // 0xCF
            Send(Commands.PanelSetting, data.Slice(0, 1));

            // SH_BDHZ = 0 : Border output normal voltage (white)
            // DDX = 1     : Pixels are transmitted as 0 == black / 1 == white
            // CDI = 0111  : Vcom and data interval = 10
            data[0] = 0b_00_0_1_0111; // 0x17
            Send(Commands.VComAndDataIntervalSetting, data.Slice(0, 1));

            // M = 111, N = 001
            // Frame rate = 137Hz
            data[0] = 0b_00_111_001; // 0x39
            Send(Commands.PllControl, data.Slice(0, 1));

            // ResolutionSetting trumps PanelSetting resolution
            data[0] = (byte)Width;
            // Most significant bit for 9 bit height
            data[1] = (byte)(Height > 0xFF ? 1 : 0);
            data[2] = (byte)Height;
            Send(Commands.ResolutionSetting, data.Slice(0, 3));

            // VDCS = 01110 : VCOM is -1.6V
            data[0] = 0b_0_0001110;
            Send(Commands.VComDcSetting, data.Slice(0, 1));

            // Set black and white lookup tables
            Send(Commands.VCom1LookupTable, s_vCom1LookupTable);
            Send(Commands.WhiteLookupTable, s_whiteLookupTable);
            Send(Commands.BlackLookupTable, s_blackLookupTable);
            Send(Commands.Gray1LookupTable, s_grayLookupTable);
            Send(Commands.Gray2LookupTable, s_grayLookupTable);

            // Set red lookup tables
            Send(Commands.VCom2LookupTable, s_vCom2AndRed2LookupTable);
            Send(Commands.Red0LookupTable, s_red0LookupTable);
            Send(Commands.Red1LookupTable, s_vCom2AndRed2LookupTable);
        }

        public override void PowerOff()
        {
            Span<byte> data = stackalloc byte[4];

            // SH_BDHZ = 0 : Border output normal voltage
            // DDX = 1     : Internal temperature switch 0 black / 1 white
            // CDI = 0111  : Vcom and data interval = 10
            data[0] = 0b_00_0_1_0111;
            Send(Commands.VComAndDataIntervalSetting, data.Slice(0, 1));

            // VDCS = 000000 : VCOM is 0V
            data[0] = 0b_0_0001110;
            Send(Commands.VComDcSetting, data.Slice(0, 1));  // to solve Vcom drop ????

            // RVSHLS = 00 : VSH 2.4V ~ 8.0V  VSL -2.4 ~ -8.0V
            // VDS_EN = 1  : Internal generation of VDH/VDL (drain)
            // VDG_EN = 0  : External generation of VGH/VGL (gate)
            data[0] = 0b_0000_00_1_0;

            // VGHL_LV = 000 : VGH = 20V, VGL = -19.3V
            // data[1] = 0b_00000_000

            // VDPS_LV = 01000 : VDPS for red lookup table is 4.0V
            // data[2] = 0b_000_00000;

            // VDNS_LV = 00000 : VDNS for red lookup table is -2.4V
            // data[3] = 0b_000_00000;

            Send(Commands.PowerSetting, data);
            WaitForIdle();
            base.PowerOff();
        }
    }
}
