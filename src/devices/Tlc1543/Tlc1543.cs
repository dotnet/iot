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

        /// <summary>
        /// Channel used between readings to charge up capacitors in ADC
        /// </summary>
        public Channel ChargeChannel = Channel.SelfTest512;
        private readonly int _chipSelect;
        private readonly int _dataOut;
        private readonly int _address;
        private readonly int _inputOutputClock;
        private readonly int _endOfConversion;
        private readonly int _channels = 11;
        private GpioController _digital = new GpioController(PinNumberingScheme.Logical);
        private bool _disposedValue;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor for Tlc1543
        /// </summary>
        /// <param name="ADDR">Address</param>
        /// <param name="CS">Chip Select</param>
        /// <param name="DOUT">Data Out</param>
        /// <param name="IOCLK">I/O Clock</param>
        public Tlc1543(int ADDR, int CS, int DOUT, int IOCLK)
        {
            _address = ADDR;
            _chipSelect = CS;
            _dataOut = DOUT;
            _inputOutputClock = IOCLK;

            _digital.OpenPin(_address, PinMode.Output);
            _digital.OpenPin(_chipSelect, PinMode.Output);
            _digital.OpenPin(_dataOut, PinMode.InputPullUp);
            _digital.OpenPin(_inputOutputClock, PinMode.Output);
        }

        /// <summary>
        /// Constructor for Tlc1543
        /// </summary>
        /// <param name="ADDR">Address</param>
        /// <param name="CS">Chip Select</param>
        /// <param name="DOUT">Data Out</param>
        /// <param name="EOC">End of Conversion</param>
        /// <param name="IOCLK">I/O Clock</param>
        public Tlc1543(int ADDR, int CS, int DOUT, int IOCLK, int EOC)
        {
            _address = ADDR;
            _chipSelect = CS;
            _dataOut = DOUT;
            _endOfConversion = EOC;
            _inputOutputClock = IOCLK;

            _digital.OpenPin(_address, PinMode.Output);
            _digital.OpenPin(_chipSelect, PinMode.Output);
            _digital.OpenPin(_dataOut, PinMode.InputPullUp);
            _digital.OpenPin(_endOfConversion, PinMode.InputPullUp);
            _digital.OpenPin(_inputOutputClock, PinMode.Output);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Reads sensor value
        /// </summary>
        /// <param name="channelNumber">Channel to be read</param>
        /// <returns>A 10 bit value corresponding to relative voltage level on specified device channel</returns>
        public int ReadChannel(Channel channelNumber)
        {
            Read((int)channelNumber);
            return Read((int)ChargeChannel);
        }

        /// <summary>
        /// Reads sensor values into an List
        /// </summary>
        /// <param name="channelList">List of channels to read</param>
        /// <returns>List of 10 bit values corresponding to relative voltage level on specified device channels</returns>
        public List<int> ReadChannels(List<Channel> channelList)
        {
            List<int> values = new List<int>(channelList.Count);
            Read((int)channelList[0]);
            for (int i = 1; i < channelList.Count; i++)
            {
                values.Add(Read((int)channelList[i]));
            }

            values.Add(Read((int)ChargeChannel));
            return values;
        }

        /// <summary>
        /// Reads all sensor values into an List
        /// </summary>
        /// <returns>List of 10 bit values corresponding to relative voltage level on all channels</returns>
        public List<int> ReadAll()
        {
            List<int> values = new List<int>(_channels);
            Read(0);
            for (int i = 1; i < values.Count; i++)
            {
                values.Add(Read(i));
            }

            values.Add(Read((int)ChargeChannel));
            return values;
        }

        /// <summary>
        /// Mode 1 (Fast mode):
        /// <br>10 clock transfer with !CS high </br>
        /// <br>Figure 9 in datasheet</br>
        /// </summary>
        /// <param name="channelNumber">Channel to read</param>
        /// <returns>10 bit value corresponding to relative voltage level on channel</returns>
        public int Read(int channelNumber)
        {
            int value = 0;
            _digital.Write(_chipSelect, 0);
            DelayHelper.DelayMicroseconds(1, false);
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
            DelayHelper.DelayMicroseconds(100, true); // t(PZH/PZL)

            return value;
        }

        #endregion

        #region Enums

        /// <summary>
        /// Available Channels to poll from on Tlc1543
        /// </summary>
        public enum Channel
        {
            /// <summary>
            /// Channel A0
            /// </summary>
            A0 = 0,

            /// <summary>
            /// Channel A1
            /// </summary>
            A1 = 1,

            /// <summary>
            /// Channel A2
            /// </summary>
            A2 = 2,

            /// <summary>
            /// Channel A3
            /// </summary>
            A3 = 3,

            /// <summary>
            /// Channel A4
            /// </summary>
            A4 = 4,

            /// <summary>
            /// Channel A5
            /// </summary>
            A5 = 5,

            /// <summary>
            /// Channel A6
            /// </summary>
            A6 = 6,

            /// <summary>
            /// Channel A7
            /// </summary>
            A7 = 7,

            /// <summary>
            /// Channel A8
            /// </summary>
            A8 = 8,

            /// <summary>
            /// Channel A9
            /// </summary>
            A9 = 9,

            /// <summary>
            /// Channel A10
            /// </summary>
            A10 = 10,

            /// <summary>
            /// Self Test channel that sets charge capacitors to (Vref+ - Vref-)/2
            /// </summary>
            SelfTest512 = 11,

            /// <summary>
            /// Self Test channel that sets charge capacitors to Vref-
            /// </summary>
            SelfTest0 = 12,

            /// <summary>
            /// Self Test channel that sets charge capacitors to Vref+
            /// </summary>
            SelfTest1024 = 13,
        }

        #endregion

        #region Disposing

        /// <summary>
        /// Dispose
        /// </summary>
        /// <param name="disposing">is disposing</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    _digital?.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                _disposedValue = true;
            }
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
