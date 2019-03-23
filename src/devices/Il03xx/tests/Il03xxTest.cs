// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.I2c;
using System.Device.Spi;
using Xunit;

namespace Iot.Device.Il03xx.Tests
{
    public class Il03xxTest
    {
        public static TheoryData<TestDevice> TestDevices
        {
            get
            {
                TheoryData<TestDevice> devices = new TheoryData<TestDevice>();

                return devices;
            }
        }

        public struct TestDevice
        {
        }

        protected class SpiDeviceMock : SpiDevice
        {
            public object DeviceMock { get; private set; }

            public SpiDeviceMock(int ports)
            {
            }

            public override void Read(Span<byte> buffer) => throw new NotImplementedException();
            public override void Write(Span<byte> data) => throw new NotImplementedException();

            public override void TransferFullDuplex(Span<byte> writeBuffer, Span<byte> readBuffer)
            {
                Write(writeBuffer);
                Read(readBuffer);
            }

            // Don't need these
            public override void WriteByte(byte data) => throw new NotImplementedException();
            public override byte ReadByte() => throw new NotImplementedException();
            public override SpiConnectionSettings ConnectionSettings => throw new NotImplementedException();
        }

        protected class I2cDeviceMock : I2cDevice
        {
            private I2cConnectionSettings _settings;
            public object DeviceMock { get; private set; }

            public I2cDeviceMock()
            {
            }

            public override I2cConnectionSettings ConnectionSettings => _settings;

            public override void Read(Span<byte> buffer) => throw new NotImplementedException();
            public override void Write(Span<byte> data) => throw new NotImplementedException();

            // Don't need these
            public override void WriteByte(byte data) => throw new NotImplementedException();
            public override byte ReadByte() => throw new NotImplementedException();
        }
    }
}
