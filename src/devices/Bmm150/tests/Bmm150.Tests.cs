// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Device.Spi;
using Iot.Device.Bmp180;
using Moq;
using Xunit;

namespace Iot.Device.Bmm150.Tests
{
    public class Bmm150Tests
    {
        [Fact]
        public void CheckCompensation()
        {
            var trimData = GetTestTrimData();

            double x = Bmm150Compensation.CompensateX(-10, 0, trimData);
            double y = Bmm150Compensation.CompensateY(10, 6831, trimData);
            double z = Bmm150Compensation.CompensateZ(-20, 6831, trimData);

            // Comparing against the implementation for the Arduino (Groove 3-axis compass Bmm150 library)
            Assert.Equal(-3.6875, x, 3);
            Assert.Equal(3.6875, y, 3);
            Assert.Equal(-7.25, z, 3);
        }

        [Fact]
        public void CompensationFailsOnInvalidInput()
        {
            var trimData = GetTestTrimData();

            double x = Bmm150Compensation.CompensateX(-4096, 0, trimData);
            double y = Bmm150Compensation.CompensateY(-4096, 6831, trimData);
            double z = Bmm150Compensation.CompensateZ(-20, 0, trimData);

            Assert.True(Double.IsNaN(x));
            Assert.True(Double.IsNaN(y));
            Assert.True(Double.IsNaN(z));
        }

        private Bmm150TrimRegisterData GetTestTrimData()
        {
            // These calibration values are from my particular sensor
            Bmm150TrimRegisterData trimData = new Bmm150TrimRegisterData();
            trimData.DigX1 = 0;
            trimData.DigY1 = 0;
            trimData.DigX2 = 30;
            trimData.DigY2 = 30;
            trimData.DigZ1 = 23425;
            trimData.DigZ2 = 720;
            trimData.DigZ3 = 0;
            trimData.DigZ4 = 0;
            trimData.DigXy1 = 29;
            trimData.DigXy2 = -3;
            trimData.DigXyz1 = 6567;
            return trimData;
        }

        internal class MockedI2cDevice : I2cDevice
        {
            public MockedI2cDevice()
            {
                ConnectionSettings = new I2cConnectionSettings(0, 0x13);
            }

            public override I2cConnectionSettings ConnectionSettings { get; }
            public override void Read(Span<byte> buffer)
            {
                throw new NotImplementedException();
            }

            public override void Write(ReadOnlySpan<byte> buffer)
            {
                throw new NotImplementedException();
            }

            public override void WriteRead(ReadOnlySpan<byte> writeBuffer, Span<byte> readBuffer)
            {
                throw new NotImplementedException();
            }
        }
    }
}
