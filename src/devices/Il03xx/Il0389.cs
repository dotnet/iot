// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device;
using System.Device.Gpio;
using System.Device.Spi;
using System.Drawing;

namespace Iot.Device.Il03xx
{
    public class Il0389 : Il03xx
    {
        public Il0389(SpiDevice spiDevice, int busyPin, int resetPin, int dataCommandPin, Size resolution, IGpioController gpioController = null)
            : base(spiDevice, busyPin, resetPin, dataCommandPin, resolution, gpioController)
        {
            if (resolution.Width > 400 || (resolution.Width & 0b_0111) != 0)
                throw new ArgumentOutOfRangeException(nameof(resolution), "Width must be 400 or less and a multiple of 8.");

            if (resolution.Height > 300)
                throw new ArgumentOutOfRangeException(nameof(resolution), "Height must be 300 or less.");
        }

        protected override byte BitsPerBlackPixel => 1;

        public override void PowerOn()
        {
            Span<byte> data = stackalloc byte[5];
            Reset();

            //  0x03, 0x00, 0x2b, 0x2b, 0x09
            // Phase A (BTPHA) start 10mS, strength 3, 6.58us min off time
            data[0] = 0b_0_00_10_111;
            // Phase B (BTPHB) start 10mS, strength 3, 6.58us min off time
            data[1] = 0b_0_00_10_111;
            // Phase C (BTPHC) start 10mS, strength 3, 6.58us min off time
            data[2] = 0b_0_00_10_111;

            Send(Commands.BoosterSoftStart, data.Slice(0, 3));

            // VDS_EN = 1  : Internal generation of VDH/VDL (drain)
            // VDG_EN = 1  : Internal generation of VGH/VGL (gate)
            data[0] = 0b_000000_1_1;

            // VCOM_HV = 0   : VCOM = VDH+VCOMDC
            // VGHL_LV = 000 : VGH = 16V, VGL = -16V
            // data[1] = 0b_00000_0_00;

            // VDH = 101011 : 11.0V high for black/white pixels
            data[2] = 0b_00_101011;

            // VDL = 101011 : -11.0V low for black/white pixels
            data[3] = 0b_00_101011;

            // VDHR = 4.2V for red pixels
            data[4] = 0b_00_001001;

            Send(Commands.PowerSetting, data);

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

            // RES = 00   : 400 x 300
            // REG_EN = 0 : LUT from OTP
            // BWR = 0    : Three color
            // UD = 1     : Scan up
            // SHL = 1    : Shift right
            // SHD_N = 1  : DC-DC converter on
            // RST_N = 1  : No effect
            data[0] = 0b_00_0_0_1_1_1_1; // 0xCF
            Send(Commands.PanelSetting, data.Slice(0, 1));

            // ResolutionSetting trumps PanelSetting resolution
            // Most significant bit for 9 bit width
            data[0] = (byte)(Width > 0xFF ? 1 : 0);
            data[1] = (byte)Width;
            // Most significant bit for 9 bit height
            data[2] = (byte)(Height > 0xFF ? 1 : 0);
            data[3] = (byte)Height;
            Send(Commands.ResolutionSetting, data.Slice(0, 4));

            // VBD = 01    : Border output LUTW (white)
            // DDX = 11    : Pixels are transmitted as 0 == black / 1 == white
            // CDI = 0111  : Vcom and data interval = 10
            data[0] = 0b_01_11_0111; // 0x17
            Send(Commands.VComAndDataIntervalSetting, data.Slice(0, 1));
        }

        public override void PowerOff()
        {
            base.PowerOff();
            DelayHelper.DelayMilliseconds(100, allowThreadYield: true);

            // 0xa5 is a hard-coded checksum
            Send(Commands.DeepSleep, 0xa5);
            DelayHelper.DelayMilliseconds(2000, allowThreadYield: true);
        }
    }
}
