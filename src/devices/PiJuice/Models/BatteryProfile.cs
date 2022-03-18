// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using UnitsNet;

#pragma warning disable CS1591, CS1572, CS1573

namespace Iot.Device.PiJuiceDevice.Models
{
    /// <summary>
    /// Battery profile
    /// </summary>
    /// <param name="Capacity">Charge capacity of battery.</param>
    /// <param name="ChargeCurrent">[550mA – 2500mA] Constant current that PiJuice battery is charged in current regulation phase of charging process as milliamps.</param>
    /// <param name="TerminationCurrent">[50mA – 400mA] When charging current drops below termination current threshold in voltage regulation phase charging process terminates as milliamps.</param>
    /// <param name="RegulationVoltage">[3500mV – 4440mV] Voltage to which voltage over battery is regulated in voltage regulation phase of charging process as millivolts.</param>
    /// <param name="CutOffVoltage">[0mV – 5100mV] Minimum voltage at which battery is fully discharged as millivolts.</param>
    /// <param name="TemperatureCold">Temperature threshold according to JEITA standard below which charging is suspended as celsius.</param>
    /// <param name="TemperatureCool">Temperature threshold according to JEITA standard below which charge current is reduced to half of programmed charge current. This threshold should be set above cold temperature as celsius.</param>
    /// <param name="TemperatureWarm">Temperature threshold according to JEITA standard above which the battery regulation voltage is reduced by 140mV from the programmed regulation voltage. This threshold should be set above cool temperature as celsius.</param>
    /// <param name="TemperatureHot">Temperature threshold according to JEITA standard above which charging is suspended. This threshold should be set above warm temperature as celsius.</param>
    /// <param name="NegativeTemperatureCoefficientB">Thermistor B constant of NTC temperature sensor if it is integrated with battery.</param>
    /// <param name="NegativeTemperatureCoefficientResistance">Nominal thermistor resistance at 25°C of NTC temperature sensor if it is integrated with battery as ohm.</param>
    public record BatteryProfile(ElectricCharge Capacity, ElectricCurrent ChargeCurrent, ElectricCurrent TerminationCurrent, ElectricPotential RegulationVoltage, ElectricPotential CutOffVoltage, Temperature TemperatureCold, Temperature TemperatureCool, Temperature TemperatureWarm, Temperature TemperatureHot, int NegativeTemperatureCoefficientB, ElectricResistance NegativeTemperatureCoefficientResistance);
}
