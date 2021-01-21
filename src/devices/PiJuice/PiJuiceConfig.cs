// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Drawing;
using Iot.Device.PiJuiceDevice.Models;
using UnitsNet;

namespace Iot.Device.PiJuiceDevice
{
    /// <summary>
    /// PiJuiceConfig class to support status of the PiJuice
    /// </summary>
    public class PiJuiceConfig
    {
        private readonly PiJuice _piJuice;

        private readonly List<ElectricCurrent> _usbMicroCurrentLimits = new List<ElectricCurrent> { ElectricCurrent.FromAmperes(1.5), ElectricCurrent.FromAmperes(2.5) };
        private readonly List<ElectricPotential> _usbMicroDpm = new List<ElectricPotential>();

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
                _usbMicroDpm.Add(ElectricPotential.FromVolts(4.2 + 0.8 * i));
            }
        }

        /// <summary>
        /// Get battery profile
        /// </summary>
        /// <returns>Battery profile</returns>
        public BatteryProfile GetBatteryProfile()
        {
            byte[] response = _piJuice.ReadCommand(PiJuiceCommand.BatteryProfile, 14);
            return new BatteryProfile(
                ElectricCharge.FromMilliampereHours(BinaryPrimitives.ReadInt16LittleEndian(response)),
                ElectricCurrent.FromMilliamperes(response[2] * 75 + 550),
                ElectricCurrent.FromMilliamperes(response[3] * 50 + 50),
                ElectricPotential.FromMillivolts(response[4] * 20 + 3500),
                ElectricPotential.FromMillivolts(response[5] * 20),
                Temperature.FromDegreesCelsius(response[6]),
                Temperature.FromDegreesCelsius(response[7]),
                Temperature.FromDegreesCelsius(response[8]),
                Temperature.FromDegreesCelsius(response[9]),
                (response[11] << 8) | response[10],
                ElectricResistance.FromOhms(((response[13] << 8) | response[12]) * 10));
        }

        /// <summary>
        /// Set a custom battery profile
        /// </summary>
        /// <param name="batteryProfile">Custom battery profile</param>
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
            data[10] = (byte)(batteryProfile.NegativeTemperatureCoefficientB & 0xFF);
            data[11] = (byte)((batteryProfile.NegativeTemperatureCoefficientB >> 8) & 0xFF);
            int ntcResistance = (int)batteryProfile.NegativeTemperatureCoefficientResistance.Ohms / 10;
            data[12] = (byte)(ntcResistance & 0xFF);
            data[13] = (byte)((ntcResistance >> 8) & 0xFF);

            _piJuice.WriteCommandVerify(PiJuiceCommand.BatteryProfile, data);
        }

        /// <summary>
        /// Get Battery extended profile
        /// </summary>
        /// <returns>Battery extended profile</returns>
        public BatteryExtendedProfile GetBatteryExtProfile()
        {
            byte[] response = _piJuice.ReadCommand(PiJuiceCommand.BatteryExtendedProfile, 17);
            BatteryChemistry chemistry;
            if (response[0] < 2)
            {
                chemistry = (BatteryChemistry)response[0];
            }
            else
            {
                chemistry = BatteryChemistry.Unknown;
            }

            return new BatteryExtendedProfile(
                chemistry,
                ElectricPotential.FromMillivolts((response[2] << 8) | response[1]),
                ElectricPotential.FromMillivolts((response[4] << 8) | response[3]),
                ElectricPotential.FromMillivolts((response[6] << 8) | response[5]),
                ElectricResistance.FromMilliohms(((response[8] << 8) | response[7]) / 100.0),
                ElectricResistance.FromMilliohms(((response[10] << 8) | response[9]) / 100.0),
                ElectricResistance.FromMilliohms(((response[12] << 8) | response[11]) / 100.0));
        }

        /// <summary>
        /// Set custom battery extended profile
        /// </summary>
        /// <param name="batteryProfile">Custom battery extended profile</param>
        public void SetCustomBatteryExtProfile(BatteryExtendedProfile batteryProfile)
        {
            var data = new byte[17];

            data[0] = (byte)batteryProfile.BatteryChemistry;
            data[1] = (byte)((int)batteryProfile.OpenCircuitVoltage10Percent.Millivolts & 0xFF);
            data[2] = (byte)(((int)batteryProfile.OpenCircuitVoltage10Percent.Millivolts >> 8) & 0xFF);
            data[3] = (byte)((int)batteryProfile.OpenCircuitVoltage50Percent.Millivolts & 0xFF);
            data[4] = (byte)(((int)batteryProfile.OpenCircuitVoltage50Percent.Millivolts >> 8) & 0xFF);
            data[5] = (byte)((int)batteryProfile.OpenCircuitVoltage90Percent.Millivolts & 0xFF);
            data[6] = (byte)(((int)batteryProfile.OpenCircuitVoltage90Percent.Millivolts >> 8) & 0xFF);
            data[7] = (byte)((int)batteryProfile.InternalResistance10Percent.Milliohms & 0xFF);
            data[8] = (byte)(((int)batteryProfile.InternalResistance10Percent.Milliohms >> 8) & 0xFF);
            data[9] = (byte)((int)batteryProfile.InternalResistance50Percent.Milliohms & 0xFF);
            data[10] = (byte)(((int)batteryProfile.InternalResistance50Percent.Milliohms >> 8) & 0xFF);
            data[11] = (byte)((int)batteryProfile.InternalResistance90Percent.Milliohms & 0xFF);
            data[12] = (byte)(((int)batteryProfile.InternalResistance90Percent.Milliohms >> 8) & 0xFF);
            data[13] = 0xFF;
            data[14] = 0xFF;
            data[15] = 0xFF;
            data[16] = 0xFF;

            _piJuice.WriteCommandVerify(PiJuiceCommand.BatteryExtendedProfile, data);
        }

        /// <summary>
        /// Get battery profile status
        /// </summary>
        /// <returns>Battery profile status</returns>
        public BatteryProfileStatus GetBatteryProfileStatus()
        {
            byte[] response = _piJuice.ReadCommand(PiJuiceCommand.BatteryProfileStatus, 1);
            SelectBatteryProfiles();
            int profileIndex = response[0] & 0x0F;
            string batteryProfile;
            if (profileIndex < _batteryProfiles.Count)
            {
                batteryProfile = _batteryProfiles[profileIndex];
            }
            else
            {
                batteryProfile = "Unknown";
            }

            return new BatteryProfileStatus(
                batteryProfile,
                (BatteryProfileSource)((response[0] >> 4) & 0x03),
                ((response[0] >> 6) & 0x01) == 0,
                (response[0] & 0x0F) == 0x0F ? BatteryOrigin.Custom : BatteryOrigin.Predefined);
        }

        /// <summary>
        /// Get how the battery temperature is taken
        /// </summary>
        /// <returns>Battery temperature sensor configuration</returns>
        public BatteryTemperatureSense GetBatteryTemperatureSenseConfig()
        {
            var response = _piJuice.ReadCommand(PiJuiceCommand.BatteryTemperatureSensorConfig, 1);

            if ((response[0] & 0x07) < 0 || (response[0] & 0x07) > 3)
            {
                throw new ArgumentOutOfRangeException("Battery temperature sensor configuration out of range");
            }

            return (BatteryTemperatureSense)(response[0] & 0x07);
        }

        /// <summary>
        /// Set how the battery temperature is taken
        /// </summary>
        /// <param name="batteryTemperatureSense">Determine how the battery temperature is taken</param>
        public void SetBatteryTemperatureSenseConfig(BatteryTemperatureSense batteryTemperatureSense)
        {
            var response = _piJuice.ReadCommand(PiJuiceCommand.BatteryTemperatureSensorConfig, 1);

            _piJuice.WriteCommandVerify(PiJuiceCommand.BatteryTemperatureSensorConfig, new byte[] { (byte)((response[0] & (~0x07)) | (int)batteryTemperatureSense) });
        }

        /// <summary>
        /// Get battery relative state-of-health estimation type
        /// </summary>
        /// <returns>Battery relative state-of-health estimation type</returns>
        public RelativeStateOfChangeEstimationType GetRelativeStateOfChangeEstimation()
        {
            var response = _piJuice.ReadCommand(PiJuiceCommand.BatteryTemperatureSensorConfig, 1);

            return (RelativeStateOfChangeEstimationType)((response[0] & 0x30) >> 4);
        }

        /// <summary>
        /// Set battery relative state-of-health estimation type
        /// </summary>
        /// <param name="estimationType">Battery relative state-of-health estimation type</param>
        public void SetRelativeStateOfChangeEstimation(RelativeStateOfChangeEstimationType estimationType)
        {
            var response = _piJuice.ReadCommand(PiJuiceCommand.BatteryTemperatureSensorConfig, 1);

            _piJuice.WriteCommandVerify(PiJuiceCommand.BatteryTemperatureSensorConfig, new byte[] { (byte)((response[0] & (~0x30)) | ((int)estimationType) << 4) });
        }

        /// <summary>
        /// Get current battery charging state
        /// </summary>
        /// <returns>Battery charging configuration</returns>
        public ChargingConfig GetChargingConfig()
        {
            byte[] response = _piJuice.ReadCommand(PiJuiceCommand.ChargingConfig, 1);

            return new ChargingConfig(
                (response[0] & 0x01) == 0x01,
                (response[0] & 0x80) == 0x80);
        }

        /// <summary>
        /// Set current battery charging state
        /// </summary>
        /// <param name="config">Battery charging configuration</param>
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
        /// Get power inputs
        /// </summary>
        /// <returns>Power inputs</returns>
        public PowerInput GetPowerInputs()
        {
            byte[] response = _piJuice.ReadCommand(PiJuiceCommand.PowerInputsConfig, 1);
            return new PowerInput(
                (PowerInputType)(response[0] & 0x01),
                (response[0] & 0x02) == 0x02,
                (response[0] & 0x04) == 0x04,
                _usbMicroCurrentLimits[(response[0] >> 3) & 0x01],
                _usbMicroDpm[(response[0] >> 4) & 0x07],
                (response[0] & 0x80) == 0x80);
        }

        /// <summary>
        /// Set power inputs
        /// </summary>
        /// <param name="powerInput">Power inputs</param>
        public void SetPowerInputs(PowerInput powerInput)
        {
            byte nonVolatile = powerInput.NonVolatile ? (byte)0x80 : (byte)0x00;
            byte precedence = powerInput.Precedence == PowerInputType.Gpio5Volt ? (byte)0x01 : (byte)0x00;
            byte gpioIn = powerInput.GpioIn ? (byte)0x02 : (byte)0x00;
            byte noBatteryTurnOn = powerInput.NoBatteryTurnOn ? (byte)0x04 : (byte)0x00;
            int index = _usbMicroCurrentLimits.IndexOf(powerInput.UsbMicroCurrentLimit);
            if (index == -1)
            {
                throw new ArgumentOutOfRangeException(nameof(powerInput.UsbMicroCurrentLimit));
            }

            byte usbMicroLimit = (byte)(index << 3);

            index = _usbMicroDpm.IndexOf(powerInput.UsbMicroDynamicPowerManagement);
            if (index == -1)
            {
                throw new ArgumentOutOfRangeException(nameof(powerInput.UsbMicroDynamicPowerManagement));
            }

            byte usbMicroDPM = (byte)((index & 0x07) << 3);

            var data = new byte[1];
            data[0] = (byte)(nonVolatile | precedence | gpioIn | noBatteryTurnOn | usbMicroLimit | usbMicroDPM);

            _piJuice.WriteCommandVerify(PiJuiceCommand.PowerInputsConfig, data);
        }

        /// <summary>
        /// Get the configuration for the specific Led
        /// </summary>
        /// <param name="led">Led to get configuration for</param>
        /// <returns>Led Configuration</returns>
        public LedConfig GetLedConfiguration(Led led)
        {
            var response = _piJuice.ReadCommand(PiJuiceCommand.LedConfig + (int)led, 4);

            if (response[0] < 0 || response[0] > 2)
            {
                throw new ArgumentOutOfRangeException("Led function type out of range");
            }

            return new LedConfig(
                led,
                (LedFunction)response[0],
                Color.FromArgb(0, response[1], response[2], response[3]));
        }

        /// <summary>
        /// Set the configuration for the specific Led
        /// </summary>
        /// <param name="ledConfig">Led configuration</param>
        public void SetLedConfiguration(LedConfig ledConfig)
        {
            _piJuice.WriteCommandVerify(PiJuiceCommand.LedConfig + (int)ledConfig.Led, new byte[] { (byte)ledConfig.LedFunction, ledConfig.Color.R, ledConfig.Color.G, ledConfig.Color.B }, 200);
        }

        /// <summary>
        /// Get power regulator mode
        /// </summary>
        /// <returns>Power regulator mode</returns>
        public PowerRegulatorMode GetPowerRegulatorMode()
        {
            var response = _piJuice.ReadCommand(PiJuiceCommand.PowerRegulatorConfig, 1);

            return (PowerRegulatorMode)response[0];
        }

        /// <summary>
        /// Set power regulator mode
        /// </summary>
        /// <param name="powerMode">Power regulator mode</param>
        public void SetPowerRegulatorMode(PowerRegulatorMode powerMode)
        {
            _piJuice.WriteCommandVerify(PiJuiceCommand.PowerRegulatorConfig, new byte[] { (byte)powerMode });
        }

        /// <summary>
        /// Get run pin configuration
        /// </summary>
        /// <returns>Run pin configuration</returns>
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
        /// <param name="runPin">Run pin configuration</param>
        public void SetRunPinConfig(RunPin runPin)
        {
            _piJuice.WriteCommandVerify(PiJuiceCommand.RunPinConfig, new byte[] { (byte)runPin });
        }

        /// <summary>
        /// Selects which preset battery profiles to use based on the PiJuice firmware version
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
