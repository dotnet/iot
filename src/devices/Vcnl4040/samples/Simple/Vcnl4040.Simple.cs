// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.IO;
using System.Threading;
using Iot.Device.Vcnl4040;
using Iot.Device.Vcnl4040.Definitions;
using UnitsNet;

PeriodicTimer loopTimer = new(TimeSpan.FromMilliseconds(1000));

/*
 Prepare the binding for use.
 */
I2cDevice i2cDevice = I2cDevice.Create(new I2cConnectionSettings(busId: 1, Vcnl4040Device.DefaultI2cAddress));
Vcnl4040Device vcnl4040;

try
{
    vcnl4040 = new(i2cDevice);
}
catch (IOException ex)
{
    Console.WriteLine("Communication with device using I2C bus is not available");
    Console.WriteLine(ex.Message);
    return;
}
catch (NotSupportedException ex)
{
    Console.WriteLine(ex.Message);
    return;
}

AmbientLightSensor als = vcnl4040.AmbientLightSensor;
ProximitySensor ps = vcnl4040.ProximitySensor;

/*
 Configuration of the Ambient Light Sensor
   - max. range is 3276.7 lux => resulting integration time is 160 ms
   - Interrupts (INT-pin function)
     - lower threshold is 1000 lux => low interrupt event
     - upper threshold is 3000 lux => high interrupt event
     - interrupt hit persistence is 4 (minor false trigger suppression, limit impact on reaction time)
 */
als.Range = AlsRange.Range3276;
AmbientLightInterruptConfiguration alsInterruptConfiguration = new(LowerThreshold: Illuminance.FromLux(1000),
                                                                   UpperThreshold: Illuminance.FromLux(3000),
                                                                   Persistence: AlsInterruptPersistence.Persistence4);
als.EnableInterrupts(alsInterruptConfiguration);

/*
  Enable Ambient Light Sensor operation
 */
als.PowerOn = true;

/*
 Configuration of the Proximity Sensor
   - Emitter, configured for max. power = distance
     - max. IR LED current of 200 mA
     - duty ratio is 1/40
     - integration time is 8T
     - multi pulses is 2 (two pulses per measurement cycle)
   - Receiver, configured for max. sensitivity
     - normal output range (12-bit)
     - cancellation level is 0
     - white channel is enabled
     - sunlight cancellation is disabled
   - Interrupts (INT-pin function)
     - lower threshold is 2000 counts => away interrupt event
     - upper threshold is 4000 counts => close interrupt event
     - trigger both, away and close event
     - interrupt hit persistence is 1 (no trigger suppression, no impact on reaction time)
     - Note: logic output is NOT demonstrated here, as this would exclude ALS interrupts
 */
EmitterConfiguration emitterConfiguration = new(Current: PsLedCurrent.I200mA,
                                                DutyRatio: PsDuty.Duty40,
                                                IntegrationTime: PsIntegrationTime.Time8_0,
                                                MultiPulses: PsMultiPulse.Pulse2);

ReceiverConfiguration receiverConfiguration = new(ExtendedOutputRange: false,
                                                  CancellationLevel: 0,
                                                  WhiteChannelEnabled: true,
                                                  SunlightCancellationEnabled: false);

ProximityInterruptConfiguration proximityInterruptConfiguration = new(LowerThreshold: 2000,
                                                                      UpperThreshold: 4000,
                                                                      Persistence: PsInterruptPersistence.Persistence1,
                                                                      SmartPersistenceEnabled: false,
                                                                      Mode: ProximityInterruptMode.CloseOrAwayInterrupt);

ps.ConfigureEmitter(emitterConfiguration);
ps.ConfigureReceiver(receiverConfiguration);
ps.EnableInterrupts(proximityInterruptConfiguration);

/*
  Enable Proximity Sensor operation
 */
ps.PowerOn = true;

/*
  Sensor loop (~1000 ms cycle time)
    - get current readings from ALS, PS and white channel
    - get and clear interrupt flags
    - display readings and flags
*/
while (!Console.KeyAvailable)
{
    Illuminance alsReading = als.Illuminance;
    int psReading = ps.Distance;
    int psWhiteChannel = ps.WhiteChannelReading;
    InterruptFlags interrupts = vcnl4040.GetAndClearInterruptFlags();

    Console.WriteLine($"Illuminance:   {alsReading}");
    Console.WriteLine($"Proximity:     {psReading}");
    Console.WriteLine($"White channel: {psWhiteChannel}");
    string intFlagsStr = interrupts.ToString();
    Console.WriteLine(intFlagsStr);
    Console.WriteLine(new string('-', intFlagsStr.Length));

    await loopTimer.WaitForNextTickAsync();
}
