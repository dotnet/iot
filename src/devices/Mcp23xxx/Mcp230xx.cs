// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Device.I2c;

namespace Iot.Device.Mcp23xxx
{
    public abstract class Mcp230xx : Mcp23xxx
    {
        private readonly I2cDevice _i2cDevice;

        /// <summary>
        /// A general purpose parallel I/O expansion for I2C applications.
        /// </summary>
        /// <param name="i2cDevice">The I2C device used for communication.</param>
        /// <param name="reset">The output pin number that is connected to the hardware reset.</param>
        /// <param name="interruptA">The input pin number that is connected to the interrupt for Port A (INTA).</param>
        /// <param name="interruptB">The input pin number that is connected to the interrupt for Port B (INTB).</param>
        public Mcp230xx(I2cDevice i2cDevice, int? reset = null, int? interruptA = null, int? interruptB = null)
            : base(i2cDevice.ConnectionSettings.DeviceAddress, reset, interruptA, interruptB)
        {
            _i2cDevice = i2cDevice;
        }

        /// <summary>
        /// Reads a number of bytes from registers.
        /// </summary>
        /// <param name="startingRegisterAddress">The starting register address to read.</param>
        /// <param name="byteCount">The number of bytes to read.</param>
        /// <param name="port">The I/O port used with the registers.</param>
        /// <param name="bank">The bank type that determines how the registers are mapped.</param>
        /// <returns>The data read from the registers.</returns>
        public override byte[] Read(Register.Address startingRegisterAddress, byte byteCount, Port port = Port.PortA, Bank bank = Bank.Bank1)
        {
            Write(startingRegisterAddress, new byte[] { }, port, bank); // Set address to register first.

            byte[] readBuffer = new byte[byteCount];
            _i2cDevice.Read(readBuffer);
            return readBuffer;
        }

        /// <summary>
        /// Writes a number of bytes to registers.
        /// </summary>
        /// <param name="startingRegisterAddress">The starting register address to write.</param>
        /// <param name="data">The data to write to registers.</param>
        /// <param name="port">The I/O port used with the registers.</param>
        /// <param name="bank">The bank type that determines how the registers are mapped.</param>
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
