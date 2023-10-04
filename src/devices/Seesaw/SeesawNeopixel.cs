// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Device.I2c;
using System.Linq;
using System.Threading;

namespace Iot.Device.Seesaw
{
    /// <summary>
    /// Seesaw GPIO driver
    /// </summary>
    public partial class Seesaw : IDisposable
    {
        /// <summary>
        /// Change the the pin number (PORTA) that is used for the NeoPixel output.
        /// </summary>
        /// <param name="pin">The new pin number.</param>
        public void SetNeopixelPin(byte pin)
        {
            if (!HasModule(SeesawModule.Neopixel))
            {
                throw new InvalidOperationException($"The hardware on I2C Bus {I2cDevice.ConnectionSettings.BusId}, Address 0x{I2cDevice.ConnectionSettings.DeviceAddress:X2} does not support Adafruit SeeSaw NeoPixel functionality");
            }

            Write(SeesawModule.Neopixel, SeesawFunction.NeopixelPin, new[] { pin });
        }

        /// <summary>
        /// Change the protocol speed. 0x00 = 400khz, 0x01 = 800khz (default).
        /// </summary>
        /// <param name="speed">The new speed.</param>
        public void SetNeopixelSpeed(NeopixelSpeed speed)
        {
            if (!HasModule(SeesawModule.Neopixel))
            {
                throw new InvalidOperationException($"The hardware on I2C Bus {I2cDevice.ConnectionSettings.BusId}, Address 0x{I2cDevice.ConnectionSettings.DeviceAddress:X2} does not support Adafruit SeeSaw NeoPixel functionality");
            }

            Write(SeesawModule.Neopixel, SeesawFunction.NeopixelSpeed, new[] { (byte)speed });
        }

        /// <summary>
        /// Change the number of bytes used for the pixel array. This is dependent on when the pixels you are using are RGB or RGBW.
        /// </summary>
        /// <param name="length">The new length.</param>
        public void SetNeopixelBufferLength(ushort length)
        {
            if (!HasModule(SeesawModule.Neopixel))
            {
                throw new InvalidOperationException($"The hardware on I2C Bus {I2cDevice.ConnectionSettings.BusId}, Address 0x{I2cDevice.ConnectionSettings.DeviceAddress:X2} does not support Adafruit SeeSaw NeoPixel functionality");
            }

            var lengthInBytes = new byte[2];
            BinaryPrimitives.WriteUInt16BigEndian(lengthInBytes.AsSpan(), length);
            Write(SeesawModule.Neopixel, SeesawFunction.NeopixelBufferLength, lengthInBytes);
        }

        /// <summary>
        /// Set the data buffer.
        /// </summary>
        /// <param name="buffer">The new buffer.</param>
        /// <param name="offset">The offset in number of bytes.</param>
        public void SetNeopixelBuffer(ReadOnlySpan<byte> buffer, ushort offset = 0)
        {
            const ushort MaxBlockLength = 22;

            if (!HasModule(SeesawModule.Neopixel))
            {
                throw new InvalidOperationException($"The hardware on I2C Bus {I2cDevice.ConnectionSettings.BusId}, Address 0x{I2cDevice.ConnectionSettings.DeviceAddress:X2} does not support Adafruit SeeSaw NeoPixel functionality");
            }

            var block = new byte[MaxBlockLength + 2];
            var span = block.AsSpan();

            while (offset < buffer.Length)
            {
                var blockLength = Math.Min(MaxBlockLength, buffer.Length - offset);
                BinaryPrimitives.WriteUInt16BigEndian(span.Slice(0, 2), offset);
                buffer.Slice(offset, blockLength).CopyTo(span.Slice(2, blockLength));
                Write(SeesawModule.Neopixel, SeesawFunction.NeopixelBuffer, span.Slice(0, blockLength + 2));

                offset += MaxBlockLength;
            }
        }

        /// <summary>
        /// Sending the SHOW command will cause the output to update.
        /// </summary>
        public void SetNeopixelShow()
        {
            if (!HasModule(SeesawModule.Neopixel))
            {
                throw new InvalidOperationException($"The hardware on I2C Bus {I2cDevice.ConnectionSettings.BusId}, Address 0x{I2cDevice.ConnectionSettings.DeviceAddress:X2} does not support Adafruit SeeSaw NeoPixel functionality");
            }

            Write(SeesawModule.Neopixel, SeesawFunction.NeopixelShow, Array.Empty<byte>());
        }
    }
}
