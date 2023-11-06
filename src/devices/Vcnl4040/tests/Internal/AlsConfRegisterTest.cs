// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Vcnl4040.Common.Defnitions;
using Iot.Device.Vcnl4040.Infrastructure;
using Iot.Device.Vcnl4040.Internal;
using Iot.Device.Vcnl4040.Tests;
using Xunit;

namespace Iot.Device.Si5351.Internal.Register.Tests
{
    public class AlsConfRegisterTest
    {
        [Theory]
        [InlineData(0b0000_0000, PowerState.PowerOn)]
        [InlineData(0b0000_0001, PowerState.PowerOff)]
        public void Read(byte data, PowerState powerState)
        {
            var testDevice = new I2cTestDevice();
            I2cInterface testBus = new(testDevice);
            testDevice.DataToRead.Enqueue(data);

            var reg = new AlsConfRegister(testBus);
            reg.Read();

            Assert.Single(testDevice.DataWritten);
            Assert.Equal((byte)CommandCode.ALS_CONF, testDevice.DataWritten.Dequeue());
            Assert.Equal(powerState, reg.AlsSd);
        }

        [Theory]
        [InlineData(0b0000_0000, PowerState.PowerOn, 0b0000_0000)]
        [InlineData(0b0000_0000, PowerState.PowerOff, 0b0000_0001)]
        [InlineData(0b0000_0001, PowerState.PowerOn, 0b0000_0000)]
        [InlineData(0b0000_0001, PowerState.PowerOff, 0b0000_0001)]
        public void Write(byte initialData, PowerState powerState, byte modifiedData)
        {
            var testDevice = new I2cTestDevice();
            I2cInterface testBus = new(testDevice);
            testDevice.DataToRead.Enqueue(initialData);

            var reg = new AlsConfRegister(testBus);
            reg.Read();
            reg.AlsSd = powerState;
            reg.Write();

            // expect 4 bytes to be written: 1x command code (read op), 1x command code (write op), 2x data
            Assert.Equal(4, testDevice.DataWritten.Count);
            Assert.Equal((byte)CommandCode.ALS_CONF, testDevice.DataWritten.Dequeue());
            Assert.Equal((byte)CommandCode.ALS_CONF, testDevice.DataWritten.Dequeue());
            Assert.Equal(modifiedData, testDevice.DataWritten.Dequeue());
            // high byte is not relevant
            Assert.Equal(0x00, testDevice.DataWritten.Dequeue());
        }
    }
}
