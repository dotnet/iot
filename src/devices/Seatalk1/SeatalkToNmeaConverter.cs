// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using Iot.Device.Common;
using Iot.Device.Nmea0183;
using Iot.Device.Nmea0183.Sentences;
using Iot.Device.Seatalk1.Messages;
using Microsoft.Extensions.Logging;
using UnitsNet;

namespace Iot.Device.Seatalk1
{
    /// <summary>
    /// Seatalk to NMEA-0183 converter.
    /// This subclasses <see cref="NmeaSinkAndSource"/> and hence can be included in a <see cref="MessageRouter"/>, which will make
    /// the Seatalk interface look very similar to an NMEA-0183 interface.
    /// This class embeds the <see cref="SeatalkInterface"/>, so no extra instance is required if this class is used. The class supports translation
    /// of directly wrapped seatalk messages (that start with $STALK) as well as a set of navigation messages (see <see cref="SentencesToTranslate"/>)
    /// </summary>
    public class SeatalkToNmeaConverter : NmeaSinkAndSource
    {
        private readonly List<SentenceId> _sentencesToTranslate;
        private SeatalkInterface _seatalkInterface;
        private bool _isDisposed;
        private ILogger _logger;

        /// <summary>
        /// Construct an instance of this class.
        /// </summary>
        /// <param name="interfaceName">Interface name, for message routing. Can be an arbitrary string.</param>
        /// <param name="portName">Interface to connect to. e.g. "/dev/ttyAMA2"</param>
        public SeatalkToNmeaConverter(string interfaceName, string portName)
            : base(interfaceName)
        {
            _sentencesToTranslate = new();
            _logger = this.GetCurrentClassLogger();
            _seatalkInterface = new SeatalkInterface(portName);
            _seatalkInterface.MessageReceived += SeatalkMessageReceived;
            _isDisposed = false;
        }

        /// <summary>
        /// List of sentences to translate between Seatalk and NMEA.
        /// </summary>
        /// <remarks>
        /// The following sentences are currently supported:
        /// HTD (Seatalk->Nmea)
        /// RSA (Seatalk->Nmea)
        /// MWV (Nmea->Seatalk)
        /// RMB (Nmea->Seatalk)
        /// </remarks>
        public List<SentenceId> SentencesToTranslate => _sentencesToTranslate;

        private void SeatalkMessageReceived(SeatalkMessage stalk)
        {
            var nmeaMsg = new SeatalkNmeaMessage(stalk.CreateDatagram(), DateTimeOffset.UtcNow);
            _logger.LogDebug($"Received Seatalk message: {stalk}");
            DispatchSentenceEvents(nmeaMsg);

            // Translations Seatalk->Nmea
            if (stalk is CompassHeadingAutopilotCourse apStatus)
            {
                if (SentencesToTranslate.Contains(HeadingAndTrackControlStatus.Id) || SentencesToTranslate.Contains(SentenceId.Any))
                {
                    string status = apStatus.AutopilotStatus switch
                    {
                        AutopilotStatus.Standby => "M",
                        AutopilotStatus.Auto => "S",
                        AutopilotStatus.Track => "T",
                        AutopilotStatus.Wind => "W", // This one is just guess
                        _ => "M",
                    };
                    var htd = new HeadingAndTrackControlStatus(status, apStatus.RudderPosition.Abs(), apStatus.RudderPosition > Angle.Zero ? "R" : "L", "N",
                        null, null, null, null, apStatus.AutoPilotCourse, null, apStatus.AutoPilotCourse, false, false, false, apStatus.Alarms != 0, apStatus.CompassHeading);
                    DispatchSentenceEvents(htd);
                }

                if (SentencesToTranslate.Contains(RudderSensorAngle.Id) || SentencesToTranslate.Contains(SentenceId.Any))
                {
                    var angle = apStatus.RudderPosition;
                    var rsa = new RudderSensorAngle(angle, null);
                    DispatchSentenceEvents(rsa);
                }
            }
        }

        /// <summary>
        /// Start receiving data
        /// </summary>
        /// <exception cref="ObjectDisposedException">The instance has already been disposed</exception>
        public override void StartDecode()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(SeatalkToNmeaConverter));
            }

            _seatalkInterface.StartDecode();
        }

        /// <summary>
        /// Send a sentence through the Seatalk port. This ignores unknown or disabled sentences.
        /// </summary>
        /// <param name="source">The source</param>
        /// <param name="sentence">The sentence to send</param>
        /// <remarks>
        /// This sends out a message when it is valid and it is a $STALK message and this instance is not the source. Or when the
        /// message is valid and one of the supported messages to send as specified in <see cref="SentencesToTranslate"/>.
        /// </remarks>
        public override void SendSentence(NmeaSinkAndSource source, NmeaSentence sentence)
        {
            if (sentence.Valid == false)
            {
                return;
            }

            if (sentence.SentenceId == SeatalkNmeaMessage.Id)
            {
                // Since we get all commands we send out back on this interface, we must make sure we're not bouncing those again
                if (sentence is SeatalkNmeaMessage msg && source != this)
                {
                    var data = msg.Datagram;
                    _seatalkInterface.SendDatagram(data);
                }
            }

            if (DoTranslate(sentence, out WindSpeedAndAngle? mwv) && mwv != null)
            {
                ApparentWindAngle awa = new ApparentWindAngle()
                    {
                        ApparentAngle = mwv.Angle
                    };
                _seatalkInterface.SendMessage(awa);

                ApparentWindSpeed aws = new ApparentWindSpeed()
                {
                    ApparentSpeed = mwv.Speed,
                };
                _seatalkInterface.SendMessage(aws);
            }

            if (DoTranslate(sentence, out RecommendedMinimumNavToDestination? rmb) && rmb != null)
            {
                // RMB should always report true bearing.
                NavigationToWaypoint nwp = new NavigationToWaypoint(rmb.CrossTrackError, rmb.BearingToWayPoint, true, rmb.DistanceToWayPoint);
                _seatalkInterface.SendMessage(nwp);
            }
        }

        private bool DoTranslate<T>(NmeaSentence sentence, out T? convertedSentence)
            where T : NmeaSentence
        {
            if (sentence is T converted && (SentencesToTranslate.Contains(sentence.SentenceId) || SentencesToTranslate.Contains(SentenceId.Any)))
            {
                convertedSentence = converted;
                return true;
            }

            convertedSentence = default(T);
            return false;
        }

        /// <summary>
        /// Terminate the parsing
        /// </summary>
        public override void StopDecode()
        {
            _seatalkInterface.Dispose();
            _isDisposed = true;
        }
    }
}
