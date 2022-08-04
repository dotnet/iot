// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.I2c;
using System.Device.Model;
using System.IO;

namespace Iot.Device.Tca9548a
{
    /// <summary>
    /// Available channels
    /// </summary>
    public enum Channels : byte
    {
        /// <summary>
        /// Channel 0 Byte (2^0 = 1)
        /// </summary>
        Channel0 = 0x01,

        /// <summary>
        /// Channel 1 Byte (2^1 = 2)
        /// </summary>
        Channel1 = 0x02,

        /// <summary>
        /// Channel 2 Byte (2^2 = 4)
        /// </summary>
        Channel2 = 0x04,

        /// <summary>
        /// Channel 3 Byte (2^3 = 8)
        /// </summary>
        Channel3 = 0x08,

        /// <summary>
        /// Channel 4 Byte (2^4 = 16)
        /// </summary>
        Channel4 = 0x10,

        /// <summary>
        /// Channel 5 Byte (2^5 = 32)
        /// </summary>
        Channel5 = 0x20,

        /// <summary>
        /// Channel 6 Byte (2^6 = 64)
        /// </summary>
        Channel6 = 0x40,

        /// <summary>
        /// Channel 7 Byte (2^7 = 128)
        /// </summary>
        Channel7 = 0x80
    }

    /// <summary>
    /// Mux States
    /// </summary>
    public enum MUXState : byte
    {
        /// <summary>
        /// Disables all channels of MuX
        /// </summary>
        DisbleAllChannels = 0x00,

        /// <summary>
        /// Enables all channels of MuX
        /// </summary>
        EnableAllChannels = 0xFF,
    }

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
            List<int> addresses = new List<int>();
            SelectChannel(channel);
            I2cDevice i2c;
            // First 8 I2C addresses are reserved, last one is 0x7F
            for (int i = 8; i < 0x80; i++)
            {
                try
                {
                    if (i != _i2CDevice.ConnectionSettings.DeviceAddress)
                    {
                        i2c = I2cDevice.Create(new I2cConnectionSettings(_i2CDevice.ConnectionSettings.BusId, i));
                        i2c.ReadByte();
                        addresses.Add(i);
                    }
                }
                catch (IOException)
                {
                    i2c = null!;
                }

            }

            return addresses;
        }

        /// <summary>
        /// Enable or Disable all Mux Channels
        /// </summary>
        /// <param name="MUXState">Multiplexer Channel State</param>
        public void SwitchTCA9548AState(MUXState MUXState)
        {
            _i2CDevice.WriteByte(Convert.ToByte(MUXState));
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_shouldDispose)
            {
                _i2CDevice?.Dispose();
                _i2CDevice = null!;
            }
        }

    }

}
