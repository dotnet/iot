// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Iot.Device.Seatalk1.Messages
{
    public record Keystroke : SeatalkMessage
    {
        public override byte CommandByte => 0x86;
        public override byte ExpectedLength => 0x04;

        /// <summary>
        /// Keypress source. Should probably be an enum, too
        /// </summary>
        public int Source
        {
            get;
            init;
        }

        public AutopilotButtons ButtonsPressed
        {
            get;
            init;
        }

        /// <summary>
        /// The format of the message is
        /// 86  X1  YY  yy, where X is the sender (1 = Remote control, 0 = Autopilot unit)
        /// and YY is the keycode. yy is the binary inverse of YY.
        /// </summary>
        /// <param name="data">The input data</param>
        /// <returns>A message</returns>
        public override SeatalkMessage CreateNewMessage(IReadOnlyList<byte> data)
        {
            VerifyPacket(data);

            int source = data[1] >> 4;

            AutopilotButtons buttons = GetButtons(data[2]);

            return new Keystroke()
            {
                ButtonsPressed = buttons, Source = source,
            };
        }

        private AutopilotButtons GetButtons(int keyCode)
        {
            switch (keyCode)
            {
                case 0x01: return AutopilotButtons.Auto;
                case 0x02: return AutopilotButtons.StandBy;
                case 0x03: return AutopilotButtons.MinusTen | AutopilotButtons.PlusTen;
                case 0x05: return AutopilotButtons.MinusOne;
                case 0x06: return AutopilotButtons.MinusTen;
                case 0x07: return AutopilotButtons.PlusOne;
                case 0x08: return AutopilotButtons.PlusTen;
                // These two are only sent in auto mode when +1 and -1 are pressed, possibly indicating whether in deadband mode or not (exact course hold mode)
                // In standby mode, this keypress toggles the display illumination, but does not send out the keystroke command.
                case 0x0A:
                case 0x09:
                    return AutopilotButtons.PlusOne | AutopilotButtons.MinusOne;
                case 0x20: return AutopilotButtons.PlusOne | AutopilotButtons.MinusOne;
                case 0x21: return AutopilotButtons.MinusOne | AutopilotButtons.MinusTen;
                case 0x22: return AutopilotButtons.PlusOne | AutopilotButtons.PlusTen;
                case 0x23: return AutopilotButtons.StandBy | AutopilotButtons.Auto;
                case 0x28: return AutopilotButtons.PlusTen | AutopilotButtons.MinusTen;
                case 0x30: return AutopilotButtons.MinusOne | AutopilotButtons.PlusTen;
                case 0x31: return AutopilotButtons.MinusTen | AutopilotButtons.PlusOne;

                case 0x41: return AutopilotButtons.Auto | AutopilotButtons.LongPress;
                case 0x63: return AutopilotButtons.Auto | AutopilotButtons.LongPress | AutopilotButtons.StandBy;

                default:
                {
                    Logger.LogWarning($"Unknown keycode 0x{keyCode:X2}!");
                    return AutopilotButtons.None;
                }
            }
        }

        public override bool MatchesMessageType(IReadOnlyList<byte> data)
        {
            // Byte 4 must be the binary inverse of byte 3
            if (data.Count < 4)
            {
                return false;
            }

            byte yy = data[2];
            byte yy2 = data[3];
            if ((~yy2 & 0xFF) != yy)
            {
                return false;
            }

            return base.MatchesMessageType(data);
        }
    }
}
