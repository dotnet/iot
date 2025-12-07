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
        public SimulatedIna236(I2cConnectionSettings settings)
            : base(settings)
        {
            RegisterMap.Add(0, new Register<ushort>(0x4127, ConfigurationRegisterHandler, null)); // 0x4127 is the power-on default of the configuration register
            RegisterMap.Add(1, new Register<ushort>());
            RegisterMap.Add(2, new Register<ushort>());
            RegisterMap.Add(3, new Register<ushort>());
            RegisterMap.Add(4, new Register<ushort>());
            RegisterMap.Add(5, new Register<ushort>());
            RegisterMap.Add(6, new Register<ushort>());
            RegisterMap.Add(7, new Register<ushort>());
            // the value is big-endian, but that is taken care of later
            RegisterMap.Add(0x3F, new Register<ushort>(0xA080, DeviceIdentificationRegisterHandler, null));
        }

        private ushort DeviceIdentificationRegisterHandler(ushort arg)
        {
            // This register is read-only
            return 0xA080;
        }

        private ushort ConfigurationRegisterHandler(ushort newValue)
        {
            // When the reset bit is set, set everything to default
            if ((newValue & 0x8000) != 0)
            {
                RegisterMap[5].WriteRegister(0); // Reset the calibration register
                return 0x4127;
            }

            return newValue;
        }

        protected override int WriteRead(byte[] inputBuffer, byte[] outputBuffer)
        {
            if (inputBuffer.Length > 0)
            {
                CurrentRegister = inputBuffer[0];
                if (inputBuffer.Length >= 3 && RegisterMap.TryGetValue(CurrentRegister, out var register))
                {
                    ushort reg = BitConverter.ToUInt16(inputBuffer, 1);
                    register.WriteRegister(reg);
                }
            }

            // All registers of this device are 16 bit, so we need to read that or nothing
            if (outputBuffer.Length >= 2 && RegisterMap.TryGetValue(CurrentRegister, out var register2))
            {
                ushort ret = (ushort)register2.ReadRegister();
                byte[] buf = BitConverter.GetBytes(ret);
                buf.CopyTo(outputBuffer, 0);
            }

            return outputBuffer.Length;
        }
    }
}
