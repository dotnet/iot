// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            return keyCode switch
            {
                0x01 => AutopilotButtons.Auto,
                0x02 => AutopilotButtons.StandBy,
                _ => AutopilotButtons.None,
            };
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
