// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.I2c;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArduinoCsCompiler
{
    internal class ArduinoNativeI2cDevice : I2cDevice
    {
        // easier to access from the firmware here than in the structure
        private int _deviceAddress; // do not remove. Used in runtime.

        public ArduinoNativeI2cDevice(ArduinoNativeBoard board, ArduinoNativeI2cBus bus, I2cConnectionSettings connectionSettings)
        {
            ConnectionSettings = connectionSettings;
            _deviceAddress = connectionSettings.DeviceAddress;
            Init(connectionSettings.BusId, connectionSettings.DeviceAddress);
        }

        public override I2cConnectionSettings ConnectionSettings { get; }

        [ArduinoImplementation("ArduinoNativeI2cDeviceInit", 0x106)]
        private void Init(int busId, int deviceAddress)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation("ArduinoNativeI2cDeviceReadByte", 0x107)]
        public override byte ReadByte()
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation("ArduinoNativeI2cDeviceReadSpan", 0x108)]
        public override void Read(Span<byte> buffer)
        {
            // Todo: implement as backend function
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = ReadByte();
            }
        }

        [ArduinoImplementation("ArduinoNativeI2cDeviceWriteByte", 0x109)]
        public override void WriteByte(byte value)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation("ArduinoNativeI2cDeviceWriteSpan", 0x10A)]
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation("ArduinoNativeI2cDeviceWriteRead", 0x10B)]
        public override void WriteRead(ReadOnlySpan<byte> writeBuffer, Span<byte> readBuffer)
        {
            throw new NotImplementedException();
        }
    }
}
