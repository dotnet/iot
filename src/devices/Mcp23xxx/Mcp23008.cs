// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Gpio;
using System.Device.I2c;

namespace Iot.Device.Mcp23xxx
{
    public class Mcp23008 : Mcp230xx
    {
        private byte _ioDirection;

        /// <summary>
        /// Initializes new instance of Mcp23008 device.
        /// A general purpose parallel I/O expansion for I2C applications.
        /// </summary>
        /// <param name="i2cDevice">The I2C device used for communication.</param>
        /// <param name="reset">The output pin number that is connected to the hardware reset.</param>
        /// <param name="interruptA">The input pin number that is connected to the interrupt for Port A (INTA).</param>
        /// <param name="interruptB">The input pin number that is connected to the interrupt for Port B (INTB).</param>
        public Mcp23008(I2cDevice i2cDevice, int? reset = null, int? interruptA = null, int? interruptB = null)
            : base(i2cDevice, reset, interruptA, interruptB)
        {
            // Set all of the pins to input
            _ioDirection = 0xFF;
            Write(Register.Address.IODIR, _ioDirection);

            // Ensure the pins are no pulled up
            Write(Register.Address.GPPU, 0x00);

            // Set all GPIO pin levels low
            Write(Register.Address.GPIO, 0x00);
        }

        /// <summary>
        /// The I/O pin count of the device.
        /// </summary>
        public override int PinCount => 8;

        /// <summary>
        /// Reads a byte from a register.
        /// </summary>
        /// <param name="registerAddress">The register address to read.</param>
        /// <returns>The data read from the register.</returns>
        public byte Read(Register.Address registerAddress)
        {
            return Read(registerAddress, Port.PortA, Bank.Bank1);
        }

        /// <summary>
        /// Reads a number of bytes from registers.
        /// </summary>
        /// <param name="startingRegisterAddress">The starting register address to read.</param>
        /// <param name="byteCount">The number of bytes to read.</param>
        /// <returns>The data read from the registers.</returns>
        public byte[] Read(Register.Address startingRegisterAddress, byte byteCount)
        {
            return Read(startingRegisterAddress, byteCount, Port.PortA, Bank.Bank1);
        }

        /// <summary>
        /// Sets a mode to a pin.
        /// </summary>
        /// <param name="pinNumber">The pin number.</param>
        /// <param name="mode">The mode to be set.</param>
        public void SetPinMode(int pinNumber, PinMode mode)
        {
            ValidateMode(mode);
            ValidatePin(pinNumber);
            _ioDirection = ChangePin(
                Register.Address.IODIR,
                pinNumber,
                mode == PinMode.Input ? PinValue.High : PinValue.Low,
                Read(Register.Address.IODIR));
        }

        /// <summary>
        /// Reads the value of a pin.
        /// </summary>
        /// <param name="pinNumber">The pin number.</param>
        /// <returns>High or low pin value.</returns>
        public PinValue ReadPin(int pinNumber)
        {
            ValidatePin(pinNumber);

            byte data = Read(Register.Address.GPIO);
            return ((data & (1 << (pinNumber % 8))) > 0) ? PinValue.High : PinValue.Low;
        }

        /// <summary>
        /// Writes a value to a pin.
        /// </summary>
        /// <param name="pinNumber">The pin number.</param>
        /// <param name="value">The value to be written.</param>
        public void WritePin(int pinNumber, PinValue value)
        {
            ValidatePin(pinNumber);
            ChangePin(Register.Address.GPIO, pinNumber, value, Read(Register.Address.OLAT));
        }

        private byte ChangePin(Register.Address register, int pinNumber, PinValue value, byte current)
        {
            switch (value)
            {
                case PinValue.High:
                    current |= (byte)(1 << pinNumber);
                    break;
                case PinValue.Low:
                    current &= (byte)~(1 << pinNumber);
                    break;
            }

            Write(register, current);
            return current;
        }

        private static void ValidateMode(PinMode mode)
        {
            if (mode != PinMode.Input && mode != PinMode.Output)
            {
                throw new ArgumentException("Mcp supports Input and Output modes only.");
            }
        }

        private void ValidatePin(int pinNumber)
        {
            if (pinNumber >= PinCount || pinNumber < 0)
            {
                throw new ArgumentOutOfRangeException($"{pinNumber} is not a valid pin on the Mcp controller.");
            }
        }
    }
}
