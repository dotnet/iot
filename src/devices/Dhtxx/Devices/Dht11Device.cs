// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Units;

namespace Dhtxx.Devices
{
    internal class Dht11Device : IDhtDevice
    {
        public double GetHumidity(byte[] readBuff)
        {
            return readBuff[0] + readBuff[1] * 0.1;
        }

        public Temperature GetTemperature(byte[] readBuff)
        {
            var temp = readBuff[2] + readBuff[3] * 0.1;
            return Temperature.FromCelsius(temp);
        }
    }
}
