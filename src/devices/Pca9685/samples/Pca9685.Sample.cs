// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.I2c;
using System.Device.I2c.Devices;

namespace Iot.Device.Pca9685.Samples
{
    /// <summary>
    /// This sample is for Raspberry Pi Model 3B+
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            var busId = 1;  // /dev/i2c-1

            var deviceAddress_fixed = 0x40;
            var deviceAddress_selectable = 0b000000;    // A5 A4 A3 A2 A1 A0
            var deviceAddress = deviceAddress_fixed | deviceAddress_selectable;

            var settings = new I2cConnectionSettings(busId, deviceAddress);
            var device = I2cDevice.Create(settings);

            using (var pca9685 = new Pca9685(device))
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"PCA9685 is ready on I2C bus {device.ConnectionSettings.BusId} with address {device.ConnectionSettings.DeviceAddress}");

                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("Command:  F {freq_hz}             set PWM frequency");
                Console.WriteLine("          P {prescale}            set PRE_SCALE register");
                Console.WriteLine("          S {off}                 set off step with on step is 0 to all channels");
                Console.WriteLine("          S {on} {off}            set on step and off step to all channels");
                Console.WriteLine("          S {on} {off} {channel}  set on step and off step to specified channel");

                Console.WriteLine();
                while (true)
                {
                    try
                    {
                        Console.ResetColor();
                        Console.Write("> ");
                        var command = Console.ReadLine().ToLower().Split(' ');

                        switch (command[0][0])
                        {
                            case 'f':   // set PWM frequency
                                {
                                    var freq = double.Parse(command[1]);
                                    pca9685.PwmFrequency = freq;

                                    Console.ForegroundColor = ConsoleColor.Green;
                                    Console.WriteLine($"PWM Frequency has been set to about {pca9685.PwmFrequency}Hz with prescale is {pca9685.Prescale}");
                                    break;
                                }
                            case 'p':   // set PRE_SCALE register
                                {
                                    var prescale = (byte)int.Parse(command[1]);
                                    pca9685.Prescale = prescale;

                                    Console.ForegroundColor = ConsoleColor.Green;
                                    Console.WriteLine($"PWM Frequency has been set to about {pca9685.PwmFrequency}Hz with prescale is {pca9685.Prescale}");
                                    break;
                                }
                            case 's':   // set PWM steps
                                {
                                    switch (command.Length)
                                    {
                                        case 2: // 1 parameter : set off step with on step is 0 to all channels
                                            {
                                                var off = int.Parse(command[1]);
                                                pca9685.SetPwm(0, off);

                                                Console.ForegroundColor = ConsoleColor.Green;
                                                Console.WriteLine($"PWM pulse width has been set to {off}/4096");
                                                break;
                                            }
                                        case 3: // 2 parametes : set on step and off step to all channels
                                            {
                                                var on = int.Parse(command[1]);
                                                var off = int.Parse(command[2]);
                                                pca9685.SetPwm(on, off);

                                                Console.ForegroundColor = ConsoleColor.Green;
                                                Console.WriteLine($"PWM pulse pull up at step {on} and pull down at step {off} on all channels");
                                                break;
                                            }
                                        case 4: // 3 parametes : set on step and off step to specified channel
                                            {
                                                var on = int.Parse(command[1]);
                                                var off = int.Parse(command[2]);
                                                var channel = int.Parse(command[3]);
                                                pca9685.SetPwm(on, off, channel);

                                                Console.ForegroundColor = ConsoleColor.Green;
                                                Console.WriteLine($"PWM pulse pull up at step {on} and pull down at step {off} on channel {channel}");
                                                break;
                                            }
                                    }
                                    break;
                                }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(ex.GetBaseException().Message);
                        Console.ResetColor();
                    }
                }
            }
        }
    }
}
