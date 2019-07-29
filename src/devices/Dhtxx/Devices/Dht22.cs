// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Device.Gpio;
using Iot.Units;

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
        public Dht22(int pin, PinNumberingScheme pinNumberingScheme = PinNumberingScheme.Logical)
            : base(pin, pinNumberingScheme)
        { }

        internal override double GetHumidity(byte[] readBuff)
        {
            return (readBuff[0] << 8 | readBuff[1]) * 0.1;
        }

        internal override Temperature GetTemperature(byte[] readBuff)
        {
            var temp = (readBuff[2] & 0x7F) + readBuff[3] * 0.1;
            // if MSB = 1 we have negative temperature
            temp = ((readBuff[2] & 0x80) == 0 ? temp : -temp);

            return Temperature.FromCelsius(temp);
        }
    }
}
