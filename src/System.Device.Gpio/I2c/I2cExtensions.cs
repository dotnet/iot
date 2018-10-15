// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Device.Gpio;
using System.Runtime.CompilerServices;

namespace System.Devices.I2c
{
    public static class I2cExtensions
    {
        public static ulong Read(this I2cDevice device, uint byteCount)
        {
            ulong result;

            switch (byteCount)
            {
                case 1:
                    result = device.Read8();
                    break;

                case 2:
                    result = device.Read16();
                    break;

                case 3:
                    result = device.Read24();
                    break;

                case 4:
                    result = device.Read32();
                    break;

                case 8:
                    result = device.Read64();
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(byteCount));
            }

            return result;
        }
    }
}
