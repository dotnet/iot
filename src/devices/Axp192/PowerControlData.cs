// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitsNet;

namespace Iot.Device.Axp192
{
    /// <summary>
    /// This class provides a convenient way of retrieving important status
    /// information from the Axp192 in a single call
    /// </summary>
    public class PowerControlData
    {
        private const double EmptyBatteryVoltage = 3.2;

        /// <summary>
        /// The internal temperature of the power controller
        /// </summary>
        public Temperature Temperature { get; init; }

        /// <summary>
        /// The input current, when powered via bus
        /// </summary>
        public ElectricCurrent InputCurrent { get; init; }

        /// <summary>
        /// The input voltage, when powered via bus
        /// </summary>
        public ElectricPotential InputVoltage { get; init; }

        /// <summary>
        /// The input status. Use this to determine the current power source
        /// </summary>
        public PowerStatus InputStatus { get; init; }

        /// <summary>
        /// The USB input voltage
        /// </summary>
        public ElectricPotential InputUsbVoltage { get; init; }

        /// <summary>
        /// The USB input current
        /// </summary>
        public ElectricCurrent InputUsbCurrent { get; init; }

        /// <summary>
        /// The charging current of the battery
        /// </summary>
        public ElectricCurrent BatteryChargingCurrent { get; init; }

        /// <summary>
        /// The status of the battery
        /// </summary>
        public BatteryStatus BatteryChargingStatus { get; init; }

        /// <summary>
        /// The battery discharge current
        /// </summary>
        public ElectricCurrent BatteryDischargeCurrent { get; init; }

        /// <summary>
        /// The power currently delivered by the battery
        /// </summary>
        public Power BatteryInstantaneousPower { get; init; }

        /// <summary>
        /// The current battery voltage
        /// </summary>
        public ElectricPotential BatteryVoltage { get; init; }

        /// <summary>
        /// Indicates whether a battery is present
        /// </summary>
        public bool BatteryPresent { get; init; }

        /// <summary>
        /// Returns the charge level of the battery.
        /// Only valid if a battery is present.
        /// </summary>
        public Ratio BatteryLevel
        {
            get
            {
                ElectricPotential value = BatteryVoltage - ElectricPotential.FromVolts(EmptyBatteryVoltage);
                return Ratio.FromPercent(MathExtensions.Clamp(value.Volts * 100, 0, 100));
            }
        }

        /// <summary>
        /// Returns the status of the battery as user-readable english text
        /// </summary>
        /// <returns>A user-readable string describing the status of the battery</returns>
        public string GetBatteryStatusAsText()
        {
            var flags = BatteryChargingStatus;
            List<string> parts = new List<string>();
            if (!BatteryPresent)
            {
                parts.Add("No Battery present");
            }
            else if ((flags & BatteryStatus.BatteryConnected) == BatteryStatus.None)
            {
                // Probably corresponds to the bit above, therefore only print either message
                parts.Add("No battery connected");
            }

            if ((flags & BatteryStatus.Overheated) != BatteryStatus.None)
            {
                parts.Add("Power controller overheated");
            }

            if ((flags & BatteryStatus.Charging) != BatteryStatus.None)
            {
                parts.Add("Charging");
            }

            if ((flags & BatteryStatus.BatteryActivationMode) != BatteryStatus.None)
            {
                parts.Add("Active");
            }
            else
            {
                parts.Add("Inactive");
            }

            if ((flags & BatteryStatus.ChargingCurrentLessThanExpected) != BatteryStatus.None)
            {
                parts.Add("Charging current low");
            }
            else
            {
                parts.Add("Charging current as expected");
            }

            // Bit 1 has nothing to do with the battery, but is dependent on the wiring and determines the powering sequence
            return string.Join(", ", parts);
        }

        /// <summary>
        /// Returns a printable representation of this structure
        /// </summary>
        /// <returns>A multiline-string</returns>
        public override string ToString()
        {
            return @$"Chip Temperature: {Temperature}
Input Current: {InputCurrent}
Input Voltage: {InputVoltage}
Input Status: {InputStatus}
Input Usb Voltage: {InputUsbVoltage}
Input Usb Current: {InputUsbCurrent}
Battery Charging Current: {BatteryChargingCurrent}
Battery Charging Status: {GetBatteryStatusAsText()}
Battery Discharge Current: {BatteryDischargeCurrent}
Battery Instantaneous Power: {BatteryInstantaneousPower}
Battery Voltage: {BatteryVoltage} ({BatteryLevel.Percent:F0}%)";
        }
    }
}
