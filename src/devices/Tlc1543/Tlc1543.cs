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
        private readonly int _endOfConversion;
        private readonly bool _shouldDispose;
        private readonly TimeSpan _conversionTime = new TimeSpan(210);
        private GpioController _controller;
        private Channel _chargeChannel = Channel.SelfTest512;
        private List<int> _values = new List<int>(16);

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
        /// <param name="address">Address pin</param>
        /// <param name="chipSelect">Chip Select pin</param>
        /// <param name="dataOut">Data Out pin</param>
        /// <param name="inputOutputClock">I/O Clock pin</param>
        /// <param name="endOfConversion">End of Conversion pin</param>
        /// <param name="controller">The GPIO controller for defined external pins. If not specified, the default controller will be used.</param>
        /// <param name="pinNumberingScheme">Pin Numbering Scheme</param>
        /// <param name="shouldDispose">True to dispose the Gpio Controller</param>
        public Tlc1543(int address, int chipSelect, int dataOut, int inputOutputClock, int endOfConversion = -1, GpioController controller = null, PinNumberingScheme pinNumberingScheme = PinNumberingScheme.Logical, bool shouldDispose = true)
        {
            _shouldDispose = controller == null || shouldDispose;
            _controller = controller ?? new GpioController(pinNumberingScheme);
            _address = address;
            _chipSelect = chipSelect;
            _dataOut = dataOut;
            _endOfConversion = endOfConversion;
            _inputOutputClock = inputOutputClock;

            _controller.OpenPin(_address, PinMode.Output);
            _controller.OpenPin(_chipSelect, PinMode.Output);
            _controller.OpenPin(_dataOut, PinMode.InputPullUp);
            _controller.OpenPin(_inputOutputClock, PinMode.Output);
            if (_endOfConversion != -1)
            {
                _controller.OpenPin(_endOfConversion, PinMode.InputPullUp);
            }
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
            _controller.Write(_chipSelect, 0);
            for (int i = 0; i < 10; i++)
            {
                if (i < 4)
                {
                    // send 4-bit Address
                    if (((channelNumber >> (3 - i)) & 0x01) == 1)
                    {
                        _controller.Write(_address, 1);
                    }
                    else
                    {
                        _controller.Write(_address, 0);
                    }
                }

                // read 10-bit data
                value <<= 1;
                if (_controller.Read(_dataOut) == PinValue.High)
                {
                    value |= 0x01;
                }

                _controller.Write(_inputOutputClock, 1);
                DelayHelper.DelayMicroseconds(0, false);
                _controller.Write(_inputOutputClock, 0);
            }

            _controller.Write(_chipSelect, 1);
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
            _values.Clear();
            ReadChannel((int)channelList[0]);
            for (int i = 1; i < channelList.Count; i++)
            {
                _values.Add(ReadChannel((int)channelList[i]));
            }

            _values.Add(ReadChannel((int)ChargeChannel));
            return _values;
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
                _controller?.Dispose();
            }
            else
            {
                _controller?.ClosePin(_address);
                _controller?.ClosePin(_chipSelect);
                _controller?.ClosePin(_dataOut);
                _controller?.ClosePin(_inputOutputClock);
                if (_endOfConversion != -1)
                {
                    _controller?.ClosePin(_endOfConversion);
                }
            }
        }

        #endregion
    }
}
