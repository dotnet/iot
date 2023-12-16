// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Device.I2c;
using System.Security.Cryptography;
using UnitsNet;
using UnitsNet.Units;

namespace Iot.Device.Axp192
{
    /// <summary>
    /// AXP192 - Enhanced single Cell Li-Battery and Power System Management IC
    /// </summary>
    public class Axp192 : IDisposable
    {
        /// <summary>
        /// Default I2C address
        /// </summary>
        public const int I2cDefaultAddress = 0x34;

        private I2cDevice _i2c;
        private byte[] _writeBuffer = new byte[2];
        private AdcPinEnabled _adcPinEnabledBeforeSleep;

        /// <summary>
        /// Creates an AXP192
        /// </summary>
        /// <param name="i2c">An I2C device.</param>
        public Axp192(I2cDevice i2c)
        {
            _i2c = i2c ?? throw new ArgumentNullException(nameof(i2c));
            _adcPinEnabledBeforeSleep = AdcPinEnabled.All;
        }

        /// <summary>
        /// Sets LDO2 or LDO3 output voltage
        /// </summary>
        /// <param name="output">The LDO port to configure. Valid values are 2 and 3 (LDO1 cannot be configured and is always on)</param>
        /// <param name="voltage">The voltage value to set. Will be chopped to the range 1.8-3.3V</param>
        public void SetLdoOutput(int output, ElectricPotential voltage)
        {
            double value = voltage.Millivolts;
            if (value < 1800)
            {
                value = 0;
            }
            else if (value > 3300)
            {
                value = 15;
            }
            else
            {
                value = (value - 1800.0) / (3300.0 - 1800.0) * 15.0;
            }

            int v = (int)value;
            byte buf = I2cRead(Register.VoltageSettingLdo2_3);
            if (output == 3)
            {
                I2cWrite(Register.VoltageSettingLdo2_3, (byte)((buf & 0xF0) | v));
            }
            else if (output == 2)
            {
                I2cWrite(Register.VoltageSettingLdo2_3, (byte)((buf & 0x0f) | (v << 4)));
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(output), "Only LDO ports 2 and 3 can be configured");
            }
        }

        /*
         * Coulomb calculation method: C - 65536 - current LSB ( charging coulomb meter value - discharge coulomb meter value) / 3600 / ADC sample rate.
         * Where: the ADC sample rate refers to the setting of REG84H, the current LSB is 0.5mA, and the calculation is in mAh.
         */

        /// <summary>
        /// Enable Coulomb counter
        /// </summary>
        public void EnableCoulombCounter() => I2cWrite(Register.CoulombCounter, 0x80);

        /// <summary>
        /// Disable Coulomb counter
        /// </summary>
        public void DisableCoulombCounter() => I2cWrite(Register.CoulombCounter, 0x00);

        /// <summary>
        /// Stops Coulomb counter
        /// </summary>
        public void StopCoulombCounter() => I2cWrite(Register.CoulombCounter, 0xC0);

        /// <summary>
        /// Clear Coulomb counter
        /// </summary>
        public void ClearCoulombCounter() => I2cWrite(Register.CoulombCounter, 0xA0);

        /// <summary>
        /// Checks if the battery is connected.
        /// </summary>
        /// <returns>True if connected.</returns>
        public bool IsBatteryConnected() => (GetBatteryChargingStatus() & BatteryStatus.BatteryConnected) == BatteryStatus.BatteryConnected;

        /// <summary>
        /// Gets the power status.
        /// </summary>
        /// <returns>The power status.</returns>
        public PowerStatus GetInputPowerStatus() => (PowerStatus)I2cRead(Register.PowerStatus);

        /// <summary>
        ///  Gets battery charging status.
        /// </summary>
        /// <returns>The battery status</returns>
        public BatteryStatus GetBatteryChargingStatus() => (BatteryStatus)I2cRead(Register.PowerModeChargingStatus);

        private uint GetCoulombCharge() => I2cRead32(Register.CoulombCounterChargingData1);

        private uint GetCoulombDischarge() => I2cRead32(Register.CoulombCounterDischargingData1);

        /// <summary>
        /// Gets Coulomb
        /// </summary>
        /// <returns>typical values are in mA per hour</returns>
        public ElectricCurrentGradient GetCoulomb()
        {
            uint coin = GetCoulombCharge();
            uint coout = GetCoulombDischarge();
            uint valueDifferent = 0;
            bool bIsNegative = false;

            if (coin > coout)
            {
                // Expected, in always more then out
                valueDifferent = coin - coout;
            }
            else
            {
                // Warning: Out is more than In, the battery is not started at 0%
                // just Flip the output sign later
                bIsNegative = true;
                valueDifferent = coout - coin;
            }

            // c = 65536 * current_LSB * (coin - coout) / 3600 / ADC rate
            // Adc rate can be read from 84H, change this variable if you change the ADC reate
            double adcDiv;
            switch (AdcFrequency)
            {
                case AdcFrequency.Frequency25Hz:
                    adcDiv = 25.0;
                    break;
                case AdcFrequency.Frequency50Hz:
                    adcDiv = 50.0;
                    break;
                case AdcFrequency.Frequency100Hz:
                    adcDiv = 100.0;
                    break;
                default:
                case AdcFrequency.Frequency200Hz:
                    adcDiv = 200.0;
                    break;
            }

            double ccc = (65536 * 0.5 * valueDifferent) / 3600.0 / adcDiv;  // Note the ADC has defaulted to be 200 Hz

            if (bIsNegative)
            {
                ccc = 0.0 - ccc;    // Flip it back to negative
            }

            // ccc is in the mA per hour.
            return ElectricCurrentGradient.FromAmperesPerSecond(ccc * 3.6);
        }

        /*
         * For all the Voltage and Current:
         * Channel 000H STEP FFFH
         * Battery Voltage 0mV 1.1mV 4.5045V
         * Bat discharge current 0mA 0.5mA 4.095A
         * Bat charge current 0mA 0.5mA 4.095A
         * ACIN volatge 0mV 1.7mV 6.9615V
         * ACIN current 0mA 0.625mA 2.5594A
         * VBUS voltage 0mV 1.7mV 6.9615V
         * VBUS current 0mA 0.375mA 1.5356A
         * Internal temperature -144.7℃ 0.1℃ 264.8℃
         * APS voltage 0mV 1.4mV 5.733V
         * TS pin input 0mV 0.8mV 3.276V
         * GPIO0 0/0.7V 0.5mV 2.0475/2.7475V
         * GPIO1 0/0.7V 0.5mV 2.0475/2.7475V
         * GPIO2 0/0.7V 0.5mV 2.0475/2.7475V
         * GPIO3 0/0.7V 0.5mV 2.0475/2.7475V
         */

        /// <summary>
        /// Gets the battery voltage.
        /// </summary>
        /// <returns>The battery voltage</returns>
        public ElectricPotential GetBatteryVoltage()
        {
            Span<byte> buf = stackalloc byte[2];
            I2cRead(Register.BatteryVoltage8bitsHigh, buf);
            ushort volt = (ushort)((buf[0] << 4) + buf[1]);
            return new ElectricPotential(volt * 1.1, ElectricPotentialUnit.Millivolt);
        }

        /// <summary>
        /// Gets the input voltage.
        /// </summary>
        /// <returns>The input voltage</returns>
        public ElectricPotential GetInputVoltage()
        {
            Span<byte> buf = stackalloc byte[2];
            I2cRead(Register.InputVoltageAdc8bitsHigh, buf);
            ushort vin = (ushort)((buf[0] << 4) + buf[1]);
            return new ElectricPotential(vin * 1.7, ElectricPotentialUnit.Millivolt);
        }

        /// <summary>
        /// Gets the input current.
        /// </summary>
        /// <returns>The input current.</returns>
        public ElectricCurrent GetInputCurrent()
        {
            ushort iin = 0;
            Span<byte> buf = stackalloc byte[2];
            I2cRead(Register.InputCurrentAdc8bitsHigh, buf);
            iin = (ushort)((buf[0] << 4) + buf[1]);
            return new ElectricCurrent(iin * 0.625, ElectricCurrentUnit.Milliampere);
        }

        /// <summary>
        /// Gets the USB voltage input.
        /// </summary>
        /// <returns>The USB voltage input.</returns>
        public ElectricPotential GetUsbVoltageInput()
        {
            ushort vin = 0;
            Span<byte> buf = stackalloc byte[2];
            I2cRead(Register.UsbVoltageAdc8bitsHigh, buf);
            vin = (ushort)((buf[0] << 4) + buf[1]);
            return new ElectricPotential(vin * 1.7, ElectricPotentialUnit.Millivolt);
        }

        /// <summary>
        /// Gets the USB current.
        /// </summary>
        /// <returns>The USB current</returns>
        public ElectricCurrent GetUsbCurrentInput()
        {
            Span<byte> buf = stackalloc byte[2];
            I2cRead(Register.UsbCurrentAdc8bitsHigh, buf);
            ushort iin = (ushort)((buf[0] << 4) + buf[1]);
            return new ElectricCurrent(iin * 0.375, ElectricCurrentUnit.Milliampere);
        }

        /// <summary>
        /// Gets the battery charge Current.
        /// </summary>
        /// <returns>The battery charge Current.</returns>
        public ElectricCurrent GetBatteryChargeCurrent()
        {
            ushort icharge = 0;
            Span<byte> buf = stackalloc byte[2];
            I2cRead(Register.BatteryChargeCurrent8bitsHigh, buf);
            icharge = (ushort)((buf[0] << 5) + buf[1]);
            return new ElectricCurrent(icharge * 0.5, ElectricCurrentUnit.Milliampere);
        }

        /// <summary>
        /// Gets the battery discharge current.
        /// </summary>
        /// <returns>The battery discharge current.</returns>
        public ElectricCurrent GetBatteryDischargeCurrent()
        {
            ushort idischarge = 0;
            Span<byte> buf = stackalloc byte[2];
            I2cRead(Register.BatteryDischargeCurrent8bitsHigh, buf);
            idischarge = (ushort)((buf[0] << 5) + buf[1]);
            return new ElectricCurrent(idischarge * 0.5, ElectricCurrentUnit.Milliampere);
        }

        /// <summary>
        /// GetInternal Temperature.
        /// </summary>
        /// <returns>The temperature.</returns>
        public Temperature GetInternalTemperature()
        {
            Span<byte> buf = stackalloc byte[2];
            I2cRead(Register.Axp192InternalTemperatureAdc8bitsHigh, buf);
            ushort temp = (ushort)((buf[0] << 4) + buf[1]);
            return new Temperature(temp * 0.1 - 144.7, TemperatureUnit.DegreeCelsius);
        }

        /// <summary>
        /// GetBattery Instantaneous Power.
        /// </summary>
        /// <returns>The power consumption.</returns>
        public Power GetBatteryInstantaneousPower()
        {
            uint power = 0;
            Span<byte> buf = stackalloc byte[3];
            I2cRead(Register.BatteryInstantaneousPower1, buf);
            power = (ushort)((buf[0] << 16) | (buf[1] << 8) | buf[2]);
            return new Power(power, PowerUnit.Milliwatt);
        }

        /// <summary>
        /// Gets the APS voltage.
        /// </summary>
        /// <returns>The APS voltage.</returns>
        public ElectricPotential GetApsVoltage()
        {
            Span<byte> buf = stackalloc byte[2];
            I2cRead(Register.ApsVoltage8bitsHigh, buf);
            ushort vaps = (ushort)((buf[0] << 4) + buf[1]);
            return new ElectricPotential(vaps * 1.4, ElectricPotentialUnit.Millivolt);
        }

        /// <summary>
        /// Enters sleep mode for the AXP and the peripherals, leaving the CPU running
        /// </summary>
        public void SetSleep()
        {
            SetSleep(true, false);
        }

        /// <summary>
        /// Set or recover from sleep. If the CPU is also switched off, it can only be restarted trough an external
        /// interrupt on AXP input PWRON (typically wired to a hardware key)
        /// </summary>
        /// <param name="enterSleep">True to enter sleep, false to recover from it</param>
        /// <param name="includingCpu">True to also switch off the CPU</param>
        public void SetSleep(bool enterSleep, bool includingCpu)
        {
            if (enterSleep)
            {
                _adcPinEnabledBeforeSleep = AdcPinEnabled;
                I2cWrite(Register.VoltageSettingOff, (byte)(I2cRead(Register.VoltageSettingOff) | (1 << 3))); // Turn on short press to wake up
                I2cWrite(Register.ControlGpio0, (byte)(I2cRead(Register.ControlGpio0) | 0x07)); // GPIO0 floating
                I2cWrite(Register.AdcPin1, 0x00); // Disable ADCs

                // Disable all outputs but DCDC1 (CPU main power)
                byte filter = 0xA1;
                if (includingCpu)
                {
                    // If we clear bit 0, the CPU is also switched off.
                    filter = 0xA0;
                }

                I2cWrite(Register.SwitchControleDcDc1_3LDO2_3, (byte)(I2cRead(Register.SwitchControleDcDc1_3LDO2_3) & filter));
            }
            else
            {
                I2cWrite(Register.VoltageSettingOff, (byte)(I2cRead(Register.VoltageSettingOff) & 4)); // Disable sleep mode
                SetGPIO0(Gpio0Behavior.NmosLeakOpenOutput);
                WriteGpioValue(0, PinValue.High);
                AdcPinEnabled = _adcPinEnabledBeforeSleep; // Enable ADCs

                I2cWrite(Register.SwitchControleDcDc1_3LDO2_3, (byte)(I2cRead(Register.SwitchControleDcDc1_3LDO2_3) | 0x5F)); // Enable everything
            }
        }

        /// <summary>
        /// Is the temperature in warning.
        /// </summary>
        /// <returns>True if internal temperature too high.</returns>
        public bool IsTemperatureWarning() => (I2cRead(Register.IrqStatus4) & 0x01) == 0x01;

        /// <summary>
        /// Get button status
        /// </summary>
        /// <returns></returns>
        public ButtonPressed GetButtonStatus()
        {
            byte state = I2cRead(Register.IrqStatus3);  // IRQ 3 status.
            if (state != 0)
            {
                I2cWrite(Register.IrqStatus3, 0x03);   // Write 1 back to clear IRQ
            }

            return (ButtonPressed)(state & 0x03);
        }

        /// <summary>
        /// Sets the button default behavior.
        /// </summary>
        /// <param name="longPress">The long press timing.</param>
        /// <param name="shortPress">The short press timing.</param>
        /// <param name="automaticShutdownAtOvertime">True if automatic shutdown should be processed when over shutdown time.</param>
        /// <param name="signalDelay">The PWROK signal delay after power start-up.</param>
        /// <param name="shutdownTiming">The shutdown timing.</param>
        public void SetButtonBehavior(LongPressTiming longPress, ShortPressTiming shortPress, bool automaticShutdownAtOvertime, SignalDelayAfterPowerUp signalDelay, ShutdownTiming shutdownTiming)
        {
            byte buf = (byte)(automaticShutdownAtOvertime ? 0b0000_1000 : 0b0000_0000);
            buf |= (byte)((byte)longPress | (byte)shortPress | (byte)signalDelay | (byte)shutdownTiming);
            I2cWrite(Register.ParameterSetting, buf);
        }

        /// <summary>
        /// Sets the state of LDO2.
        /// </summary>
        /// <remarks>On M5Stack, can turn LCD Backlight OFF for power saving</remarks>
        /// <param name="State">True for on/high/1, false for off/low/O</param>
        public void EnableLDO2(bool State)
        {
            byte buf = I2cRead(Register.SwitchControleDcDc1_3LDO2_3);
            if (State == true)
            {
                buf = (byte)((1 << 2) | buf);
            }
            else
            {
                buf = (byte)(~(1 << 2) & buf);
            }

            I2cWrite(Register.SwitchControleDcDc1_3LDO2_3, buf);
        }

        /// <summary>
        /// Sets the state of LDO3.
        /// </summary>
        /// <param name="State">True to enable LDO3.</param>
        public void EnableLDO3(bool State)
        {
            byte buf = I2cRead(Register.SwitchControleDcDc1_3LDO2_3);
            if (State == true)
            {
                buf = (byte)((1 << 3) | buf);
            }
            else
            {
                buf = (byte)(~(1 << 3) & buf);
            }

            I2cWrite(Register.SwitchControleDcDc1_3LDO2_3, buf);
        }

        /// <summary>
        /// Sets the state of DC-DC3.
        /// </summary>
        /// <param name="State">True to enable DC-DC3.</param>
        public void EnableDCDC3(bool State)
        {
            byte buf = I2cRead(Register.SwitchControleDcDc1_3LDO2_3);
            if (State == true)
            {
                buf = (byte)((1 << 1) | buf);
            }
            else
            {
                buf = (byte)(~(1 << 1) & buf);
            }

            I2cWrite(Register.SwitchControleDcDc1_3LDO2_3, buf);
        }

        /// <summary>
        /// Sets the state of DC-DC1.
        /// </summary>
        /// <param name="State">True to enable DC-DC1.</param>
        public void EnableDCDC1(bool State)
        {
            byte buf = I2cRead(Register.SwitchControleDcDc1_3LDO2_3);
            if (State == true)
            {
                buf = (byte)(1 | buf);
            }
            else
            {
                buf = (byte)(~1 & buf);
            }

            I2cWrite(Register.SwitchControleDcDc1_3LDO2_3, buf);
        }

        /// <summary>
        /// Enable the various LDO and DC pins.
        /// </summary>
        public LdoDcPinsEnabled LdoDcPinsEnabled
        {
            get => (LdoDcPinsEnabled)(I2cRead(Register.SwitchControleDcDc1_3LDO2_3) & 0b0000_1111);

            set
            {
                byte buf = I2cRead(Register.SwitchControleDcDc1_3LDO2_3);
                buf &= 0b1111_0000;
                buf |= (byte)value;
                I2cWrite(Register.SwitchControleDcDc1_3LDO2_3, buf);
            }
        }

        /// <summary>
        /// Sets GPIO0 state
        /// </summary>
        /// <param name="state">The GPIO0 behavior</param>
        public void SetGPIO0(Gpio0Behavior state)
        {
            I2cWrite(Register.ControlGpio0, (byte)state);
        }

        /// <summary>
        /// Sets GPIO1 state
        /// </summary>
        /// <param name="state">The GPIO1 behavior</param>
        public void SetGPIO1(GpioBehavior state)
        {
            I2cWrite(Register.ControlGpio1, (byte)state);
        }

        /// <summary>
        /// Sets GPIO2 state
        /// </summary>
        /// <param name="state">The GPIO1 behavior</param>
        public void SetGPIO2(GpioBehavior state)
        {
            I2cWrite(Register.ControlGpio2, (byte)state);
        }

        /// <summary>
        /// Sets GPIO4 state
        /// </summary>
        /// <param name="state">The GPIO4 behavior</param>
        public void SetGPIO4(GpioBehavior state)
        {
            // Note: Adapt this if GPIO3 is also public
            if (state == GpioBehavior.NmosLeakOpenOutput)
            {
                I2cWrite(Register.ControlGpio34, 0x84);
            }
            else if (state == GpioBehavior.UniversalInputFunction)
            {
                I2cWrite(Register.ControlGpio34, 0x88);
            }
            else
            {
                throw new NotSupportedException($"Mode {state} is not supported on GPIO4");
            }
        }

        /// <summary>
        /// Reads GPIO Pins. This method works only for pins set to an input mode.
        /// </summary>
        public PinValue ReadGpioValue(int pin)
        {
            // If we were to read back the current output level, we would have to read the lower nibble instead
            int value = I2cRead(Register.ReadWriteGpio012);

            switch (pin)
            {
                case 0:
                    return (value & 0x10) != 0 ? PinValue.High : PinValue.Low;
                case 1:
                    return (value & 0x20) != 0 ? PinValue.High : PinValue.Low;
                case 2:
                    return (value & 0x40) != 0 ? PinValue.High : PinValue.Low;
                case 3:
                    value = I2cRead(Register.ReadWriteGpio34);
                    return (value & 0x10) != 0 ? PinValue.High : PinValue.Low;
                case 4:
                    value = I2cRead(Register.ReadWriteGpio34);
                    return (value & 0x20) != 0 ? PinValue.High : PinValue.Low;
            }

            throw new ArgumentOutOfRangeException(nameof(pin), "Valid pin numbers are 0-4");
        }

        /// <summary>
        /// Configure the given GPIO Pin
        /// </summary>
        /// <param name="pin">Pin number</param>
        /// <param name="value">Value</param>
        public void WriteGpioValue(int pin, PinValue value)
        {
            if (pin < 0 || pin > 4)
            {
                throw new ArgumentOutOfRangeException(nameof(pin), "Valid pin numbers are 0-4");
            }

            int values;
            int mask;

            if (pin == 4 || pin == 3)
            {
                values = I2cRead(Register.ReadWriteGpio34);
                mask = pin == 4 ? 2 : 1;
                values = (values & ~mask);
                if (value == PinValue.High)
                {
                    values = values | mask;
                }

                I2cWrite(Register.ReadWriteGpio34, (byte)values);
                return;
            }

            values = I2cRead(Register.ReadWriteGpio012);
            mask = 1 << pin;
            values = (values & ~mask);
            if (value == PinValue.High)
            {
                values = values | mask;
            }

            I2cWrite(Register.ReadWriteGpio012, (byte)values);
        }

        /// <summary>
        /// Sets the high temperature threshold for the battery
        /// </summary>
        /// <param name="potential">From 0 to 3.264V. Anything higher will be caped to the maximum.</param>
        public void SetBatteryHighTemperatureThreshold(ElectricPotential potential)
        {
            // Docs says Battery high temperature threshold setting when charging, N
            // N * 10H
            // When N-1FH,
            // corresponding to 0.397V;
            // The voltage can be 0V to 3.264V
            byte voltage = (byte)(potential.Volts / 0.0128);
            I2cWrite(Register.HigTemperatureAlarm, voltage);
        }

        /// <summary>
        /// Sets the backup battery charging control.
        /// </summary>
        /// <param name="enabled">True to enable the charging.</param>
        /// <param name="voltage">The desired backup battery charging voltage.</param>
        /// <param name="current">Te desired backup battery charging current.</param>
        public void SetBackupBatteryChargingControl(bool enabled, BackupBatteryCharingVoltage voltage, BackupBatteryChargingCurrent current)
        {
            byte buf = (byte)(enabled ? 0b1000_0000 : 0);
            buf |= (byte)voltage;
            buf |= (byte)current;
            I2cWrite(Register.BackupBatteryChargingControl, buf);
        }

        /// <summary>
        /// Sets shutdown battery detection control.
        /// </summary>
        /// <param name="turnOffAxp192">True to shutdown the AXP192.</param>
        /// <param name="enabled">True to enable the control.</param>
        /// <param name="function">The pin function.</param>
        /// <param name="pinControl">True to enable the pin function.</param>
        /// <param name="timing">Delay after AXP192 lowered to higher.</param>
        public void SetShutdownBatteryDetectionControl(bool turnOffAxp192, bool enabled, ShutdownBatteryPinFunction function, bool pinControl, ShutdownBatteryTiming timing)
        {
            byte buf = (byte)(turnOffAxp192 ? 0b1000_0000 : 0);
            buf |= (byte)(enabled ? 0b0100_0000 : 0);
            buf |= (byte)function;
            buf |= (byte)(pinControl ? 0b0000_1000 : 0);
            buf |= (byte)timing;
            I2cWrite(Register.ShutdownBatteryDetectionControl, buf);
        }

        /// <summary>
        /// Sets the output voltage of one of the DC-DC inverters
        /// </summary>
        /// <param name="number">The DC-DC inverter number. Valid choices are 1, 2, and 3</param>
        /// <param name="voltage">The voltage to set. Valid values are 0.7-3.5 V for inverters 1 and 3 and 0.7-2.275V for inverter 2. The
        /// value will be chopped accordingly</param>
        public void SetDcVoltage(int number, ElectricPotential voltage)
        {
            Register addr;

            int val = (int)Math.Round(voltage.Millivolts);
            val = (val < 700) ? 0 : (val - 700) / 25;
            switch (number)
            {
                case 1:
                    addr = Register.VoltageSettingDcDc1;
                    break;
                case 2:
                    addr = Register.VoltageSettingDcDc2;
                    // Inverter 2 has only 6 bits
                    if (val > 0x3F)
                    {
                        val = 0x3F;
                    }

                    break;
                case 3:
                    addr = Register.VoltageSettingDcDc3;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(number), "Valid DC Ports are 1-3");
            }

            val = val & 0x7F;
            I2cWrite(addr, (byte)val);
        }

        /// <summary>
        /// Gets or sets the charging voltage
        /// </summary>
        public ChargingVoltage ChargingVoltage
        {
            get => (ChargingVoltage)(I2cRead(Register.ChargeControl1) & 0x60);

            set
            {
                byte buf = I2cRead(Register.ChargeControl1);
                buf = (byte)((buf & ~(0x60)) | ((byte)value & 0x60));
                I2cWrite(Register.ChargeControl1, buf);
            }
        }

        /// <summary>
        /// Gets or sets the charging current
        /// </summary>
        /// <remarks>Not recommend to set charge current > 100mA, since Battery is only 80mAh.
        /// more then 1C charge-rate may shorten battery life-span.</remarks>
        public ChargingCurrent ChargingCurrent
        {
            get => (ChargingCurrent)(I2cRead(Register.ChargeControl1) & 0x07);

            set
            {
                byte buf = I2cRead(Register.ChargeControl1);
                buf = (byte)((buf & 0xf0) | (byte)value);
                I2cWrite(Register.ChargeControl1, buf);
            }
        }

        /// <summary>
        /// Charging threshold when battery should stop charging
        /// </summary>
        public ChargingStopThreshold ChargingStopThreshold
        {
            get => (ChargingStopThreshold)(I2cRead(Register.ChargeControl1) & 0b0001_0000);

            set
            {
                byte buf = I2cRead(Register.ChargeControl1);
                buf = (byte)((buf & 0b0001_0000) | (byte)((byte)value & 0b0001_0000));
                I2cWrite(Register.ChargeControl1, buf);
            }
        }

        /// <summary>
        /// Set the charging functions
        /// </summary>
        /// <param name="includeExternal">True to include the external.</param>
        /// <param name="chargingVoltage">Charging voltage.</param>
        /// <param name="chargingCurrent">Charging current.</param>
        /// <param name="stopThreshold">Stop threshold.</param>
        public void SetChargingFunctions(bool includeExternal, ChargingVoltage chargingVoltage, ChargingCurrent chargingCurrent, ChargingStopThreshold stopThreshold)
        {
            byte buf = (byte)(includeExternal ? 0b1000_0000 : 0);
            buf |= (byte)chargingVoltage;
            buf |= (byte)chargingCurrent;
            buf |= (byte)stopThreshold;
            I2cWrite(Register.ChargeControl1, buf);
        }

        /// <summary>
        /// Sets or gets the global pin output voltage
        /// </summary>
        public PinOutputVoltage PinOutputVoltage
        {
            get => (PinOutputVoltage)(I2cRead(Register.VoltageOutputSettingGpio0Ldo) & 0b1111_0000);

            set
            {
                byte buf = I2cRead(Register.VoltageOutputSettingGpio0Ldo);
                buf = (byte)((buf & 0b1111_0000) | (byte)((byte)value & 0b1111_0000));
                I2cWrite(Register.VoltageOutputSettingGpio0Ldo, buf);
            }
        }

        /// <summary>
        /// Sets the VBUS settings
        /// </summary>
        /// <param name="vbusIpsOut">The VBUS-IPSOUT path selects the control signal when VBUS is available.</param>
        /// <param name="vbusLimit">True to limit VBUS VHOLD control.</param>
        /// <param name="vholdVoltage">VHOLD Voltage.</param>
        /// <param name="currentLimitEnable">True to limit VBUS current.</param>
        /// <param name="vbusCurrent">VBUS Current limit.</param>
        public void SetVbusSettings(bool vbusIpsOut, bool vbusLimit, VholdVoltage vholdVoltage, bool currentLimitEnable, VbusCurrentLimit vbusCurrent)
        {
            byte buf = (byte)(vbusIpsOut ? 0b1000_0000 : 0);
            buf |= (byte)(vbusLimit ? 0b0100_0000 : 0);
            buf |= (byte)vholdVoltage;
            buf |= (byte)(currentLimitEnable ? 0b0000_0010 : 0);
            buf |= (byte)vbusCurrent;
            I2cWrite(Register.PathSettingVbus, buf);
        }

        /// <summary>
        /// Gets or sets the ADC pin enabled.
        /// </summary>
        public AdcPinEnabled AdcPinEnabled
        {
            get => (AdcPinEnabled)I2cRead(Register.AdcPin1);

            set
            {
                I2cWrite(Register.AdcPin1, (byte)value);
            }
        }

        /// <summary>
        /// Sets or gets power off voltage.
        /// </summary>
        public VoffVoltage VoffVoltage
        {
            get => (VoffVoltage)(I2cRead(Register.VoltageSettingOff) & 0xf8);

            set => I2cWrite(Register.VoltageSettingOff, (byte)((I2cRead(Register.VoltageSettingOff) & 0xf8) | (byte)value));
        }

        /// <summary>
        /// Cut all power, except for LDO1 (RTC)
        /// </summary>
        public void PowerOff()
        {
            I2cWrite(Register.ShutdownBatteryDetectionControl, (byte)(I2cRead(Register.ShutdownBatteryDetectionControl) | 0x80));     // MSB for Power Off
        }

        /// <summary>
        /// Sets the ADC state.
        /// </summary>
        /// <param name="state">True to enable, false to disable.</param>
        public void SetAdcState(bool state)
        {
            I2cWrite(Register.AdcPin1, (byte)(state ? 0xff : 0x00));  // Enable / Disable all ADCs
        }

        /// <summary>
        /// Disable all Irq
        /// </summary>
        public void DisableAllIRQ()
        {
            I2cWrite(Register.IrqEnable1, 0x00);
            I2cWrite(Register.IrqEnable2, 0x00);
            I2cWrite(Register.IrqEnable3, 0x00);
            I2cWrite(Register.IrqEnable4, 0x00);
            I2cWrite(Register.IrqEnable5, 0x00);
        }

        /// <summary>
        /// Enable the button to be pressed and raise IRQ events.
        /// </summary>
        /// <param name="button">Button pressed behavior.</param>
        public void EnableButtonPressed(ButtonPressed button)
        {
            byte value = I2cRead(Register.IrqEnable2);
            value &= 0xfc;
            value |= (byte)(button);
            I2cWrite(Register.IrqEnable2, value);
        }

        /// <summary>
        /// Clears all Irq.
        /// </summary>
        public void ClearAllIrq()
        {
            I2cWrite(Register.IrqStatus1, 0xff);
            I2cWrite(Register.IrqStatus2, 0xff);
            I2cWrite(Register.IrqStatus3, 0xff);
            I2cWrite(Register.IrqStatus4, 0xff);
            I2cWrite(Register.IrqStatus5, 0xff);

        }

        /// <summary>
        /// Sets or gets the ADC frequency
        /// </summary>
        public AdcFrequency AdcFrequency
        {
            get => (AdcFrequency)(I2cRead(Register.AdcFrequency) & 0xc0);

            set
            {
                byte buf = I2cRead(Register.AdcFrequency);
                buf = (byte)((buf & ~(0xc0)) | ((byte)value & 0xc0));
                I2cWrite(Register.AdcFrequency, buf);
            }
        }

        /// <summary>
        /// Sets or gets the ADC Pin output Current
        /// </summary>
        public AdcPinCurrent AdcPinCurrent
        {
            get => (AdcPinCurrent)(I2cRead(Register.AdcFrequency) & 0b0011_0000);

            set
            {
                byte buf = I2cRead(Register.AdcFrequency);
                buf = (byte)((buf & ~(0b0011_0000)) | ((byte)value & 0b0011_0000));
                I2cWrite(Register.AdcFrequency, buf);
            }
        }

        /// <summary>
        /// Sets or gets ADC battery temperature monitoring function.
        /// </summary>
        public bool BatteryTemperatureMonitoring
        {
            get => (I2cRead(Register.AdcFrequency) & 0b0000_0100) == 0;

            set
            {
                byte buf = I2cRead(Register.AdcFrequency);
                buf = (byte)((buf & ~(0b0000_0100)) | (value ? 0 : 0b0000_0100));
                I2cWrite(Register.AdcFrequency, buf);
            }
        }

        /// <summary>
        /// Sets or gets ADC pin current settings.
        /// </summary>
        public AdcPinCurrentSetting AdcPinCurrentSetting
        {
            get => (AdcPinCurrentSetting)(I2cRead(Register.AdcFrequency) & 0b0000_0011);

            set
            {
                byte buf = I2cRead(Register.AdcFrequency);
                buf = (byte)((buf & ~(0b0000_0011)) | ((byte)value & 0b0000_0011));
                I2cWrite(Register.AdcFrequency, buf);
            }
        }

        /// <summary>
        /// Reads the 6 bytes from the storage.
        /// AXP192 have a 6 byte storage, when the power is still valid, the data will not be lost.
        /// </summary>
        /// <param name="buffer">A 6 bytes buffer.</param>
        public void Read6BytesStorage(Span<byte> buffer)
        {
            if (buffer.Length != 6)
            {
                throw new ArgumentException("Buffer must be 6 bytes long.", nameof(buffer));
            }

            // Address from 0x06 - 0x0B
            I2cRead(Register.Storage1, buffer);
        }

        /// <summary>
        /// Stores data in the storage. 6 bytes are available.
        /// AXP192 have a 6 byte storage, when the power is still valid, the data will not be lost.
        /// </summary>
        /// <param name="buffer">A 6 bytes buffer</param>
        public void Write6BytesStorage(ReadOnlySpan<byte> buffer)
        {
            if (buffer.Length != 6)
            {
                throw new ArgumentException("Buffer must be 6 bytes long.", nameof(buffer));
            }

            // Address from 0x06 - 0x0B
            for (byte i = 0; i < buffer.Length; i++)
            {
                _writeBuffer[0] = (byte)((byte)Register.Storage1 + i);
                _writeBuffer[1] = buffer[i];
                _i2c.Write(_writeBuffer);
            }
        }

        private void I2cWrite(Register command, byte data)
        {
            _writeBuffer[0] = (byte)command;
            _writeBuffer[1] = data;
            _i2c.Write(_writeBuffer);
        }

        private byte I2cRead(Register command)
        {
            _i2c.WriteByte((byte)command);
            return _i2c.ReadByte();
        }

        private void I2cRead(Register command, Span<byte> buffer)
        {
            _i2c.WriteByte((byte)command);
            _i2c.Read(buffer);
        }

        private uint I2cRead32(Register command)
        {
            Span<byte> buffer = stackalloc byte[4];
            _i2c.WriteByte((byte)command);
            _i2c.Read(buffer);
            return (uint)(buffer[0] << 24 | buffer[1] << 16 | buffer[2] << 8 | buffer[1]);
        }

        /// <summary>
        /// Enable or disable specified LDO output
        /// </summary>
        /// <param name="number">Port number. Valid values: 2 and 3</param>
        /// <param name="enable">Enable or disable</param>
        public void SetLdoEnable(int number, bool enable)
        {
            byte mark = 0x01;
            if ((number < 2) || (number > 3))
            {
                throw new ArgumentOutOfRangeException(nameof(number));
            }

            mark <<= number;
            if (enable)
            {
                I2cWrite(Register.SwitchControleDcDc1_3LDO2_3, (byte)(I2cRead(Register.SwitchControleDcDc1_3LDO2_3) | mark));
            }
            else
            {
                I2cWrite(Register.SwitchControleDcDc1_3LDO2_3, (byte)(I2cRead(Register.SwitchControleDcDc1_3LDO2_3) & (~mark)));
            }
        }

        /// <summary>
        /// Returns all relevant power and current information in a single call
        /// </summary>
        /// <returns>An instance of <see cref="PowerControlData"/>.</returns>
        public PowerControlData GetPowerControlData()
        {
            PowerControlData powerControlData = new PowerControlData()
            {
                Temperature = GetInternalTemperature(),
                InputCurrent = GetInputCurrent(),
                InputVoltage = GetInputVoltage(),
                InputStatus = GetInputPowerStatus(),
                InputUsbVoltage = GetUsbVoltageInput(),
                InputUsbCurrent = GetUsbCurrentInput(),
                BatteryChargingCurrent = GetBatteryChargeCurrent(),
                BatteryChargingStatus = GetBatteryChargingStatus(),
                BatteryDischargeCurrent = GetBatteryDischargeCurrent(),
                BatteryInstantaneousPower = GetBatteryInstantaneousPower(),
                BatteryVoltage = GetBatteryVoltage(),
                BatteryPresent = IsBatteryConnected()
            };

            return powerControlData;
        }

        /// <summary>
        /// Standard dispose method
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
               _i2c.Dispose();
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
