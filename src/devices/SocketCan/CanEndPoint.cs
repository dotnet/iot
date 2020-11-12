// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Net;
using System.Net.Sockets;

namespace Iot.Device.SocketCan
{
    internal class CanEndPoint : EndPoint
    {
        private SocketAddress _sockAddr;

        public CanEndPoint(Socket socket, string name)
        {
            _sockAddr = Interop.GetCanSocketAddress(socket, name);
        }

        public override SocketAddress Serialize()
        {
            var ret = new SocketAddress(_sockAddr.Family, _sockAddr.Size);
            for (int i = 0; i < _sockAddr.Size; i++)
            {
                ret[i] = _sockAddr[i];
            }

            return ret;
        }
    }
}
