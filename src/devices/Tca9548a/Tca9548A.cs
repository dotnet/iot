// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.I2c;
using System.Device.Model;
using System.IO;
using Iot.Device.Board;

namespace Iot.Device.Tca9548a
{
    /// <summary>
    /// Tca9548A - 8-Channel I2C Switch/Multiplexer
    /// </summary>
    [Interface("BNO055 - 8-Channel I2C Switch/Multiplexer")]
    public class Tca9548A : IDisposable
    {
        private readonly bool _shouldDispose;

        /// <summary>
        /// The default I2C Address, page 15 of the main documentation
        /// https://www.ti.com/lit/ds/symlink/tca9548a.pdf
        /// </summary>
        public const byte DefaultI2cAddress = 0x70;

        /// <summary>
        /// Array of all possible Multiplexer Channels
        /// </summary>
        public static readonly Channels[] DeviceChannels = (Channels[])Enum.GetValues(typeof(Channels));

        private I2cDevice _i2CDevice;

        /// <summary>
        /// Creates a Multiplexer Instance
        /// </summary>
        /// <param name="i2cDevice">The I2C Device</param>
        /// <param name="shouldDispose">true to dispose the I2C device at dispose</param>
        /// <exception cref="ArgumentNullException">Exception thrown if I2C device is null</exception>
        public Tca9548A(I2cDevice i2cDevice, bool shouldDispose = true)
        {
            _i2CDevice = i2cDevice ?? throw new ArgumentNullException(nameof(i2cDevice));
            _shouldDispose = shouldDispose;
        }

        /// <summary>
        /// Select a multiplex channel
        /// </summary>
        /// <param name="multiplexChannel">The channel to select [2^channel number (0-7)]</param>
        public void SelectChannel(byte multiplexChannel)
        {
            try
            {
                _i2CDevice.WriteByte(multiplexChannel);
                _i2CDevice.ReadByte();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Unable to select ({_i2CDevice.ConnectionSettings.BusId} ,{_i2CDevice.ConnectionSettings.DeviceAddress}, {multiplexChannel}): {e}");
            }
        }

        /// <summary>
        /// Select a multiplex channel
        /// </summary>
        /// <param name="multiplexChannel">The channel to select</param>
        public void SelectChannel(Channels multiplexChannel)
        {
            SelectChannel(Convert.ToByte(multiplexChannel));
        }

        /// <summary>
        /// Try getting the selected channel on Multiplexer
        /// </summary>
        /// <param name="selectedChannel"> selected Multiplexer Channel</param>
        /// <returns> true if able to retrieve selected channel</returns>
        public bool TryGetSelectedChannel(out Channels selectedChannel)
        {
            try
            {
                var channel = _i2CDevice.ReadByte();
                if (Enum.IsDefined(typeof(Channels), channel))
                {
                    selectedChannel = (Channels)channel;
                    return true;
                }
                else
                {
                    selectedChannel = Channels.Channel0;
                    return false;
                }
            }
            catch (Exception)
            {
                selectedChannel = Channels.Channel0;
                return false;
            }
        }

        /// <summary>
        /// Returns all the connected devices on all channels of Multiplexer
        /// </summary>
        /// <returns> Dictionary having Keys as Channels and Values as IEnumerable integer type</returns>
        public Dictionary<Channels, IEnumerable<int>> ScanAllChannelsForDeviceAddress()
        {
            Dictionary<Channels, IEnumerable<int>> channels = new Dictionary<Channels, IEnumerable<int>>();
            foreach (Channels channel in DeviceChannels)
            {
                channels.Add(channel, ScanChannelsForDeviceAddress(channel));
            }

            return channels;
        }

        /// <summary>
        /// Returns all the connected device on selected channel
        /// </summary>
        /// <param name="channel">Selected Channel on Multiplexer</param>
        /// <returns></returns>
        public IEnumerable<int> ScanChannelsForDeviceAddress(Channels channel)
        {
            SelectChannel(channel);
            var devices = _i2CDevice.CreateBusFromI2CDevice().PerformBusScan();
            if (devices.Contains(_i2CDevice.ConnectionSettings.DeviceAddress))
            {
                devices.Remove(_i2CDevice.ConnectionSettings.DeviceAddress);
            }

            return devices;
        }

        /// <summary>
        /// Enable or Disable all Mux Channels
        /// </summary>
        /// <param name="MuxState">Multiplexer Channel State</param>
        public void SwitchTCA9548AState(MuxState MuxState)
        {
            _i2CDevice.WriteByte(Convert.ToByte(MuxState));
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_shouldDispose)
            {
                _i2CDevice?.Dispose();
            }

            _i2CDevice = null!;
        }
    }

}
