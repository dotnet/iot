// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Buffers.Binary;

namespace Iot.Device.Seesaw
{
    public partial class Seesaw : IDisposable
    {
        /// <summary>
        /// Read the current position of the encoder
        /// </summary>
        /// <returns>The encoder position</returns>
        public int GetEncoderPosition(byte encoder = 0)
        {
            if (!HasModule(SeesawModule.Encoder))
            {
                throw new InvalidOperationException($"The hardware on I2C Bus {I2cDevice.ConnectionSettings.BusId}, Address 0x{I2cDevice.ConnectionSettings.DeviceAddress:X2} does not support Adafruit SeeSaw encoder functionality");
            }

            var test = BinaryPrimitives.ReadInt32BigEndian(Read(SeesawModule.Encoder, SeesawFunction.EncoderPosition + encoder, 4, 8000));
            return test;
        }

    }
}
