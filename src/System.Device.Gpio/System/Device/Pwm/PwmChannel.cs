// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Device.Pwm
{
    /// <summary>
    /// Represents a single PWM channel.
    /// </summary>
    public abstract partial class PwmChannel : IDisposable
    {
        /// <summary>
        /// The frequency in hertz.
        /// </summary>
        public abstract int Frequency { get; set; }

        /// <summary>
        /// The duty cycle represented as a value between 0.0 and 1.0.
        /// </summary>
        public abstract double DutyCycle { get; set; }

        /// <summary>
        /// Starts the PWM channel.
        /// </summary>
        public abstract void Start();

        /// <summary>
        /// Stops the PWM channel.
        /// </summary>
        public abstract void Stop();

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Nothing to do in base class.
        }
    }
}
