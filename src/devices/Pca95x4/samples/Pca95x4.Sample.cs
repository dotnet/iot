// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Gpio;
using System.Device.I2c;
using System.Threading;

namespace Iot.Device.Pca95x4.Samples
{
    class Program
    {
        private static readonly int s_deviceAddress = 0x38;

        static void Main(string[] args)
        {
            Console.WriteLine("Hello Pca95x4 Sample!");

            using (Pca95x4 pca95x4 = GetPca95x4Device())
            {
                //CycleOutputBits(pca95x4);
                //ReadInputPort(pca95x4);
                CheckInputRegisterPolarityInversion(pca95x4);
            }
        }

        private static Pca95x4 GetPca95x4Device()
        {
            var i2cConnectionSettings = new I2cConnectionSettings(1, s_deviceAddress);
            var i2cDevice = I2cDevice.Create(i2cConnectionSettings);
            return new Pca95x4(i2cDevice);
        }

        private static void CycleOutputBits(Pca95x4 pca95x4)
        {
            pca95x4.Write(Register.Configuration, 0x00);  // Make all outputs.
            pca95x4.Write(Register.OutputPort, 0xFF);  // Set all outputs.

            for (int bitNumber = 0; bitNumber < 8; bitNumber++)
            {
                pca95x4.WriteBit(Register.OutputPort, bitNumber, false);  // Clear output.
                Thread.Sleep(500);
                pca95x4.WriteBit(Register.OutputPort, bitNumber, true);  // Set output.
            }
        }

        private static void ReadInputPort(Pca95x4 pca95x4)
        {
            pca95x4.Write(Register.Configuration, 0xFF);  // Make all inputs.
            byte data = pca95x4.Read(Register.InputPort);
            Console.WriteLine($"Input Port: 0x{data:X2}");
        }

        private static void CheckInputRegisterPolarityInversion(Pca95x4 pca95x4)
        {
            pca95x4.Write(Register.Configuration, 0xFF);  // Make all inputs.
            byte data = pca95x4.Read(Register.InputPort);
            Console.WriteLine($"Input Register: 0x{data:X2}");
            pca95x4.InvertInputRegisterPolarity(true);
            data = pca95x4.Read(Register.InputPort);
            Console.WriteLine($"Input Register Polarity Inverted: 0x{data:X2}");
            pca95x4.InvertInputRegisterPolarity(false);
            data = pca95x4.Read(Register.InputPort);
            Console.WriteLine($"Input Register: 0x{data:X2}");
        }
    }
}
