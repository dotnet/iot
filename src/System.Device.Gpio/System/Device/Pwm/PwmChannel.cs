// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;

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

        /// <inheritdoc cref="IDisposable.Dispose"/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes this instance
        /// </summary>
        /// <param name="disposing"><see langword="true"/> if explicitly disposing, <see langword="false"/> if in finalizer</param>
        protected virtual void Dispose(bool disposing)
        {
            // Nothing to do in base class.
        }

        /// <summary>
        /// Creates a new instance of the <see cref="PwmChannel"/> running on the current platform. (Windows 10 IoT or Unix/Raspbian)
        /// </summary>
        /// <param name="chip">The PWM chip number.</param>
        /// <param name="channel">The PWM channel number.</param>
        /// <param name="frequency">The frequency in hertz.</param>
        /// <param name="dutyCyclePercentage">The duty cycle percentage represented as a value between 0.0 and 1.0.</param>
        /// <returns>A PWM channel running on Windows 10 IoT.</returns>
        public static PwmChannel Create(
            int chip,
            int channel,
            int frequency = 400,
            double dutyCyclePercentage = 0.5)
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                return CreateWindows10PwmChannel(
                    chip,
                    channel,
                    frequency,
                    dutyCyclePercentage);
            }
            else if (IsBeagleBoneKernel())
            {
                return new Channels.BeagleBonePwmChannel(
                    chip,
                    channel,
                    frequency,
                    dutyCyclePercentage);
            }
            else
            {
                return new Channels.UnixPwmChannel(
                    chip,
                    channel,
                    frequency,
                    dutyCyclePercentage);
            }
        }

        private static bool IsBeagleBoneKernel()
        {
            try
            {
                string kernelVersionInfo = IO.File.ReadAllText(@"/proc/version");
                // The String.Contains(string, StringComparison) overload wasn't introduced until .Net Standard 2.1
                return kernelVersionInfo.IndexOf("beagle", StringComparison.InvariantCulture) > 0;
            }
            catch (SystemException ex) when (ex is IO.IOException || ex is Security.SecurityException || ex is UnauthorizedAccessException)
            {
                // If we can't read the file for some reason, assume it's not a beagle bone.
                // This ensures we're not suddenly throwing exceptions the clients weren't expecting
                // at the cost of some potentially weird errors if it is actually a beagle bone kernel.
                return false;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static PwmChannel CreateWindows10PwmChannel(int chip, int channel, int frequency, double dutyCyclePercentage)
        {
            // This wrapper is needed to prevent Mono from loading Windows10PwmChannel
            // which causes all fields to be loaded - one of such fields is WinRT type which does not
            // exist on Linux which causes TypeLoadException.
            // Using NoInlining and no explicit type prevents this from happening.
            return new Channels.Windows10PwmChannel(
                    chip,
                    channel,
                    frequency,
                    dutyCyclePercentage);
        }
    }
}
