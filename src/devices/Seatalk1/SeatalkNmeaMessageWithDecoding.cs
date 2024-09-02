// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Iot.Device.Nmea0183;
using Iot.Device.Nmea0183.Sentences;
using Iot.Device.Seatalk1.Messages;

namespace Iot.Device.Seatalk1
{
    /// <summary>
    /// Similar to it's base class, but transports the original message so that it can be printed or interpreted directly.
    /// </summary>
    /// <remarks>This is necessary because at least in split build mode NMEA classes can't reference Seatalk messages</remarks>
    public class SeatalkNmeaMessageWithDecoding : SeatalkNmeaMessage
    {
        private SeatalkMessage _decodedMessage;

        /// <summary>
        /// Constructs a new instance of this message
        /// </summary>
        /// <param name="datagram">The binary datagram of the seatalk message</param>
        /// <param name="decodedMessage">The decoded seatalk message</param>
        /// <param name="time">The event time</param>
        public SeatalkNmeaMessageWithDecoding(byte[] datagram, SeatalkMessage decodedMessage, DateTimeOffset time)
            : base(datagram, time)
        {
            _decodedMessage = decodedMessage ?? throw new ArgumentNullException(nameof(decodedMessage));
        }

        /// <summary>
        /// Returns the decoded Seatalk message
        /// </summary>
        public SeatalkMessage SourceMessage => _decodedMessage;

        /// <inheritdoc />
        public override string ToReadableContent()
        {
            return _decodedMessage.ToString() ?? base.ToReadableContent();
        }
    }
}
