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
        private static readonly Dictionary<int, AutopilotButtons> s_codeToButtonMap;
        private static readonly Dictionary<AutopilotButtons, int> s_buttonToCodeMap;

        static Keystroke()
        {
            s_codeToButtonMap = new Dictionary<int, AutopilotButtons>()
            {
                { 0x01, AutopilotButtons.Auto },
                { 0x02, AutopilotButtons.StandBy },
                { 0x03, AutopilotButtons.MinusTen | AutopilotButtons.PlusTen },
                { 0x04, AutopilotButtons.Disp },
                { 0x05, AutopilotButtons.MinusOne },
                { 0x06, AutopilotButtons.MinusTen },
                { 0x07, AutopilotButtons.PlusOne },
                { 0x08, AutopilotButtons.PlusTen },
                // These two are only sent in auto mode when +1 and -1 are pressed.
                // Whether 0x0A or ox09 is used is possibly indicating whether in deadband mode or not (exact course hold mode).
                // In standby mode, this keypress toggles the display illumination, but does not send out the keystroke command.
                { 0x0A, AutopilotButtons.PlusOne | AutopilotButtons.MinusOne },
                { 0x09, AutopilotButtons.PlusOne | AutopilotButtons.MinusOne },
                { 0x20, AutopilotButtons.PlusOne | AutopilotButtons.MinusOne },
                { 0x21, AutopilotButtons.MinusOne | AutopilotButtons.MinusTen },
                { 0x22, AutopilotButtons.PlusOne | AutopilotButtons.PlusTen },
                { 0x23, AutopilotButtons.StandBy | AutopilotButtons.Auto },
                { 0x28, AutopilotButtons.PlusTen | AutopilotButtons.MinusTen },
                { 0x30, AutopilotButtons.MinusOne | AutopilotButtons.PlusTen },
                { 0x31, AutopilotButtons.MinusTen | AutopilotButtons.PlusOne },
                { 0x41, AutopilotButtons.Auto | AutopilotButtons.LongPress },
                { 0x63, AutopilotButtons.Auto | AutopilotButtons.LongPress | AutopilotButtons.StandBy }
            };

            s_buttonToCodeMap = new Dictionary<AutopilotButtons, int>();
            foreach (var e in s_codeToButtonMap)
            {
                // The above list is not perfectly reversible (see 0x0A and 0x09) therefore use TryAdd
                s_buttonToCodeMap.TryAdd(e.Value, e.Key);
            }
        }

        public Keystroke()
        {
            ButtonsPressed = AutopilotButtons.None;
            Source = 0;
        }

        public Keystroke(AutopilotButtons buttonsToPress, int source = 1)
        {
            ButtonsPressed = buttonsToPress;
            Source = source;
        }

        /// <summary>
        /// The command byte for keypresses is 0x86
        /// </summary>
        public override byte CommandByte => 0x86;

        /// <summary>
        /// This message is always 4 bytes long
        /// </summary>
        public override byte ExpectedLength => 0x04;

        /// <summary>
        /// Keypress source. This is 0 for ST1000+ or ST2000+, 2 for ST4000 or ST600R, and 1 for remote controllers.
        /// So when we send a keypress command to the autopilot, we need to set it to 1
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

        public override byte[] CreateDatagram()
        {
            byte keyCode = 0;
            if (s_buttonToCodeMap.TryGetValue(ButtonsPressed, out int code))
            {
                keyCode = (byte)code;
                byte inverseCode = (byte)~keyCode;
                int attributeByte = ExpectedLength - 3 | (Source << 4);
                return new byte[]
                {
                    CommandByte, (byte)attributeByte, keyCode, inverseCode
                };
            }

            throw new ArgumentException($"The button combination {ButtonsPressed} is not a valid button combination");
        }

        private AutopilotButtons GetButtons(int keyCode)
        {
            if (s_codeToButtonMap.TryGetValue(keyCode, out var result))
            {
                return result;
            }

            Logger.LogWarning($"Unknown keycode {keyCode} in command");
            return AutopilotButtons.None;
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
