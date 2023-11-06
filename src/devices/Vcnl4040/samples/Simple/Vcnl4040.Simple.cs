// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System;
using System.Device.I2c;
using System.Threading;
using Iot.Device.Vcnl4040;
using Iot.Device.Vcnl4040.Common.Defnitions;
using UnitsNet;

PeriodicTimer loopTimer = new PeriodicTimer(TimeSpan.FromMilliseconds(200));

/*
 Prepare of the binding for use.
 */
I2cDevice i2cDevice = I2cDevice.Create(new I2cConnectionSettings(busId: 1, Vcnl4040Device.DefaultI2cAddress));
Vcnl4040Device vcnl4040 = new Vcnl4040Device(i2cDevice);

/*
 Configuration of the Ambient Light Sensor
   - max. Range is 3276.7 lux => resulting integration time is 160 ms
   - lower interrupt threshold is 3000 lux
   - upper interrupt threshold is 5000 lux
   - interrupt hit persistence is 4
 */
AmbientLightSensor als = vcnl4040.AmbientLightSensor;
als.Range = AlsRange.Range_3276;
als.ConfigureInterrupt(Illuminance.FromLux(3000),
                       Illuminance.FromLux(5000),
                       AlsInterruptPersistence.Persistence4);

/*
  Enable Ambient Light Sensor operation
    - turn sensor on
    - turn interrupts on
 */
als.PowerOn = true;
als.InterruptEnabled = true;

/*
  Sensor loop
    - get current readings
    - get and clear interrupt flags
    - display
    - wait ~200 ms
*/
while (!Console.KeyAvailable)
{
    Illuminance alsReading = als.Reading;
    InterruptFlags interrupts = vcnl4040.GetAndClearInterruptFlags();

    Console.WriteLine($"Illuminance: {alsReading}");
    Console.WriteLine(interrupts);
    Console.WriteLine("-----------------------------------------");

    await loopTimer.WaitForNextTickAsync();
}
