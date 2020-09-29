// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.Amg88xx;
using Xunit;

namespace Iot.Device.Amg88xx.Tests
{
    public class Amg88xxTests
    {
        [Fact]
        public void GetSensorTemperature()
        {
            I2cTestDevice i2cDevice = new I2cTestDevice();
            Amg88xx sensor = new Amg88xx(i2cDevice);

            // target temperature is 25°C which is equivalent to 0x190
            i2cDevice.DataToRead.Enqueue(0x90);
            i2cDevice.DataToRead.Enqueue(0x01);

            Assert.Equal(25, sensor.GetSensorTemperature().DegreesCelsius);
            Assert.Equal(2, i2cDevice.DataWritten.Count);
            Assert.Equal((byte)Register.TTHL, i2cDevice.DataWritten.Dequeue());
            Assert.Equal((byte)Register.TTHH, i2cDevice.DataWritten.Dequeue());
        }
    }
}
