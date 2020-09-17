//// Licensed to the .NET Foundation under one or more agreements.
//// The .NET Foundation licenses this file to you under the MIT license.
//// See the LICENSE file in the project root for more information.

using System;
using System.Device.I2c;
using System.Runtime.InteropServices;

namespace Iot.Device.QwiicButton
{
    internal class I2cBusAccess : IDisposable
    {
        private I2cDevice _device;

        public I2cBusAccess(I2cDevice device)
        {
            _device = device;
        }

        /*------------------------- Internal I2C Abstraction ---------------- */
        internal byte ReadSingleRegister(Register register)
        {
            Span<byte> readBuffer = new byte[1];
            _device.WriteRead(ToReadOnlySpan(register), readBuffer);
            if (readBuffer.IsEmpty)
            {
                return 0;
            }

            return MemoryMarshal.Read<byte>(readBuffer);

            // _i2cPort->beginTransmission(_deviceAddress);
            // _i2cPort->write(reg);
            // _i2cPort->endTransmission();

            // typecasting the 1 parameter in requestFrom so that the compiler
            // doesn't give us a warning about multiple candidates
            // if (_i2cPort->requestFrom(_deviceAddress, static_cast<byte>(1)) != 0)
            // {
            //     return _i2cPort->read();
            // }

            // return 0;
        }

        internal ushort ReadDoubleRegister(Register register)
        {
            var readBuffer = new Span<byte>();
            _device.WriteRead(ToReadOnlySpan(register), readBuffer);
            if (readBuffer.IsEmpty)
            {
                return 0;
            }

            return MemoryMarshal.Read<ushort>(readBuffer);

            // ushort data = readBuffer[0];
            // data |= (ushort)(readBuffer[1] << 8);
            // return data;

            // Span<short> singleShort = MemoryMarshal.Cast<byte, short>(readBuffer);
            // return (ushort)singleShort[0];

            // //little endian
            // _i2cPort->beginTransmission(_deviceAddress);
            // _i2cPort->write(reg);
            // _i2cPort->endTransmission();

            // //typecasting the 2 parameter in requestFrom so that the compiler
            // //doesn't give us a warning about multiple candidates
            // if (_i2cPort->requestFrom(_deviceAddress, static_cast<byte>(2)) != 0)
            // {
            //     ushort data = _i2cPort->read();
            //     data |= (_i2cPort->read() << 8);
            //     return data;
            // }
            // return 0;
        }

        internal uint ReadQuadRegister(Register register)
        {
            var readBuffer = new Span<byte>();
            _device.WriteRead(ToReadOnlySpan(register), readBuffer);
            if (readBuffer.IsEmpty)
            {
                return 0;
            }

            return MemoryMarshal.Read<uint>(readBuffer);

            // Span<int> singleInt = MemoryMarshal.Cast<byte, int>(readBuffer);
            // return (ulong)singleInt[0];

            // _i2cPort->beginTransmission(_deviceAddress);
            // _i2cPort->write(reg);
            // _i2cPort->endTransmission();

            // union databuffer {
            //     byte array[4];
            //     ulong integer;
            // };

            // databuffer data;

            // //typecasting the 4 parameter in requestFrom so that the compiler
            // //doesn't give us a warning about multiple candidates
            // if (_i2cPort->requestFrom(_deviceAddress, static_cast<byte>(4)) != 0)
            // {
            //     for (byte i = 0; i < 4; i++)
            //     {
            //         data.array[i] = _i2cPort->read();
            //     }
            // }
            // return data.integer;
        }

        internal bool WriteSingleRegister(Register register, byte data)
        {
            _device.Write(new[] { (byte)register, data });
            return true;
        }

        internal bool WriteDoubleRegister(Register register, ushort data)
        {
            _device.WriteByte((byte)register);
            byte lower = (byte)(data & 0xff);
            byte upper = (byte)(data >> 8);
            _device.WriteByte(lower);
            _device.WriteByte(upper);
            return true;

            // _i2cPort->beginTransmission(_deviceAddress);
            // _i2cPort->write(reg);
            // _i2cPort->write(lowByte(data));
            // _i2cPort->write(highByte(data));
            // if (_i2cPort->endTransmission() == 0)
            //     return true;
            // return false;
        }

        internal byte WriteSingleRegisterWithReadback(Register register, byte data)
        {
            if (WriteSingleRegister(register, data))
            {
                return 1;
            }

            if (ReadSingleRegister(register) != data)
            {
                return 2;
            }

            return 0;
        }

        internal ushort WriteDoubleRegisterWithReadback(Register register, ushort data)
        {
            if (WriteDoubleRegister(register, data))
            {
                return 1;
            }

            if (ReadDoubleRegister(register) != data)
            {
                return 2;
            }

            return 0;
        }

        private static ReadOnlySpan<byte> ToReadOnlySpan(Register registerValue)
        {
            return new ReadOnlySpan<byte>(new[] { (byte)registerValue });
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _device?.Dispose();
            _device = null;
        }
    }
}
