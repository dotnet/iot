// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Device.I2c;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tca955x.Tests
{
    internal class Tca955xSimulatedDevice : I2cSimulatedDeviceBase
    {
        private Register<byte> _register0; // pin register 0
        private Register<byte> _register2; // polarity inversion register
        private Register<byte> _register3; // Configuration Register

        public Tca955xSimulatedDevice(I2cConnectionSettings settings)
            : base(settings)
        {
            _register0 = new Register<byte>(1); // Pin 0 is high
            _register2 = new Register<byte>(0);
            _register3 = new Register<byte>(0);
        }

        public void SetPinState(int pin, PinValue value)
        {
            int bit = 1 << pin;
            if (value == PinValue.High)
            {
                _register0.WriteRegister(_register0.ReadRegister() | bit);
            }
            else
            {
                _register0.WriteRegister(_register0.ReadRegister() & ~bit);
            }
        }

        public override int WriteRead(byte[] inputBuffer, byte[] outputBuffer)
        {
            if (inputBuffer.Length >= 1)
            {
                CurrentRegister = inputBuffer[0];
            }

            if (CurrentRegister == 0)
            {
                outputBuffer[0] = _register0.Value;
                return 1;
            }

            if (CurrentRegister == 2)
            {
                if (inputBuffer.Length > 1)
                {
                    _register2.Value = inputBuffer[1];
                }

                if (outputBuffer.Length > 0)
                {
                    outputBuffer[0] = _register2.Value;
                    return 1;
                }
            }

            if (CurrentRegister == 3)
            {
                if (inputBuffer.Length > 1)
                {
                    _register3.Value = inputBuffer[1];
                }

                if (outputBuffer.Length > 0)
                {
                    outputBuffer[0] = _register3.Value;
                    return 1;
                }
            }

            return 0;
        }
    }
}
