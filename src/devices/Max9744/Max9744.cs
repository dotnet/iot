// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Max9744Device.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Pi.IO.Devices.Amplifiers.Max9744
{
    using System;
    using System.Device.Gpio;
    using System.Device.I2c;

    /// <inheritdoc />
    /// <summary>
    /// I2C connection to the MAX 9744 amplifier.
    /// </summary>
    /// <seealso cref="T:System.IDisposable" />
    public class Max9744Device : IDisposable
    {
        private const byte MinVolume = 0;
        private const byte MaxVolume = 63;
        private readonly I2cDevice i2cDevice;
        private readonly GpioController gpioController;
        private readonly int mutePin;
        private readonly int shutdownPin;

        /// <summary>
        /// Initializes a new instance of the <see cref="Max9744Device" /> class.
        /// </summary>
        /// <param name="i2cDevice">The i2c device.</param>
        /// <param name="gpioController">The gpioController.</param>
        /// <param name="mutePin">The mute pin.</param>
        /// <param name="shutdownPin">The shutdown pin.</param>
        public Max9744Device(
            I2cDevice i2cDevice,
            GpioController gpioController,
            int mutePin,
            int shutdownPin)
        {
            // connect volume via i2c
            this.i2cDevice = i2cDevice;
            this.gpioController = gpioController;
            this.mutePin = mutePin;
            this.shutdownPin = shutdownPin;

            // connect mute and shutdown via gpio
            this.gpioController.OpenPin(mutePin, PinMode.Output);
            this.gpioController.OpenPin(shutdownPin, PinMode.Output);
        }

        /// <summary>
        /// Gets a value indicating whether this instance is muted.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is muted; otherwise, <c>false</c>.
        /// </value>
        public bool IsMuted { get; set; }

        /// <summary>
        /// Sets the volume.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The actual volume.</returns>
        public byte SetVolume(byte value)
        {
            var volume = Math.Max(MinVolume, Math.Min(value, MaxVolume));
            this.i2cDevice.WriteByte(volume);
            return volume;
        }

        /// <summary>
        /// Sets the state of the mute.
        /// </summary>
        /// <param name="mute">if set to <c>true</c> [mute].</param>
        /// <returns>The current mute state.</returns>
        public bool SetMuteState(bool mute)
        {
            this.IsMuted = true;
            this.gpioController.Write(this.mutePin, mute ? PinValue.Low : PinValue.High);
            return mute;
        }

        /// <summary>
        /// Toggles the mute.
        /// </summary>
        public void ToggleMute()
        {
            this.gpioController.Write(this.mutePin, !this.IsMuted);
        }

        /// <summary>
        /// Sets the state of the shutdown pin.
        /// </summary>
        /// <param name="isShutdown">if set to <c>true</c> the amplifier is shutdown.</param>
        public void SetShutdownState(bool isShutdown)
        {
            this.gpioController.Write(this.mutePin, isShutdown ? PinValue.Low : PinValue.High);
        }

        /// <summary>
        /// Gets the volume.
        /// </summary>
        /// <returns>The current volume.</returns>
        public byte GetVolume()
        {
            return this.i2cDevice.ReadByte();
        }

        /// <inheritdoc />
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        void IDisposable.Dispose()
        {
            this.SetShutdownState(true);
            this.gpioController.ClosePin(this.mutePin);
            this.gpioController.ClosePin(this.shutdownPin);
        }
    }
}
