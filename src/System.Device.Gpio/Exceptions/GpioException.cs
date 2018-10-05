// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Devices.Gpio
{
    public class GpioException : Exception
    {
        public GpioException() : base() { }

        public GpioException(string message) : base(message) { }
    }
}
