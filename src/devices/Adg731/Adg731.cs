using System;
using System.Device.Spi;
using System.Diagnostics;
using System.Runtime.Serialization.Formatters.Binary;

namespace Iot.Device.Adg731
{
    /// <summary>
    /// Represents the Analog Devices ADG731, the 32-Channel, Serially Controlled 4 Ohm 1.8 V to 5.5 V, +/- 2.5 V, Analog Multiplexer
    /// </summary>
    public class Adg731 : IDisposable
    {
        private SpiDevice _spiDevice = null;

        /// <summary>
        /// Metadata of ADG731
        /// </summary>
        protected static Adg731Metadata _metadata = new Adg731Metadata();

        private bool _isEnabled;

        /// <summary>
        /// Is the device enabled
        /// </summary>
        public bool IsEnabled
        {
            get
            {
                return _isEnabled;
            }

            set
            {
                _isEnabled = value;
                StateChanged();
            }
        }

        private bool _isSelected;

        /// <summary>
        /// Is the device selected
        /// </summary>
        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }

            set
            {
                _isSelected = value;
                StateChanged();
            }
        }

        private int _activeChannel;

        /// <summary>
        /// The currently selected channel
        /// </summary>
        public int ActiveChannel
        {
            get
            {
                return _activeChannel;
            }

            set
            {
                _activeChannel = value % ((IMultiplexerDeviceMetadata)GetDeviceMetadata()).MultiplexerChannelCount;
                StateChanged();
            }
        }

        /// <summary>
        /// Gets the metadata class for the device
        /// </summary>
        /// <returns></returns>
        public static IDeviceMetadata GetDeviceMetadata()
        {
            return Adg731._metadata;
        }

        /// <summary>
        /// Initializes the ADC
        /// </summary>
        /// <param name="spiDevice">The SPI device to initialize the ADC on</param>
        public Adg731(SpiDevice spiDevice)
        {
            if ((spiDevice.ConnectionSettings.Mode & _metadata.ValidSpiModes) != spiDevice.ConnectionSettings.Mode)
            {
                throw new Exception("SPI device must be in SPI mode 1 or 2 in order to work with ADG731.");
            }

            if (spiDevice.ConnectionSettings.ClockFrequency > _metadata.MaximumSpiFrequency)
            {
                throw new Exception($"SPI device must have a lower clock frequency, because ADG731's maximum operating SPI frequncy is {_metadata.MaximumSpiFrequency} Hz.");
            }

            _spiDevice = spiDevice;
            StateChanged();
        }

        private void StateChanged()
        {
            byte[] data = new byte[1];

            data[0] = (byte)((!IsEnabled ? (byte)BitMask.EN : 0) | (!IsSelected ? (byte)BitMask.CS : 0) | (byte)(ActiveChannel & (byte)BitMask.Ch));

            _spiDevice.Write(data);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"ADG731 - EN:{IsEnabled}  CS:{IsSelected}  Ch:{ActiveChannel}";
        }

        /// <summary>
        /// Disposes the ADG731 object and closes the SPI device
        /// </summary>
        public void Dispose()
        {
            _spiDevice?.Dispose();
            _spiDevice = null;
        }
    }
}
