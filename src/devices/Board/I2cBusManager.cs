// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device;
using System.Device.Gpio;
using System.Device.I2c;
using System.Text;

namespace Iot.Device.Board
{
    /// <summary>
    /// Manages an I2C bus instance
    /// </summary>
    public class I2cBusManager : I2cBus, IDisposable, IDeviceManager
    {
        private readonly int _sdaPin;
        private readonly int _sclPin;
        private readonly int _bus;
        private readonly Dictionary<int, I2cDevice> _devices;
        private I2cBus _busInstance;
        private Board? _board;

        /// <summary>
        /// Creates an instance of an I2C bus, given the pins used for that bus
        /// </summary>
        /// <param name="board">The board that provides this bus</param>
        /// <param name="bus">The bus number</param>
        /// <param name="pins">The pins, in the logical scheme of the board. This must be an array of exactly two pins (for SCL and SDA)</param>
        /// <param name="busInstance">The wrapped bus instance</param>
        public I2cBusManager(Board board, int bus, int[]? pins, I2cBus busInstance)
        {
            if (pins == null || pins.Length != 2)
            {
                throw new ArgumentException("Must provide a valid set of 2 pins", nameof(pins));
            }

            _board = board;
            _bus = bus;
            _busInstance = busInstance ?? throw new ArgumentNullException(nameof(busInstance));
            _devices = new Dictionary<int, I2cDevice>();

            _sdaPin = pins[0];
            _sclPin = pins[1];
            try
            {
                _board.ReservePin(_sdaPin, PinUsage.I2c, this);
                _board.ReservePin(_sclPin, PinUsage.I2c, this);
            }
            catch (Exception)
            {
                _board.ReleasePin(_sdaPin, PinUsage.I2c, this);
                _board.ReleasePin(_sclPin, PinUsage.I2c, this);
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
        /// <param name="deviceAddress">The device address</param>
        /// <returns>An I2C device</returns>
        /// <remarks>No test is performed whether the given device exists and is usable</remarks>
        public override I2cDevice CreateDevice(int deviceAddress)
        {
            I2cDevice newDevice = _busInstance.CreateDevice(deviceAddress);
            _devices[deviceAddress] = newDevice;
            return newDevice;
        }

        /// <summary>
        /// Disposes and removes a device from the bus.
        /// No exception is thrown if the device is not open
        /// </summary>
        /// <param name="deviceAddress">Address of the device to dispose</param>
        public override void RemoveDevice(int deviceAddress)
        {
            if (_devices.TryGetValue(deviceAddress, out I2cDevice? device))
            {
                device.Dispose();
                _devices.Remove(deviceAddress);
            }
        }

        /// <summary>
        /// Disposes this I2C bus instance. Also disposes all devices associated with this bus.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_board != null)
                {
                    if (_board.RemoveBus(this))
                    {
                        _board.ReleasePin(_sdaPin, PinUsage.I2c, this);
                        _board.ReleasePin(_sclPin, PinUsage.I2c, this);
                    }
                }

                foreach (KeyValuePair<int, I2cDevice> dev in _devices)
                {
                    dev.Value.Dispose();
                }

                _devices.Clear();

                _busInstance?.Dispose();
                _busInstance = null!;

                _board = null;
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Query the component information (the tree of active drivers) for diagnostic purposes.
        /// </summary>
        /// <returns>A <see cref="ComponentInformation"/> instance</returns>
        public override ComponentInformation QueryComponentInformation()
        {
            return new ComponentInformation(this, $"I2C Bus Manager, Bus number {_bus}");
        }

        /// <inheritdoc />
        public IReadOnlyCollection<int> GetActiveManagedPins()
        {
            return new List<int>()
            {
                _sclPin, _sdaPin
            };
        }
    }
}
