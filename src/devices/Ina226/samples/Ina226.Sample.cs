// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Ina226;

// Default i2c address for the INA226
const byte INA226_I2cAddress = 0x40;

// Default i2c bus on raspberry pi
const byte INA226_I2cBus = 0x1;

using (Ina226 ina226 = new(new System.Device.I2c.I2cConnectionSettings(INA226_I2cBus, INA226_I2cAddress)))
{
    // reset device (resets all values to default)
    ina226.Reset();

    // Set number of samples to average in this example to 4
    ina226.SamplesAveraged = Ina226SamplesAveraged.Quantity_4;

    // Set shunt conversion time of a sample to 2,116 microseconds
    ina226.ShuntConvTime = Ina226ShuntConvTime.Time2116us;

    // Set bus voltage conversion time of a sample to 2,116 microseconds
    ina226.BusConvTime = Ina226BusVoltageConvTime.Time2116us;

    // Set the alert pin mode (When shunt voltage over the trigger limit value then trigger pin will by default be pulled low unless alert polarity has been set to true)
    ina226.AlertMode = Ina226AlertMode.ShuntOverVoltage;

    // Set the alert trigger limit value (14.25 volts in this case)
    ina226.AlertLimitVoltage = UnitsNet.ElectricPotential.FromVolts(14.25);

    // Setting the alert polarity to trigger high instead of default trigger low. (false)
    ina226.AlertPolarity = true;

    // Set Calibration
    ina226.SetCalibration(0.00075, 109.226); // 109.226 being the max current in amps at the max mv reading capability of the INA226, so 100 amp * (81.92/75)

    do
    {
        while (!System.Console.KeyAvailable)
        {
            System.Console.Clear();
            System.Console.WriteLine("Press ESC to stop");
            System.Console.WriteLine();

            // Read and print to console the values stored in the INA226 Registers
            System.Console.WriteLine($"Bus Voltage: {ina226.ReadBusVoltage().Volts}v");
            System.Console.WriteLine($"Shunt Voltage: {ina226.ReadShuntVoltage().Millivolts}mV");
            System.Console.WriteLine($"Current: {ina226.ReadCurrent().Amperes} amps");
            System.Console.WriteLine($"Power: {ina226.ReadPower().Watts} watts");
            System.Console.WriteLine($"Alert Mode: {ina226.AlertMode}");
            System.Console.WriteLine($"Alert Trigger Value: {ina226.AlertLimitVoltage.Volts}v");
            System.Console.WriteLine($"Alert Latch Enabled Flag:{ina226.AlertLatchEnable}");
            System.Console.WriteLine($"Alert Polarity: {ina226.AlertPolarity}");

            System.Threading.Thread.Sleep(1000);
        }
    }
    while (System.Console.ReadKey(true).Key != System.ConsoleKey.Escape);
}
