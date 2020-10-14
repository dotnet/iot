// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Drawing;
using Iot.Device.PiJuiceDevice.Models;
using UnitsNet;
using UnitsNet.Units;

namespace Iot.Device.PiJuiceDevice
{
    /// <summary>
    /// PiJuiceConfig class to support status of the PiJuice
    /// </summary>
    public class PiJuiceConfig
    {
        private readonly PiJuice _piJuice;

        private readonly List<ElectricCurrent> _usbMicroCurrentLimits = new List<ElectricCurrent> { new ElectricCurrent(1.5, ElectricCurrentUnit.Ampere), new ElectricCurrent(2.5, ElectricCurrentUnit.Ampere) };
        private readonly List<ElectricPotential> _usbMicroDPMs = new List<ElectricPotential>();

        private List<string> _batteryProfiles = new List<string>();

        /// <summary>
        /// PiJuiceConfig constructor
        /// </summary>
        /// <param name="piJuice">The PiJuice class</param>
        public PiJuiceConfig(PiJuice piJuice)
        {
            _piJuice = piJuice;

            for (int i = 0; i < 8; i++)
            {
                _usbMicroDPMs.Add(new ElectricPotential(4.2 + 0.8 * i, ElectricPotentialUnit.Volt));
            }
        }

        /// <summary>
        /// Get battery profile
        /// </summary>
        public BatteryProfile GetBatteryProfile()
        {
            var batteryProfile = new BatteryProfile();

            var response = _piJuice.ReadCommand(PiJuiceCommand.BatteryProfile, 14);

            batteryProfile.Capacity = new ElectricCharge((response[1] << 8) | response[0], ElectricChargeUnit.MilliampereHour);
            batteryProfile.ChargeCurrent = new ElectricCurrent(response[2] * 75 + 550, ElectricCurrentUnit.Milliampere);
            batteryProfile.TerminationCurrent = new ElectricCurrent(response[3] * 50 + 50, ElectricCurrentUnit.Milliampere);
            batteryProfile.RegulationVoltage = new ElectricPotential(response[4] * 20 + 3500, ElectricPotentialUnit.Millivolt);
            batteryProfile.CutOffVoltage = new ElectricPotential(response[5] * 20, ElectricPotentialUnit.Millivolt);
            batteryProfile.TemperatureCold = new Temperature(response[6], TemperatureUnit.DegreeCelsius);
            batteryProfile.TemperatureCool = new Temperature(response[7], TemperatureUnit.DegreeCelsius);
            batteryProfile.TemperatureWarm = new Temperature(response[8], TemperatureUnit.DegreeCelsius);
            batteryProfile.TemperatureHot = new Temperature(response[9], TemperatureUnit.DegreeCelsius);
            batteryProfile.NTCB = (response[11] << 8) | response[10];
            batteryProfile.NTCResistance = new ElectricResistance(((response[13] << 8) | response[12]) * 10, ElectricResistanceUnit.Ohm);

            return batteryProfile;
        }

        /// <summary>
        /// Set a custom battery profile
        /// </summary>
        public void SetCustomBatteryProfile(BatteryProfile batteryProfile)
        {
            var data = new byte[14];

            data[0] = (byte)((int)(batteryProfile.Capacity.MilliampereHours) & 0xFF);
            data[1] = (byte)(((int)(batteryProfile.Capacity.MilliampereHours) >> 8) & 0xFF);
            data[2] = (byte)Math.Round((batteryProfile.ChargeCurrent.Milliamperes - 550) / 75, 0, MidpointRounding.AwayFromZero);
            data[3] = (byte)Math.Round((batteryProfile.TerminationCurrent.Milliamperes - 50) / 50, 0, MidpointRounding.AwayFromZero);
            data[4] = (byte)Math.Round((batteryProfile.RegulationVoltage.Millivolts - 3500) / 20, 0, MidpointRounding.AwayFromZero);
            data[5] = (byte)Math.Round(batteryProfile.CutOffVoltage.Millivolts / 20, 0, MidpointRounding.AwayFromZero);
            data[6] = (byte)batteryProfile.TemperatureCold.DegreesCelsius;
            data[7] = (byte)batteryProfile.TemperatureCool.DegreesCelsius;
            data[8] = (byte)batteryProfile.TemperatureWarm.DegreesCelsius;
            data[9] = (byte)batteryProfile.TemperatureHot.DegreesCelsius;
            data[10] = (byte)(batteryProfile.NTCB & 0xFF);
            data[11] = (byte)((batteryProfile.NTCB >> 8) & 0xFF);
            int ntcResistance = (int)batteryProfile.NTCResistance.Ohms / 10;
            data[12] = (byte)(ntcResistance & 0xFF);
            data[13] = (byte)((ntcResistance >> 8) & 0xFF);

            _piJuice.WriteCommandVerify(PiJuiceCommand.BatteryProfile, data);
        }

        /// <summary>
        /// TODO: Fill In
        /// </summary>
        public BatteryExtProfile GetBatteryExtProfile()
        {
            var batteryProfileExt = new BatteryExtProfile();

            var response = _piJuice.ReadCommand(PiJuiceCommand.BatteryProfileExt, 17);

            if (response[0] < 2)
            {
                batteryProfileExt.BatteryChemistry = (BatteryChemistry)response[0];
            }
            else
            {
                batteryProfileExt.BatteryChemistry = BatteryChemistry.Unknown;
            }

            batteryProfileExt.OCV10 = new ElectricPotential((response[2] << 8) | response[1], ElectricPotentialUnit.Millivolt);
            batteryProfileExt.OCV50 = new ElectricPotential((response[4] << 8) | response[3], ElectricPotentialUnit.Millivolt);
            batteryProfileExt.OCV90 = new ElectricPotential((response[6] << 8) | response[5], ElectricPotentialUnit.Millivolt);
            batteryProfileExt.R10 = new ElectricResistance(((response[8] << 8) | response[7]) / 100.0, ElectricResistanceUnit.Milliohm);
            batteryProfileExt.R50 = new ElectricResistance(((response[10] << 8) | response[9]) / 100.0, ElectricResistanceUnit.Milliohm);
            batteryProfileExt.R90 = new ElectricResistance(((response[12] << 8) | response[11]) / 100.0, ElectricResistanceUnit.Milliohm);

            return batteryProfileExt;
        }

        /// <summary>
        /// TODO: Fill In
        /// </summary>
        public void SetCustomBatteryExtProfile(BatteryExtProfile batteryProfile)
        {
            var data = new byte[17];

            data[0] = (byte)batteryProfile.BatteryChemistry;
            data[1] = (byte)((int)batteryProfile.OCV10.Millivolts & 0xFF);
            data[2] = (byte)(((int)batteryProfile.OCV10.Millivolts >> 8) & 0xFF);
            data[3] = (byte)((int)batteryProfile.OCV50.Millivolts & 0xFF);
            data[4] = (byte)(((int)batteryProfile.OCV50.Millivolts >> 8) & 0xFF);
            data[5] = (byte)((int)batteryProfile.OCV90.Millivolts & 0xFF);
            data[6] = (byte)(((int)batteryProfile.OCV90.Millivolts >> 8) & 0xFF);
            data[7] = (byte)((int)batteryProfile.R10.Milliohms & 0xFF);
            data[8] = (byte)(((int)batteryProfile.R10.Milliohms >> 8) & 0xFF);
            data[9] = (byte)((int)batteryProfile.R50.Milliohms & 0xFF);
            data[10] = (byte)(((int)batteryProfile.R50.Milliohms >> 8) & 0xFF);
            data[11] = (byte)((int)batteryProfile.R90.Milliohms & 0xFF);
            data[12] = (byte)(((int)batteryProfile.R90.Milliohms >> 8) & 0xFF);
            data[13] = 0xFF;
            data[14] = 0xFF;
            data[15] = 0xFF;
            data[16] = 0xFF;

            _piJuice.WriteCommandVerify(PiJuiceCommand.BatteryProfileExt, data);
        }

        /// <summary>
        /// Get battery profile status
        /// </summary>
        public BatteryProfileStatus GetBatteryProfileStatus()
        {
            var batteryProfileStatus = new BatteryProfileStatus();

            var response = _piJuice.ReadCommand(PiJuiceCommand.BatteryProfileId, 1);

            batteryProfileStatus.BatteryOrigin = (response[0] & 0x0F) == 0x0F ? BatteryOrigin.Custom : BatteryOrigin.Predefined;
            batteryProfileStatus.BatteryProfileSource = (BatteryProfileSource)((response[0] >> 4) & 0x03);
            batteryProfileStatus.BatteryProfileValid = ((response[0] >> 6) & 0x01) == 0;

            SelectBatteryProfiles();
            int profileIndex = response[0] & 0x0F;
            if (profileIndex < _batteryProfiles.Count)
            {
                batteryProfileStatus.BatteryProfile = _batteryProfiles[profileIndex];
            }
            else
            {
                batteryProfileStatus.BatteryProfile = "Unknown";
            }

            return batteryProfileStatus;
        }

        /// <summary>
        /// Get how the battery temperature is taken
        /// </summary>
        public BatteryTempSense GetBatteryTempSenseConfig()
        {
            var response = _piJuice.ReadCommand(PiJuiceCommand.BatteryTempSenseConfig, 1);

            if ((response[0] & 0x07) < 0 || (response[0] & 0x07) > 3)
            {
                throw new ArgumentOutOfRangeException("ff");
            }

            return (BatteryTempSense)(response[0] & 0x07);
        }

        /// <summary>
        /// Set how the battery temperature is taken
        /// </summary>
        public void SetBatteryTempSenseConfig(BatteryTempSense batteryTempSense)
        {
            var response = _piJuice.ReadCommand(PiJuiceCommand.BatteryTempSenseConfig, 1);

            _piJuice.WriteCommandVerify(PiJuiceCommand.BatteryTempSenseConfig, new byte[] { (byte)((response[0] & (~0x07)) | (int)batteryTempSense) });
        }

        /// <summary>
        /// TODO: Fill In
        /// </summary>
        public RSOCEstimationType GetRSOCEstimation()
        {
            var response = _piJuice.ReadCommand(PiJuiceCommand.BatteryTempSenseConfig, 1);

            if ((response[0] & 0x30) >> 4 > 2)
            {
                throw new ArgumentOutOfRangeException("ff");
            }

            return (RSOCEstimationType)((response[0] & 0x30) >> 4);
        }

        /// <summary>
        /// TODO: Fill In
        /// </summary>
        public void SetRSOCEstimation(RSOCEstimationType estimationType)
        {
            var response = _piJuice.ReadCommand(PiJuiceCommand.BatteryTempSenseConfig, 1);

            _piJuice.WriteCommandVerify(PiJuiceCommand.BatteryTempSenseConfig, new byte[] { (byte)((response[0] & (~0x30)) | ((int)estimationType) << 4) });
        }

        /// <summary>
        /// Get current battery charging state
        /// </summary>
        public ChargingConfig GetChargingConfig()
        {
            var chargingConfig = new ChargingConfig();

            var response = _piJuice.ReadCommand(PiJuiceCommand.ChargingConfig, 1);

            chargingConfig.Enabled = (response[0] & 0x01) == 0x01;
            chargingConfig.NonVolatile = (response[0] & 0x80) == 0x80;

            return chargingConfig;
        }

        /// <summary>
        /// Set current battery charging state
        /// </summary>
        public void SetChargingConfig(ChargingConfig config)
        {
            var data = new byte[1];

            if (config.Enabled)
            {
                data[0] |= 0x01;
            }

            if (config.NonVolatile)
            {
                data[0] |= 0x80;
            }

            _piJuice.WriteCommandVerify(PiJuiceCommand.ChargingConfig, data);
        }

        /// <summary>
        /// TODO: Fill In
        /// </summary>
        public PowerInput GetPowerInputs()
        {
            var powerInput = new PowerInput();

            var response = _piJuice.ReadCommand(PiJuiceCommand.PowerInputsConfig, 1);

            powerInput.Precedence = (PowerInputType)(response[0] & 0x01);
            powerInput.GPIOIn = (response[0] & 0x02) == 0x02;
            powerInput.NoBatteryTurnOn = (response[0] & 0x04) == 0x04;
            powerInput.USBMicroCurrentLimit = _usbMicroCurrentLimits[(response[0] >> 3) & 0x01];
            powerInput.USBMicroDPM = _usbMicroDPMs[(response[0] >> 4) & 0x07];
            powerInput.NonVolatile = (response[0] & 0x80) == 0x80;

            return powerInput;
        }

        /// <summary>
        /// TODO: Fill In
        /// </summary>
        public void SetPowerInputs(PowerInput powerInput)
        {
            byte nonVolatile = powerInput.NonVolatile ? (byte)0x80 : (byte)0x00;
            byte precedence = powerInput.Precedence == PowerInputType.GPIO5V ? (byte)0x01 : (byte)0x00;
            byte gpioIn = powerInput.GPIOIn ? (byte)0x02 : (byte)0x00;
            byte noBatteryTurnOn = powerInput.NoBatteryTurnOn ? (byte)0x04 : (byte)0x00;
            int index = _usbMicroCurrentLimits.IndexOf(powerInput.USBMicroCurrentLimit);
            if (index == -1)
            {
                throw new ArgumentOutOfRangeException(nameof(powerInput.USBMicroCurrentLimit));
            }

            byte usbMicroLimit = (byte)(index << 3);

            index = _usbMicroDPMs.IndexOf(powerInput.USBMicroDPM);
            if (index == -1)
            {
                throw new ArgumentOutOfRangeException(nameof(powerInput.USBMicroDPM));
            }

            byte usbMicroDPM = (byte)((index & 0x07) << 3);

            var data = new byte[1];
            data[0] = (byte)(nonVolatile | precedence | gpioIn | noBatteryTurnOn | usbMicroLimit | usbMicroDPM);

            _piJuice.WriteCommandVerify(PiJuiceCommand.PowerInputsConfig, data);
        }

        /// <summary>
        /// Get the configuration for the specific LED
        /// </summary>
        public LEDConfig GetLedConfiguration(LED led)
        {
            var response = _piJuice.ReadCommand(PiJuiceCommand.LEDConfig + (int)led, 4);

            if (response[0] < 0 || response[0] > 2)
            {
                throw new ArgumentOutOfRangeException("ff");
            }

            return new LEDConfig
            {
                LedFunction = (LEDFunction)response[0],
                RGB = Color.FromArgb(0, response[1], response[2], response[3])
            };
        }

        /// <summary>
        /// Set the configuration for the specific LED
        /// </summary>
        public void SetLedConfiguration(LED led, LEDConfig ledConfig)
        {
            _piJuice.WriteCommandVerify(PiJuiceCommand.LEDConfig + (int)led, new byte[] { (byte)ledConfig.LedFunction, ledConfig.RGB.R, ledConfig.RGB.G, ledConfig.RGB.B }, 200);
        }

        /// <summary>
        /// Get power regulator mode
        /// </summary>
        public PowerRegulatorMode GetPowerRegulatorMode()
        {
            var response = _piJuice.ReadCommand(PiJuiceCommand.PowerRegulatorConfig, 1);

            return (PowerRegulatorMode)response[0];
        }

        /// <summary>
        /// Set power regulator mode
        /// </summary>
        public void SetPowerRegulatorMode(PowerRegulatorMode powerMode)
        {
            _piJuice.WriteCommandVerify(PiJuiceCommand.PowerRegulatorConfig, new byte[] { (byte)powerMode });
        }

        /// <summary>
        /// Get run pin configuration
        /// </summary>
        public RunPin GetRunPinConfig()
        {
            var response = _piJuice.ReadCommand(PiJuiceCommand.RunPinConfig, 1);

            if (response[0] < 0 || response[0] > 1)
            {
                throw new ArgumentOutOfRangeException("Run pin out of range.");
            }

            return (RunPin)response[0];
        }

        /// <summary>
        /// Set run pin configuration
        /// </summary>
        public void SetRunPinConfig(RunPin runPin)
        {
            _piJuice.WriteCommandVerify(PiJuiceCommand.RunPinConfig, new byte[] { (byte)runPin });
        }

        /// <summary>
        /// TODO: Fill In
        /// </summary>
        private void SelectBatteryProfiles()
        {
            var firmware = _piJuice.GetFirmwareVerion();
            var version = (firmware.Major << 4) + firmware.Minor;

            if (version >= 0x14)
            {
                _batteryProfiles = new List<string> { "PJZERO_1000", "BP7X_1820", "SNN5843_2300", "PJLIPO_12000", "PJLIPO_5000", "PJBP7X_1600", "PJSNN5843_1300", "PJZERO_1200", "BP6X_1400", "PJLIPO_600", "PJLIPO_500" };
            }
            else if (version == 0x13)
            {
                _batteryProfiles = new List<string> { "BP6X_1400", "BP7X_1820", "SNN5843_2300", "PJLIPO_12000", "PJLIPO_5000", "PJBP7X_1600", "PJSNN5843_1300", "PJZERO_1200", "PJZERO_1000", "PJLIPO_600", "PJLIPO_500" };
            }
            else
            {
                _batteryProfiles = new List<string> { "BP6X", "BP7X", "SNN5843", "LIPO8047109" };
            }
        }
    }
}
