// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using UnitsNet;

namespace Iot.Device.Ip5306
{
    /// <summary>
    /// IP5306 - Power management device
    /// </summary>
    public class Ip5306
    {
        /// <summary>
        /// Default IP5306 I2C address
        /// </summary>
        public const int DefaultI2cAddress = 0xEA;

        /// <summary>
        /// Possible address as well, used in M5Stack
        /// </summary>
        public const int SecondaryI2cAddress = 0x75;

        private I2cDevice _i2c;

        /// <summary>
        /// Creates an instance of IP5306
        /// </summary>
        /// <param name="i2c">The I2C device.</param>
        public Ip5306(I2cDevice i2c)
        {
            _i2c = i2c ?? throw new ArgumentNullException(nameof(i2c));
        }

        /// <summary>
        /// Button off enabled.
        /// False as default.
        /// </summary>
        public bool ButtonOffEnabled
        {
            get => (I2cRead(Register.SYS_CTL0) & 0b0000_0001) == 0b0000_0001;
            set
            {
                var buf = I2cRead(Register.SYS_CTL0);
                buf = (byte)(buf & ~0b0000_0001);
                buf |= (byte)(value ? 0b0000_0001 : 0);
                I2cWrite(Register.SYS_CTL0, buf);
            }
        }

        /// <summary>
        /// Boost output enabled.
        /// True as default.
        /// </summary>
        public bool BoostOutputEnabled
        {
            get => (I2cRead(Register.SYS_CTL0) & 0b0000_0010) == 0b0000_0010;
            set
            {
                var buf = I2cRead(Register.SYS_CTL0);
                buf = (byte)(buf & ~0b0000_0010);
                buf |= (byte)(value ? 0b0000_0010 : 0);
                I2cWrite(Register.SYS_CTL0, buf);
            }
        }

        /// <summary>
        /// Auto power on enabled.
        /// True as default.
        /// </summary>
        public bool AutoPowerOnEnabled
        {
            get => (I2cRead(Register.SYS_CTL0) & 0b0000_0100) == 0b0000_0100;
            set
            {
                var buf = I2cRead(Register.SYS_CTL0);
                buf = (byte)(buf & ~0b0000_0100);
                buf |= (byte)(value ? 0b0000_0100 : 0);
                I2cWrite(Register.SYS_CTL0, buf);
            }
        }

        /// <summary>
        /// Charger enabled.
        /// true as default.
        /// </summary>
        public bool ChargerEnabled
        {
            get => (I2cRead(Register.SYS_CTL0) & 0b0001_0000) == 0b0001_0000;
            set
            {
                var buf = I2cRead(Register.SYS_CTL0);
                buf = (byte)(buf & ~0b0001_0000);
                buf |= (byte)(value ? 0b0001_0000 : 0);
                I2cWrite(Register.SYS_CTL0, buf);
            }
        }

        /// <summary>
        /// Boost enabled.
        /// True as default.
        /// </summary>
        public bool BoostEnabled
        {
            get => (I2cRead(Register.SYS_CTL0) & 0b0010_0000) == 0b0010_0000;
            set
            {
                var buf = I2cRead(Register.SYS_CTL0);
                buf = (byte)(buf & ~0b0010_0000);
                buf |= (byte)(value ? 0b0010_0000 : 0);
                I2cWrite(Register.SYS_CTL0, buf);
            }
        }

        /// <summary>
        /// Low power off enabled.
        /// True as default.
        /// </summary>
        public bool LowPowerOffEnabled
        {
            get => (I2cRead(Register.SYS_CTL1) & 0b0000_0001) == 0b0000_0001;
            set
            {
                var buf = I2cRead(Register.SYS_CTL1);
                buf = (byte)(buf & ~0b0000_0001);
                buf |= (byte)(value ? 0b0000_0001 : 0);
                I2cWrite(Register.SYS_CTL1, buf);
            }
        }

        /// <summary>
        /// Boost when V in unplugged enabled.
        /// True as default.
        /// </summary>
        public bool BoostWhenVinUnpluggedEnabled
        {
            get => (I2cRead(Register.SYS_CTL1) & 0b0000_0100) == 0b0000_0100;
            set
            {
                var buf = I2cRead(Register.SYS_CTL1);
                buf = (byte)(buf & ~0b0000_0100);
                buf |= (byte)(value ? 0b0000_0100 : 0);
                I2cWrite(Register.SYS_CTL1, buf);
            }
        }

        /// <summary>
        /// Short press to switch boost enabled.
        /// False as default.
        /// </summary>
        public bool ShortPressToSwitchBosst
        {
            get => (I2cRead(Register.SYS_CTL1) & 0b0010_0000) == 0b0010_0000;
            set
            {
                var buf = I2cRead(Register.SYS_CTL1);
                buf = (byte)(buf & ~0b0010_0000);
                buf |= (byte)(value ? 0b0010_0000 : 0);
                I2cWrite(Register.SYS_CTL1, buf);
            }
        }

        /// <summary>
        /// Flash light behavior.
        /// Default to double click.
        /// </summary>
        public ButtonPress FlashLightBehavior
        {
            get => (ButtonPress)(I2cRead(Register.SYS_CTL1) & 0b0100_0000);
            set
            {
                var buf = I2cRead(Register.SYS_CTL1);
                buf = (byte)(buf & ~0b0100_0000);
                buf |= (byte)(value);
                I2cWrite(Register.SYS_CTL1, buf);
            }
        }

        /// <summary>
        /// Switch off boost behavior.
        /// Default to long press.
        /// </summary>
        public ButtonPress SwitchOffBoostBehavior
        {
            // Warning, the button setu is inverted compare to the previous one
            get => ((I2cRead(Register.SYS_CTL1) & 0b1000_0000) == 1) ? ButtonPress.Doubleclick : ButtonPress.LongPress;
            set
            {
                var buf = I2cRead(Register.SYS_CTL1);
                buf = (byte)(buf & ~0b1000_0000);
                // Button here is inverted compare to previous case
                buf |= (byte)(value == ButtonPress.LongPress ? ButtonPress.Doubleclick : ButtonPress.LongPress);
                I2cWrite(Register.SYS_CTL1, buf);
            }
        }

        /// <summary>
        /// Light duty shutdown time.
        /// Default to 8 seconds.
        /// </summary>
        public LightDutyShutdownTime LightDutyShutdownTime
        {
            get => (LightDutyShutdownTime)(I2cRead(Register.SYS_CTL2) & 0b0000_1100);
            set
            {
                var buf = I2cRead(Register.SYS_CTL2);
                buf = (byte)(buf & ~0b0000_1100);
                buf |= (byte)(value);
                I2cWrite(Register.SYS_CTL2, buf);
            }
        }

        /// <summary>
        /// Charging cut off voltage.
        /// Default to 4.185 Volt.
        /// </summary>
        public ChargingCutOffVoltage ChargingCuttOffVoltage
        {
            get => (ChargingCutOffVoltage)(I2cRead(Register.Charger_CTL0) & 0b0000_0011);
            set
            {
                var buf = I2cRead(Register.Charger_CTL0);
                buf = (byte)(buf & ~0b0000_0011);
                buf |= (byte)(value);
                I2cWrite(Register.Charger_CTL0, buf);
            }
        }

        /// <summary>
        /// Charging cut off current.
        /// Default to 400 mA.
        /// </summary>
        public ChargingCutOffCurrent ChargingCutOffCurrent
        {
            get => (ChargingCutOffCurrent)(I2cRead(Register.Charger_CTL1) & 0b1100_0000);
            set
            {
                var buf = I2cRead(Register.Charger_CTL1);
                buf = (byte)(buf & ~0b1100_0000);
                buf |= (byte)(value);
                I2cWrite(Register.Charger_CTL1, buf);
            }
        }

        /// <summary>
        /// Charging under voltage.
        /// Default to 4.7 Volt.
        /// </summary>
        public ChargingUnderVoltage ChargingUnderVoltage
        {
            get => (ChargingUnderVoltage)(I2cRead(Register.Charger_CTL1) & 0b0001_1100);
            set
            {
                var buf = I2cRead(Register.Charger_CTL1);
                buf = (byte)(buf & ~0b0001_1100);
                buf |= (byte)(value);
                I2cWrite(Register.Charger_CTL1, buf);
            }
        }

        /// <summary>
        /// Charging battery voltage.
        /// Default to 4.2 Volt.
        /// </summary>
        public ChargingBatteryVoltage ChargingBatteryVoltage
        {
            get => (ChargingBatteryVoltage)(I2cRead(Register.Charger_CTL2) & 0b0000_1100);
            set
            {
                var buf = I2cRead(Register.Charger_CTL1);
                buf = (byte)(buf & ~0b0000_1100);
                buf |= (byte)(value);
                I2cWrite(Register.Charger_CTL1, buf);
            }
        }

        /// <summary>
        /// Constant charging voltage.
        /// Default to 14 milli Volt.
        /// </summary>
        public ConstantChargingVoltage ConstantChargingVoltage
        {
            get => (ConstantChargingVoltage)(I2cRead(Register.Charger_CTL2) & 0b0000_0011);
            set
            {
                var buf = I2cRead(Register.Charger_CTL1);
                buf = (byte)(buf & ~0b0000_0011);
                buf |= (byte)(value);
                I2cWrite(Register.Charger_CTL1, buf);
            }
        }

        /// <summary>
        /// Charging loop selection.
        /// Defautl to V in.
        /// </summary>
        public ChargingLoopSelection ChargingLoopSelection
        {
            get => (ChargingLoopSelection)(I2cRead(Register.Charger_CTL3) & 0b0000_1000);
            set
            {
                var buf = I2cRead(Register.Charger_CTL1);
                buf = (byte)(buf & ~0b0000_1000);
                buf |= (byte)(value);
                I2cWrite(Register.Charger_CTL1, buf);
            }
        }

        /// <summary>
        /// Charging current.
        /// </summary>
        /// <remarks>Typical valut is between 50 and 3150 milli Ampere. Values are capted to 50 for anything under or 3150 for anything higher.</remarks>
        public ElectricCurrent ChargingCurrent
        {
            get
            {
                var buf = I2cRead(Register.CHG_DIG_CTL0) & 0b0001_1111;
                // I=0.05+b0*0.1+b1*0.2+b2*0.4+b3*0.8+b4*1.6A
                // I = buff * 100 + 50 mA
                return ElectricCurrent.FromMilliamperes(buf * 100 + 50);
            }

            set
            {
                // Cap the values to 0 or 3100 mA
                var currentmA = value.Milliamperes - 50;
                if (currentmA < 0)
                {
                    currentmA = 0;
                }
                else if (currentmA > 3100)
                {
                    currentmA = 3100;
                }

                currentmA = currentmA / 100;
                var buf = I2cRead(Register.CHG_DIG_CTL0);
                buf = (byte)(buf & ~0b0001_1111);
                buf |= (byte)currentmA;
                I2cWrite(Register.CHG_DIG_CTL0, buf);
            }
        }

        /// <summary>
        /// True if the battery is charging.
        /// </summary>
        public bool IsCharging
        {
            get => (I2cRead(Register.REG_READ0) & 0b0000_1000) == 0b0000_1000;
        }

        /// <summary>
        /// True if the battery is full.
        /// </summary>
        public bool IsBatteryFull
        {
            get => (I2cRead(Register.REG_READ1) & 0b0000_1000) == 0b0000_1000;
        }

        /// <summary>
        /// True if the output is loaded to high.
        /// </summary>
        public bool IsOutputLoadHigh
        {
            get => (I2cRead(Register.REG_READ2) & 0b0000_0100) == 0b0000_0100;
        }

        /// <summary>
        /// Gets the button status.
        /// </summary>
        public ButtonPressed GetButtonStatus()
        {
                var buf = I2cRead(Register.REG_READ3);
                I2cWrite(Register.REG_READ3, buf);
                return (ButtonPressed)(buf & 0b0000_0111);
        }

        private void I2cWrite(Register reg, byte data)
        {
            Span<byte> writeBuffer = stackalloc byte[2] { (byte)reg, data };
            _i2c.Write(writeBuffer);
        }

        private byte I2cRead(Register reg)
        {
            _i2c.WriteByte((byte)reg);
            return _i2c.ReadByte();
        }
    }
}
