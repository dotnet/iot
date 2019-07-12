// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.IO;
using System.Threading;

namespace System.Device.Pwm.Channels
{
    /// <summary>
    /// Represents a PWM channel running on Windows 10 IoT.
    /// </summary>
    internal class Windows10PwmChannel : PwmChannel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Windows10PwmChannel"/> class.
        /// </summary>
        /// <param name="chip">The PWM chip number.</param>
        /// <param name="channel">The PWM channel number.</param>
        /// <param name="frequency">The frequency in hertz.</param>
        /// <param name="dutyCyclePercentage">The duty cycle percentage represented as a value between 0.0 and 1.0.</param>
        public Windows10PwmChannel(
            int chip,
            int channel,
            int frequency = 400,
            double dutyCyclePercentage = 0.5)
        {
            // TODO: This is just a placeholder to complete later.
        }

        public override int Frequency { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override double DutyCyclePercentage { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override void Start()
        {
            throw new NotImplementedException();
        }

        public override void Stop()
        {
            throw new NotImplementedException();
        }
    }
}
