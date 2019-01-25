// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Device.Gpio
{
    /// <summary>
    /// Delegate that defines the structure for callbacks when a pin value changed event occurs.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="pinValueChangedEventArgs">The pin value changed arguments from the event.</param>
    public delegate void PinChangeEventHandler(object sender, PinValueChangedEventArgs pinValueChangedEventArgs);
}
