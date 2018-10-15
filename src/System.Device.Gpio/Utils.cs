// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.IO;
using System.Runtime.CompilerServices;

namespace System.Device.Gpio
{
    internal static class Utils
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort SwapBytes(ushort x)
        {
            x = (ushort)((x << 8) | (x >> 8));
            return x;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint SwapBytes(uint x)
        {
            x = (uint)((SwapBytes((ushort)(x & ushort.MaxValue)) << 16) | SwapBytes((ushort)(x >> 16)));
            return x;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong SwapBytes(ulong x)
        {
            x = (SwapBytes((uint)(x & uint.MaxValue)) << 32) | SwapBytes((uint)(x >> 32));
            return x;
        }

        public static IOException CreateIOException(string message, int result)
        {
            Interop.ErrorInfo info = Interop.Sys.GetLastErrorInfo();
            message = $"{message}\nResult: {result} {info}";

            return new IOException(message);
        }
    }
}
