// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Xunit;

namespace Iot.Device.Mcp23xxx.Tests
{
    public class RegisterTests
    {
        [Theory]
        // Port A; Bank 0
        [InlineData(Register.Address.IODIR, Port.PortA, Bank.Bank0, 0b0000_0000)]
        [InlineData(Register.Address.IPOL, Port.PortA, Bank.Bank0, 0b0000_0010)]
        [InlineData(Register.Address.GPINTEN, Port.PortA, Bank.Bank0, 0b0000_0100)]
        [InlineData(Register.Address.DEFVAL, Port.PortA, Bank.Bank0, 0b0000_0110)]
        [InlineData(Register.Address.INTCON, Port.PortA, Bank.Bank0, 0b0000_1000)]
        [InlineData(Register.Address.IOCON, Port.PortA, Bank.Bank0, 0b0000_1010)]
        [InlineData(Register.Address.GPPU, Port.PortA, Bank.Bank0, 0b0000_1100)]
        [InlineData(Register.Address.INTF, Port.PortA, Bank.Bank0, 0b0000_1110)]
        [InlineData(Register.Address.INTCAP, Port.PortA, Bank.Bank0, 0b0001_0000)]
        [InlineData(Register.Address.GPIO, Port.PortA, Bank.Bank0, 0b0001_0010)]
        [InlineData(Register.Address.OLAT, Port.PortA, Bank.Bank0, 0b0001_0100)]
        // Port A; Bank 1
        [InlineData(Register.Address.IODIR, Port.PortA, Bank.Bank1, 0b0000_0000)]
        [InlineData(Register.Address.IPOL, Port.PortA, Bank.Bank1, 0b0000_0001)]
        [InlineData(Register.Address.GPINTEN, Port.PortA, Bank.Bank1, 0b0000_0010)]
        [InlineData(Register.Address.DEFVAL, Port.PortA, Bank.Bank1, 0b0000_0011)]
        [InlineData(Register.Address.INTCON, Port.PortA, Bank.Bank1, 0b0000_0100)]
        [InlineData(Register.Address.IOCON, Port.PortA, Bank.Bank1, 0b0000_0101)]
        [InlineData(Register.Address.GPPU, Port.PortA, Bank.Bank1, 0b0000_0110)]
        [InlineData(Register.Address.INTF, Port.PortA, Bank.Bank1, 0b0000_0111)]
        [InlineData(Register.Address.INTCAP, Port.PortA, Bank.Bank1, 0b0000_1000)]
        [InlineData(Register.Address.GPIO, Port.PortA, Bank.Bank1, 0b0000_1001)]
        [InlineData(Register.Address.OLAT, Port.PortA, Bank.Bank1, 0b0000_1010)]
        // Port B; Bank 0
        [InlineData(Register.Address.IODIR, Port.PortB, Bank.Bank0, 0b0000_0001)]
        [InlineData(Register.Address.IPOL, Port.PortB, Bank.Bank0, 0b0000_0011)]
        [InlineData(Register.Address.GPINTEN, Port.PortB, Bank.Bank0, 0b0000_0101)]
        [InlineData(Register.Address.DEFVAL, Port.PortB, Bank.Bank0, 0b0000_0111)]
        [InlineData(Register.Address.INTCON, Port.PortB, Bank.Bank0, 0b0000_1001)]
        [InlineData(Register.Address.IOCON, Port.PortB, Bank.Bank0, 0b0000_1011)]
        [InlineData(Register.Address.GPPU, Port.PortB, Bank.Bank0, 0b0000_1101)]
        [InlineData(Register.Address.INTF, Port.PortB, Bank.Bank0, 0b0000_1111)]
        [InlineData(Register.Address.INTCAP, Port.PortB, Bank.Bank0, 0b0001_0001)]
        [InlineData(Register.Address.GPIO, Port.PortB, Bank.Bank0, 0b0001_0011)]
        [InlineData(Register.Address.OLAT, Port.PortB, Bank.Bank0, 0b0001_0101)]
        // Port B; Bank 1
        [InlineData(Register.Address.IODIR, Port.PortB, Bank.Bank1, 0b0001_0000)]
        [InlineData(Register.Address.IPOL, Port.PortB, Bank.Bank1, 0b0001_0001)]
        [InlineData(Register.Address.GPINTEN, Port.PortB, Bank.Bank1, 0b0001_0010)]
        [InlineData(Register.Address.DEFVAL, Port.PortB, Bank.Bank1, 0b0001_0011)]
        [InlineData(Register.Address.INTCON, Port.PortB, Bank.Bank1, 0b0001_0100)]
        [InlineData(Register.Address.IOCON, Port.PortB, Bank.Bank1, 0b0001_0101)]
        [InlineData(Register.Address.GPPU, Port.PortB, Bank.Bank1, 0b0001_0110)]
        [InlineData(Register.Address.INTF, Port.PortB, Bank.Bank1, 0b0001_0111)]
        [InlineData(Register.Address.INTCAP, Port.PortB, Bank.Bank1, 0b0001_1000)]
        [InlineData(Register.Address.GPIO, Port.PortB, Bank.Bank1, 0b0001_1001)]
        [InlineData(Register.Address.OLAT, Port.PortB, Bank.Bank1, 0b0001_1010)]
        public void Get_Mapped_Address(Register.Address address, Port port, Bank bank, byte expectedMappedAddress)
        {
            byte actualMappedAddress = Register.GetMappedAddress(address, port, bank);
            Assert.Equal(expectedMappedAddress, actualMappedAddress);
        }
    }
}
