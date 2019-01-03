// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Device.I2c;

namespace Iot.Device.Mcp23xxx
{
    public abstract class Mcp230xx : Mcp23xxx
    {
        private readonly I2cDevice _i2cDevice;

        public Mcp230xx(I2cDevice i2cDevice, int? reset = null, int? intA = null, int? intB = null)
            : base(i2cDevice.ConnectionSettings.DeviceAddress, reset, intA, intB)
        {
            _i2cDevice = i2cDevice;
        }

        public override byte Read(Register.Address registerAddress, Port port = Port.PortA, Bank bank = Bank.Bank1)
        {
            byte[] data = Read(registerAddress, 1, port, bank);
            return data[0];
        }

        public override byte[] Read(Register.Address startingRegisterAddress, byte byteCount, Port port = Port.PortA, Bank bank = Bank.Bank1)
        {
            Write(startingRegisterAddress, new byte[] { }, port, bank); // Set address to register first.

            byte[] readBuffer = new byte[byteCount];
            _i2cDevice.Read(readBuffer);
            return readBuffer;
        }

        public override void Write(Register.Address registerAddress, byte data, Port port = Port.PortA, Bank bank = Bank.Bank1)
        {
            Write(registerAddress, new byte[] { data }, port, bank);
        }

        public override void Write(Register.Address startingRegisterAddress, byte[] data, Port port = Port.PortA, Bank bank = Bank.Bank1)
        {
            byte[] writeBuffer = new byte[data.Length + 1]; // Include Register Address.
            writeBuffer[0] = Register.GetMappedAddress(startingRegisterAddress, port, bank);
            data.CopyTo(writeBuffer, 1);
            _i2cDevice.Write(writeBuffer);
        }

        public override void Dispose()
        {
            _i2cDevice?.Dispose();
            base.Dispose();
        }
    }
}
