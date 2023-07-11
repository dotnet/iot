// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device;
using System.Device.I2c;
using System.Globalization;
using Iot.Device.Board;

namespace Iot.Device.Tca954x
{
    /// <summary>
    /// I2C BUs of TCA9548A channel
    /// </summary>
    public class Tca9548AChannelBus : I2cBus
    {
        private readonly I2cBus _channelBus;
        private readonly Tca9548A _tca9548A;
        private readonly MultiplexerChannel _tcaChannel;
        private readonly Dictionary<int, I2cDevice> _devices;

        /// <summary>
        /// Creates new I2C bus Instance for multiplexer channel
        /// </summary>
        /// <param name="tca9548A">TCA9548A multiplexer </param>
        /// <param name="mainBus">Main Bus</param>
        /// <param name="channels">Selected Channel on Multiplexer</param>
        internal Tca9548AChannelBus(Tca9548A tca9548A, I2cBus mainBus, MultiplexerChannel channels)
        {
            _tca9548A = tca9548A;
            _tcaChannel = channels;
            _channelBus = mainBus;
            _devices = new Dictionary<int, I2cDevice>();
        }

        private void SelectDeviceChannel() => _tca9548A.SelectChannel(_tcaChannel);

        /// <summary>
        /// Creates I2C device
        /// </summary>
        /// <param name="deviceAddress">Device address related with the device to create.</param>
        /// <returns>I2cDevice instance.</returns>
        public override I2cDevice CreateDevice(int deviceAddress)
        {
            SelectDeviceChannel();
            if (_devices.ContainsKey(deviceAddress))
            {
                throw new InvalidOperationException($"Channel {_tcaChannel} has already a device with address 0x{deviceAddress:x2}");
            }

            var device = new Tca9548AI2cDevice(_tca9548A, _tcaChannel, _channelBus.CreateDevice(deviceAddress));
            _devices[deviceAddress] = device;
            return device;
        }

        /// <summary>
        /// Returns all the connected device on selected channel
        /// </summary>
        /// <returns>The list of used addresses</returns>
        public List<int> PerformBusScan()
        {
            SelectDeviceChannel();
            return _channelBus.PerformBusScan();
        }

        /// <summary>
        /// Removes I2C device.
        /// </summary>
        /// <param name="deviceAddress">Device address to remove.</param>m
        public override void RemoveDevice(int deviceAddress)
        {
            SelectDeviceChannel();
            _devices.Remove(deviceAddress);
        }

        /// <inheritdoc />
        public override ComponentInformation QueryComponentInformation()
        {
            var self = new ComponentInformation(this, $"Tca9548A Channel {_tcaChannel}");
            self.Properties["Channel"] = _tcaChannel.ToString();
            foreach (var device in _devices)
            {
                self.AddSubComponent(device.Value.QueryComponentInformation());
            }

            return self;
        }
    }
}
