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
    /// Sent by the plotter to toggle the state of a switch on a CZone board.
    /// </summary>
    public sealed class CzoneCircuitControl : Nmea2000PackedMessage
    {
        private ushort _rawChannel;

        /// <summary>
        /// Hexadecimal identifier for this message
        /// </summary>
        public const int HexId = 0xFF00;

        /// <inheritdoc/>
        public override bool ReplacesOlderInstance => false;

        /// <summary>
        /// Manufacturer code. For this message, this should be Bep Marine 2 (295)
        /// </summary>
        public ManufacturerCode Manufacturer
        {
            get;
        }

        /// <summary>
        /// Industry code. For this message, this should be Marine (4)
        /// </summary>
        public IndustryCode Industry
        {
            get;
        }

        /// <summary>
        /// The offset between reported channel numbers and the actual channel number.
        /// I don't know why this is the case, but when button 1 is pressed, a message about channel 5 is seen.
        /// </summary>
        public ushort ButtonOffset
        {
            get;
            set;
        }

        /// <summary>
        /// What should happen to the switch.
        /// </summary>
        public SwitchStatus NewStatus
        {
            get;
            set;
        }

        /// <summary>
        /// The channel that was toggled. Corrected with the offset, if properly supplied.
        /// </summary>
        public int Channel
        {
            get
            {
                // Can't be negative. Should preferably throw here, but since this
                // can also happen due to corrupted data, we handle it gracefully.
                if (_rawChannel < ButtonOffset)
                {
                    return 0;
                }

                return _rawChannel - ButtonOffset;
            }
        }

        /// <summary>
        /// Creates an instance of this class to trigger a particular switch.
        /// </summary>
        public CzoneCircuitControl(ushort rawchannel, ushort offset, SwitchStatus newStatus)
        {
            // For some reason, I saw this message sending the channel number + 5, meaning
            // when button 1 was pressed, a message about channel 5 is seen.
            _rawChannel = rawchannel;
            ButtonOffset = offset;
            NewStatus = newStatus;
            Manufacturer = ManufacturerCode.BepMarine2;
            Industry = IndustryCode.Marine;
        }

        /// <summary>
        /// Create a message object from a sentence
        /// </summary>
        /// <param name="sentence">The sentence</param>
        /// <param name="time">The current time</param>
        public CzoneCircuitControl(TalkerSentence sentence, DateTimeOffset time)
            : this(sentence.TalkerId, Matches(sentence) ? sentence.Fields : throw new ArgumentException($"SentenceId does not match expected id '{Id}'"), time)
        {
        }

        /// <summary>
        /// Creates a message object from a decoded sentence
        /// </summary>
        /// <param name="talkerId">The source talker id</param>
        /// <param name="fields">The parameters</param>
        /// <param name="time">The current time</param>
        public CzoneCircuitControl(TalkerId talkerId, IEnumerable<string> fields, DateTimeOffset time)
            : base(talkerId, Id, time)
        {
            IEnumerator<string> field = fields.GetEnumerator();

            ParseCommonFields(field);

            string data = ReadString(field);

            if (ReadManufacturerAndIndustryFromHexString(data, 0, out var manufacturer, out var industry))
            {
                Manufacturer = manufacturer;
                Industry = industry;
            }

            if (ReadByteFromHexString(data, 4, out var rawChannel))
            {
                _rawChannel = rawChannel;
            }

            if (ReadByteFromHexString(data, 12, out byte b))
            {
                if (b == 0xF1)
                {
                    NewStatus = SwitchStatus.On;
                }
                else if (b == 0xF2)
                {
                    NewStatus = SwitchStatus.Off;
                }
                else
                {
                    NewStatus = SwitchStatus.NoAction;
                }
            }

            Valid = true;
        }

        /// <inheritdoc/>
        public override string ToNmeaParameterList()
        {
            string manufacturer = WriteManufacturerAndIndustryToHex(Manufacturer, Industry);
            string channel = WriteUshortToHex(_rawChannel);
            string status = NewStatus switch
            {
                SwitchStatus.On => WriteByteToHex(0xF1),
                SwitchStatus.Off => WriteByteToHex(0xF2),
                _ => WriteByteToHex(4),
            };

            return base.ToNmeaParameterList() + manufacturer + channel + "00C8" + status + "00";
        }

        /// <inheritdoc/>
        public override string ToReadableContent()
        {
            return $"Switch control message: Button {Channel} set to {NewStatus}";
        }

        /// <inheritdoc/>
        public override uint Identifier => HexId;
    }
}
