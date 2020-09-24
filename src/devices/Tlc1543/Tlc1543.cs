// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device;
using System.Device.Gpio;
using System.Collections.Generic;

namespace Iot.Device.Tlc1543
{
    /// <summary>
    /// Add documentation here
    /// </summary>
    public class Tlc1543 : IDisposable
    {
        #region Members

        private readonly int _chipSelect;
        private readonly int _dataOut;
        private readonly int _address;
        private readonly int _inputOutputClock;
        private readonly int? _endOfConversion;
        private readonly int _channels = 11;
        private readonly TimeSpan _conversionTime = new TimeSpan(210);
        private GpioController _digital;
        private bool _shouldDispose;
        private Channel _chargeChannel = Channel.SelfTest512;

        /// <summary>
        /// Channel used between readings to charge up capacitors in ADC
        /// <remarks>
        /// <br>Refer to table 2 and 3 in TLC1543 documentation for more information</br>
        /// </remarks>
        /// </summary>
        public Channel ChargeChannel
        {
            get => _chargeChannel;
            set
            {
                if ((int)value >= 0 & (int)value < 14)
                {
                    _chargeChannel = value;
                }
                else
                {
                    throw new ArgumentOutOfRangeException("ChargeChannel", "Out of range. \nMust be non-negative and less than 14. \nRefer to table 2 and 3 in TLC1543 documentation for more information");
                }
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor for Tlc1543
        /// </summary>
        /// <param name="address">Address</param>
        /// <param name="chipSelect">Chip Select</param>
        /// <param name="dataOut">Data Out</param>
        /// <param name="inputOutputClock">I/O Clock</param>
        /// <param name="controller"> s</param>
        /// <param name="pinNumberingScheme"> s</param>
        /// <param name="shouldDispose"> s</param>
        public Tlc1543(int address, int chipSelect, int dataOut, int inputOutputClock, GpioController controller = null, PinNumberingScheme pinNumberingScheme = PinNumberingScheme.Logical, bool shouldDispose = true)
        {
            _shouldDispose = controller == null || shouldDispose;
            _digital = controller ?? new GpioController(pinNumberingScheme);
            _address = address;
            _chipSelect = chipSelect;
            _dataOut = dataOut;
            _inputOutputClock = inputOutputClock;

            _digital.OpenPin(_address, PinMode.Output);
            _digital.OpenPin(_chipSelect, PinMode.Output);
            _digital.OpenPin(_dataOut, PinMode.InputPullUp);
            _digital.OpenPin(_inputOutputClock, PinMode.Output);
        }

        /// <summary>
        /// Constructor for Tlc1543
        /// </summary>
        /// <param name="address">Address</param>
        /// <param name="chipSelect">Chip Select</param>
        /// <param name="dataOut">Data Out</param>
        /// <param name="endOfConversion">End of Conversion</param>
        /// <param name="inputOutputClock">I/O Clock</param>
        public Tlc1543(int address, int chipSelect, int dataOut, int inputOutputClock, int endOfConversion)
        {
            _address = address;
            _chipSelect = chipSelect;
            _dataOut = dataOut;
            _endOfConversion = endOfConversion;
            _inputOutputClock = inputOutputClock;

            _digital.OpenPin(_address, PinMode.Output);
            _digital.OpenPin(_chipSelect, PinMode.Output);
            _digital.OpenPin(_dataOut, PinMode.InputPullUp);
            _digital.OpenPin((int)_endOfConversion, PinMode.InputPullUp);
            _digital.OpenPin(_inputOutputClock, PinMode.Output);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Reads sensor value in Fast Mode 1 (10 clock transfer using !chipSelect)
        /// <remarks>
        /// <br>1 cycle used - gets data on second call of this method</br>
        /// <br>First cycle: Ask for value on the channel</br>
        /// </remarks>
        /// </summary>
        /// <param name="channelNumber">Channel to read</param>
        /// <returns>10 bit value corresponding to relative voltage level on channel</returns>
        public int ReadChannel(int channelNumber)
        {
            if (channelNumber < 0 & channelNumber > 14)
            {
                throw new ArgumentOutOfRangeException("channelNumber", "Out of range. \nMust be non-negative and less than 14. \nRefer to table 2 and 3 in TLC1543 documentation for more information");
            }

            int value = 0;
            _digital.Write(_chipSelect, 0);
            for (int i = 0; i < 10; i++)
            {
                if (i < 4)
                {
                    // send 4-bit Address
                    if ((channelNumber >> (3 - i) & 0x01) != 0)
                    {
                        _digital.Write(_address, 1);
                    }
                    else
                    {
                        _digital.Write(_address, 0);
                    }
                }

                // read 10-bit data
                value <<= 1;
                if (_digital.Read(_dataOut) == PinValue.High)
                {
                    value |= 0x01;
                }

                _digital.Write(_inputOutputClock, 1);
                _digital.Write(_inputOutputClock, 0);
            }

            _digital.Write(_chipSelect, 1);
            // Max conversion time (21us) as seen in table on page 10 in TLC1543 documentation
            DelayHelper.Delay(_conversionTime, false);
            return value;
        }

        /// <summary>
        /// Reads sensor value in Fast Mode 1 (2 cycle used)
        /// <br>First cycle: Ask for value on the channel (10 clock transfer with !chipSelect high)</br>
        /// <br>Second cycle: Return value from the channel while charging to <seealso cref="ChargeChannel"/></br>
        /// </summary>
        /// <param name="channelNumber">Channel to be read</param>
        /// <returns>A 10 bit value corresponding to relative voltage level on specified device channel</returns>
        public int ReadChannel(Channel channelNumber)
        {
            ReadChannel((int)channelNumber);
            return ReadChannel((int)ChargeChannel);
        }

        /// <summary>
        /// Reads sensor values into an List
        /// <remarks>
        /// <br>Fastest and most efficient to use</br>
        /// <br>Reads channels in order in the list</br>
        /// <br>Uses (list size + 1) cycles </br>
        /// </remarks>
        /// </summary>
        /// <param name="channelList">List of channels to read</param>
        /// <returns>List of 10 bit values corresponding to relative voltage level on specified device channels</returns>
        public List<int> ReadChannels(List<Channel> channelList)
        {
            List<int> values = new List<int>(channelList.Count);
            ReadChannel((int)channelList[0]);
            for (int i = 1; i < channelList.Count; i++)
            {
                values.Add(ReadChannel((int)channelList[i]));
            }

            values.Add(ReadChannel((int)ChargeChannel));
            return values;
        }

        /// <summary>
        /// Reads all sensor values into an List
        /// </summary>
        /// <returns>List of 10 bit values corresponding to relative voltage level on all channels</returns>
        public List<int> ReadAll()
        {
            List<int> values = new List<int>(_channels);
            ReadChannel(0);
            for (int i = 1; i < values.Count; i++)
            {
                values.Add(ReadChannel(i));
            }

            values.Add(ReadChannel((int)ChargeChannel));
            return values;
        }

        #endregion

        #region Disposing

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            if (_shouldDispose)
            {
                _digital?.Dispose();
            }
            else
            {
                _digital?.ClosePin(_address);
                _digital?.ClosePin(_chipSelect);
                _digital?.ClosePin(_dataOut);
                _digital?.ClosePin(_inputOutputClock);
                if (_endOfConversion.HasValue)
                {
                    _digital?.ClosePin((int)_endOfConversion);
                }
            }
        }

        #endregion
    }
}
