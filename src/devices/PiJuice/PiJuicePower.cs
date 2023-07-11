// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Buffers.Binary;
using Iot.Device.PiJuiceDevice.Models;
using UnitsNet;

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
        /// The delay before the PiJuice removes power to the GPIO pins
        /// </summary>
        /// <value>The delay in seconds</value>
        public TimeSpan PowerOff
        {
            get
            {
                var response = _piJuice.ReadCommand(PiJuiceCommand.PowerOff, 1);

                return new TimeSpan(0, 0, response[0]);
            }
            set
            {
                if (value.TotalSeconds < 0 || value.TotalSeconds > 255)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "TotalSeconds must be between 0 and 255.");
                }

                _piJuice.WriteCommand(PiJuiceCommand.PowerOff, new byte[] { (byte)value.TotalSeconds });
            }
        }

        /// <summary>
        /// The current state of the Wakeup on charge
        /// </summary>
        /// <returns>Current state of the wake up on charge function</returns>
        public WakeUpOnCharge WakeUpOnCharge
        {
            get
            {
                var response = _piJuice.ReadCommand(PiJuiceCommand.WakeUpOnCharge, 1);

                return new WakeUpOnCharge(
                    response[0] == 0x7F,
                    Ratio.FromPercent(response[0] == 0x7F ? 0 : response[0]));
            }
            set
            {
                if (!value.Disabled && (value.WakeUpPercentage.Percent < 0 || value.WakeUpPercentage.Percent > 100))
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "WakeUpPercentage.Percent must be between 0 and 100.");
                }

                _piJuice.WriteCommandVerify(PiJuiceCommand.WakeUpOnCharge, new byte[] { (byte)(value.Disabled ? 0x7F : (short)value.WakeUpPercentage.Percent) });
            }
        }

        /// <summary>
        /// The current watchdog timer time after which it will power cycle if it does not receive a heartbeat signal
        /// </summary>
        /// <value>Time in minutes after which PiJuice will power cycle if it does not receive a heartbeat signal </value>
        public TimeSpan WatchdogTimer
        {
            get
            {
                var response = _piJuice.ReadCommand(PiJuiceCommand.WatchdogActiviation, 2);

                return new TimeSpan(0, BinaryPrimitives.ReadInt16LittleEndian(response), 0);
            }
            set
            {
                if (value.TotalMinutes < 0 || value.TotalMinutes > 65535)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "TotalMinutes must be between 0 and 65535.");
                }

                var minutes = (int)value.TotalMinutes & 0xFFFF;

                _piJuice.WriteCommand(PiJuiceCommand.WatchdogActiviation, new byte[] { (byte)(minutes & 0xFF), (byte)((minutes >> 8) & 0xFF) });
            }
        }

        /// <summary>
        /// The current state of system switch
        /// </summary>
        /// <returns>Current state of system switch</returns>
        public SystemPowerSwitch SystemPowerSwitch
        {
            get
            {
                var response = _piJuice.ReadCommand(PiJuiceCommand.SystemPowerSwitch, 1);

                return (SystemPowerSwitch)(response[0] * 100);
            }
            set
            {
                _piJuice.WriteCommand(PiJuiceCommand.SystemPowerSwitch, new byte[] { (byte)(((int)value) / 100) });
            }
        }
    }
}
