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
    public struct CanId
    {
        // Raw (can_id) includes EFF, RTR and ERR flags
        internal uint Raw { get; set; }

        public uint Value => ExtendedFrameFormat ? Extended : Standard;

        public uint Standard
        {
            get
            {
                if (ExtendedFrameFormat)
                    throw new InvalidOperationException($"{nameof(Standard)} can be obtained only when {nameof(ExtendedFrameFormat)} is not set.");
                
                return Raw & Interop.CAN_SFF_MASK;
            }
            set
            {
                if ((value & ~Interop.CAN_SFF_MASK) != 0)
                    throw new InvalidOperationException($"{nameof(value)} must be 11 bit identifier.");

                ExtendedFrameFormat = false;
                // note: we clear all bits, not just SFF
                Raw &= ~Interop.CAN_EFF_MASK;
                Raw |= value;
            }
        }

        public uint Extended
        {
            get
            {
                if (!ExtendedFrameFormat)
                    throw new InvalidOperationException($"{nameof(Extended)} can be obtained only when {nameof(ExtendedFrameFormat)} is set.");
                
                return Raw & Interop.CAN_EFF_MASK;
            }
            set
            {
                if ((value & ~Interop.CAN_EFF_MASK) != 0)
                    throw new InvalidOperationException($"{nameof(value)} must be 29 bit identifier.");

                ExtendedFrameFormat = true;
                Raw &= ~Interop.CAN_EFF_MASK;
                Raw |= value;
            }
        }

        public bool Error
        {
            get => ((CanFlags)Raw).HasFlag(CanFlags.Error);
            set => SetCanFlag(CanFlags.Error, value);
        }

        public bool ExtendedFrameFormat
        {
            get => ((CanFlags)Raw).HasFlag(CanFlags.ExtendedFrameFormat);
            set => SetCanFlag(CanFlags.ExtendedFrameFormat, value);
        }

        public bool RemoteTransmissionRequest
        {
            get => ((CanFlags)Raw).HasFlag(CanFlags.RemoteTransmissionRequest);
            set => SetCanFlag(CanFlags.RemoteTransmissionRequest, value);
        }

        public bool IsValid
        {
            get
            {
                uint idMask = ExtendedFrameFormat ? Interop.CAN_EFF_MASK : Interop.CAN_SFF_MASK;
                return !Error && (Raw & idMask) == (Raw & Interop.CAN_EFF_MASK);
            }
        }

        private void SetCanFlag(CanFlags flag, bool value)
        {
            if (value)
            {
                Raw |= (uint)flag;
            }
            else
            {
                Raw &= ~(uint)flag;
            }
        }
    }
}