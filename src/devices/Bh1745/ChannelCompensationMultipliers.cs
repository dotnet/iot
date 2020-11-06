// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Bh1745
{
    /// <summary>
    /// Channel compensation multipliers used to compensate the 4 color channels of the Bh1745.
    /// </summary>
    public record ChannelCompensationMultipliers(double Red, double Green, double Blue, double Clear);
}

#if NETCOREAPP2_1 || NETCOREAPP3_1
#pragma warning disable SA1403
namespace System.Runtime.CompilerServices
{
    public class IsExternalInit
    {
    }
}
#endif
