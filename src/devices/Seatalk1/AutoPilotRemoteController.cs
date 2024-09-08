// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Iot.Device.Common;
using Iot.Device.Nmea0183.Sentences;
using Iot.Device.Seatalk1.Messages;
using Microsoft.Extensions.Logging;
using UnitsNet;

namespace Iot.Device.Seatalk1
{
    /// <summary>
    /// Remote controller for an autopilot connected via Seatalk1. To get an instance of this class, call <see cref="SeatalkInterface.GetAutopilotRemoteController"/>.
    /// </summary>
    /// <remarks>
    /// Type is not disposable, to prevent accidental disposal by clients. They don't get ownership of the instance.
    /// </remarks>
    public class AutoPilotRemoteController : MarshalByRefObject
    {
        private const double AngleEpsilon = 0.9; // The protocol can only give angles in whole degrees
        private static readonly TimeSpan MaximumTimeout = TimeSpan.FromSeconds(6);
        private readonly SeatalkInterface _parentInterface;
        private readonly object _lock = new object();
        private readonly ILogger _logger;
        private ConcurrentDictionary<AutopilotCalibrationParameter, AutopilotCalibrationParameterMessage> _calibrationParameters;

        private DateTime _lastUpdateTime = new DateTime(0);
        private bool _buttonOnApPressed = false;

        /// <summary>
        /// Internal constructor, used by the owning instance of <see cref="SeatalkInterface"/>
        /// </summary>
        /// <param name="parentInterface">The owner</param>
        internal AutoPilotRemoteController(SeatalkInterface parentInterface)
        {
            _logger = this.GetCurrentClassLogger();
            _calibrationParameters = new();
            _parentInterface = parentInterface ?? throw new ArgumentNullException(nameof(parentInterface));
            _parentInterface.MessageReceived += AutopilotMessageInterpretation;
            RudderAngleAvailable = false;
            Status = AutopilotStatus.Offline;
            Alarms = AutopilotAlarms.None;
            DeadbandMode = DeadbandMode.Automatic;
            DefaultTimeout = TimeSpan.FromSeconds(3);
        }

        /// <summary>
        /// Current value of the heading sensor internal to the autopilot (if present).
        /// When the autopilot controller has such a sensor, this value is always available, however it is
        /// useless on tiler pilots such as ST2000 in standby mode, because that one needs to be removed from the tiller for manual steering and thus
        /// will read random values.
        /// </summary>
        public Angle? AutopilotHeading { get; private set; }

        /// <summary>
        /// Desired heading of the autopilot.
        /// This value is only valid when the autopilot is active (not standby)
        /// </summary>
        public Angle? AutopilotDesiredHeading { get; private set; }

        /// <summary>
        /// Current rudder angle. Positive for turning to starboard.
        /// This value is only available when a rudder position sensor is fitted.
        /// </summary>
        public Angle? RudderAngle { get; private set; }

        /// <summary>
        /// True when the rudder angle is valid. This stays false when either no rudder sensor is fitted or the reported values are apparently wrong.
        /// </summary>
        public bool RudderAngleAvailable { get; private set; }

        /// <summary>
        /// Current status of the autopilot
        /// </summary>
        public AutopilotStatus Status { get; private set; }

        /// <summary>
        /// Active autopilot alarms
        /// </summary>
        public AutopilotAlarms Alarms { get; private set; }

        /// <summary>
        /// Current deadband mode. The value is only meaningful in auto mode
        /// </summary>
        public DeadbandMode DeadbandMode { get; private set; }

        /// <summary>
        /// Type of autopilot controller. Known values: 05 for 150G type, 08 for most other types
        /// </summary>
        public int AutopilotType { get; private set; }

        /// <summary>
        /// Timeout for changing the status
        /// </summary>
        public TimeSpan DefaultTimeout { get; set; }

        /// <summary>
        /// Current turning direction of the boat. Depending on the type, this may be derived from heading changes and therefore not really reliable.
        /// </summary>
        public TurnDirection TurnDirection { get; private set; }

        /// <summary>
        /// True if the autopilot is active
        /// </summary>
        public bool IsOperating => Status is AutopilotStatus.Auto or AutopilotStatus.Track or AutopilotStatus.Wind;

        /// <summary>
        /// True if the autopilot is in Standby
        /// </summary>
        public bool IsStandby => Status is AutopilotStatus.Standby or AutopilotStatus.InactiveTrack or AutopilotStatus.InactiveWind;

        /// <summary>
        /// Warning flags from the course computer
        /// </summary>
        public CourseComputerWarnings CourseComputerStatus { get; private set; }

        /// <summary>
        /// This event is fired when keys on the controller are pressed.
        /// </summary>
        public event Action<Keystroke>? AutopilotKeysPressed;

        private void AutopilotMessageInterpretation(SeatalkMessage obj)
        {
            lock (_lock)
            {
                if (obj is CompassHeadingAutopilotCourse ch)
                {
                    // Only update this when we see an autopilot message, otherwise we declare the autopilot as online
                    // if all we see is the echo of some of the repeating messages we send out.
                    _lastUpdateTime = DateTime.UtcNow;
                    Status = ch.AutopilotStatus;
                    AutopilotHeading = ch.CompassHeading;
                    AutopilotDesiredHeading = !IsStandby ? ch.AutoPilotCourse : null;
                    AutopilotType = ch.AutoPilotType;
                    TurnDirection = ch.TurnDirection;
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
                    TurnDirection = rb.TurnDirection;
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
                else if (obj is AutopilotCalibrationParameterMessage calibParam)
                {
                    _logger.LogInformation($"Received calibration value for {calibParam.Parameter}. Current {calibParam.CurrentSetting} Min {calibParam.MinValue} Max {calibParam.MaxValue}");
                    _calibrationParameters.AddOrUpdate(calibParam.Parameter, x => calibParam, (a, b) => calibParam);
                }
                else if (obj is CourseComputerStatus status)
                {
                    _logger.LogWarning($"Course computer has warnings: {status.Warnings}");
                    CourseComputerStatus = status.Warnings;
                }

                if (IsStandby || Status == AutopilotStatus.Offline)
                {
                    DeadbandMode = DeadbandMode.Automatic; // Resets automatically when going to standby (the message is not sent periodically)
                    CourseComputerStatus = CourseComputerWarnings.None;
                }
            }
        }

        /// <summary>
        /// Change the autopilot status.
        /// </summary>
        /// <param name="status">The new desired status</param>
        /// <param name="directionConfirmation">When changing to track mode, confirm a turn in this direction.</param>
        /// <returns>True if the desired mode was reached, false if not. Reasons that this could fail are: Operation cancelled by user
        /// (user pressed standby while remote control was trying to switch to auto); No data available for change to wind or track mode</returns>
        /// <remarks>
        /// <paramref name="directionConfirmation"/> should initially be set to null. When requesting a change to track mode, it will return a value
        /// indicating the required direction change to return to the track. Then call the method again with that value to confirm the change.
        /// </remarks>
        public bool SetStatus(AutopilotStatus status, ref TurnDirection? directionConfirmation)
        {
            return SetStatus(status, DefaultTimeout, ref directionConfirmation);
        }

        /// <summary>
        /// Returns the currently known parameter values.
        /// Note: The values can currently only be read out by manually entering the calibration mode once.
        /// </summary>
        /// <returns>A list of parameter values</returns>
        public List<AutopilotCalibrationParameterMessage> GetCalibrationParameters()
        {
            return _calibrationParameters.Values.ToList();
        }

        /// <summary>
        /// Attempts to set the Autopilot to the given state.
        /// </summary>
        /// <param name="newStatus">The status to set</param>
        /// <param name="timeout">How long to try. For safety reasons (see remark) the maximum value is limited</param>
        /// <param name="directionConfirmation">If entering track mode (<paramref name="newStatus"/>==<see cref="AutopilotStatus.Track"/>), confirm a turn
        /// in the given direction. If left null, will return the required direction for the turn. The method has then to be called again with the parameter set</param>
        /// <returns>True on success, false if the change didn't succeed within the timeout. Also returns false if a change to track mode wasn't confirmed yet.</returns>
        /// <remarks>
        /// Don't be tempted to attempt to set the mode to Auto with a large timeout, as this could
        /// override the user's desire to disable the Autopilot and is therefore VERY DANGEROUS!
        /// </remarks>
        public bool SetStatus(AutopilotStatus newStatus, TimeSpan timeout, ref TurnDirection? directionConfirmation)
        {
            if (Status == newStatus && CourseComputerStatus == 0)
            {
                _logger.LogInformation("Not setting status {NewStatus} because already set.", newStatus);
                return true; // nothing to do
            }

            if (Status == AutopilotStatus.Offline)
            {
                return false;
            }

            AutopilotButtons buttonToPress = newStatus switch
            {
                AutopilotStatus.Auto => AutopilotButtons.Auto,
                AutopilotStatus.Standby => AutopilotButtons.StandBy,
                AutopilotStatus.Track => AutopilotButtons.PlusTen | AutopilotButtons.MinusTen,
                AutopilotStatus.Wind => AutopilotButtons.Auto | AutopilotButtons.StandBy,
                _ => throw new ArgumentException($"Status {newStatus} is not valid", nameof(newStatus)),
            };

            _logger.LogInformation("Setting status {Status} by pressing button(s) {Button}", newStatus, buttonToPress);

            // For setting wind or track modes, we need to first set auto mode.
            // Setting wind mode without auto works (and is returned as status 0x4), but has no visible effect.
            if (newStatus == AutopilotStatus.Wind || newStatus == AutopilotStatus.Track)
            {
                if (!SendMessageAndVerifyStatus(new Keystroke(AutopilotButtons.Auto, 1), timeout, () => Status is AutopilotStatus.Auto or AutopilotStatus.Wind or AutopilotStatus.Track))
                {
                    _logger.LogInformation($"Could not transition from Auto to {newStatus} mode");
                    return false;
                }
            }

            bool ret = SendMessageAndVerifyStatus(new Keystroke(buttonToPress, 1), timeout, () => (Status == newStatus) || ((newStatus == AutopilotStatus.Standby) && IsStandby));

            if (ret && newStatus == AutopilotStatus.Track)
            {
                // Wait until the AP reports a course computer warning - and then just confirm it.
                Stopwatch sw = Stopwatch.StartNew();
                // This needs a longer timeout, since it can take several seconds for the confirmation to appear
                while (sw.Elapsed < TimeSpan.FromSeconds(5))
                {
                    if ((CourseComputerStatus & CourseComputerWarnings.DriveFailure) != 0)
                    {
                        TurnDirection directionRequired =
                            (CourseComputerStatus & CourseComputerWarnings.CourseChangeToPort) == CourseComputerWarnings.CourseChangeToPort ? TurnDirection.Port : TurnDirection.Starboard;
                        if (directionConfirmation == null)
                        {
                            directionConfirmation = directionRequired;
                            return false;
                        }

                        if (directionRequired == directionConfirmation &&
                            SendMessageAndVerifyStatus(new Keystroke(0x28, 1), timeout,
                                () => Status == AutopilotStatus.Track))
                        {
                            CourseComputerStatus = 0; // Apparently, the clearance of this warning is not sent
                            break;
                        }
                    }

                    Thread.Sleep(150);
                }

                ret = Status == AutopilotStatus.Track && CourseComputerStatus == CourseComputerWarnings.None;
            }

            if (ret)
            {
                _logger.LogInformation($"Status {Status} set successfully");
            }
            else
            {
                _logger.LogError("Status was not set correctly");
            }

            return ret;
        }

        /// <summary>
        /// Change deadband mode
        /// </summary>
        /// <param name="mode">The new mode</param>
        /// <returns>True if the value was set, false otherwise. This method fails if the autopilot is in standby mode</returns>
        public bool SetDeadbandMode(DeadbandMode mode)
        {
            if (Status == AutopilotStatus.Offline || IsStandby)
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

        /// <summary>
        /// Turns to the desired heading
        /// </summary>
        /// <param name="degrees">New desired heading</param>
        /// <param name="direction">Desired turn direction. Pass null for taking the default (shorter) turn.</param>
        /// <returns>True on success, false otherwise</returns>
        /// <remarks>This operation can take a significant time</remarks>
        public bool TurnTo(Angle degrees, TurnDirection? direction)
        {
            return TurnTo(degrees, direction, CancellationToken.None);
        }

        /// <summary>
        /// Same as <see cref="TurnTo(UnitsNet.Angle,System.Nullable{Iot.Device.Seatalk1.Messages.TurnDirection})"/>, except in async mode
        /// </summary>
        /// <param name="degrees">New desired heading</param>
        /// <param name="direction">Desired turn direction</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>True on success, false otherwise</returns>
        public Task<bool> TurnToAsync(Angle degrees, TurnDirection? direction, CancellationToken token)
        {
            return Task.Factory.StartNew(() => TurnTo(degrees, direction, token));
        }

        /// <summary>
        /// Turn by a certain value
        /// </summary>
        /// <param name="degrees">Degrees to turn, unsigned</param>
        /// <param name="direction">Direction to turn</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>True on success, false otherwise</returns>
        public Task<bool> TurnByAsync(Angle degrees, TurnDirection direction, CancellationToken token)
        {
            return Task.Factory.StartNew(() => TurnBy(degrees, direction, token));
        }

        /// <summary>
        /// Turns to the desired heading
        /// </summary>
        /// <param name="degrees">New desired heading</param>
        /// <param name="direction">Desired turn direction. Pass null for taking the default (shorter) turn.</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>True on success, false otherwise</returns>
        /// <remarks>This operation can take a significant time</remarks>
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

            _logger.LogInformation($"New desired heading: {degrees}");

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

                if (token.IsCancellationRequested)
                {
                    _logger.LogWarning("Turning cancelled because operation was externally aborted");
                    break;
                }

                if (!IsOperating)
                {
                    _logger.LogWarning($"Turning cancelled because Autopilot is no longer in the correct state. Current State {Status}");
                    break;
                }

                currentHeading1 = AutopilotDesiredHeading;
                if (currentHeading1.HasValue == false)
                {
                    break;
                }

                currentDesiredHeading = currentHeading1.Value;
            }

            bool ret = currentHeading1.HasValue && AnglesAreClose(currentHeading1.Value, degrees);
            if (ret)
            {
                _logger.LogInformation($"Reached new desired course {currentHeading1.GetValueOrDefault()}");
            }
            else
            {
                _logger.LogWarning($"TurnTo terminated prematurely, desired new heading not reached");
            }

            return ret;
        }

        internal bool AnglesAreClose(Angle angle1, Angle angle2)
        {
            if (angle1.Equals(angle2, Angle.FromDegrees(AngleEpsilon)))
            {
                return true;
            }

            return UnitMath.Abs(AngleExtensions.Difference(angle1, angle2)) < Angle.FromDegrees(1.5);
        }

        /// <summary>
        /// Turn by a certain value
        /// </summary>
        /// <param name="degrees">Degrees to turn, unsigned</param>
        /// <param name="direction">Direction to turn</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>True on success, false otherwise</returns>
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
                throw new ArgumentOutOfRangeException(nameof(timeout), $"The maximum timeout is {MaximumTimeout}, see remarks on AutoPilotRemoteController.SetStatus in the documentation");
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
                // Netstandard 2.0 doesn't support the multiply operator on timespan
                TimeSpan twice = new TimeSpan(DefaultTimeout.Ticks * 2);
                if (_lastUpdateTime + twice < DateTime.UtcNow)
                {
                    // The autopilot hasn't sent anything for several seconds. Assume it's offline
                    if (Status != AutopilotStatus.Offline) // don't repeat message
                    {
                        _logger.LogWarning("Autopilot connection timed out. Assuming it's offline");
                    }

                    Status = AutopilotStatus.Offline;
                    DeadbandMode = DeadbandMode.Automatic;
                    RudderAngle = null;
                    AutopilotDesiredHeading = null;
                    AutopilotHeading = null;
                    CourseComputerStatus = CourseComputerWarnings.None;
                }
            }
        }

        /* Unfortunately, the exact sequence to remotely enter calibration mode appears to be different for each type of remote control and autopilot
            I have not found out how it works for the ST2000+. I also have not found out how to read out parameter values without entering calibration mode
        public bool ReadCalibrationValues()
        {
            if (Status != AutopilotStatus.Standby)
            {
                return false;
            }

            _calibrationParameters.Clear();

            Keystroke ks;

            Thread.Sleep(100);
            ks = new Keystroke(0x42);
            SendMessage(ks);
            Thread.Sleep(200);
            ks = new Keystroke(0x68);
            SendMessage(ks);
            Thread.Sleep(500);
            ks = new Keystroke(0x84);
            SendMessage(ks);
            Thread.Sleep(500);

            ks = new Keystroke(0x04);
            SendMessage(ks);

            ks = new Keystroke(AutopilotButtons.MinusOne | AutopilotButtons.PlusOne);
            SendMessage(ks);

            Stopwatch sw = Stopwatch.StartNew();
            int currentEntries = _calibrationParameters.Count;
            while (sw.ElapsedMilliseconds < 3500)
            {
                ks = new Keystroke(AutopilotButtons.Auto);
                SendMessage(ks);
                _parentInterface.SendDatagram(new byte[]
                {
                    0x92, 1, 1
                });
                Thread.Sleep(1500);
                if (currentEntries != _calibrationParameters.Count)
                {
                    currentEntries = _calibrationParameters.Count;
                    sw.Restart();
                }
            }

            ks = new Keystroke(AutopilotButtons.StandBy);
            SendMessage(ks);
            return true;
        }*/

        /// <summary>
        /// Returns a textual representation of the current AP status
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string hdg = AutopilotHeading.HasValue ? AutopilotHeading.Value.ToString() : "N/A";
            string desiredHdg = AutopilotDesiredHeading.HasValue ? AutopilotDesiredHeading.Value.ToString() : "N/A";
            string rudderAngle = RudderAngleAvailable ? RudderAngle.GetValueOrDefault().ToString() : "N/A";
            string ret = $"MODE:{Status}; HDG:{hdg}; DES:{desiredHdg}; RUD:{rudderAngle}; DB:{DeadbandMode}; ALRT:{Alarms}; TD:{TurnDirection}";
            return ret;
        }
    }
}
