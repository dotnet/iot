// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
        private GpioController? _controller;
        private Channel _chargeChannel = Channel.SelfTest512;
        private Mode _operatingMode = Mode.Mode1;
        private List<int> _values = new List<int>(16);
        private int _value;

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
                if (IsValidChannel((int)value))
                {
                    _chargeChannel = value;
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(ChargeChannel), "Value must be non-negative and less than 14. Refer to table 2 and 3 in TLC1543 documentation for more information");
                }
            }
        }

        /// <summary>
        /// Operating mode used to transfer data to and from ADC
        /// <remarks>
        /// <br>Refer to table 1 in TLC1543 documentation for more information</br>
        /// </remarks>
        /// </summary>
        public Mode OperatingMode
        {
            get => _operatingMode;
            set
            {
                if ((int)value > 0 && (int)value < 6)
                {
                    _operatingMode = value;
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(OperatingMode), "Value must be positive and less than 6. Refer to table 1 in TLC1543 documentation for more information");
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
        public Tlc1543(int address, int chipSelect, int dataOut, int inputOutputClock, int endOfConversion = -1, GpioController? controller = null, PinNumberingScheme pinNumberingScheme = PinNumberingScheme.Logical, bool shouldDispose = true)
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

            _controller.Write(_chipSelect, PinValue.High);

            if (_endOfConversion != -1)
            {
                _controller.OpenPin(_endOfConversion, PinMode.InputPullUp);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Check if channelNumber is valid
        /// </summary>
        /// <param name="channelNumber">Channel Address</param>
        /// <returns></returns>
        protected static bool IsValidChannel(int channelNumber) => channelNumber >= 0 && channelNumber <= 13;

        /// <summary>
        /// Reads sensor value
        /// <remarks>
        /// <br>1 cycle used - gets data on second call of this method</br>
        /// <br>First cycle: Ask for value on the channel</br>
        /// </remarks>
        /// </summary>
        /// <param name="channelNumber">Channel to read</param>
        /// <returns>10 bit value corresponding to relative voltage level on channel</returns>
        public int ReadChannel(int channelNumber)
        {
            if (!IsValidChannel(channelNumber))
            {
                throw new ArgumentOutOfRangeException(nameof(channelNumber), "Value must be non-negative and less than 14. Refer to table 2 and 3 in TLC1543 documentation for more information");
            }

            _value = 0;
            _controller!.Write(_chipSelect, 0);

            switch (_operatingMode)
            {
                case Mode.Mode1:
                    for (int i = 0; i < 10; i++)
                    {
                        if (i < 4)
                        {
                            SendAddress(channelNumber, i);
                        }

                        // read 10-bit data
                        _value <<= 1;
                        _value |= (int)_controller.Read(_dataOut);

                        ClockImpulse();
                    }

                    _controller.Write(_chipSelect, 1);
                    if (_endOfConversion != -1)
                    {
                        // Wait for ADC to report end of conversion or timeout at max conversion time
                        _controller.WaitForEvent(_endOfConversion, PinEventTypes.Rising, _conversionTime);
                    }
                    else
                    {
                        // Max conversion time (21us) as seen in table on page 10 in TLC1543 documentation
                        DelayHelper.Delay(_conversionTime, false);
                    }

                    break;
                case Mode.Mode2:
                    for (int i = 0; i < 10; i++)
                    {
                        if (i < 4)
                        {
                            SendAddress(channelNumber, i);
                        }

                        // read 10-bit data
                        _value <<= 1;
                        _value |= (int)_controller.Read(_dataOut);

                        ClockImpulse();
                    }

                    if (_endOfConversion != -1)
                    {
                        // Wait for ADC to report end of conversion or timeout at max conversion time
                        _controller.WaitForEvent(_endOfConversion, PinEventTypes.Rising, _conversionTime);
                    }
                    else
                    {
                        // Max conversion time (21us) as seen in table on page 10 in TLC1543 documentation
                        DelayHelper.Delay(_conversionTime, false);
                    }

                    break;
                case Mode.Mode3:
                    for (int i = 0; i < 16; i++)
                    {
                        if (i < 4)
                        {
                            SendAddress(channelNumber, i);
                        }

                        // read 10-bit data
                        if (i <= 10)
                        {
                            _value <<= 1;
                            _value |= (int)_controller.Read(_dataOut);
                        }

                        if (i > 10 && _controller.Read(_endOfConversion) == PinValue.High)
                        {
                            break;
                        }

                        ClockImpulse();
                    }

                    _controller.Write(_chipSelect, 1);
                    break;
                case Mode.Mode4:
                    for (int i = 0; i < 16; i++)
                    {
                        if (i < 4)
                        {
                            SendAddress(channelNumber, i);
                        }

                        // read 10-bit data
                        if (i <= 10)
                        {
                            _value <<= 1;
                            _value |= (int)_controller.Read(_dataOut);
                        }

                        if (i > 10 && _controller.Read(_endOfConversion) == PinValue.High)
                        {
                            break;
                        }

                        ClockImpulse();
                    }

                    _controller.WaitForEvent(_endOfConversion, PinEventTypes.Rising, _conversionTime);

                    break;
                case Mode.Mode5:
                    for (int i = 0; i < 16; i++)
                    {
                        if (i < 4)
                        {
                            SendAddress(channelNumber, i);
                        }

                        // read 10-bit data
                        if (i <= 10)
                        {
                            _value <<= 1;
                            _value |= (int)_controller.Read(_dataOut);
                        }

                        if (i > 11 && _controller.Read(_endOfConversion) == PinValue.High)
                        {
                            break;
                        }

                        ClockImpulse();
                    }

                    _controller.Write(_chipSelect, 1);
                    break;
                case Mode.Mode6:
                    for (int i = 0; i < 16; i++)
                    {
                        if (i < 4)
                        {
                            SendAddress(channelNumber, i);
                        }

                        // read 10-bit data
                        if (i <= 10)
                        {
                            _value <<= 1;
                            _value |= (int)_controller.Read(_dataOut);
                        }

                        ClockImpulse();
                    }

                    break;
                default:
                    break;
            }

            return _value;
        }

        /// <summary>
        /// Sends 4-bit Address
        /// </summary>
        /// <param name="channelNumber">Channel number to send</param>
        /// <param name="byteNumber">Which bit to send</param>
        private void SendAddress(int channelNumber, int byteNumber)
        {
            if (((channelNumber >> (3 - byteNumber)) & 0x01) == 1)
            {
                _controller!.Write(_address, 1);
            }
            else
            {
                _controller!.Write(_address, 0);
            }
        }

        /// <summary>
        /// Sends Clock impulse
        /// </summary>
        private void ClockImpulse()
        {
            _controller!.Write(_inputOutputClock, 1);
            DelayHelper.DelayMicroseconds(0, false);
            _controller.Write(_inputOutputClock, 0);
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
                _controller = null!;
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
