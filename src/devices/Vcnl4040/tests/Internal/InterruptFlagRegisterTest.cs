// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using Iot.Device.Vcnl4040.Definitions;
using Iot.Device.Vcnl4040.Internal;
using Xunit;

namespace Iot.Device.Vcnl4040.Tests.Internal
{
    /// <summary>
    /// This is a test against the register specification in the datasheet.
    /// </summary>
    public class InterruptFlagRegisterTest : RegisterTest
    {
        [Theory]
        [InlineData(0b0000_0000, false, false, false, false, false)]
        // PS_IF_AWAY
        [InlineData(0b0000_0001, true, false, false, false, false)]
        // PS_IF_CLOSE
        [InlineData(0b0000_0010, false, true, false, false, false)]
        // ALS_IF_H
        [InlineData(0b0001_0000, false, false, true, false, false)]
        // ALS_IF_L
        [InlineData(0b0010_0000, false, false, false, true, false)]
        // PS_SPFLAG
        [InlineData(0b0100_0000, false, false, false, false, true)]
        // reserved bits
        [InlineData(0b1000_1100, false, false, false, false, false)]
        public void Read(byte highByte, bool psIfAway, bool psIfClose, bool alsIfH, bool alsIfL, bool psSpFlag)
        {
            var testDevice = new I2cTestDevice();
            testDevice.DataToRead.Enqueue(0x00);
            testDevice.DataToRead.Enqueue(highByte);

            var reg = new InterruptFlagRegister(testDevice);
            reg.Read();

            Assert.Single(testDevice.DataWritten);
            Assert.Equal((byte)CommandCode.INT_Flag, testDevice.DataWritten.Dequeue());
            Assert.Equal(psIfAway, reg.PsIfAway);
            Assert.Equal(psIfClose, reg.PsIfClose);
            Assert.Equal(alsIfL, reg.AlsIfL);
            Assert.Equal(alsIfH, reg.AlsIfH);
            Assert.Equal(psSpFlag, reg.PsSpFlag);
        }
    }
}
