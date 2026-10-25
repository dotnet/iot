// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Iot.Device.Common;
using Iot.Device.Nmea0183;
using Iot.Device.Nmea0183.Sentences;
using Iot.Device.Seatalk1.Messages;
using Microsoft.Extensions.Logging;
using UnitsNet;

namespace Iot.Device.Seatalk1
{
    /// <summary>
    /// Seatalk to NMEA-0183 converter for autopilot and wind messages.
    /// This class can be used to translate between Seatalk and NMEA-0183 sentences, and to control an autopilot through a NMEA0183 interface.
    /// Together with <see cref="Nmea2000AutopilotEmulator"/> this class can be used to connect a Seatalk autopilot to a NMEA2000 interface.
    /// </summary>
    public class SeatalkToNmeaConverter : NmeaSinkAndSource
    {
        private readonly AutoPilotRemoteController? _autoPilotRemoteController;
        private readonly List<SentenceId> _sentencesToTranslate;
        private bool _isDisposed;
        private BlockingCollection<Action> _sendQueue;
        private Thread? _sendThread;
        private CancellationTokenSource _terminatingCancellationTokenSource;

        /// <summary>
        /// Construct an instance of this class.
        /// </summary>
        /// <param name="interfaceName">Interface name, for message routing. Can be an arbitrary string.</param>
        /// <param name="seatalkInterfaceName">The name of the interface that provides Seatalk1 messages. Can be null if filtering is not required</param>
        /// <param name="autoPilotRemoteController">The autopilot remote controller instance. Can be null if no autopilot is connected to
        /// the Seatalk interface. Should be started before this instance's <see cref="StartDecode"/> is called</param>
        public SeatalkToNmeaConverter(string interfaceName, string? seatalkInterfaceName, AutoPilotRemoteController? autoPilotRemoteController)
            : base(interfaceName)
        {
            _autoPilotRemoteController = autoPilotRemoteController;
            SeatalkInterfaceName = seatalkInterfaceName;
            _terminatingCancellationTokenSource = new CancellationTokenSource();
            _sendQueue = new BlockingCollection<Action>();
            _sentencesToTranslate = new();
            SeatalkInterfaceName = seatalkInterfaceName;
            _isDisposed = false;
            UseRudderAngle = true;
        }

        /// <summary>
        /// Name of the interface that is being observed.
        /// </summary>
        public string? SeatalkInterfaceName { get; }

        /// <summary>
        /// List of sentences to translate between Seatalk and NMEA.
        /// </summary>
        /// <remarks>
        /// The following sentences are currently supported:
        /// HTD (Seatalk->Nmea)
        /// RSA (Seatalk->Nmea)
        /// MWV (Nmea->Seatalk)
        /// RMB (Nmea->Seatalk)
        /// HTC (Nmea->Seatalk, translated into commands)
        /// </remarks>
        public List<SentenceId> SentencesToTranslate => _sentencesToTranslate;

        /// <summary>
        /// True if the rudder angle should be sent as a NMEA sentence (RudderSensorAngle) when a Seatalk autopilot status message is received.
        /// Default is true. This should be set to false if another source is already providing the rudder angle, to avoid duplicates.
        /// </summary>
        public bool UseRudderAngle
        {
            get;
            set;
        }

        private void SeatalkMessageReceived(SeatalkMessage stalk)
        {
            var nmeaMsg = new SeatalkNmeaMessageWithDecoding(stalk.CreateDatagram(), stalk, DateTimeOffset.UtcNow);
            Logger.LogDebug($"Received Seatalk message: {stalk}");
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
                    var htd = new HeadingAndTrackControlStatus(status, null, string.Empty, "N",
                        null, null, null, null, apStatus.AutoPilotCourse, null, apStatus.AutoPilotCourse, false, false, false, apStatus.Alarms != 0, apStatus.CompassHeading);
                    DispatchSentenceEvents(htd);
                }

                if (SentencesToTranslate.Contains(RudderSensorAngle.Id) || SentencesToTranslate.Contains(SentenceId.Any))
                {
                    var angle = apStatus.RudderPosition;
                    if (UseRudderAngle)
                    {
                        var rsa = new RudderSensorAngle(angle, null);
                        DispatchSentenceEvents(rsa);
                    }
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

            _sendThread = new Thread(ProcessSendQueue);
            _sendThread.Start();
        }

        private void ProcessSendQueue()
        {
            try
            {
                while (!_terminatingCancellationTokenSource.IsCancellationRequested)
                {
                    if (_sendQueue.TryTake(out var item, -1, _terminatingCancellationTokenSource.Token))
                    {
                        item();
                    }
                }
            }
            catch (OperationCanceledException x)
            {
                Logger.LogDebug(x, "Send task terminating, caught OperationCancelledException");
            }
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
                if (SeatalkInterfaceName != null && source.InterfaceName != SeatalkInterfaceName)
                {
                    // This is not the interface we are observing, so ignore it
                    return;
                }

                // Since we get all commands we send out back on a Seatalk interface, we must make sure we're not bouncing those again
                if (source == this)
                {
                    return;
                }
            }

            // Translations NMEA->Seatalk
            if (DoTranslate(sentence, out WindSpeedAndAngle? mwv) && mwv != null)
            {
                ApparentWindAngle awa = new ApparentWindAngle()
                    {
                        ApparentAngle = mwv.Angle
                    };
                DispatchSentenceEvents(awa);

                ApparentWindSpeed aws = new ApparentWindSpeed()
                {
                    ApparentSpeed = mwv.Speed,
                };
                DispatchSentenceEvents(aws);
            }

            if (DoTranslate(sentence, out RecommendedMinimumNavToDestination? rmb) && rmb != null)
            {
                // RMB should always report true bearing.
                NavigationToWaypoint nwp = new NavigationToWaypoint(rmb.CrossTrackError, rmb.BearingToWayPoint, true, rmb.DistanceToWayPoint);
                DispatchSentenceEvents(nwp);
            }

            var ap = _autoPilotRemoteController;

            if (DoTranslate(sentence, out HeadingAndTrackControl? htc) && htc != null && ap != null)
            {
                // Empty the queue when this (rare) message arrives, so we can directly issue its commands
                while (_sendQueue.TryTake(out var dispose))
                {
                    // Wait
                }

                Logger.LogInformation($"Received HTC message from {source.InterfaceName}: {htc.ToNmeaMessage()}");

                AutopilotStatus desiredStatus = htc.Status switch
                {
                    "M" => AutopilotStatus.Standby,
                    "S" => AutopilotStatus.Auto,
                    "W" => AutopilotStatus.Wind,
                    "T" => AutopilotStatus.Track,
                    "" => AutopilotStatus.Undefined,
                    _ => AutopilotStatus.Undefined,
                };

                _sendQueue.Add(() =>
                {
                    TurnDirection? confirm = null;

                    if (desiredStatus == AutopilotStatus.Track)
                    {
                        if (!ap.SetStatus(desiredStatus, ref confirm))
                        {
                            ap.SetStatus(desiredStatus, ref confirm);
                        }
                    }
                    else if (desiredStatus != AutopilotStatus.Undefined)
                    {
                        // Don't touch the status if the command has the status parameter unset, meaning "don't touch"
                        ap.SetStatus(desiredStatus, ref confirm);
                    }

                    if (ap.IsOperating && htc.DesiredHeading.HasValue)
                    {
                        ap.TurnTo(htc.DesiredHeading.Value,
                            htc.CommandedTurnDirection == TurnDirection.NoCommand ? null : htc.CommandedTurnDirection);
                    }
                });

                if (ap.DeadbandMode == DeadbandMode.Automatic && htc.OffHeadingLimit.HasValue && htc.OffHeadingLimit.Value.Equals(Angle.Zero, Angle.FromDegrees(0.5)))
                {
                    _sendQueue.Add(() => ap.SetDeadbandMode(DeadbandMode.Minimal));
                }

                if (ap.DeadbandMode == DeadbandMode.Minimal && htc.OffHeadingLimit.HasValue && !htc.OffHeadingLimit.Value.Equals(Angle.Zero, Angle.FromDegrees(0.5)))
                {
                    _sendQueue.Add(() => ap.SetDeadbandMode(DeadbandMode.Automatic));
                }
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
            _terminatingCancellationTokenSource.Cancel();
            if (!_isDisposed)
            {
                if (!_sendQueue.IsAddingCompleted)
                {
                    _sendQueue.CompleteAdding();
                }

                if (_sendThread != null)
                {
                    _sendThread.Join();
                    _sendThread = null;
                }

                _sendQueue.Dispose();
            }

            _isDisposed = true;
        }
    }
}
