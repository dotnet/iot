// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Buffers.Binary;

namespace Iot.Device.Seesaw
{
    public partial class Seesaw : IDisposable
    {
        /// <summary>
        /// Reads the analog value on an capacitive touch-enabled pin. 
        /// </summary>
        /// <param name="pinId">The number of the pin to read.</param>
        /// <returns>An analogue value betweeen 0 and 1023 that represents the capacitance read from the pin.</returns>
        public ushort TouchRead(byte pinId)
        {
            if (!HasModule(SeesawModule.Touch))
            {
                throw new InvalidOperationException($"The hardware on I2C Bus {I2cDevice.ConnectionSettings.BusId}, Address 0x{I2cDevice.ConnectionSettings.DeviceAddress:X2} does not support Adafruit SeeSaw touch functionality");
            }

            return BinaryPrimitives.ReadUInt16BigEndian(Read(SeesawModule.Touch, SeesawFunction.TouchChannelOffset + pinId, 2, 1000));
        }
    }
}
