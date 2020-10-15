// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Iot.Device.PiJuiceDevice.Models;

namespace Iot.Device.PiJuiceDevice
{
    /// <summary>
    /// PiJuicePower class to support status of the PiJuice
    /// </summary>
    public class PiJuicePower
    {
        private readonly PiJuice _piJuice;

        /// <summary>
        /// PiJuicePower constructor
        /// </summary>
        /// <param name="piJuice">The PiJuice class</param>
        public PiJuicePower(PiJuice piJuice)
        {
            _piJuice = piJuice;
        }

        /// <summary>
        /// Gets the delay before the PiJuice removes power to the GPIO pins
        /// </summary>
        /// <returns>The delay in seconds</returns>
        public byte GetPowerOff()
        {
            var response = _piJuice.ReadCommand(PiJuiceCommand.PowerOff, 1);

            return response[0];
        }

        /// <summary>
        /// Sets the delay before the PiJuice removes power to the GPIO pins
        /// </summary>
        /// <param name="delaySeconds">The delay in seconds between 0 and 255</param>
        public void SetPowerOff(byte delaySeconds)
        {
            if (delaySeconds < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(delaySeconds));
            }

            _piJuice.WriteCommand(PiJuiceCommand.PowerOff, new byte[] { delaySeconds });
        }

        /// <summary>
        /// Get the current state of the Wakeup on charge
        /// </summary>
        /// <returns>Current state of the wake up on charge function</returns>
        public WakeUpOnCharge GetWakeUpOnCharge()
        {
            var response = _piJuice.ReadCommand(PiJuiceCommand.WakeUpOnCharge, 1);

            return new WakeUpOnCharge
            {
                Disabled = response[0] == 0xFF,
                WakeUpPercentage = (short)(response[0] == 0xFF ? 0 : response[0])
            };
        }

        /// <summary>
        /// Wakeup the Raspberry Pi when the battery charge level reaches the specified percentage
        /// </summary>
        /// <param name="wakeUpOnCharge">Wake up on charge function details</param>
        public void SetWakeUpOnCharge(WakeUpOnCharge wakeUpOnCharge)
        {
            if (!wakeUpOnCharge.Disabled && (wakeUpOnCharge.WakeUpPercentage < 0 || wakeUpOnCharge.WakeUpPercentage > 100))
            {
                throw new ArgumentOutOfRangeException(nameof(wakeUpOnCharge.WakeUpPercentage));
            }

            _piJuice.WriteCommandVerify(PiJuiceCommand.WakeUpOnCharge, new byte[] { (byte)(wakeUpOnCharge.Disabled ? 0x7F : wakeUpOnCharge.WakeUpPercentage) });
        }

        /// <summary>
        /// Gets the current watchdog timer time after which it will power cycle if it does not receive a heartbeat signal
        /// </summary>
        /// <returns>Time in minutes after which PiJuice will power cycle if it does not receive a heartbeat signal</returns>
        public TimeSpan GetWatchdogTimer()
        {
            var response = _piJuice.ReadCommand(PiJuiceCommand.WatchdogActiviation, 2);

            return new TimeSpan(0, (response[1] << 8) | response[0], 0);
        }

        /// <summary>
        /// Configure watchdog timer time after which it will power cycle if it does not receive a heartbeat signal
        /// </summary>
        /// <param name="time">Time in minutes after which PiJuice will power cycle if it does not receive a heartbeat signal. Time is between 0 and 65535, 0 disables watchdog timer</param>
        public void SetWatchdogTimer(TimeSpan time)
        {
            if (time.TotalMinutes < 0 || time.TotalMinutes > 65535)
            {
                throw new ArgumentOutOfRangeException(nameof(time.TotalMinutes));
            }

            var minutes = (int)time.TotalMinutes & 0xFFFF;

            _piJuice.WriteCommand(PiJuiceCommand.WatchdogActiviation, new byte[] { (byte)(minutes & 0xFF), (byte)((minutes >> 8) & 0xFF) });
        }

        /// <summary>
        /// Gets the current state of system switch
        /// </summary>
        /// <returns>Current state of system switch</returns>
        public SystemPowerSwitch GetSystemPowerSwitch()
        {
            var response = _piJuice.ReadCommand(PiJuiceCommand.SystemPowerSwitch, 1);

            return (SystemPowerSwitch)(response[0] * 100);
        }

        /// <summary>
        /// Sets the state of the system switch
        /// </summary>
        /// <param name="powerSwitch">Desired current limit in milliampere</param>
        public void SetSystemPowerSwitch(SystemPowerSwitch powerSwitch)
        {
            _piJuice.WriteCommand(PiJuiceCommand.SystemPowerSwitch, new byte[] { (byte)(((int)powerSwitch) / 100) });
        }
    }
}
