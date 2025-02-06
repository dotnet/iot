// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Device.I2c;

namespace Iot.Device.Tca955x
{
    /// <summary>
    /// Class for the Tca9555 16-Bit I/O Exander
    /// </summary>
    public class Tca9555 : Tca955x
    {
        /// <summary>
        /// Constructor for the Tca9555 I2C I/O Expander.
        /// </summary>
        /// <param name="device">The I2C Device the device is connected to. Expect an I2C Adress between 0x20 and 0x27</param>
        /// <param name="interrupt">The input pin number that is connected to the interrupt.</param>
        /// <param name="gpioController">The controller for the reset and interrupt pins. If not specified, the default controller will be used.</param>
        /// <param name="shouldDispose">True to dispose the Gpio Controller.</param>
        public Tca9555(I2cDevice device, int interrupt = -1, GpioController? gpioController = null, bool shouldDispose = true)
            : base(device, interrupt, gpioController, shouldDispose)
        {
        }

        /// <inheritdoc/>
        protected override int PinCount => 16;

        /// <inheritdoc/>
        protected override byte GetByteRegister(int pinNumber, ushort value)
        {
            if (pinNumber >= 0 &&
                pinNumber <= 7)
            {
                return (byte)(value & 0xFF);
            }
            else if (pinNumber >= 8 &&
                     pinNumber <= 15)
            {
                return (byte)((value >> 8) & 0xFF);
            }

            return 0;
        }

        /// <inheritdoc/>
        protected override int GetBitNumber(int pinNumber)
        {
            if (pinNumber >= 0 &&
                pinNumber <= 7)
            {
                return pinNumber;
            }
            else if (pinNumber >= 8 &&
                     pinNumber <= 15)
            {
                return pinNumber - 8;
            }

            return 0;
        }

        /// <inheritdoc/>
        protected override byte GetRegisterIndex(int pinNumber, Register registerType)
        {
            byte register = (byte)registerType;
            register += (byte)registerType;

            if (pinNumber >= 0 &&
                pinNumber <= 7)
            {
                return register;
            }
            else if (pinNumber >= 8 &&
                     pinNumber <= 15)
            {
                return ++register;
            }

            throw new ArgumentOutOfRangeException(nameof(pinNumber));
        }

        /// <summary>
        /// Write a byte to the given Register.
        /// </summary>
        /// <param name="register">The given Register.</param>
        /// <param name="value">The value to write.</param>
        public void WriteByte(Tca9555Register register, byte value) => InternalWriteByte((byte)register, value);

        /// <summary>
        /// Read a byte from the given Register.
        /// </summary>
        /// <param name="register">The given Register.</param>
        /// <returns>The readed byte from the Register.</returns>
        public byte ReadByte(Tca9555Register register) => InternalReadByte((byte)register);
    }
}
