// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.I2c;

namespace Iot.Device.SenseHat
{
    /// <summary>
    /// SenseHAT - Joystick
    /// </summary>
    public class SenseHatJoystick : IDisposable
    {
        /// <summary>
        /// Default I2C address
        /// </summary>
        public const int I2cAddress = 0x46;
        private const byte StateRegister = 0xF2;

        private I2cDevice _i2c;

        /// <summary>
        /// Constructs SenseHatJoystick instance
        /// </summary>
        /// <param name="i2cDevice">I2C device used to communicate with the device</param>
        public SenseHatJoystick(I2cDevice i2cDevice = null)
        {
            _i2c = i2cDevice ?? CreateDefaultI2cDevice();
            Read();
        }

        /// <summary>
        /// Is holding left
        /// </summary>
        public bool HoldingLeft { get; private set; }

        /// <summary>
        /// Is holding right
        /// </summary>
        public bool HoldingRight { get; private set; }

        /// <summary>
        /// Is holding up
        /// </summary>
        public bool HoldingUp { get; private set; }

        /// <summary>
        /// Is holding down
        /// </summary>
        public bool HoldingDown { get; private set; }

        /// <summary>
        /// Is holding button
        /// </summary>
        public bool HoldingButton { get; private set; }

        /// <summary>
        /// Read joystick state
        /// </summary>
        public void Read()
        {
            JoystickState state = ReadState();
            HoldingLeft = state.HasFlag(JoystickState.Left);
            HoldingRight = state.HasFlag(JoystickState.Right);
            HoldingUp = state.HasFlag(JoystickState.Up);
            HoldingDown = state.HasFlag(JoystickState.Down);
            HoldingButton = state.HasFlag(JoystickState.Button);
        }

        private static I2cDevice CreateDefaultI2cDevice()
        {
            var settings = new I2cConnectionSettings(1, I2cAddress);
            return I2cDevice.Create(settings);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _i2c?.Dispose();
            _i2c = null;
        }

        private JoystickState ReadState()
        {
            _i2c.WriteByte(StateRegister);
            return (JoystickState)_i2c.ReadByte();
        }

        [Flags]
        private enum JoystickState : byte
        {
            Down = 1,
            Right = 1 << 1,
            Up = 1 << 2,
            Button = 1 << 3,
            Left = 1 << 4,
        }
    }
}
