// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Gpio;
using System.Diagnostics;

namespace Iot.Device.RotaryEncoder
{
    /// <summary>
    /// EventArgs used with the RotaryEncode binding to pass event information when the Value changes.
    /// </summary>
    /// <typeparam name="T">The element type of the Value controlled by the RotaryEncoder</typeparam>
    public class RotaryEncoderEventArgs<T> : EventArgs
    {
        /// <summary>The Value property represents current value associated with the RotaryEncoder.</summary>
        public T Value { get; private set; }

        /// <summary>
        /// Construct a new RotaryEncoderEventArgs
        /// </summary>
        /// <param name="value">Current value associated with the rotary encoder</param>
        public RotaryEncoderEventArgs(T value)
        {
            Value = value;
        }
    }
}
