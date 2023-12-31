// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Iot.Device.Common;
using Iot.Device.Seatalk1.Messages;
using UnitsNet;

namespace Iot.Device.Seatalk1
{
    /// <summary>
    /// Remote controller for an autopilot connected via Seatalk1
    /// </summary>
    /// <remarks>
    /// Type is not disposable, to prevent accidental disposal by clients. They don't get ownership of the instance.
    /// </remarks>
    public class AutoPilotRemoteController
    {
        private const double AngleEpsilon = 1.1; // The protocol can only give angles in whole degrees
        private static readonly TimeSpan MaximumTimeout = TimeSpan.FromSeconds(6);
        private readonly SeatalkInterface _parentInterface;
        private readonly object _lock = new object();
        private DateTime _lastUpdateTime = new DateTime(0);
        private bool _buttonOnApPressed = false;

        internal AutoPilotRemoteController(SeatalkInterface parentInterface)
        {
            _parentInterface = parentInterface ?? throw new ArgumentNullException(nameof(parentInterface));
            _parentInterface.MessageReceived += AutopilotMessageInterpretation;
            RudderAngleAvailable = false;
            Status = AutopilotStatus.Offline;
            Alarms = AutopilotAlarms.None;
            DeadbandMode = DeadbandMode.Automatic;
            DefaultTimeout = TimeSpan.FromSeconds(3);
        }

        public Angle? AutopilotHeading { get; private set; }

        public Angle? AutopilotDesiredHeading { get; private set; }

        public Angle? RudderAngle { get; private set; }

        public bool RudderAngleAvailable { get; private set; }

        public AutopilotStatus Status { get; private set; }

        public AutopilotAlarms Alarms { get; private set; }

        public DeadbandMode DeadbandMode { get; private set; }

        public TimeSpan DefaultTimeout { get; set; }

        public bool IsOperating => Status is AutopilotStatus.Auto or AutopilotStatus.Track or AutopilotStatus.Wind;

        public event Action<Keystroke>? AutopilotKeysPressed;

        private void AutopilotMessageInterpretation(SeatalkMessage obj)
        {
            lock (_lock)
            {
                _lastUpdateTime = DateTime.UtcNow;
                if (obj is CompassHeadingAutopilotCourse ch)
                {
                    Status = ch.AutopilotStatus;
                    AutopilotHeading = ch.CompassHeading;
                    AutopilotDesiredHeading = Status != AutopilotStatus.Standby ? ch.AutoPilotCourse : null;
                    if (!RudderAngleAvailable)
                    {
                        RudderAngleAvailable = !ch.RudderPosition.Equals(Angle.Zero, Angle.FromDegrees(0.5));
                    }

                    if (RudderAngleAvailable)
                    {
                        RudderAngle = ch.RudderPosition;
                    }
                    else
                    {
                        RudderAngle = null;
                    }

                    Alarms = ch.Alarms;
                }
                else if (obj is CompassHeadingAndRudderPosition rb)
                {
                    AutopilotHeading = rb.CompassHeading;
                    if (!RudderAngleAvailable)
                    {
                        RudderAngleAvailable = !rb.RudderPosition.Equals(Angle.Zero, Angle.FromDegrees(0.5));
                    }

                    if (RudderAngleAvailable)
                    {
                        RudderAngle = rb.RudderPosition;
                    }
                    else
                    {
                        RudderAngle = null;
                    }
                }
                else if (obj is DeadbandSetting dbs)
                {
                    DeadbandMode = dbs.Mode;
                }
                else if (obj is Keystroke keystroke)
                {
                    if (keystroke.Source != 1)
                    {
                        _buttonOnApPressed = true;
                    }

                    AutopilotKeysPressed?.Invoke(keystroke);
                }

                if (Status == AutopilotStatus.Standby || Status == AutopilotStatus.Offline)
                {
                    DeadbandMode = DeadbandMode.Automatic; // Resets automatically when going to standby (the message is not sent periodically)
                }
            }
        }

        public bool SetStatus(AutopilotStatus status)
        {
            return SetStatus(status, DefaultTimeout);
        }

        /// <summary>
        /// Attempts to set the Autopilot to the given state.
        /// </summary>
        /// <param name="newStatus">The status to set</param>
        /// <param name="timeout">How long to try. For safety reasons (see remark) the maximum value is limited</param>
        /// <returns>True on success, false if the change didn't succeed within the timeout.</returns>
        /// <remarks>
        /// Don't be tempted to attempt to set the mode to Auto with a large timeout, as this could
        /// override the user's desire to disable the Autopilot and is therefore VERY DANGEROUS!
        /// </remarks>
        public bool SetStatus(AutopilotStatus newStatus, TimeSpan timeout)
        {
            if (Status == newStatus)
            {
                return true; // nothing to do
            }

            AutopilotButtons buttonToPress = newStatus switch
            {
                AutopilotStatus.Auto => AutopilotButtons.Auto,
                AutopilotStatus.Standby => AutopilotButtons.StandBy,
                AutopilotStatus.Track => AutopilotButtons.Track,
                AutopilotStatus.Wind => AutopilotButtons.Auto | AutopilotButtons.StandBy,
                _ => throw new ArgumentException($"Status {newStatus} is not valid", nameof(newStatus)),
            };

            return SendMessageAndVerifyStatus(new Keystroke(buttonToPress, 1), timeout, () => Status == newStatus);
        }

        public bool SetDeadbandMode(DeadbandMode mode)
        {
            if (Status == AutopilotStatus.Offline || Status == AutopilotStatus.Standby)
            {
                // The Deadband mode can only be set in auto mode
                return false;
            }

            if (mode != DeadbandMode.Automatic && mode != DeadbandMode.Minimal)
            {
                throw new ArgumentException("Invalid Deadband mode", nameof(mode));
            }

            if (mode == DeadbandMode)
            {
                return true;
            }

            Keystroke ks;
            // If we are in automatic (default) mode, we need to send 0x0a to enter minimal deadband mode,
            // but if we already are in minimal deadband mode, we need to send 0x09 to leave.
            if (DeadbandMode == DeadbandMode.Automatic)
            {
                ks = new Keystroke(0x0A);
            }
            else
            {
                ks = new Keystroke(0x09);
            }

            return SendMessageAndVerifyStatus(ks, DefaultTimeout, () => DeadbandMode == mode);
        }

        public bool TurnTo(Angle degrees, TurnDirection? direction)
        {
            return TurnTo(degrees, direction, CancellationToken.None);
        }

        public Task<bool> TurnToAsync(Angle degrees, TurnDirection? direction, CancellationToken token)
        {
            return Task.Factory.StartNew(() => TurnTo(degrees, direction, token));
        }

        public Task<bool> TurnByAsync(Angle degrees, TurnDirection direction, CancellationToken token)
        {
            return Task.Factory.StartNew(() => TurnBy(degrees, direction, token));
        }

        public bool TurnTo(Angle degrees, TurnDirection? direction, CancellationToken token)
        {
            degrees = degrees.Normalize(true);

            var currentHeading1 = AutopilotDesiredHeading;
            if (!IsOperating || !currentHeading1.HasValue)
            {
                return false;
            }

            var currentDesiredHeading = currentHeading1.Value;

            if (!direction.HasValue)
            {
                Angle diff = AngleExtensions.Difference(currentDesiredHeading, degrees);
                if (diff < Angle.Zero)
                {
                    direction = TurnDirection.Starboard;
                }
                else
                {
                    direction = TurnDirection.Port;
                }
            }

            while (!AnglesAreClose(currentDesiredHeading, degrees))
            {
                // Should also work if diff is small, but we intend to go the other way (make a full 360)
                Angle diff = AngleExtensions.Difference(currentDesiredHeading, degrees);
                if (diff.Abs() > Angle.FromDegrees(10))
                {
                    SendMessage(new Keystroke(direction == TurnDirection.Starboard ? AutopilotButtons.PlusTen : AutopilotButtons.MinusTen));
                }
                else
                {
                    SendMessage(new Keystroke(direction == TurnDirection.Starboard ? AutopilotButtons.PlusOne : AutopilotButtons.MinusOne));
                }

                token.WaitHandle.WaitOne(TimeSpan.FromSeconds(2));

                if (token.IsCancellationRequested || !IsOperating)
                {
                    break;
                }

                currentHeading1 = AutopilotDesiredHeading;
                if (currentHeading1.HasValue == false)
                {
                    break;
                }

                currentDesiredHeading = currentHeading1.Value;
            }

            return currentHeading1.HasValue && AnglesAreClose(currentHeading1.Value, degrees);
        }

        private bool AnglesAreClose(Angle angle1, Angle angle2)
        {
            if (angle1.Equals(angle2, Angle.FromDegrees(AngleEpsilon)))
            {
                return true;
            }

            if (angle1 >= Angle.FromDegrees(359) && angle2 <= Angle.FromDegrees(1))
            {
                return true;
            }

            if (angle2 >= Angle.FromDegrees(359) && angle1 <= Angle.FromDegrees(1))
            {
                return true;
            }

            return false;
        }

        public bool TurnBy(Angle degrees, TurnDirection direction, CancellationToken token)
        {
            if (degrees < Angle.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(degrees), "Turn amount must be positive. User the direction argument instead of a sign");
            }

            var currentHeading1 = AutopilotDesiredHeading;
            if (!IsOperating || !currentHeading1.HasValue)
            {
                return false;
            }

            Angle target = direction == TurnDirection.Starboard ? currentHeading1.Value + degrees : currentHeading1.Value - degrees;
            target = target.Normalize(true);
            return TurnTo(target, direction, token);
        }

        private bool SendMessage(SeatalkMessage message)
        {
            return _parentInterface.SendMessage(message);
        }

        private bool SendMessageAndVerifyStatus(SeatalkMessage message, TimeSpan timeout, Func<bool> successCondition)
        {
            if (timeout > MaximumTimeout)
            {
                throw new ArgumentOutOfRangeException(nameof(timeout), $"The maximum timeout is {MaximumTimeout}, see remarks in documentation");
            }

            _buttonOnApPressed = false;
            Stopwatch sw = Stopwatch.StartNew();
            while (sw.Elapsed < timeout)
            {
                // If a button on the AP was pressed, abort immediately, as that one must always have precedence.
                if (_buttonOnApPressed)
                {
                    return successCondition();
                }

                if (successCondition())
                {
                    return true;
                }

                _parentInterface.SendMessage(message);
                Thread.Sleep(137); // Some random number
            }

            return successCondition();
        }

        internal void UpdateStatus()
        {
            lock (_lock)
            {
                if (_lastUpdateTime + TimeSpan.FromSeconds(5) < DateTime.UtcNow)
                {
                    // The autopilot hasn't sent anything for 5 seconds. Assume it's offline
                    Status = AutopilotStatus.Offline;
                    DeadbandMode = DeadbandMode.Automatic;
                    RudderAngle = null;
                    AutopilotDesiredHeading = null;
                    AutopilotHeading = null;
                }
            }
        }

        /// <summary>
        /// Returns a textual representation of the current AP status
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string hdg = AutopilotHeading.HasValue ? AutopilotHeading.Value.ToString() : "N/A";
            string desiredHdg = AutopilotDesiredHeading.HasValue ? AutopilotDesiredHeading.Value.ToString() : "N/A";
            string rudderAngle = RudderAngleAvailable ? RudderAngle.GetValueOrDefault().ToString() : "N/A";
            string ret = $"MODE: {Status}; HDG: {hdg}; DESHDG: {desiredHdg}; RUD: {rudderAngle}; DB: {DeadbandMode}; ALRT: {Alarms}";
            return ret;
        }
    }
}
