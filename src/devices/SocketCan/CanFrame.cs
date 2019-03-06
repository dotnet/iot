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
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct CanFrame
    {
        private const int CAN_MAX_DLEN = 8;

        // RawId (can_id) includes EFF, RTR and ERR flags
        public uint RawId { get; set; }

        // data length code (can_dlc)
        // see: ISO 11898-1 Chapter 8.4.2.4
        private byte _length;
        private byte _pad;
        private byte _res0;
        private byte _res1;
        private fixed byte _data[CAN_MAX_DLEN];

        public uint Id => ExtendedFrameFormat ? ExtendedId : StandardId;

        public uint StandardId
        {
            get
            {
                if (ExtendedFrameFormat)
                    throw new InvalidOperationException($"{nameof(StandardId)} can be obtained only when {nameof(ExtendedFrameFormat)} is not set.");
                
                return RawId & Interop.CAN_SFF_MASK;
            }
            set
            {
                if ((value & ~Interop.CAN_SFF_MASK) != 0)
                    throw new InvalidOperationException($"{nameof(value)} must be 11 bit identifier.");

                ExtendedFrameFormat = false;
                RawId &= ~Interop.CAN_SFF_MASK;
                RawId |= value;
            }
        }

        public uint ExtendedId
        {
            get
            {
                if (!ExtendedFrameFormat)
                    throw new InvalidOperationException($"{nameof(ExtendedId)} can be obtained only when {nameof(ExtendedFrameFormat)} is set.");
                
                return RawId & Interop.CAN_EFF_MASK;
            }
            set
            {
                if ((value & ~Interop.CAN_EFF_MASK) != 0)
                    throw new InvalidOperationException($"{nameof(value)} must be 29 bit identifier.");

                ExtendedFrameFormat = true;
                RawId &= ~Interop.CAN_EFF_MASK;
                RawId |= value;
            }
        }

        public bool Error
        {
            get => ((CanFlags)RawId).HasFlag(CanFlags.Error);
            set => SetCanFlag(CanFlags.Error, value);
        }

        public bool ExtendedFrameFormat
        {
            get => ((CanFlags)RawId).HasFlag(CanFlags.ExtendedFrameFormat);
            set => SetCanFlag(CanFlags.ExtendedFrameFormat, value);
        }

        public bool RemoteTransmissionRequest
        {
            get => ((CanFlags)RawId).HasFlag(CanFlags.RemoteTransmissionRequest);
            set => SetCanFlag(CanFlags.RemoteTransmissionRequest, value);
        }

        public bool IsValid
        {
            get
            {
                uint idMask = ExtendedFrameFormat ? Interop.CAN_EFF_MASK : Interop.CAN_SFF_MASK;
                return !Error && _length <= CAN_MAX_DLEN && (RawId & ~idMask) == 0;
            }
        }

        public ReadOnlySpan<byte> Data
        {
            get
            {
                if (_length > CAN_MAX_DLEN)
                {
                    throw new InvalidOperationException($"Length {_length} exceed maximum allowed: {CAN_MAX_DLEN}");
                }

                fixed (byte* d = _data)
                {
                    return new ReadOnlySpan<byte>(d, _length);
                }
            }
            set
            {
                if (value.Length > CAN_MAX_DLEN)
                    throw new InvalidOperationException($"Data length cannot exceed {CAN_MAX_DLEN} bytes");
                
                _length = (byte)value.Length;

                fixed (byte* d = _data)
                {
                    var buff = new Span<byte>(d, value.Length);
                    value.CopyTo(buff);

                    // fill remainder of the buffer with zeros for sanity
                    var remainder = new Span<byte>(d + value.Length, CAN_MAX_DLEN - value.Length);
                    remainder.Fill(0);
                }
            }
        }

        private void SetCanFlag(CanFlags flag, bool value)
        {
            if (value)
            {
                RawId |= (uint)flag;
            }
            else
            {
                RawId &= ~(uint)flag;
            }
        }
    }
}