// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;

namespace Iot.Device.Display
{
    /// <summary>
    /// HT16K33 LED display driver
    /// </summary>
    /// <remarks>
    /// Datasheet: https://www.adafruit.com/datasheets/ht16K33v110.pdf
    /// Sources:
    /// https://github.com/adafruit/Adafruit_LED_Backpack/blob/master/Adafruit_LEDBackpack.cpp
    /// https://github.com/sobek1985/Adafruit_LEDBackpack/blob/master/Adafruit_LEDBackpack/AlphaNumericFourCharacters.cs
    /// </remarks>
    public abstract partial class Ht16k33 : IDisposable
    {
        #region Constants

        /// <summary>
        /// HT16K33 default I2C address
        /// </summary>
        public const int DefaultI2cAddress = 0x70;

        /// <summary>
        /// Maximum level of brightness
        /// </summary>
        public const byte MaxBrightness = 15;
        #endregion

        #region Protected members

        /// <summary>
        /// I2C device interface
        /// </summary>
        protected I2cDevice _i2cDevice;

        /// <summary>
        /// Buffering enabled
        /// </summary>
        protected bool _bufferingEnabled = false;

        /// <summary>
        /// Calls <see cref="Flush"/> if <see cref="BufferingEnabled"/> is false
        /// </summary>
        protected void AutoFlush()
        {
            if (!_bufferingEnabled)
            {
                Flush();
            }
        }
        #endregion

        #region Private members

        /// <summary>
        /// Display on/off state
        /// </summary>
        private bool _displayOn = true;

        /// <summary>
        /// Initial brightness level
        /// </summary>
        private byte _brightness;

        /// <summary>
        /// Initial blinking rate
        /// </summary>
        private BlinkRate _blinkRate;

        /// <summary>
        /// Send turn on oscillator command to device
        /// </summary>
        private void TurnOnOscillator() =>
            _i2cDevice.WriteByte((byte)Command.TurnOnOscillator);

        /// <summary>
        /// Send set brightness command to device
        /// </summary>
        private void SetBrightness() =>
            _i2cDevice.WriteByte((byte)((byte)Command.SetBrightness | _brightness & (byte)Command.BrightnessMask));

        /// <summary>
        /// Send Blink/DisplayOn command to device
        /// </summary>
        private void SetBlinkDisplayOn() =>
            _i2cDevice.WriteByte((byte)((byte)Command.Blink | (byte)Command.BlinkDisplayOn | (byte)_blinkRate << 1));
        #endregion

        #region Public members

        /// <summary>
        /// Initialize HT16K33 device
        /// </summary>
        /// <param name="i2cDevice">The <see cref="System.Device.I2c.I2cDevice"/> to create with.</param>
        public Ht16k33(I2cDevice i2cDevice)
        {
            // Initialize I2C device
            _i2cDevice = i2cDevice;

            TurnOnOscillator();
            SetBrightness();
            SetBlinkDisplayOn();
        }

        /// <summary>
        /// Gets or sets whether buffering is enabled
        /// </summary>
        /// <remarks>
        /// When set to true the display buffer is only flushed to the device when explicitly calling the <see cref="Flush"/> method.
        /// Setting it to false also implicitly calls <see cref="Flush"/>
        /// </remarks>
        public bool BufferingEnabled
        {
            get => _bufferingEnabled;
            set
            {
                _bufferingEnabled = value;
                AutoFlush();
            }
        }

        /// <summary>
        /// Gets or sets level of display brightness
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Brightness must be between 0 and <see cref="MaxBrightness"/></exception>
        public byte Brightness
        {
            get => _brightness;
            set
            {
                if (value == _brightness)
                {
                    return;
                }

                if (value > MaxBrightness)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), $"Brightness must be between 0 and {MaxBrightness}");
                }

                _brightness = value;

                SetBrightness();
            }
        }

        /// <summary>
        /// Gets or sets display blink rate, also turns on display if turned off
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException" />
        public BlinkRate BlinkRate
        {
            get => _blinkRate;
            set
            {
                if (value == _blinkRate)
                {
                    return;
                }

                if (!Enum.IsDefined(typeof(BlinkRate), value))
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _blinkRate = value;

                SetBlinkDisplayOn();
            }
        }

        /// <summary>
        /// Sets screen on/off
        /// </summary>
        public bool DisplayOn
        {
            get => _displayOn;
            set
            {
                if (_displayOn = value)
                {
                    SetBlinkDisplayOn();
                }
                else
                {
                    _i2cDevice.WriteByte((byte)Command.Blink);
                }
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _i2cDevice?.Dispose();
            _i2cDevice = null!;
        }

        #region Abstract members

        /// <summary>
        /// Write raw data to display buffer
        /// </summary>
        /// <param name="data">Array of bytes to write to the display</param>
        /// <param name="startAddress">Address to start writing from</param>
        public abstract void Write(ReadOnlySpan<byte> data, int startAddress = 0);

        /// <summary>
        /// Clear display buffer
        /// </summary>
        public abstract void Clear();

        /// <summary>
        /// Flush buffer to display
        /// </summary>
        public abstract void Flush();
        #endregion
        #endregion

        #region Enums
        private enum Command : byte
        {
            /// <summary>
            /// HT16K33 blink command
            /// </summary>
            Blink = 0b1000_0000,

            /// <summary>
            /// HT16K33 blink display on command
            /// </summary>
            BlinkDisplayOn = 0b0000_0001,

            /// <summary>
            /// HT16K33 turn on oscillator command
            /// </summary>
            TurnOnOscillator = 0b0010_0001,

            /// <summary>
            /// HT16K33 set brightness command
            /// </summary>
            SetBrightness = 0b1110_0000,

            /// <summary>
            /// HT16K33 brightness bit mask
            /// </summary>
            BrightnessMask = 0b0000_1111
        }
        #endregion
    }
}
