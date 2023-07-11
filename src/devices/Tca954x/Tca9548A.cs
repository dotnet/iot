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
            if (TryGetSelectedChannel(out var channel) && channel != multiplexChannels)
            {
                _i2CDevice.WriteByte(Convert.ToByte(multiplexChannels));
                _activeChannels = multiplexChannels;
            }
        }

        /// <summary>
        /// Try getting the selected channel on Multiplexer
        /// </summary>
        /// <param name="selectedChannel"> selected Multiplexer Channel</param>
        /// <returns> true if able to retrieve selected channel</returns>
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
    }
}
