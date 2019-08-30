// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Buffers.Binary;

namespace Iot.Device.Seesaw
{
    public partial class Seesaw : IDisposable
    {
        /// <summary>
        /// Reads the value of an analog pin.
        /// </summary>
        /// <param name="pin">The pin number in the devices numbering scheme.</param>
        /// <returns>A value between 0..1023 that represents the analog value.</returns> 
        public ushort AnalogRead(byte pin)
        {
            const int AdcConversionDelayMicroseconds = 500; //500 microSeconds delay between read and write to allow Adc conversion time.

            if (!HasModule(SeesawModule.Gpio))
            {
                throw new InvalidOperationException($"The hardware on I2C Bus {I2cDevice.ConnectionSettings.BusId}, Address 0x{I2cDevice.ConnectionSettings.DeviceAddress:X2} does not support Adafruit SeeSaw ADC functionality");
            }

            if (pin < 2 || pin > 5)
            {
                throw new ArgumentOutOfRangeException("ADC pin must be within 2-5 range.");
            }

            return BinaryPrimitives.ReadUInt16BigEndian(Read(SeesawModule.Adc, SeesawFunction.AdcChannelOffset + pin - 2, 2, AdcConversionDelayMicroseconds));
        }
    }
}
