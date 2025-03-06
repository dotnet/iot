// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.Gpio;
using System.Device.I2c;

namespace Iot.Device.Tca955x
{
    /// <summary>
    /// Class for the Tca9554 8-Bit I/O Exander
    /// </summary>
    public class Tca9554 : Tca955x
    {
        /// <summary>
        /// Constructor for the Tca9554 I2C I/O Expander.
        /// </summary>
        /// <param name="device">The I2C device used for communication</param>
        /// <param name="interrupt">The input pin number that is connected to the interrupt.</param>
        /// <param name="gpioController">The controller for the reset and interrupt pins. If not specified, the default controller will be used.</param>
        /// <param name="shouldDispose">True to dispose the Gpio Controller.</param>
        public Tca9554(I2cDevice device, int interrupt = -1, GpioController? gpioController = null, bool shouldDispose = true)
            : base(device, interrupt, gpioController, shouldDispose)
        {
        }

        /// <inheritdoc/>
        protected override int PinCount => 8;

        /// <inheritdoc/>
        protected override byte GetByteRegister(int pinNumber, ushort value)
        {
            return (byte)(value & 0xFF);
        }

        /// <inheritdoc/>
        protected override int GetBitNumber(int pinNumber)
        {
            return pinNumber;
        }

        /// <inheritdoc/>
        protected override byte GetRegisterIndex(int pinNumber, Register registerType)
        {
            // No conversion for 8-Bit devices
            return (byte)registerType;
        }

        /// <summary>
        /// Write a byte to the given Register.
        /// </summary>
        /// <param name="register">The given Register.</param>
        /// <param name="value">The value to write.</param>
        public void WriteByte(Register register, byte value) => InternalWriteByte((byte)register, value);

        /// <summary>
        /// Read a byte from the given Register.
        /// </summary>
        /// <param name="register">The given Register.</param>
        /// <returns>The readed byte from the Register.</returns>
        public byte ReadByte(Register register) => InternalReadByte((byte)register);
    }
}
