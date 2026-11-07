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
    /// See also <see cref="CzoneCircuitStatus"/> which is partially redundant
    /// </summary>
    public sealed class CzoneChannelState : Nmea2000PackedMessage
    {
        private readonly byte _dipSwitch;
        private readonly SwitchStatus[] _switches;

        /// <summary>
        /// Hexadecimal identifier for this message
        /// </summary>
        public const int HexId = 0xFF03;

        /// <summary>
        /// The number of switches supported by this message. This is the constant 6.
        /// </summary>
        public int MaxNumberOfSwitches => 6;

        /// <summary>
        /// The Dip switch value for this board. Default for the first board is 0x80.
        /// </summary>
        public byte DipSwitch => _dipSwitch;

        /// <inheritdoc/>
        public override bool ReplacesOlderInstance => true;

        /// <summary>
        /// Manufacturer code. Must be "BepMarine2"
        /// </summary>
        public ManufacturerCode Manufacturer
        {
            get;
            private set;
        }

        /// <summary>
        /// Industry code. Must be "Marine"
        /// </summary>
        public IndustryCode Industry
        {
            get;
            private set;
        }

        /// <summary>
        /// Creates an empty instance of this class. Can be updated later.
        /// </summary>
        /// <param name="dipSwitch">Dip switch value for this board (0-252)</param>
        public CzoneChannelState(byte dipSwitch)
            : this(dipSwitch, new Dictionary<int, SwitchStatus>())
        {
        }

        /// <summary>
        /// Create an instance of this class from a list of switches
        /// </summary>
        public CzoneChannelState(byte dipSwitch, Dictionary<int, SwitchStatus> switches)
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
        public CzoneChannelState(TalkerSentence sentence, DateTimeOffset time)
            : this(sentence.TalkerId, Matches(sentence) ? sentence.Fields : throw new ArgumentException($"SentenceId does not match expected id '{Id}'"), time)
        {
        }

        /// <summary>
        /// Creates a message object from a decoded sentence
        /// </summary>
        /// <param name="talkerId">The source talker id</param>
        /// <param name="fields">The parameters</param>
        /// <param name="time">The current time</param>
        public CzoneChannelState(TalkerId talkerId, IEnumerable<string> fields, DateTimeOffset time)
            : base(talkerId, Id, time)
        {
            _switches = new SwitchStatus[6];
            IEnumerator<string> field = fields.GetEnumerator();

            ParseCommonFields(field);

            string data = ReadString(field);

            if (ReadManufacturerAndIndustryFromHexString(data, 0, out ManufacturerCode manufacturer,
                    out IndustryCode industry))
            {
                Manufacturer = manufacturer;
                Industry = industry;
                if (Manufacturer != ManufacturerCode.BepMarine2 || Industry != IndustryCode.Marine)
                {
                    Valid = false;
                    return;
                }
            }

            if (ReadByteFromHexString(data, 4, out byte b))
            {
                _dipSwitch = b;
            }

            if (ReadByteFromHexString(data, 6, out byte s))
            {
                if ((s & 0x1) != 0)
                {
                    _switches[0] = SwitchStatus.On;
                }

                if ((s & 0x4) != 0)
                {
                    _switches[1] = SwitchStatus.On;
                }

                if ((s & 0x10) != 0)
                {
                    _switches[2] = SwitchStatus.On;
                }

                if ((s & 0x40) != 0)
                {
                    _switches[4] = SwitchStatus.On;
                }
            }

            Valid = true;
        }

        /// <inheritdoc/>
        public override string ToNmeaParameterList()
        {
            string manufacturer = WriteManufacturerAndIndustryToHex(ManufacturerCode.BepMarine2, IndustryCode.Marine);
            string instance = WriteByteToHex(_dipSwitch);
            string channelStates = WriteChannelStates();
            return base.ToNmeaParameterList() + manufacturer + instance + channelStates + "000000E0";
        }

        private string WriteChannelStates()
        {
            // Generates one byte for the state of channels 0-3 (the YDCC-04 which I was able to simulate has only 4 channels)
            byte result = 0;
            if (_switches[0] == SwitchStatus.On)
            {
                // There are 2 bits per channel for the state, but I don't know what the other values mean.
                // Probably this is actually an OFF_ON enumeration (in which case the values 2 and 3 have no
                // known meaning)
                result = 1;
            }

            if (_switches[1] == SwitchStatus.On)
            {
                result |= 4;
            }

            if (_switches[2] == SwitchStatus.On)
            {
                result |= 0x10;
            }

            if (_switches[3] == SwitchStatus.On)
            {
                result |= 0x40;
            }

            return WriteByteToHex(result);
        }

        /// <summary>
        /// Sets the state of a particular switch
        /// </summary>
        public void SetSwitchState(int index, SwitchStatus status)
        {
            _switches[index] = status;
        }

        /// <summary>
        /// Returns the status of the given switch
        /// </summary>
        /// <param name="index">The switch number</param>
        /// <returns>The status of a switch</returns>
        /// <exception cref="IndexOutOfRangeException">Invalid switch index</exception>
        public SwitchStatus GetSwitchState(int index)
        {
            return _switches[index];
        }

        /// <inheritdoc/>
        public override string ToReadableContent()
        {
            StringBuilder s = new StringBuilder("Switch status: ");
            for (int i = 0; i < _switches.Length; i++)
            {
                s.AppendLine($"{i}: {_switches[i]}");
            }

            return s.ToString();
        }

        /// <inheritdoc/>
        public override uint Identifier => HexId;
    }
}
