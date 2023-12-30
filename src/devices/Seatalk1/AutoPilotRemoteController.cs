// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
                    return false;
                }

                if (Status == newStatus)
                {
                    return true;
                }

                AutopilotButtons buttonToPress = newStatus switch
                {
                    AutopilotStatus.Auto => AutopilotButtons.Auto,
                    AutopilotStatus.Standby => AutopilotButtons.StandBy,
                    AutopilotStatus.Track => AutopilotButtons.Track,
                    AutopilotStatus.Wind => AutopilotButtons.Auto | AutopilotButtons.StandBy,
                    _ => throw new ArgumentException($"Status {newStatus} is not valid", nameof(newStatus)),
                };

                _parentInterface.SendMessage(new Keystroke(buttonToPress, 1));
                Thread.Sleep(137); // Some random number
            }

            return false;
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
