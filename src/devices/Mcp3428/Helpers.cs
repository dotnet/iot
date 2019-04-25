// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.Mcp3428
{
    internal static class Helpers
    {
        public static double LSBValue(ResolutionEnum res)
        {
            switch (res)
            {
                case ResolutionEnum.Bit12:
                    return 1e-3;

                case ResolutionEnum.Bit14:
                    return 250e-6;

                case ResolutionEnum.Bit16:
                    return 62.5e-6;

                default:
                    throw new ArgumentOutOfRangeException(nameof(res), res, null);
            }
        }

        /// <summary>
        /// Determine device I2C address based on the configuration pin states. Based on documentation TABLE 5-3-
        /// </summary>
        /// <param name="Adr0">The adr0 pin state</param>
        /// <param name="Adr1">The adr1 pin state</param>
        /// <returns>System.Int32.</returns>
        public static byte I2CAddressFromPins(PinState Adr0, PinState Adr1)
        {
            byte addr = 0b1101000; // Base value from doc

            var idx = (byte)Adr0 << 4 + (byte)Adr1;

            // switch (new ValueTuple<PinState, PinState>(Adr0, Adr1))
            switch (idx)
            {
                case 0:
                case 0x22:
                    break;

                case 0x02:
                    addr += 1;
                    break;

                case 0x01:
                    addr += 2;
                    break;

                case 0x10:
                    addr += 4;
                    break;

                case 0x12:
                    addr += 5;
                    break;

                case 0x11:
                    addr += 6;
                    break;

                case 0x20:
                    addr += 3;
                    break;

                case 0x21:
                    addr += 7;
                    break;

                default:
                    throw new ArgumentException("Invalid combination");
            }

            return addr;
        }

        public static byte SetChannelBits(byte configByte, int channel)
        {
            if (channel > 3 || channel < 0)
                throw new ArgumentException("Channel numbers are only valid 0 to 3", nameof(channel));
            return (byte)((configByte & ~Helpers.Masks.ChannelMask) | ((byte)channel << 5));
        }

        public static byte SetGainBits(byte configByte, GainEnum gain)
        {
            return (byte)((configByte & ~Helpers.Masks.GainMask) | (byte)gain);
        }

        public static byte SetModeBit(byte configByte, ModeEnum mode)
        {
            return (byte)((configByte & ~Helpers.Masks.ModeMask) | (byte)mode);
        }

        public static byte SetReadyBit(byte configByte, bool ready)
        {
            return (byte)(ready ? configByte & ~Helpers.Masks.ReadyMask : configByte | Helpers.Masks.ReadyMask);
        }

        public static byte SetResolutionBits(byte configByte, ResolutionEnum resolution)
        {
            return (byte)((configByte & ~Helpers.Masks.ResolutionMask) | (byte)resolution);
        }

        public static int UpdateFrequency(ResolutionEnum res)
        {
            switch (res)
            {
                case ResolutionEnum.Bit12:
                    return 240;

                case ResolutionEnum.Bit14:
                    return 60;

                case ResolutionEnum.Bit16:
                    return 15;

                default:
                    throw new ArgumentOutOfRangeException(nameof(res), res, null);
            }
        }

        // From datasheet 5.2
        public static class Masks
        {
            public const byte ChannelMask = 0b01100000;
            public const byte GainMask = 0b00000011;
            public const byte ModeMask = 0b00010000;
            public const byte ReadyMask = 0b10000000;
            public const byte ResolutionMask = 0b00001100;
        }
    }
}
