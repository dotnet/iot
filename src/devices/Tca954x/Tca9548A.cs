// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Device;
using System.Device.I2c;
using System.Device.Model;
using System.IO;

namespace Iot.Device.Tca954x
{
    /// <summary>
    /// Tca9548A - 8-Channel I2C Switch/Multiplexer
    /// </summary>
    [Interface("Tca9548A - 8-Channel I2C Switch/Multiplexer")]
    public class Tca9548A : IList<I2cBus>, IDisposable
    {
        private readonly bool _shouldDispose;
        private readonly List<Tca9548AChannelBus> _channelBuses = new List<Tca9548AChannelBus>();

        /// <summary>
        /// Shadows the device list of the master bus, but supports duplicates
        /// </summary>
        private readonly Dictionary<int, (I2cDevice Device, int Usages)> _devicesInUse = new Dictionary<int, (I2cDevice, int)>();

        private readonly I2cBus _mainBus;
        private MultiplexerChannel? _activeChannels;

        /// <summary>
        /// The default I2C Address, page 15 of the main documentation
        /// https://www.ti.com/lit/ds/symlink/tca9548a.pdf
        /// </summary>
        public const byte DefaultI2cAddress = 0x70;

        /// <summary>
        /// Array of all possible Multiplexer Channels
        /// </summary>
        private static readonly MultiplexerChannel[] DeviceChannels = new MultiplexerChannel[]
        {
            MultiplexerChannel.Channel0, MultiplexerChannel.Channel1, MultiplexerChannel.Channel2, MultiplexerChannel.Channel3, MultiplexerChannel.Channel4,
            MultiplexerChannel.Channel5, MultiplexerChannel.Channel6, MultiplexerChannel.Channel7
        };

        private I2cDevice _i2CDevice;

        /// <summary>
        /// Creates a Multiplexer Instance
        /// </summary>
        /// <param name="i2cDevice">The I2C Device of the Mux itself</param>
        /// <param name="mainBus">The bus the mux is connected to</param>
        /// <param name="shouldDispose">true to dispose the I2C device at dispose</param>
        /// <exception cref="ArgumentNullException">Exception thrown if I2C device is null</exception>
        public Tca9548A(I2cDevice i2cDevice, I2cBus mainBus, bool shouldDispose = true)
        {
            _i2CDevice = i2cDevice ?? throw new ArgumentNullException(nameof(i2cDevice));
            _mainBus = mainBus;
            _shouldDispose = shouldDispose;
            _activeChannels = null; // We don't know the state of the multiplexer
            foreach (var channel in DeviceChannels)
            {
                _channelBuses.Add(new Tca9548AChannelBus(this, mainBus, channel));
            }
        }

        /// <summary>
        /// Gets Channel busses of the multiplexer
        /// </summary>
        /// <param name="index">channel number</param>
        /// <returns></returns>
        public I2cBus this[int index] => _channelBuses[index];

        /// <summary>
        /// Select a group of multiplexer channels.
        /// </summary>
        /// <param name="multiplexChannels">The channels to select</param>
        /// <remarks>
        /// In most cases, a single channel will be selected at a time, but it is possible to write to several channels at once. Reading
        /// from multiple channels at once will result in undefined behavior.
        /// </remarks>
        public void SelectChannel(MultiplexerChannel multiplexChannels)
        {
            if (_activeChannels.HasValue)
            {
                MultiplexerChannel selectedChannel = _activeChannels.Value;
                if (selectedChannel == multiplexChannels)
                {
                    return;
                }
                else
                {
                    _i2CDevice.WriteByte(Convert.ToByte(multiplexChannels));
                    _activeChannels = multiplexChannels;
                }
            }

            if (TryGetSelectedChannel(out var channel) && channel != multiplexChannels)
            {
                _i2CDevice.WriteByte(Convert.ToByte(multiplexChannels));
                _activeChannels = multiplexChannels;
            }
        }

        /// <summary>
        /// Try getting the selected channel on Multiplexer
        /// </summary>
        /// <param name="selectedChannel">Selected Multiplexer Channel</param>
        /// <returns>True if able to retrieve selected channel. Returns false otherwise. Also
        /// returns false if more than one channel is selected.</returns>
        public bool TryGetSelectedChannel(out MultiplexerChannel selectedChannel)
        {
            try
            {
                if (_activeChannels.HasValue)
                {
                    selectedChannel = _activeChannels.Value;
                    return true;
                }

                var channel = _i2CDevice.ReadByte();
                if (Enum.IsDefined(typeof(MultiplexerChannel), channel))
                {
                    // This also returns true if the selected channel is "None", meaning
                    // the mux is disabled. Be careful when fixing this, as the above method
                    // SelectChannel is initially depending on it.
                    selectedChannel = (MultiplexerChannel)channel;
                    return true;
                }
                else
                {
                    selectedChannel = MultiplexerChannel.Channel0;
                    return false;
                }
            }
            catch (Exception)
            {
                selectedChannel = MultiplexerChannel.Channel0;
                return false;
            }
        }

        /// <summary>
        /// Returns the given channel.
        /// </summary>
        /// <param name="channelNo">The channel number (0-7)</param>
        /// <returns>An <see cref="I2cBus"/> representing the provided channel</returns>
        public I2cBus GetChannel(int channelNo)
        {
            if (channelNo < 0 || channelNo >= 8)
            {
                throw new ArgumentOutOfRangeException(nameof(channelNo), "Valid channels are 0-7");
            }

            return GetChannel((MultiplexerChannel)(1 << channelNo));
        }

        /// <summary>
        /// Returns the given channel
        /// </summary>
        /// <param name="channel">A single channel value</param>
        /// <returns>The given channel as <see cref="I2cBus"/> instance</returns>
        /// <exception cref="ArgumentOutOfRangeException">The channel value is not valid or represents more than one channel</exception>
        public I2cBus GetChannel(MultiplexerChannel channel)
        {
            return channel switch
            {
                MultiplexerChannel.Channel0 => this[0],
                MultiplexerChannel.Channel1 => this[1],
                MultiplexerChannel.Channel2 => this[2],
                MultiplexerChannel.Channel3 => this[3],
                MultiplexerChannel.Channel4 => this[4],
                MultiplexerChannel.Channel5 => this[5],
                MultiplexerChannel.Channel6 => this[6],
                MultiplexerChannel.Channel7 => this[7],
                _ => throw new ArgumentOutOfRangeException(nameof(channel), $"Not a valid single channel selection: {channel}"),
            };
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

        /// <inheritdoc/>
        public int Count => _channelBuses.Count;

        /// <inheritdoc/>
        public bool IsReadOnly => true;

        I2cBus IList<I2cBus>.this[int index] { get => _channelBuses[index]; set => _channelBuses[index] = (Tca9548AChannelBus)value; }

        /// <inheritdoc/>
        public int IndexOf(I2cBus item)
        {
            return _channelBuses.IndexOf((Tca9548AChannelBus)item);
        }

        /// <inheritdoc/>
        public void Insert(int index, I2cBus item)
        {
            _channelBuses.Insert(index, (Tca9548AChannelBus)item);
        }

        /// <inheritdoc/>
        public void RemoveAt(int index)
        {
            _channelBuses.RemoveAt(index);
        }

        /// <inheritdoc/>
        public void Add(I2cBus item)
        {
            _channelBuses.Add((Tca9548AChannelBus)item);
        }

        /// <inheritdoc/>
        public void Clear()
        {
            _channelBuses.Clear();
        }

        /// <inheritdoc/>
        public bool Contains(I2cBus item)
        {
            return _channelBuses.Contains((Tca9548AChannelBus)item);
        }

        /// <inheritdoc/>
        public void CopyTo(I2cBus[] array, int arrayIndex)
        {
            _channelBuses.CopyTo((Tca9548AChannelBus[])array, arrayIndex);

        }

        /// <inheritdoc/>
        public bool Remove(I2cBus item)
        {
            return _channelBuses.Remove((Tca9548AChannelBus)item);
        }

        /// <inheritdoc/>
        public IEnumerator<I2cBus> GetEnumerator()
        {
            return _channelBuses.GetEnumerator();
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Gets an I2c Device to use on a <see cref="Tca9548AChannelBus"/>. The device is identical to that
        /// of the master bus, but we need to verify that we only create it once, or the master bus controller
        /// will complain about duplicate address use.
        /// </summary>
        /// <param name="deviceAddress">I2C address of the new device</param>
        /// <returns>Either a new device or a cached one</returns>
        internal I2cDevice CreateOrGetMasterBusDevice(int deviceAddress)
        {
            if (deviceAddress == _i2CDevice.ConnectionSettings.DeviceAddress)
            {
                throw new InvalidOperationException(
                    $"The Mux I2c address {_i2CDevice.ConnectionSettings.DeviceAddress} cannot be used for client devices.");
            }

            if (_devicesInUse.TryGetValue(deviceAddress, out var entry))
            {
                entry.Usages++;
                return entry.Device;
            }

            I2cDevice newDevice = _mainBus.CreateDevice(deviceAddress);
            _devicesInUse.Add(deviceAddress, (newDevice, 1));
            return newDevice;
        }

        /// <summary>
        /// Releases the device, disposing it when it's the last one with this address
        /// </summary>
        /// <param name="device">The device (must have been created by the above method)</param>
        /// <param name="deviceAddress">Address of the device</param>
        internal void ReleaseDevice(I2cDevice device, int deviceAddress)
        {
            // This should always work (or we're releasing something twice)
            if (_devicesInUse.TryGetValue(deviceAddress, out var entry))
            {
                entry.Usages--;
                if (entry.Usages == 0)
                {
                    _devicesInUse.Remove(deviceAddress);
                    device.Dispose();
                }
            }
        }
    }
}
