using Iot.Device.Pca9685;
using System;

namespace Iot.Device.Pca9685.Samples
{
    class Program
    {
        static void Main(string[] args)
        {
            var pca9685 = new Pca9685();

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"PCA9685 is ready on I2C bus {pca9685.BusId} with address {pca9685.Address}");

            while (true)
            {
                Console.WriteLine();

                try
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("Command:   F {freq_hz} | P {prescale} | S {off} | S {on} {off} | S {on} {off} {channel}");
                    Console.ResetColor();

                    var command = Console.ReadLine().ToLower().Split(' ');
                    switch (command[0][0])
                    {
                        case 'f':
                            {
                                var freq = double.Parse(command[1]);
                                pca9685.SetPwmFrequency(freq);

                                var prescale = pca9685.GetPrescale(freq);
                                freq = pca9685.GetFreq(prescale);
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine($"PWM Frequency has been set to about {freq}Hz with prescale is {prescale}");
                                break;
                            }
                        case 'p':
                            {
                                var prescale = (byte)int.Parse(command[1]);
                                pca9685.SetPwmFrequency(prescale);

                                var freq = pca9685.GetFreq(prescale);
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine($"PWM Frequency has been set to about {freq}Hz with prescale is {prescale}");
                                break;
                            }
                        case 's':
                            {
                                switch (command.Length)
                                {
                                    case 2:
                                        {
                                            var off = int.Parse(command[1]);
                                            pca9685.SetPwm(0, off);

                                            Console.ForegroundColor = ConsoleColor.Green;
                                            Console.WriteLine($"PWM pulse width has been set to {off}/4096");
                                            break;
                                        }
                                    case 3:
                                        {
                                            var on = int.Parse(command[1]);
                                            var off = int.Parse(command[2]);
                                            pca9685.SetPwm(on, off);

                                            Console.ForegroundColor = ConsoleColor.Green;
                                            Console.WriteLine($"PWM pulse pull up at {on} and pull down at {off}");
                                            break;
                                        }
                                    case 4:
                                        {
                                            var on = int.Parse(command[1]);
                                            var off = int.Parse(command[2]);
                                            var channel = int.Parse(command[3]);
                                            pca9685.SetPwm(on, off, channel);

                                            Console.ForegroundColor = ConsoleColor.Green;
                                            Console.WriteLine($"PWM pulse pull up at {on} and pull down at {off} on channel {channel}");
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
