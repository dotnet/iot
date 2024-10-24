// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device;
using System.Device.Gpio;
using System.IO;

namespace Iot.Device.Tm16xx
{
    /// <summary>
    /// Represents some Titanmec led devices which use I2C like communication protocol. This is an abstract class.
    /// </summary>
    /// <remarks>The communication protocol of some Titanmec led devices is like I2C, using one cable for data and one for clock, but not following the standard of I2C.</remarks>
    public abstract class Tm16xxI2CLikeBase : Tm16xxBase, IDisposable
    {
        private readonly int _clockWidthMicroseconds;
        private readonly bool _shouldDispose;
        private bool _disposedValue;
        private protected byte[] _characterOrder = new byte[0];
        private protected bool _screenOn;
        private protected byte _brightness;
        private protected int _maxCharacters;

        #region Gpio

        private protected int PinClk { get; }
        private protected int PinDio { get; }
        private protected GpioController Controller { get; }

        #endregion

        #region CharacterOrder

        private protected virtual void OnCharacterOrderChanged()
        {
        }

        /// <inheritdoc />
        public override byte[] CharacterOrder
        {
            get => _characterOrder;
            set
            {
                if (value.Length != _maxCharacters)
                {
                    throw new ArgumentException($"Value must be {_maxCharacters} bytes.", nameof(CharacterOrder));
                }

                // Check if we have all values from 0 to 5
                var allExist = true;
                for (var i = 0; i < _maxCharacters; i++)
                {
                    allExist &= Array.Exists(value, e => e == i);
                }

                if (!allExist)
                {
                    throw new ArgumentException(
                        $"{nameof(CharacterOrder)} needs to have all existing characters from 0 to {_maxCharacters - 1}.", nameof(CharacterOrder));
                }

                value.CopyTo(_characterOrder, 0);
                OnCharacterOrderChanged();
            }
        }

        #endregion

        #region State

        private protected abstract void OnStateChanged();

        /// <inheritdoc />
        public override bool IsScreenOn
        {
            get => _screenOn;
            set
            {
                if (_screenOn != value)
                {
                    _screenOn = value;
                    OnStateChanged();
                }
            }
        }

        /// <inheritdoc />
        public override byte ScreenBrightness
        {
            get => _brightness;
            set
            {
                if (_brightness != value)
                {
                    _brightness = value;
                    OnStateChanged();
                }
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes an instance with Gpio controller specified.
        /// </summary>
        /// <param name="pinClk">The clock pin.</param>
        /// <param name="pinDio">The data pin.</param>
        /// <param name="clockWidthMicroseconds">Waiting time between clock up and down.</param>
        /// <param name="controller">The instance of the Gpio controller which will not be disposed with this object.</param>
        protected Tm16xxI2CLikeBase(int pinClk, int pinDio, int clockWidthMicroseconds, GpioController controller)
            : this(pinClk, pinDio, clockWidthMicroseconds, controller, false)
        {
        }

        /// <summary>
        /// Initializes an instance with new Gpio controller which will be disposed with this object. Pin numbering scheme is set to logical.
        /// </summary>
        /// <param name="pinClk">The clock pin.</param>
        /// <param name="pinDio">The data pin.</param>
        /// <param name="clockWidthMicroseconds">Waiting time between clock up and down.</param>
        protected Tm16xxI2CLikeBase(int pinClk, int pinDio, int clockWidthMicroseconds)
            : this(pinClk, pinDio, clockWidthMicroseconds, null, true)
        {
        }

        /// <summary>
        /// Initializes an instance.
        /// </summary>
        /// <param name="pinClk">The clock pin.</param>
        /// <param name="pinDio">The data pin.</param>
        /// <param name="clockWidthMicroseconds">Waiting time between clock up and down.</param>
        /// <param name="gpioController">The instance of the gpio controller. Set to <see langword="null" /> to create a new one.</param>
        /// <param name="shouldDispose">Sets to <see langword="true" /> to dispose the Gpio controller with this object. If the <paramref name="gpioController"/> is set to <see langword="null"/>, this parameter will be ignored and the new created Gpio controller will always be disposed with this object.</param>
        protected Tm16xxI2CLikeBase(int pinClk, int pinDio, int clockWidthMicroseconds,
            GpioController? gpioController = null, bool shouldDispose = true)
        {
            PinClk = pinClk;
            PinDio = pinDio;
            _clockWidthMicroseconds = clockWidthMicroseconds;
            Controller = gpioController ?? new GpioController();
            _shouldDispose = shouldDispose || gpioController is null;
            Controller.OpenPin(pinClk, PinMode.Output);
            Controller.OpenPin(pinDio, PinMode.Output);
            Controller.Write(pinClk, PinValue.High);
            Controller.Write(pinDio, PinValue.High);
        }

        #endregion

        #region Dispose

        private protected virtual void BeforeDisposing(bool willDisposeGpioController)
        {
        }

        /// <summary>
        /// Disposes the class.
        /// </summary>
        /// <param name="disposing">True to dispose.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    BeforeDisposing(_shouldDispose);
                    if (_shouldDispose)
                    {
                        Controller?.Dispose();
                    }
                }

                _disposedValue = true;
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Low level IO

        private protected virtual void StartTransmission()
        {
            // should already be high
            Controller.Write(PinClk, PinValue.High);

            // should already be high
            Controller.Write(PinDio, PinValue.High);
            DelayHelper.DelayMicroseconds(_clockWidthMicroseconds, true);
            Controller.Write(PinDio, PinValue.Low);
            DelayHelper.DelayMicroseconds(_clockWidthMicroseconds, true);
        }

        private protected virtual void StopTransmission()
        {
            // should be changed from high to low
            Controller.Write(PinClk, PinValue.Low);

            // just changed to output, state is not sure
            Controller.Write(PinDio, PinValue.Low);
            DelayHelper.DelayMicroseconds(_clockWidthMicroseconds, true);
            Controller.Write(PinClk, PinValue.High);
            DelayHelper.DelayMicroseconds(_clockWidthMicroseconds, true);
            Controller.Write(PinDio, PinValue.High);
        }

        private protected virtual PinValue WriteByteAndWaitAcknowledge(byte data)
        {
            WriteByte(data);
            return WaitAcknowledge();
        }

        private protected virtual void WriteByte(byte data)
        {
            foreach (bool bit in ByteToBitConverter(data))
            {
                Controller.Write(PinClk, PinValue.Low);
                DelayHelper.DelayMicroseconds(_clockWidthMicroseconds, true);
                Controller.Write(PinDio, bit ? PinValue.High : PinValue.Low);
                Controller.Write(PinClk, PinValue.High);
                DelayHelper.DelayMicroseconds(_clockWidthMicroseconds, true);
            }
        }

        private protected abstract IEnumerable<bool> ByteToBitConverter(byte data);

        private protected virtual PinValue WaitAcknowledge()
        {
            Controller.Write(PinClk, PinValue.Low);
            Controller.Write(PinDio, PinValue.High);
            Controller.SetPinMode(PinDio, PinMode.Input);
            // Wait for the acknowledge
            DelayHelper.DelayMicroseconds(_clockWidthMicroseconds, true);
            var ack = Controller.Read(PinDio);
            if (ThrowWhenIoError && ack == PinValue.High)
            {
                throw new IOException("Device reports an IO error.");
            }

            Controller.SetPinMode(PinDio, PinMode.Output);
            Controller.Write(PinClk, PinValue.High);
            DelayHelper.DelayMicroseconds(_clockWidthMicroseconds, true);

            return ack;
        }

        #endregion
    }
}
