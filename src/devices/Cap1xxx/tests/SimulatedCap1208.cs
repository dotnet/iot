// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Device.I2c;

namespace Iot.Device.Cap1xxx.Tests
{
    /// <summary>
    /// A CAP1208 with its power-on register values. Touches are simulated with <see cref="Touch"/> and <see cref="Release"/>.
    /// </summary>
    internal class SimulatedCap1208 : I2cSimulatedDeviceBase
    {
        private byte _mainControl;
        private byte _status;
        private byte _held;

        public SimulatedCap1208(byte productId = 0x6B, byte manufacturerId = 0x5D)
            : base(new I2cConnectionSettings(1, 0x28))
        {
            RegisterMap.Add(0x00, new Register<byte>(0x00, MainControlWritten, _ => _mainControl));
            RegisterMap.Add(0x03, new Register<byte>(0x00, null, _ => _status));
            RegisterMap.Add(0x1F, new Register<byte>(0x2F));
            RegisterMap.Add(0x26, new Register<byte>(0x00));
            RegisterMap.Add(0x2A, new Register<byte>(0x80));
            RegisterMap.Add(0xFD, new Register<byte>(productId, null, _ => productId));
            RegisterMap.Add(0xFE, new Register<byte>(manufacturerId, null, _ => manufacturerId));
        }

        /// <summary>
        /// Every register write, in order: (register, value).
        /// </summary>
        public List<(byte Register, byte Value)> Writes { get; } = new List<(byte Register, byte Value)>();

        /// <summary>
        /// A touch starts: the status bit is set and the INT bit is set.
        /// </summary>
        public void Touch(SensorInputs inputs)
        {
            _held |= (byte)inputs;
            _status |= (byte)inputs;
            _mainControl |= 0x01;
        }

        /// <summary>
        /// A touch ends: the status bit stays set until the INT bit is cleared.
        /// </summary>
        public void Release(SensorInputs inputs)
        {
            _held &= (byte)~inputs;
        }

        public override void WriteRead(byte[] inputBuffer, byte[] outputBuffer)
        {
            if (inputBuffer.Length > 0)
            {
                CurrentRegister = inputBuffer[0];
                if (inputBuffer.Length >= 2 && RegisterMap.TryGetValue(CurrentRegister, out var register))
                {
                    Writes.Add((CurrentRegister, inputBuffer[1]));
                    register.WriteRegister(inputBuffer[1]);
                }
            }

            if (outputBuffer.Length >= 1 && RegisterMap.TryGetValue(CurrentRegister, out var register2))
            {
                outputBuffer[0] = (byte)register2.ReadRegister();
            }
        }

        private byte MainControlWritten(byte newValue)
        {
            // Clearing the INT bit clears the status bits of the inputs that are no longer touched
            if ((newValue & 0x01) == 0)
            {
                _status &= _held;
            }

            _mainControl = newValue;
            return newValue;
        }
    }
}
