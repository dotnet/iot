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
    /// <summary>
    /// Represents CAN identifier (11 or 29-bit)
    /// </summary>
    public struct CanId
    {
        // Raw (can_id) includes EFF, RTR and ERR flags
        internal uint Raw { get; set; }

        /// <summary>
        /// Gets value of identifier (11 or 29-bit)
        /// </summary>
        public uint Value => ExtendedFrameFormat ? Extended : Standard;

        /// <summary>
        /// Gets or sets standard (11-bit) identifier
        /// </summary>
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

        /// <summary>
        /// Gets or sets extended (29-bit) identifier
        /// </summary>
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

        /// <summary>
        /// Gets error (ERR) flag
        /// </summary>
        public bool Error
        {
            get => ((CanFlags)Raw).HasFlag(CanFlags.Error);
            set => SetCanFlag(CanFlags.Error, value);
        }

        /// <summary>
        /// True if extended frame format (EFF) flag is set
        /// </summary>
        public bool ExtendedFrameFormat
        {
            get => ((CanFlags)Raw).HasFlag(CanFlags.ExtendedFrameFormat);
            set => SetCanFlag(CanFlags.ExtendedFrameFormat, value);
        }

        /// <summary>
        /// Gets remote transimission request (RTR) flag
        /// </summary>
        public bool RemoteTransmissionRequest
        {
            get => ((CanFlags)Raw).HasFlag(CanFlags.RemoteTransmissionRequest);
            set => SetCanFlag(CanFlags.RemoteTransmissionRequest, value);
        }

        /// <summary>
        /// Checks if identifier is valid: error flag is not set and if address can fit selected format (11/29 bit)
        /// </summary>
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