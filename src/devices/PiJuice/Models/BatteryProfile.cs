// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using UnitsNet;

namespace Iot.Device.PiJuiceDevice.Models
{
    /// <summary>
    /// Battery profile
    /// </summary>
    public class BatteryProfile
    {
        /// <summary>
        /// Charge capacity of battery
        /// </summary>
        public ElectricCharge Capacity { get; set; }

        /// <summary>
        /// [550mA – 2500mA] Constant current that PiJuice battery is charged in current regulation phase of charging process as milliamps
        /// </summary>
        public ElectricCurrent ChargeCurrent { get; set; }

        /// <summary>
        /// [50mA – 400mA] When charging current drops below termination current threshold in voltage regulation phase charging process terminates as milliamps
        /// </summary>
        public ElectricCurrent TerminationCurrent { get; set; }

        /// <summary>
        /// [3500mV – 4440mV] Voltage to which voltage over battery is regulated in voltage regulation phase of charging process as millivolts
        /// </summary>
        public ElectricPotential RegulationVoltage { get; set; }

        /// <summary>
        /// [0mV – 5100mV] Minimum voltage at which battery is fully discharged as millivolts
        /// </summary>
        public ElectricPotential CutOffVoltage { get; set; }

        /// <summary>
        /// Temperature threshold according to JEITA standard below which charging is suspended as celsius
        /// </summary>
        public Temperature TemperatureCold { get; set; }

        /// <summary>
        /// Temperature threshold according to JEITA standard below which charge current is reduced to half of programmed charge current. This threshold should be set above cold temperature as celsius
        /// </summary>
        public Temperature TemperatureCool { get; set; }

        /// <summary>
        /// Temperature threshold according to JEITA standard above which the battery regulation voltage is reduced by 140mV from the programmed regulation voltage. This threshold should be set above cool temperature as celsius
        /// </summary>
        public Temperature TemperatureWarm { get; set; }

        /// <summary>
        /// Temperature threshold according to JEITA standard above which charging is suspended. This threshold should be set above warm temperature as celsius
        /// </summary>
        public Temperature TemperatureHot { get; set; }

        /// <summary>
        /// Thermistor B constant of NTC temperature sensor if it is integrated with battery
        /// </summary>
        public int NegativeTemperatureCoefficientB { get; set; }

        /// <summary>
        /// Nominal thermistor resistance at 25°C of NTC temperature sensor if it is integrated with battery as ohm
        /// </summary>
        public ElectricResistance NegativeTemperatureCoefficientResistance { get; set; }
    }
}
