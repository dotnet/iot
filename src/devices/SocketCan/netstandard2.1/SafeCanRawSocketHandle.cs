// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace Iot.Device.SocketCan
{
    internal class SafeCanRawSocketHandle : SafeHandle
    {
        public SafeCanRawSocketHandle(string networkInterface)
            : base(Interop.CreateCanRawSocket(networkInterface), true)
        {
        }

        public override bool IsInvalid => (int)handle == -1;

        protected override bool ReleaseHandle()
        {
            Interop.CloseSocket(handle);
            return true;
        }
    }
}
