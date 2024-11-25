﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.Gpio;
using Iot.Device.Common;
using UnitsNet;

namespace Iot.Device.DHTxx
{
    /// <summary>
    /// Temperature and Humidity Sensor DHT21
    /// </summary>
    public class Dht21 : DhtBase
    {
        /// <summary>
        /// Create a DHT22 sensor
        /// </summary>
        /// <param name="pin">The pin number (GPIO number)</param>
        /// <param name="gpioController"><see cref="GpioController"/> related with operations on pins</param>
        /// <param name="shouldDispose">True to dispose the Gpio Controller</param>
        public Dht21(int pin, GpioController? gpioController = null, bool shouldDispose = true)
            : base(pin, gpioController, shouldDispose)
        {
        }

        internal override RelativeHumidity GetHumidity(Span<byte> readBuff) => RelativeHumidity.FromPercent((readBuff[0] << 8 | readBuff[1]) * 0.1);

        internal override Temperature GetTemperature(Span<byte> readBuff)
        {
            var temp = ((readBuff[2] & 0x7F) << 8 | readBuff[3]) * 0.1;
            // if MSB = 1 we have negative temperature
            temp = ((readBuff[2] & 0x80) == 0 ? temp : -temp);

            return Temperature.FromDegreesCelsius(temp);
        }
    }
}
