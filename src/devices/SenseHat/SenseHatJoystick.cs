// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;
using System.Device.Gpio;
using System.Device.I2c;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Device.I2c.Drivers;

namespace Iot.Device.SenseHat
{
    public class SenseHatJoystick : IDisposable
    {
        public const int I2cAddress = 0x46;
        private const byte StateRegister = 0xF2;

        private I2cDevice _i2c;

        public SenseHatJoystick(I2cDevice i2cDevice = null)
        {
            _i2c = i2cDevice ?? CreateDefaultI2cDevice();
            Read();
        }

        public bool HoldingLeft { get; private set; }
        public bool HoldingRight { get; private set; }
        public bool HoldingUp { get; private set; }
        public bool HoldingDown { get; private set; }
        public bool HoldingButton { get; private set; }

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
