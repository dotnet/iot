// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Device;
using System.Device.I2c;
using System.Globalization;
using System.Linq;

namespace Iot.Device.Arduino
{
    internal class ArduinoI2cBus : I2cBus
    {
        private readonly ArduinoBoard _board;
        private readonly int _busId;
        private readonly ConcurrentDictionary<int, ArduinoI2cDevice> _usedAddresses;
        private bool _busInitialized;

        public ArduinoI2cBus(ArduinoBoard board, int busId)
        {
            _board = board;
            _busId = busId;
            _usedAddresses = new ConcurrentDictionary<int, ArduinoI2cDevice>();
        }

        /// <inheritdoc />
        public override I2cDevice CreateDevice(int deviceAddress)
        {
            if (_usedAddresses.ContainsKey(deviceAddress))
            {
                throw new InvalidOperationException($"Device number {deviceAddress} is already in use");
            }

            if (!_busInitialized)
            {
                // Ensure the corresponding pins are set to I2C (not strictly necessary, but consistent)
                foreach (SupportedPinConfiguration supportedPinConfiguration in _board.SupportedPinConfigurations.Where(x => x.PinModes.Contains(SupportedMode.I2c)))
                {
                    _board.Firmata.SetPinMode(supportedPinConfiguration.Pin, SupportedMode.I2c);
                }

                _board.Firmata.SendI2cConfigCommand();
                _busInitialized = true;
            }

            var device = new ArduinoI2cDevice(_board, this, new I2cConnectionSettings(_busId, deviceAddress));
            _usedAddresses.TryAdd(deviceAddress, device);
            return device;
        }

        public override void RemoveDevice(int deviceAddress)
        {
            _usedAddresses.TryRemove(deviceAddress, out _);
        }

        public override ComponentInformation QueryComponentInformation()
        {
            var self = new ComponentInformation(this, "Arduino I2C Bus driver");
            self.Properties["BusNo"] = _busId.ToString(CultureInfo.InvariantCulture);
            foreach (var device in _usedAddresses)
            {
                self.AddSubComponent(device.Value.QueryComponentInformation());
            }

            return self;
        }
    }
}
