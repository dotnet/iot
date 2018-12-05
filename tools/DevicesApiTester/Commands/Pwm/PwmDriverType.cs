// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Device.Pwm.Drivers;
using DeviceApiTester.Infrastructure;

namespace DeviceApiTester.Commands.Pwm
{
    public enum PwmDriverType
    {
        [ImplementationType(typeof(Windows10PwmDriver))]
        Windows,

        [ImplementationType(typeof(UnixPwmDriver))]
        Unix,
    }
}
