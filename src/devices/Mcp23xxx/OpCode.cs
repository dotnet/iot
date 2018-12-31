// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Mcp23xxx
{
    public class OpCode
    {
        public static byte GetOpCode(int deviceAddress, bool isReadCommand)
        {
            int opCode = deviceAddress << 1;

            if (isReadCommand)
            {
                opCode |= 0b000_0001;  // Set read bit.
            }

            return (byte)opCode;
        }
    }
}
