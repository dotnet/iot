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
    /// <summary>
    /// Keypress messages from/to the autopilot
    /// </summary>
    public record Keystroke : SeatalkMessage
    {
        private static readonly Dictionary<int, AutopilotButtons> s_codeToButtonMap;
        private static readonly Dictionary<AutopilotButtons, int> s_buttonToCodeMap;

        static Keystroke()
        {
            s_codeToButtonMap = new Dictionary<int, AutopilotButtons>()
            {
                { 0x00, AutopilotButtons.None },
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
                { 0x42, AutopilotButtons.StandBy | AutopilotButtons.LongPress },
                { 0x63, AutopilotButtons.Auto | AutopilotButtons.LongPress | AutopilotButtons.StandBy },

                // This one can mean: Return to previous track, but is also sent when Standby is pressed for more than 5 seconds (entering calibration)
                // In the later case, it comes after 0x42
                { 0x68, AutopilotButtons.PlusTen | AutopilotButtons.MinusTen | AutopilotButtons.LongPress },
            };

            s_buttonToCodeMap = new Dictionary<AutopilotButtons, int>();
            foreach (var e in s_codeToButtonMap)
            {
                // The above list is not perfectly reversible (see 0x0A and 0x09) therefore use TryAdd
                s_buttonToCodeMap.TryAdd(e.Value, e.Key);
            }
        }

        /// <summary>
        /// Empty constructor
        /// </summary>
        public Keystroke()
        {
            KeyCode = 0;
            Source = 0;
        }

        /// <summary>
        /// Create a button command
        /// </summary>
        /// <param name="buttonsToPress">Button combination to press</param>
        /// <param name="source">Whether the message is from (0) or to (1) the autopilot</param>
        /// <remarks>When sending a keystroke command to the AP, make sure the source is 1, otherwise it is filtered by the sender,
        /// because the source is the only to way to distinguish own messages from remote messages on the bus.</remarks>
        public Keystroke(AutopilotButtons buttonsToPress, int source = 1)
        {
            Source = source;

            if (s_buttonToCodeMap.TryGetValue(buttonsToPress, out int code))
            {
                KeyCode = code;
            }
            else
            {
                throw new ArgumentException($"The button combination {buttonsToPress} is not a valid button combination");
            }
        }

        /// <summary>
        /// Send a keystroke message, directly with the bytecode
        /// </summary>
        /// <param name="keyCode">Bytecode of the message</param>
        /// <param name="source">Source of the command</param>
        public Keystroke(byte keyCode, int source = 1)
        {
            Source = source;
            KeyCode = keyCode;
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

        /// <summary>
        /// The buttons that where pressed. Note that not all keycodes are valid and autopilots/remote controllers translate some keycodes differently. (e.g.
        /// some remote controllers have a "track" button, while others use a dual-button combination for this)
        /// </summary>
        public AutopilotButtons ButtonsPressed
        {
            get
            {
                return GetButtons(KeyCode);
            }
        }

        /// <summary>
        /// The keycode.
        /// </summary>
        public int KeyCode
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

            // Just as a verification step
            GetButtons(data[2]);

            return new Keystroke()
            {
                Source = source, KeyCode = data[2]
            };
        }

        /// <inheritdoc />
        public override byte[] CreateDatagram()
        {
            byte inverseCode = (byte)~KeyCode;
            int attributeByte = ExpectedLength - 3 | (Source << 4);
            return new byte[]
            {
                CommandByte, (byte)attributeByte, (byte)KeyCode, inverseCode
            };
        }

        private AutopilotButtons GetButtons(int keyCode)
        {
            if (s_codeToButtonMap.TryGetValue(keyCode, out var result))
            {
                return result;
            }

            Logger.LogWarning($"Unknown keycode 0x{keyCode:X2} in command");
            return AutopilotButtons.None;
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        protected override bool PrintMembers(StringBuilder stringBuilder)
        {
            base.PrintMembers(stringBuilder);
            stringBuilder.Append($", Source = {Source}, KeyCode = 0x{KeyCode:X2}, Buttons = {GetButtons(KeyCode)}");
            return true;
        }
    }
}
