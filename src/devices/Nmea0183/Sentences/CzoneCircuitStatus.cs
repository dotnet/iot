// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Iot.Device.Common;

namespace Iot.Device.Nmea0183.Sentences
{
    /// <summary>
    /// Represents the state of an array of on/off switches.
    /// </summary>
    public sealed class CzoneCircuitStatus : Nmea2000PackedMessage
    {
        private readonly byte _dipSwitch;
        private readonly SwitchStatus[] _switches;

        /// <summary>
        /// Hexadecimal identifier for this message
        /// </summary>
        public const int HexId = 0xFF04;

        /// <summary>
        /// The Dip switch value for this board. Default for the first board is 0x80.
        /// </summary>
        public byte DipSwitch => _dipSwitch;

        /// <inheritdoc/>
        public override bool ReplacesOlderInstance => true;

        /// <summary>
        /// Creates an empty instance of this class. Can be updated later.
        /// </summary>
        /// <param name="dipSwitch">Dip switch value for this board (0-252)</param>
        public CzoneCircuitStatus(byte dipSwitch)
            : this(dipSwitch, new Dictionary<int, SwitchStatus>())
        {
        }

        /// <summary>
        /// Create an instance of this class from a list of switches
        /// </summary>
        public CzoneCircuitStatus(byte dipSwitch, Dictionary<int, SwitchStatus> switches)
        {
            _dipSwitch = dipSwitch;
            _switches = new SwitchStatus[6];
            for (int i = 0; i < _switches.Length; i++)
            {
                _switches[i] = SwitchStatus.Off;
            }

            foreach (var s in switches)
            {
                _switches[s.Key] = s.Value;
            }

            Valid = true;
        }

        /// <summary>
        /// Create a message object from a sentence
        /// </summary>
        /// <param name="sentence">The sentence</param>
        /// <param name="time">The current time</param>
        public CzoneCircuitStatus(TalkerSentence sentence, DateTimeOffset time)
            : this(sentence.TalkerId, Matches(sentence) ? sentence.Fields : throw new ArgumentException($"SentenceId does not match expected id '{Id}'"), time)
        {
        }

        /// <summary>
        /// Creates a message object from a decoded sentence
        /// </summary>
        /// <param name="talkerId">The source talker id</param>
        /// <param name="fields">The parameters</param>
        /// <param name="time">The current time</param>
        public CzoneCircuitStatus(TalkerId talkerId, IEnumerable<string> fields, DateTimeOffset time)
            : base(talkerId, Id, time)
        {
            _switches = new SwitchStatus[6];
            IEnumerator<string> field = fields.GetEnumerator();

            ParseCommonFields(field);

            string data = ReadString(field);

            if (ReadByteFromHexString(data, 0, out byte b))
            {
                _dipSwitch = b;
            }

            Valid = true;
        }

        /// <inheritdoc/>
        public override string ToNmeaParameterList()
        {
            string manufacturer = WriteManufacturerAndIndustryToHex(ManufacturerCode.BepMarine2, IndustryCode.Marine);
            string instance = WriteByteToHex(_dipSwitch);
            string circuitStatusType = WriteByteToHex(0x0F);
            byte stateBitmap = 0; // This is a 32-bit value with one bit per channel.
            // This information seems redundant to CzoneChannelState. I don't know yet what the difference is.
            if (_switches[0] == SwitchStatus.On)
            {
                stateBitmap = 1;
            }

            if (_switches[1] == SwitchStatus.On)
            {
                stateBitmap |= 2;
            }

            if (_switches[2] == SwitchStatus.On)
            {
                stateBitmap |= 4;
            }

            if (_switches[3] == SwitchStatus.On)
            {
                stateBitmap |= 8;
            }

            string stateBitmapString = WriteByteToHex(stateBitmap);
            return base.ToNmeaParameterList() + manufacturer + instance + circuitStatusType + stateBitmapString + "000000";
        }

        /// <summary>
        /// Sets the state of a particular switch
        /// </summary>
        public void SetSwitchState(int index, SwitchStatus status)
        {
            _switches[index] = status;
        }

        /// <inheritdoc/>
        public override string ToReadableContent()
        {
            return "Switch status";
        }

        /// <inheritdoc/>
        public override uint Identifier => HexId;
    }
}
