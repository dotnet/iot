// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.HuskyLens
{
    internal static class ChecksumExensions
    {
        public static byte CalculateChecksum(this byte[] data) => (new ReadOnlySpan<byte>(data)).CalculateChecksum();

        public static byte CalculateChecksum(this ReadOnlySpan<byte> data)
        {
            var sum = 0;
            for (int i = 0; i < data.Length; i++)
            {
                sum += data[i];
            }

            return (byte)(sum & 0xFF);
        }
    }
}
