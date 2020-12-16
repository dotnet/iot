// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.Gpio;
using System.Device.I2c;
using UnitsNet;

namespace Iot.Device.DHTxx
{
    /// <summary>
    /// Temperature and Humidity Sensor DHT12
    /// </summary>
    public class Dht12 : DhtBase
    {
        /// <summary>
        /// DHT12 Default I2C Address
        /// </summary>
        public const byte DefaultI2cAddress = 0x5C;

        /// <summary>
        /// Create a DHT12 sensor
        /// </summary>
        /// <param name="pin">The pin number (GPIO number)</param>
        /// <param name="pinNumberingScheme">The GPIO pin numbering scheme</param>
        /// <param name="gpioController"><see cref="GpioController"/> related with operations on pins</param>
        /// <param name="shouldDispose">True to dispose the Gpio Controller</param>
        public Dht12(int pin, PinNumberingScheme pinNumberingScheme = PinNumberingScheme.Logical, GpioController? gpioController = null, bool shouldDispose = true)
            : base(pin, pinNumberingScheme, gpioController, shouldDispose)
        {
        }

        /// <summary>
        /// Create a DHT12 sensor through I2C
        /// </summary>
        /// <param name="i2cDevice">I2C Device</param>
        public Dht12(I2cDevice i2cDevice)
            : base(i2cDevice)
        {
        }

        internal override RelativeHumidity GetHumidity(byte[] readBuff) => RelativeHumidity.FromPercent(readBuff[0] + readBuff[1] * 0.1);

        internal override Temperature GetTemperature(byte[] readBuff)
        {
            var temp = readBuff[2] + (readBuff[3] & 0x7F) * 0.1;
            // if MSB = 1 we have negative temperature
            temp = (readBuff[3] & 0x80) == 0 ? temp : -temp;

            return Temperature.FromDegreesCelsius(temp);
        }
    }
}
