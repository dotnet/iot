// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Buffers.Binary;

namespace Iot.Device.Seesaw
{
    public partial class Seesaw : IDisposable
    {
        /// <summary>
        /// Read the current position of the encoder.
        /// </summary>
        /// <param name="encoder">Which encoder to use, defaults to 0.</param>
        /// <returns>The encoder position as a 32 bit signed integer.</returns>
        /// <exception cref="InvalidOperationException">The hardware does not support Adafruit SeeSaw encoder functionality.</exception>
        public int GetEncoderPosition(byte encoder = 0)
        {
            if (!HasModule(SeesawModule.Encoder))
            {
                throw new InvalidOperationException($"The hardware on I2C Bus {I2cDevice.ConnectionSettings.BusId}, Address 0x{I2cDevice.ConnectionSettings.DeviceAddress:X2} does not support Adafruit SeeSaw encoder functionality");
            }

            return BinaryPrimitives.ReadInt32BigEndian(Read(SeesawModule.Encoder, SeesawFunction.EncoderPosition + encoder, 4, 8000));
        }

        /// <summary>
        /// Set the current position of the encoder.
        /// </summary>
        /// <param name="position">Encoder position.</param>
        /// <param name="encoder">Which encoder to use, defaults to 0.</param>
        /// <exception cref="InvalidOperationException">The hardware does not support Adafruit SeeSaw encoder functionality.</exception>
        public void SetEncoderPosition(int position, byte encoder = 0)
        {
            if (!HasModule(SeesawModule.Encoder))
            {
                throw new InvalidOperationException($"The hardware on I2C Bus {I2cDevice.ConnectionSettings.BusId}, Address 0x{I2cDevice.ConnectionSettings.DeviceAddress:X2} does not support Adafruit SeeSaw encoder functionality");
            }

            Span<byte> buffer = stackalloc byte[4];
            BinaryPrimitives.WriteInt32BigEndian(buffer, position);

            Write(SeesawModule.Encoder, SeesawFunction.EncoderPosition + encoder, buffer);
        }

        /// <summary>
        /// The change in encoder position since it was last read.
        /// </summary>
        /// <param name="encoder">Which encoder to use, defaults to 0.</param>
        /// <returns>The encoder change as a 32 bit signed integer.</returns>
        /// <exception cref="InvalidOperationException">The hardware does not support Adafruit SeeSaw encoder functionality.</exception>
        public int GetEncoderDelta(byte encoder = 0)
        {
            if (!HasModule(SeesawModule.Encoder))
            {
                throw new InvalidOperationException($"The hardware on I2C Bus {I2cDevice.ConnectionSettings.BusId}, Address 0x{I2cDevice.ConnectionSettings.DeviceAddress:X2} does not support Adafruit SeeSaw encoder functionality");
            }

            return BinaryPrimitives.ReadInt32BigEndian(Read(SeesawModule.Encoder, SeesawFunction.EncoderDelta + encoder, 4, 8000));
        }

        /// <summary>
        /// Enable the interrupt to fire when the encoder changes position.
        /// </summary>
        /// <param name="encoder">Which encoder to use, defaults to 0.</param>
        /// <exception cref="InvalidOperationException">The hardware does not support Adafruit SeeSaw encoder functionality.</exception>
        public void EnableEncoderInterrupt(byte encoder = 0)
        {
            if (!HasModule(SeesawModule.Encoder))
            {
                throw new InvalidOperationException($"The hardware on I2C Bus {I2cDevice.ConnectionSettings.BusId}, Address 0x{I2cDevice.ConnectionSettings.DeviceAddress:X2} does not support Adafruit SeeSaw encoder functionality");
            }

            WriteByte(SeesawModule.Encoder, SeesawFunction.EncoderIntenset + encoder, 0x01);
        }

        /// <summary>
        /// Disable the interrupt from firing when the encoder changes.
        /// </summary>
        /// <param name="encoder">Which encoder to use, defaults to 0.</param>
        /// <exception cref="InvalidOperationException">The hardware does not support Adafruit SeeSaw encoder functionality.</exception>
        public void DisableEncoderInterrupt(byte encoder = 0)
        {
            if (!HasModule(SeesawModule.Encoder))
            {
                throw new InvalidOperationException($"The hardware on I2C Bus {I2cDevice.ConnectionSettings.BusId}, Address 0x{I2cDevice.ConnectionSettings.DeviceAddress:X2} does not support Adafruit SeeSaw encoder functionality");
            }

            WriteByte(SeesawModule.Encoder, SeesawFunction.EncoderIntenclr + encoder, 0x01);
        }
    }
}
