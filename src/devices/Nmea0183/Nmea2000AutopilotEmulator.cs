// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Reflection.Metadata;
using System.Threading;
using Iot.Device.Common;
using Iot.Device.Nmea0183.Sentences;
using Microsoft.Extensions.Logging;
using UnitsNet;
using UnitsNet.Units;

#pragma warning disable CS1591
namespace Iot.Device.Nmea0183
{
    /// <summary>
    /// Allows translating the input/output of an NMEA0183/Seatalk1 Autopilot to NMEA2000, so that
    /// the pilot can be controlled from there.
    /// This is for cases when the AP is on the NMEA0183 network and the keypad/plotter is NMEA2000.
    /// It currently emulates an EV1-type autopilot from Raymarine. Unfortunately, the protocol messages
    /// to control autopilots are highly manufacturer-dependent, so that this might not work with non-Raymarine
    /// equipment (e.g. a B&amp;G plotter)
    /// </summary>
    public sealed class Nmea2000AutopilotEmulator : NmeaSinkAndSource
    {
        private readonly TimeSpan _defaultMessageTimeout = TimeSpan.FromSeconds(5);
        private readonly SentenceCache _sentencesCache;
        private int _messageCounter;
        private Thread? _updateThread;
        private CancellationTokenSource _cancellationTokenSource;
        private AutopilotStatus _currentAutopilotStatus;
        private Angle _currentAwa;
        private Angle? _currentDesiredAwa;
        private Angle _currentHeading;

        public string? Nmea2000Source { get; }

        public Nmea2000AutopilotEmulator(string interfaceName, string? nmea2000Source)
            : base(interfaceName)
        {
            Nmea2000Source = nmea2000Source;
            _messageCounter = 0;
            _currentAutopilotStatus = AutopilotStatus.Offline;
            _sentencesCache = new SentenceCache(null);
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public override void StartDecode()
        {
            _updateThread = new Thread(Updater);
            _updateThread.Start();
        }

        public override void StopDecode()
        {
            _cancellationTokenSource.Cancel();
            _updateThread?.Join();
            _updateThread = null;
        }

        public override void SendSentence(NmeaSinkAndSource source, NmeaSentence sentence)
        {
            // We received a sentence, check if we can handle it
            if (Nmea2000Source == null || source.InterfaceName == Nmea2000Source)
            {
                // We received something from the NMEA2000 interface.
                if (sentence is GroupFunctionMessage gf)
                {
                    if (gf.Pgn == SeatalkNgPilotStatus.HexId && gf.ParameterConstantsMatch() &&
                        gf.Parameters[0].Constant == 1851 && gf.Function == GroupFunction.Command)
                    {
                        int newMode = gf.Parameters[3].Value.GetValueOrDefault();
                        AutopilotStatus desiredStatus = SeatalkNgPilotStatus.AutopilotStatusFromNumber(newMode);
                        Angle? desiredNewHeading = null;
                        if (newMode == 0xFFFF && _currentAutopilotStatus == AutopilotStatus.Wind)
                        {
                            _sentencesCache.TryGetLastSentence(HeadingAndTrackControlStatus.Id,
                                out HeadingAndTrackControlStatus? st1);
                            // Seen this when a tack is requested. However, the plotter offers the wrong tack
                            // direction right now.
                            // Submode is 4 for tack to starboard and 3 for tack to port. We can use this to determine the correct tack direction.
                            int subMode = gf.Parameters[4].Value.GetValueOrDefault();
                            if (st1 != null && (subMode == 4 || subMode == 3))
                            {
                                var currentAwa = _currentAwa;
                                Angle newAwa;
                                if (currentAwa.Abs().Degrees < 90) // tack
                                {
                                    newAwa = (-currentAwa).Normalize(true);
                                }
                                else
                                {
                                    // gybe
                                    newAwa = (Angle.FromDegrees(360) - currentAwa).Normalize(true);
                                }

                                desiredNewHeading = NewWindAngle(newAwa, _currentDesiredAwa.GetValueOrDefault());
                                _currentDesiredAwa = newAwa;
                            }

                            desiredStatus = AutopilotStatus.Wind;
                        }

                        if (desiredStatus != AutopilotStatus.Undefined)
                        {
                            Logger.LogInformation($"New status was commanded: {desiredStatus}!");
                            var reply = gf.CreateAck();
                            DispatchSentenceEvents(reply);
                            HeadingAndTrackControl control = new HeadingAndTrackControl(
                                HeadingAndTrackControlStatus.FromAutopilotStatus(desiredStatus),
                                null, string.Empty, string.Empty,
                                null, null, null, null,
                                desiredNewHeading, null, null, false);
                            DispatchSentenceEvents(control);
                        }
                        else
                        {
                            Logger.LogWarning($"Unknown Autopilot status value {newMode}");
                        }
                    }
                    else if (gf.Pgn == SeatalkNgPilotLockedHeading.HexId && gf.ParameterConstantsMatch() &&
                                  gf.Parameters[0].Constant == 1851 && gf.Function == GroupFunction.Command)
                    {
                        double newDirection = gf.Parameters[5].Value.GetValueOrDefault(); // New magnetic heading
                        var currentDesiredHeading = Angle.FromRadians(newDirection * 0.0001).ToUnit(AngleUnit.Degree);
                        Logger.LogInformation($"Updated desired heading to {currentDesiredHeading}");
                        var reply = gf.CreateAck();
                        DispatchSentenceEvents(reply);
                        HeadingAndTrackControl control = new HeadingAndTrackControl(string.Empty,
                            null, string.Empty, string.Empty, null, null, null, null, currentDesiredHeading, null, null, false);
                        DispatchSentenceEvents(control);
                    }
                    else if (gf.Pgn == SeatalkNgPilotWindStatus.HexId && gf.ParameterConstantsMatch() &&
                             gf.Parameters[0].Constant == 1851 && gf.Function == GroupFunction.Command)
                    {
                        double newWindAngle = gf.Parameters[3].Value.GetValueOrDefault(); // New wind angle
                        Angle newAwa = Angle.FromRadians(newWindAngle * 0.0001).ToUnit(AngleUnit.Degree).Normalize(true);
                        Angle desiredNewHeading = NewWindAngle(newAwa, _currentDesiredAwa.GetValueOrDefault());
                        _currentDesiredAwa = newAwa;
                        Logger.LogInformation($"Updated desired wind angle to {newAwa} (absolute {desiredNewHeading})");
                        var reply = gf.CreateAck();
                        DispatchSentenceEvents(reply);
                        HeadingAndTrackControl control = new HeadingAndTrackControl(HeadingAndTrackControlStatus.FromAutopilotStatus(AutopilotStatus.Wind),
                            null, string.Empty, string.Empty, null, null, null, null,
                            desiredNewHeading, null, null, false);
                        DispatchSentenceEvents(control);
                    }
                    else if (gf.Pgn == 126720 && gf.ParameterConstantsMatch() && gf.Function == GroupFunction.Request)
                    {
                        int prop = gf.Parameters[3].Value.GetValueOrDefault();
                        int command = gf.Parameters[4].Value.GetValueOrDefault();
                        if (prop == 108 && command == 38)
                        {
                            SeatalkNgPilotConfigurationValue value = new SeatalkNgPilotConfigurationValue()
                            {
                                Command = 38,
                                ProprietaryId = 108,
                                DateTime = DateTimeOffset.UtcNow,
                                Value = false,
                            };

                            DispatchSentenceEvents(value);
                        }
                        else
                        {
                            var reply = gf.CreateNoAck(x => x.Description == "Command" ? 5 : null);
                            DispatchSentenceEvents(reply);
                        }
                    }
                    else
                    {
                        Logger.LogWarning($"Unknown Group function '{gf.Function}' message about {gf.Pgn}");
                    }
                }

                return;
            }

            if (sentence is HeadingAndTrackControlStatus st)
            {
                _currentAutopilotStatus = st.PilotStatus;
                if (_currentAutopilotStatus == AutopilotStatus.Wind)
                {
                    if (!_currentDesiredAwa.HasValue)
                    {
                        _currentDesiredAwa = _currentAwa;
                    }
                }
                else
                {
                    _currentDesiredAwa = null;
                }

                if (st.ActualHeading.HasValue)
                {
                    _currentHeading = st.ActualHeading.Value;
                }

                _sentencesCache.Add(source, st);
            }
            else if (sentence is WindSpeedAndAngle wsa)
            {
                if (wsa.Relative)
                {
                    _currentAwa = wsa.Angle;
                }
            }
            else if (sentence is RudderSensorAngle rud)
            {
                _sentencesCache.Add(source, rud);
            }
            else if (sentence is RecommendedMinimumNavToDestination rmc)
            {
                _sentencesCache.Add(source, rmc);
            }
            else if (sentence is RecommendedMinimumNavigationInformation rmb)
            {
                _sentencesCache.Add(source, rmb);
            }
        }

        private Angle NewWindAngle(Angle newAwa, Angle oldAwa)
        {
            var diff = (newAwa - oldAwa);
            return (_currentHeading + diff).Normalize(true);
        }

        private void Updater()
        {
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                if (_sentencesCache.TryGetLastSentence(HeadingAndTrackControlStatus.Id,
                        out HeadingAndTrackControlStatus? st) && st.Age < _defaultMessageTimeout)
                {
                    // NMEA0183 autopilot status available (and not outdated).
                    // Send out an NMEA2000 autopilot status
                    if (_messageCounter % 4 == 0)
                    {
                        SeatalkNgPilotStatus pilotStatus = new SeatalkNgPilotStatus(st.PilotStatus);
                        DispatchSentenceEvents(pilotStatus);
                    }
                    else if (_messageCounter % 4 == 1)
                    {
                        if (_currentAutopilotStatus == AutopilotStatus.Wind)
                        {
                            SeatalkNgPilotWindStatus windStatus = new SeatalkNgPilotWindStatus(_currentDesiredAwa, _currentAwa);
                            Logger.LogInformation($"Wind status: {windStatus.ToReadableContent()}");
                            DispatchSentenceEvents(windStatus);
                        }

                        SeatalkNgPilotHeading pilotHeading = new SeatalkNgPilotHeading(null, st.ActualHeading);
                        DispatchSentenceEvents(pilotHeading);
                    }
                    else if (_messageCounter % 4 == 2)
                    {
                        SeatalkNgPilotLockedHeading
                            lockedHeading = new SeatalkNgPilotLockedHeading(null, st.DesiredHeading);
                        DispatchSentenceEvents(lockedHeading);
                    }
                    else
                    {
                        Angle? actualRudderAngle = null;
                        Angle? commandedRudderAngle = null;
                        _sentencesCache.TryGetLastSentence(RudderSensorAngle.Id,
                            out RudderSensorAngle? rudderSensorAngle);
                        if (st.CommandedRudderAngle.HasValue)
                        {
                            commandedRudderAngle = st.CommandedRudderAngle.Value;
                        }

                        if (rudderSensorAngle != null && rudderSensorAngle.Age < _defaultMessageTimeout)
                        {
                            actualRudderAngle = rudderSensorAngle.Starboard;
                        }

                        // In the ideal case, both angles are already valid at that point, but with an ST-2000 we have
                        // neither (because the hardware is missing an encoder)
                        if (!commandedRudderAngle.HasValue)
                        {
                            if (_currentAutopilotStatus == AutopilotStatus.Auto ||
                                _currentAutopilotStatus == AutopilotStatus.Wind)
                            {
                                Angle diff = AngleExtensions.Difference(st.DesiredHeading.GetValueOrDefault(),
                                    st.ActualHeading.GetValueOrDefault());
                                commandedRudderAngle = diff;
                            }
                            else
                            {
                                // Use difference between COG and BWP instead (also in track mode!)
                                if (_sentencesCache.TryGetLastSentence(RecommendedMinimumNavToDestination.Id,
                                        out RecommendedMinimumNavToDestination? rmb) && rmb.Age < _defaultMessageTimeout
                                    && _sentencesCache.TryGetLastSentence(RecommendedMinimumNavigationInformation.Id,
                                        out RecommendedMinimumNavigationInformation? rmc) && rmc.Age < _defaultMessageTimeout)
                                {
                                    Angle diff = AngleExtensions.Difference(rmb.BearingToWayPoint.GetValueOrDefault(), rmc.TrackMadeGoodInDegreesTrue);
                                    commandedRudderAngle = diff;
                                }
                            }
                        }

                        // Another option for the actual angle would be to derive it from the turn speed.
                        if (!actualRudderAngle.HasValue)
                        {
                            actualRudderAngle = commandedRudderAngle;
                        }

                        Rudder rudder = new Rudder(actualRudderAngle, commandedRudderAngle, TurnDirection.NoCommand, 0);
                        DispatchSentenceEvents(rudder);
                    }

                    _messageCounter++;
                }

                _cancellationTokenSource.Token.WaitHandle.WaitOne(TimeSpan.FromMilliseconds(150));
            }
        }
    }
}
