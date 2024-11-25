﻿// Licensed to the .NET Foundation under one or more agreements.
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
        /// <param name="bus">The I2C Bus the device is connected to.</param>
        /// <param name="interrupt">The input pin number that is connected to the interrupt.</param>
        /// <param name="gpioController">The controller for the reset and interrupt pins. If not specified, the default controller will be used.</param>
        /// <param name="shouldDispose">True to dispose the Gpio Controller.</param>
        /// <param name="adress">
        /// The device address for the connection on the I2C bus.
        /// Start with 0x20 and ends with 0x27
        /// </param>
        public Tca9554(I2cDevice bus, int interrupt = -1, GpioController? gpioController = null, bool shouldDispose = true, byte adress = 0x20)
            : base(bus, interrupt, gpioController, shouldDispose, adress)
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
