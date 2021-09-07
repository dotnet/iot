// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.FtCommon
{
    internal enum FtFlowControl : ushort
    {
        // No flow control
        FT_FLOW_NONE = 0,
        // RTS/CTS flow control
        FT_FLOW_RTS_CTS = 256,
        // DTR/DSR flow control
        FT_FLOW_DTR_DSR = 512,
        // Xon/Xoff flow control
        FT_FLOW_XON_XOFF = 1024,
    }
}
