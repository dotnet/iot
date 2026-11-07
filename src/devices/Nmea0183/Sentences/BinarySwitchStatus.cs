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
    public sealed class BinarySwitchStatus : Nmea2000PackedMessage
    {
        private readonly int _instance;
        private readonly SwitchStatus[] _switches;

        /// <summary>
        /// Hexadecimal identifier for this message
        /// </summary>
        public const int HexId = 0x01F20D;

        /// <summary>
        /// The number of switches supported by this message. This is the constant 28.
        /// </summary>
        public int MaxNumberOfSwitches => 28;

        /// <summary>
        /// The instance number of this set of switches. Any number between 0 and 252.
        /// </summary>
        public int Instance => _instance;

        /// <inheritdoc/>
        public override bool ReplacesOlderInstance => true;

        /// <summary>
        /// Creates an empty instance of this class. Can be updated later.
        /// </summary>
        /// <param name="instance">Instance number of this switch bank (0-252)</param>
        public BinarySwitchStatus(int instance)
            : this(instance, new Dictionary<int, SwitchStatus>())
        {
        }

        /// <summary>
        /// Create an instance of this class from a list of switches
        /// </summary>
        public BinarySwitchStatus(int instance, Dictionary<int, SwitchStatus> switches)
        {
            _instance = instance;
            _switches = new SwitchStatus[28];
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
        public BinarySwitchStatus(TalkerSentence sentence, DateTimeOffset time)
            : this(sentence.TalkerId, Matches(sentence) ? sentence.Fields : throw new ArgumentException($"SentenceId does not match expected id '{Id}'"), time)
        {
        }

        /// <summary>
        /// Creates a message object from a decoded sentence
        /// </summary>
        /// <param name="talkerId">The source talker id</param>
        /// <param name="fields">The parameters</param>
        /// <param name="time">The current time</param>
        public BinarySwitchStatus(TalkerId talkerId, IEnumerable<string> fields, DateTimeOffset time)
            : base(talkerId, Id, time)
        {
            _switches = new SwitchStatus[28];
            IEnumerator<string> field = fields.GetEnumerator();

            ParseCommonFields(field);

            string data = ReadString(field);

            if (ReadByteFromHexString(data, 0, out byte inst))
            {
                _instance = inst;
            }

            for (int i = 0; i < 7; i++)
            {
                ReadUnsignedFromHexString(data, i + 2, 2, false, out uint v);
                // v now contains the bits for 4 switches
                uint bits = (v >> 6) & 0x3;
                _switches[i * 4] = (SwitchStatus)bits;
                bits = (v >> 4) & 0x3;
                _switches[(i * 4) + 1] = (SwitchStatus)bits;
                bits = (v >> 2) & 0x3;
                _switches[(i * 4) + 2] = (SwitchStatus)bits;
                bits = v & 0x3;
                _switches[(i * 4) + 3] = (SwitchStatus)bits;
            }

            Valid = true;
        }

        /// <inheritdoc/>
        public override string ToNmeaParameterList()
        {
            string instance = Instance.ToString("X2", CultureInfo.InvariantCulture);
            return base.ToNmeaParameterList() + instance + "00000000000000";
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
