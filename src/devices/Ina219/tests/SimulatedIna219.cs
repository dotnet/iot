// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Buffers.Binary;
using System.Device.I2c;

namespace Ina219.Tests
{
    internal class SimulatedIna219 : I2cSimulatedDeviceBase
    {
        // Power-on default of the configuration register: 32V range, +/-320mV, 12 bit for both ADCs, continuous mode
        public const ushort DefaultConfiguration = 0x399F;

        public SimulatedIna219(I2cConnectionSettings settings)
            : base(settings)
        {
            RegisterMap.Add(0, new Register<ushort>(DefaultConfiguration, ConfigurationRegisterHandler, null));
            RegisterMap.Add(1, new Register<ushort>()); // Shunt voltage
            RegisterMap.Add(2, new Register<ushort>()); // Bus voltage
            RegisterMap.Add(3, new Register<ushort>()); // Power
            RegisterMap.Add(4, new Register<ushort>()); // Current
            RegisterMap.Add(5, new Register<ushort>()); // Calibration
        }

        private ushort ConfigurationRegisterHandler(ushort newValue)
        {
            // When the reset bit is set, set everything to default
            if ((newValue & 0x8000) != 0)
            {
                RegisterMap[5].WriteRegister(0); // Reset the calibration register
                return DefaultConfiguration;
            }

            return newValue;
        }

        public override void WriteRead(byte[] inputBuffer, byte[] outputBuffer)
        {
            if (inputBuffer.Length > 0)
            {
                CurrentRegister = inputBuffer[0];
                if (inputBuffer.Length >= 3 && RegisterMap.TryGetValue(CurrentRegister, out var register))
                {
                    ushort reg = BinaryPrimitives.ReadUInt16BigEndian(inputBuffer.AsSpan().Slice(1));
                    register.WriteRegister(reg);
                }
            }

            // All registers of this device are 16 bit, so we need to read that or nothing
            if (outputBuffer.Length >= 2 && RegisterMap.TryGetValue(CurrentRegister, out var register2))
            {
                ushort ret = (ushort)register2.ReadRegister();
                BinaryPrimitives.WriteUInt16BigEndian(outputBuffer, ret);
            }
        }
    }
}
