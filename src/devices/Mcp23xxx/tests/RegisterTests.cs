// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Xunit;

namespace Iot.Device.Mcp23xxx.Tests
{
    public class RegisterTests : Mcp23xxxTest
    {
        [Theory]
        // Port A; Bank 0
        [InlineData(Register.IODIR, Port.PortA, BankStyle.Sequential, 0x00)]
        [InlineData(Register.IPOL, Port.PortA, BankStyle.Sequential, 0x02)]
        [InlineData(Register.GPINTEN, Port.PortA, BankStyle.Sequential, 0x04)]
        [InlineData(Register.DEFVAL, Port.PortA, BankStyle.Sequential, 0x06)]
        [InlineData(Register.INTCON, Port.PortA, BankStyle.Sequential, 0x08)]
        [InlineData(Register.IOCON, Port.PortA, BankStyle.Sequential, 0x0A)]
        [InlineData(Register.GPPU, Port.PortA, BankStyle.Sequential, 0x0C)]
        [InlineData(Register.INTF, Port.PortA, BankStyle.Sequential, 0x0E)]
        [InlineData(Register.INTCAP, Port.PortA, BankStyle.Sequential, 0x10)]
        [InlineData(Register.GPIO, Port.PortA, BankStyle.Sequential, 0x12)]
        [InlineData(Register.OLAT, Port.PortA, BankStyle.Sequential, 0x14)]
        // Port A; Bank 1
        [InlineData(Register.IODIR, Port.PortA, BankStyle.Separated, 0x00)]
        [InlineData(Register.IPOL, Port.PortA, BankStyle.Separated, 0x01)]
        [InlineData(Register.GPINTEN, Port.PortA, BankStyle.Separated, 0x02)]
        [InlineData(Register.DEFVAL, Port.PortA, BankStyle.Separated, 0x03)]
        [InlineData(Register.INTCON, Port.PortA, BankStyle.Separated, 0x04)]
        [InlineData(Register.IOCON, Port.PortA, BankStyle.Separated, 0x05)]
        [InlineData(Register.GPPU, Port.PortA, BankStyle.Separated, 0x06)]
        [InlineData(Register.INTF, Port.PortA, BankStyle.Separated, 0x07)]
        [InlineData(Register.INTCAP, Port.PortA, BankStyle.Separated, 0x08)]
        [InlineData(Register.GPIO, Port.PortA, BankStyle.Separated, 0x09)]
        [InlineData(Register.OLAT, Port.PortA, BankStyle.Separated, 0x0A)]
        // Port B; Bank 0
        [InlineData(Register.IODIR, Port.PortB, BankStyle.Sequential, 0x01)]
        [InlineData(Register.IPOL, Port.PortB, BankStyle.Sequential, 0x03)]
        [InlineData(Register.GPINTEN, Port.PortB, BankStyle.Sequential, 0x05)]
        [InlineData(Register.DEFVAL, Port.PortB, BankStyle.Sequential, 0x07)]
        [InlineData(Register.INTCON, Port.PortB, BankStyle.Sequential, 0x09)]
        [InlineData(Register.IOCON, Port.PortB, BankStyle.Sequential, 0x0B)]
        [InlineData(Register.GPPU, Port.PortB, BankStyle.Sequential, 0x0D)]
        [InlineData(Register.INTF, Port.PortB, BankStyle.Sequential, 0x0F)]
        [InlineData(Register.INTCAP, Port.PortB, BankStyle.Sequential, 0x11)]
        [InlineData(Register.GPIO, Port.PortB, BankStyle.Sequential, 0x13)]
        [InlineData(Register.OLAT, Port.PortB, BankStyle.Sequential, 0x15)]
        // Port B; Bank 1
        [InlineData(Register.IODIR, Port.PortB, BankStyle.Separated, 0x010)]
        [InlineData(Register.IPOL, Port.PortB, BankStyle.Separated, 0x11)]
        [InlineData(Register.GPINTEN, Port.PortB, BankStyle.Separated, 0x12)]
        [InlineData(Register.DEFVAL, Port.PortB, BankStyle.Separated, 0x13)]
        [InlineData(Register.INTCON, Port.PortB, BankStyle.Separated, 0x14)]
        [InlineData(Register.IOCON, Port.PortB, BankStyle.Separated, 0x15)]
        [InlineData(Register.GPPU, Port.PortB, BankStyle.Separated, 0x16)]
        [InlineData(Register.INTF, Port.PortB, BankStyle.Separated, 0x17)]
        [InlineData(Register.INTCAP, Port.PortB, BankStyle.Separated, 0x18)]
        [InlineData(Register.GPIO, Port.PortB, BankStyle.Separated, 0x19)]
        [InlineData(Register.OLAT, Port.PortB, BankStyle.Separated, 0x1A)]
        public void Get_Mapped_Address(Register register, Port port, BankStyle bankStyle, byte expectedMappedAddress)
        {
            BusMock busMock = new BusMock(ports: 2);
            BankStyleMock mock = new BankStyleMock(busMock, bankStyle);
            mock.Read(register, port);
            Assert.Equal(expectedMappedAddress, busMock.LastReadRegister);
        }

        private class BankStyleMock : Mcp23xxx
        {
            public BankStyleMock(IBusDevice device, BankStyle bankStyle)
                : base(device, 0x20, bankStyle: bankStyle)
            {
            }

            public byte Read(Register register, Port port) => InternalReadByte(register, port);

            public override int PinCount => 16;
        }
    }
}
