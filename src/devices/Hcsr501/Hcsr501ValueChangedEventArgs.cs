// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Gpio;

namespace Iot.Device.Hcsr501
{
    /// <summary>
    /// HC-SR501 Value Changed Event Args
    /// </summary>
    public class Hcsr501ValueChangedEventArgs : EventArgs
    {
        /// <summary>
        /// HC-SR501 OUT Pin Value
        /// </summary>
        public PinValue PinValue { get; private set; }
        public Hcsr501ValueChangedEventArgs(PinValue value)
        {
            PinValue = value;
        }
    }
}
