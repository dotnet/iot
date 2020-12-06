// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Device.I2c;
using System.Text;

namespace Iot.Device.Board
{
    /// <summary>
    /// Represents an I2C bus instance
    /// </summary>
    public class I2cBus : IDisposable
    {
        private readonly int _sdaPin;
        private readonly int _sclPin;
        private readonly Func<I2cConnectionSettings, I2cDevice> _creationFunction;
        private readonly int _bus;
        private Board? _board;

        /// <summary>
        /// Creates an instance of an I2C bus, given the pins used for that bus
        /// </summary>
        /// <param name="board">The board that provides this bus</param>
        /// <param name="bus">The bus number</param>
        /// <param name="pins">The pins, in the logical scheme of the board. This must be an array of exactly two pins (for SCL and SDA)</param>
        /// <param name="creationFunction">Delegate that creates an instance of an I2c device on this bus</param>
        public I2cBus(Board board, int bus, int[]? pins, Func<I2cConnectionSettings, I2cDevice> creationFunction)
        {
            if (pins == null || pins.Length != 2)
            {
                throw new ArgumentException("Must provide a valid set of 2 pins", nameof(pins));
            }

            _board = board;
            _bus = bus;
            _creationFunction = creationFunction ?? throw new ArgumentNullException(nameof(creationFunction));
            _sdaPin = pins[0];
            _sclPin = pins[1];
            try
            {
                _board.ReservePin(_board.ConvertPinNumber(_sdaPin, PinNumberingScheme.Logical, _board.DefaultPinNumberingScheme), PinUsage.I2c, this);
                _board.ReservePin(_board.ConvertPinNumber(_sclPin, PinNumberingScheme.Logical, _board.DefaultPinNumberingScheme), PinUsage.I2c, this);
            }
            catch (Exception)
            {
                _board.ReleasePin(_board.ConvertPinNumber(_sdaPin, PinNumberingScheme.Logical, _board.DefaultPinNumberingScheme), PinUsage.I2c, this);
                _board.ReleasePin(_board.ConvertPinNumber(_sclPin, PinNumberingScheme.Logical, _board.DefaultPinNumberingScheme), PinUsage.I2c, this);
                throw;
            }
        }

        /// <summary>
        /// The Bus Id of this bus
        /// </summary>
        public int BusId => _bus;

        /// <summary>
        /// Creates a device on this bus
        /// </summary>
        /// <param name="settings">The connection settings (particularly the device address)</param>
        /// <returns>An I2C device</returns>
        /// <remarks>No test is performed whether the given device exists and is usable</remarks>
        /// <exception cref="InvalidOperationException">The bus number provided in <paramref name="settings"/> does not match this bus.</exception>
        public virtual I2cDevice CreateDevice(I2cConnectionSettings settings)
        {
            if (settings.BusId != _bus)
            {
                throw new InvalidOperationException($"Attempt to create a device on bus {settings.BusId} trough bus {_bus}.");
            }

            return _creationFunction(settings);
        }

        /// <summary>
        /// Creates a device on this bus
        /// </summary>
        /// <param name="deviceAddress">The device address</param>
        /// <returns>An I2C device</returns>
        /// <remarks>No test is performed whether the given device exists and is usable</remarks>
        public I2cDevice CreateDevice(int deviceAddress)
        {
            return CreateDevice(new I2cConnectionSettings(_bus, deviceAddress));
        }

        /// <inheritdoc cref="IDisposable.Dispose"/>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_board != null)
                {
                    _board.RemoveBus(this);
                    _board.ReleasePin(_sdaPin, PinUsage.I2c, this);
                    _board.ReleasePin(_sclPin, PinUsage.I2c, this);
                }

                _board = null;
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
        }
    }
}
