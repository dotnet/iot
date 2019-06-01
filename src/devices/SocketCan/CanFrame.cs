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
    internal unsafe struct CanFrame
    {
        public const int MaxLength = 8;

        // RawId (can_id) includes EFF, RTR and ERR flags
        public CanId Id;

        // data length code (can_dlc)
        // see: ISO 11898-1 Chapter 8.4.2.4
        public byte Length;
        private byte _pad;
        private byte _res0;
        private byte _res1;
        public fixed byte Data[MaxLength];

        public bool IsValid
        {
            get
            {
                return Length <= MaxLength && Id.IsValid;
            }
        }
    }
}