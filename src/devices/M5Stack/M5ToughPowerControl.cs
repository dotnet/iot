// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.Analog;
using System.Device.Gpio;
using System.Device.I2c;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Iot.Device.Arduino;
using Iot.Device.Common;
using Iot.Device.Axp192;
using Microsoft.Extensions.Logging;
using UnitsNet;

namespace Iot.Device.M5Stack
{
    /// <summary>
    /// High-level abstraction for the AXP192 in an M5Tough enclosure.
    /// This binding knows the hardware connections from the ESP32 CPU to the AXP192 and the connected hardware and directly offers to toggle named devices (e.g. switch the
    /// display backlight on and off.
    /// </summary>
    /// <remarks>
    /// This binding is useful together with the Arduino/Firmata binding or the Arduino Compiler.
    /// </remarks>
    public class M5ToughPowerControl : IDisposable
    {
        private Axp192.Axp192 _axp;
        private ILogger _logger;
        private Board.Board _board;

        /// <summary>
        /// Create an instance of this class
        /// </summary>
        /// <param name="board">The board connection (typically an instance of ArduinoBoard)</param>
        public M5ToughPowerControl(Board.Board board)
        {
            _board = board;
            var bus = board.CreateOrGetI2cBus(0, new int[]
            {
                21, 22
            });

            _axp = new Axp192.Axp192(bus.CreateDevice(Axp192.Axp192.I2cDefaultAddress));
            _logger = this.GetCurrentClassLogger();

            Init();
        }

        /// <summary>
        /// True to enable the speaker, false to mute it.
        /// </summary>
        public bool EnableSpeaker
        {
            get
            {
                return _axp.ReadGpioValue(2) == PinValue.High;
            }
            set
            {
                _axp.WriteGpioValue(2, value);
            }
        }

        /// <summary>
        /// Configures the AXP hardware to default operational settings.
        /// Enables the LCD and sets the default charging mode for the optional battery.
        /// </summary>
        protected virtual void Init()
        {
            _axp.SetVbusSettings(true, false, VholdVoltage.V4_0, false, VbusCurrentLimit.MilliAmper500);
            _logger.LogInformation("axp: VBus limit off");

            // Configure GPIO outputs
            // GPIO 0: Bus power enable
            _axp.SetGPIO0(Gpio0Behavior.NmosLeakOpenOutput);
            _axp.WriteGpioValue(0, PinValue.High);
            // GPIO 1: Touch pad reset
            _axp.SetGPIO1(GpioBehavior.NmosLeakOpenOutput);
            // GPIO 2: Speaker enable
            _axp.SetGPIO2(GpioBehavior.NmosLeakOpenOutput);
            // GPIO 4: LCD Reset
            _axp.SetGPIO4(GpioBehavior.NmosLeakOpenOutput);

            _logger.LogInformation("GPIO Ports configured");

            _axp.SetBackupBatteryChargingControl(true, BackupBatteryCharingVoltage.V3_0, BackupBatteryChargingCurrent.MicroAmperes200);

            // Set MCU voltage (probably not a good idea to mess to much with this one)
            _axp.SetDcVoltage(1, ElectricPotential.FromMillivolts(3350));

            // LCD backlight voltage
            SetLcdVoltage(ElectricPotential.FromMillivolts(3000));
            // Peripheral bus voltage (for SD card and LCD logic)
            _axp.SetLdoOutput(2, ElectricPotential.FromMillivolts(3300));

            // LDO2: Peripheral voltage
            _axp.SetLdoEnable(2, true);
            // LDO3: LCD Backlight
            _axp.SetLdoEnable(3, true);

            // Unfortunately, the button cannot really be pressed on the M5Tough, unless the housing is not screwed together
            _axp.SetButtonBehavior(LongPressTiming.S2_5, ShortPressTiming.Ms128, true, SignalDelayAfterPowerUp.Ms64, ShutdownTiming.S6);

            // Enable all ADCs
            _axp.SetAdcState(true);

            // Pull GPIO4 low for a moment to reset the LCD driver IC
            _axp.WriteGpioValue(4, PinValue.Low);
            // Pull GPIO1 low for resetting the touch driver IC as well
            _axp.WriteGpioValue(1, PinValue.Low);
            Thread.Sleep(100);
            _axp.WriteGpioValue(4, PinValue.High);
            _axp.WriteGpioValue(1, PinValue.High);
            // Ensure GPIO2 (Speaker enable) is high
            _axp.WriteGpioValue(2, PinValue.High);
            Thread.Sleep(100);

            // Configure charging for external battery (not fitted to M5Tough by default)
            _axp.SetChargingFunctions(true, ChargingVoltage.V4_2, ChargingCurrent.Current450mA, ChargingStopThreshold.Percent10);
        }

        /// <summary>
        /// Beeps using the speaker for the given time, using a fixed frequency
        /// </summary>
        /// <param name="duration">The duration of the beep</param>
        public void Beep(TimeSpan duration)
        {
            bool isEnabled = EnableSpeaker;
            EnableSpeaker = true;
            using var pwm = _board.CreatePwmChannel(0, 2);
            pwm.DutyCycle = 0.5f;
            pwm.Start();
            Thread.Sleep(duration);
            pwm.Stop();
            EnableSpeaker = isEnabled;
        }

        /// <summary>
        /// Set LCD backlight voltage. Valid values range from 1.8 - 3.3V. Low values switch off the backlight completely
        /// </summary>
        /// <param name="voltage">The voltage to set</param>
        public virtual void SetLcdVoltage(ElectricPotential voltage)
        {
            if (voltage.Millivolts < 1800)
            {
                _axp.SetLdoEnable(3, false);
            }
            else
            {
                _axp.SetLdoEnable(3, true);
            }

            _axp.SetLdoOutput(3, voltage);
            _logger.LogInformation($"LCD voltage set to {voltage}");
        }

        /// <summary>
        /// Enter and leave low-power mode.
        /// The physical power button that is normally used to recover the AXP from sleep mode is not accessible on the M5Tough when the case is closed.
        /// After setting the AXP to sleep, we send the CPU to sleep as well. It will only wake up on tapping the screen.
        /// </summary>
        /// <param name="enterSleep">True to enter sleep, false to recover from sleep.</param>
        /// <remarks>After recovering from sleep mode, some peripheral devices might need to be restarted (such as the display controller)</remarks>
        public void Sleep(bool enterSleep)
        {
            _axp.SetSleep(enterSleep, false);
            if (enterSleep)
            {
                SendCpuToSleep();
            }
        }

        private void SendCpuToSleep()
        {
            // Sends the board to sleep mode
            TimeSpan sleepDelay = TimeSpan.FromSeconds(10);
            const int wakeupPin = 39; // The screen interrupt is connected to this pin

            if (!(_board is ArduinoBoard ab))
            {
                return;
            }

            // Trigger value is low-active on the M5Tough
            if (!ab.SetSystemVariable(SystemVariable.SleepModeInterruptEnable, wakeupPin, 0))
            {
                return;
            }

            if (!ab.SetSystemVariable(SystemVariable.EnterSleepMode, wakeupPin, (int)sleepDelay.TotalSeconds))
            {
                return;
            }
        }

        /// <summary>
        /// Gets a data set of current power parameters (power consumption, battery level, etc.)
        /// </summary>
        /// <returns></returns>
        public PowerControlData GetPowerControlData()
        {
            return _axp.GetPowerControlData();
        }

        /// <summary>
        /// Default dispose implementation
        /// </summary>
        /// <param name="disposing">True to dispose managed resources, false to dispose only unmanaged resources</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _axp.Dispose();
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
