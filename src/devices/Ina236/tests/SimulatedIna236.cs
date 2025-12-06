// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.I2c;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ina236.Tests
{
    internal class SimulatedIna236 : I2cSimulatedDeviceBase
    {
        private byte _currentRegister;

        public SimulatedIna236(I2cConnectionSettings settings)
            : base(settings)
        {
            _currentRegister = 0;
            RegisterMap.Add(0, new Register<ushort>(0x4127));
        }

        protected override int WriteRead(byte[] inputBuffer, byte[] outputBuffer)
        {
            if (inputBuffer.Length > 0)
            {
                _currentRegister = inputBuffer[0];
                if (inputBuffer.Length >= 3 && RegisterMap.TryGetValue(_currentRegister, out var register))
                {
                    ushort reg = BitConverter.ToUInt16(inputBuffer, 1);
                    register.WriteRegister(reg);
                }
            }

            // All registers of this device are 16 bit, so we need to read that or nothing
            if (outputBuffer.Length >= 2 && RegisterMap.TryGetValue(_currentRegister, out var register2))
            {
                ushort ret = (ushort)register2.ReadRegister();
            }

            return outputBuffer.Length;
        }
    }
}
