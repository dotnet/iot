// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Device.I2c;
using System.Device.Model;

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
        /// The default I2C Address, page 15 of the main documentation
        /// https://www.ti.com/lit/ds/symlink/tca9548a.pdf
        /// </summary>
        public const byte DefaultI2cAddress = 0x70;

        /// <summary>
        /// Array of all possible Multiplexer Channels
        /// </summary>
        public static readonly MultiplexerChannel[] DeviceChannels = (MultiplexerChannel[])Enum.GetValues(typeof(MultiplexerChannel));

        internal I2cDevice _i2CDevice;

        /// <summary>
        /// Gets Channel busses of the multiplexer
        /// </summary>
        /// <param name="index">channel number</param>
        /// <returns></returns>
        public I2cBus this[int index] => _channelBuses[index];

        /// <summary>
        /// Creates a Multiplexer Instance
        /// </summary>
        /// <param name="i2CDevice">The I2C Device on which Mux is</param>
        /// <param name="shouldDispose">true to dispose the I2C device at dispose</param>
        /// <exception cref="ArgumentNullException">Exception thrown if I2C device is null</exception>
        public Tca9548A(I2cDevice i2CDevice, bool shouldDispose = true)
        {
            _i2CDevice = i2CDevice ?? throw new ArgumentNullException(nameof(i2CDevice));
            _shouldDispose = shouldDispose;
            foreach (var channel in DeviceChannels)
            {
                _channelBuses.Add(new Tca9548AChannelBus(this, channel));
            }
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
        public void SelectChannel(MultiplexerChannel multiplexChannel)
        {
            if (TryGetSelectedChannel(out var channel) && channel != multiplexChannel)
            {
                SelectChannel(Convert.ToByte(multiplexChannel));
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
