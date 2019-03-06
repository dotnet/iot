// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

namespace Iot.Device.SocketCan
{
    public class CanRaw : IDisposable
    {
        private SafeCanRawSocketHandle _handle;

        public CanRaw(string networkInterface = "can0")
        {
            _handle = new SafeCanRawSocketHandle(networkInterface);
        }

        public void WriteFrame(ref CanFrame frame)
        {
            ReadOnlySpan<CanFrame> frameSpan = MemoryMarshal.CreateReadOnlySpan(ref frame, 1);
            ReadOnlySpan<byte> buff = MemoryMarshal.AsBytes(frameSpan);
            Interop.Write(_handle, buff);
        }

        public void ReadFrame(ref CanFrame frame)
        {
            Span<CanFrame> frameSpan = MemoryMarshal.CreateSpan(ref frame, 1);
            Span<byte> buff = MemoryMarshal.AsBytes(frameSpan);

            while (buff.Length > 0)
            {
                int read = Interop.Read(_handle, buff);
                buff = buff.Slice(read);
            }
        }

        public void Filter(bool extendedFrameFormat, uint id)
        {
            uint idMask = extendedFrameFormat ? Interop.CAN_EFF_MASK : Interop.CAN_SFF_MASK;

            if ((id & idMask) != id)
                throw new ArgumentOutOfRangeException($"{nameof(id)} must not be {(extendedFrameFormat ? 29 : 11)} bit identifier");

            if (extendedFrameFormat)
            {
                id |= (uint)CanFlags.ExtendedFrameFormat;
            }

            Span<Interop.CanFilter> filters = stackalloc Interop.CanFilter[1];
            filters[0].can_id = id;
            filters[0].can_mask = idMask | (uint)CanFlags.ExtendedFrameFormat | (uint)CanFlags.RemoteTransmissionRequest;

            Interop.SetCanRawSocketOption<Interop.CanFilter>(_handle, Interop.CanSocketOption.CAN_RAW_FILTER, filters);
        }

        private static bool IsEff(uint address)
        {
            // has explicit flag or address does not fit in SFF addressing mode
            return (address & (uint)CanFlags.ExtendedFrameFormat) != 0
                || (address & Interop.CAN_EFF_MASK) != (address & Interop.CAN_SFF_MASK);
        }

        public void Dispose()
        {
            _handle.Dispose();
        }
    }
}
