// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Device.Gpio;
using UnitsNet;

namespace Iot.Device.DHTxx
{
    /// <summary>
    /// Temperature and Humidity Sensor DHT22
    /// </summary>
    public class Dht22 : DhtBase
    {
        /// <summary>
        /// Create a DHT22 sensor
        /// </summary>
        /// <param name="pin">The pin number (GPIO number)</param>
        /// <param name="pinNumberingScheme">The GPIO pin numbering scheme</param>
        /// <param name="gpioController"><see cref="GpioController"/> related with operations on pins</param>
        /// <param name="shouldDispose">True to dispose the Gpio Controller</param>
        public Dht22(int pin, PinNumberingScheme pinNumberingScheme = PinNumberingScheme.Logical, GpioController gpioController = null, bool shouldDispose = true)
            : base(pin, pinNumberingScheme, gpioController, shouldDispose)
        {
        }

        internal override Ratio GetHumidity(byte[] readBuff)
        {
            return Ratio.FromPercent((readBuff[0] << 8 | readBuff[1]) * 0.1);
        }

        internal override Temperature GetTemperature(byte[] readBuff)
        {
            var temp = ((readBuff[2] & 0x7F) * 256 + readBuff[3]) * 0.1;
            // if MSB = 1 we have negative temperature
            temp = ((readBuff[2] & 0x80) == 0 ? temp : -temp);

            return Temperature.FromDegreesCelsius(temp);
        }
    }
}
