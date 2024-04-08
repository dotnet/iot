// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System;
using Iot.Device.Vcnl4040.Definitions;
using Xunit;

namespace Iot.Device.Vcnl4040.Tests
{
    public class Vcnl4040DeviceTest
    {
        private readonly Vcnl4040TestDevice _testDevice = new();

        public Vcnl4040DeviceTest()
        {
            _testDevice.SetData(CommandCode.ID, 0x0186);
        }

        [Fact]
        public void Reset()
        {
            Vcnl4040Device vcnl4040 = new(_testDevice);

            // fill all registers with random data
            for (int addr = 0; addr <= 0x0c; addr++)
            {
                _testDevice.SetData((CommandCode)addr, 0x55aa);
            }

            vcnl4040.Reset();

            // check whether all registers relevant for device configuration have been reset to their defaults
            Assert.Equal(0x0001, _testDevice.GetData(CommandCode.ALS_CONF));
            Assert.Equal(0x0000, _testDevice.GetData(CommandCode.ALS_THDL));
            Assert.Equal(0x0000, _testDevice.GetData(CommandCode.ALS_THDH));
            Assert.Equal(0x0001, _testDevice.GetData(CommandCode.PS_CONF_1_2));
            Assert.Equal(0x0000, _testDevice.GetData(CommandCode.PS_CONF_3_MS));
            Assert.Equal(0x0000, _testDevice.GetData(CommandCode.PS_CANC));
            Assert.Equal(0x0000, _testDevice.GetData(CommandCode.PS_THDL));
            Assert.Equal(0x0000, _testDevice.GetData(CommandCode.PS_THDH));
        }

        [Fact]
        public void VerifyDevice()
        {
            Vcnl4040Device vcnl4040 = new(_testDevice);

            // set correct Id
            _testDevice.SetData(CommandCode.ID, 0x0186);
            vcnl4040.VerifyDevice();

            // set some incompatible Id
            _testDevice.SetLsb(CommandCode.ID, 0x33);
            Assert.Throws<NotSupportedException>(() => vcnl4040.VerifyDevice());
        }

        [Fact]
        public void DeviceId_Get()
        {
            Vcnl4040Device vcnl4040 = new(_testDevice);

            // set correct Id according to data sheet
            _testDevice.SetData(CommandCode.ID, 0x0186);

            Assert.Equal(0x0186, vcnl4040.DeviceId);
        }

        [Theory]
        [InlineData(0b0000_0000, false, false, false, false, false)]
        [InlineData(0b0101_0001, true, false, true, false, true)]
        [InlineData(0b0010_0010, false, true, false, true, false)]
        public void GetAndClearInterruptFlags(byte data, bool psIfAway, bool psIfClose, bool alsIfH, bool alsIfL, bool psSpFlag)
        {
            Vcnl4040Device vcnl4040 = new(_testDevice);

            _testDevice.SetMsb(CommandCode.INT_Flag, data);

            InterruptFlags flags = vcnl4040.GetAndClearInterruptFlags();
            Assert.Equal(psIfAway, flags.PsAway);
            Assert.Equal(psIfClose, flags.PsClose);
            Assert.Equal(alsIfH, flags.AlsHigh);
            Assert.Equal(alsIfL, flags.AlsLow);
            Assert.Equal(psSpFlag, flags.PsProtectionMode);
        }
    }
}
