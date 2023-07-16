// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device;
using System.Device.Gpio;
using System.Linq;
using UnitsNet;

namespace Iot.Device.Hx711
{
    internal sealed class Hx711Reader
    {
        private readonly GpioController _gpioController;
        private readonly int _pinDout;
        private readonly int _pinPD_Sck;

        private readonly object _readLock;

        private readonly Hx711Options _options;
        private readonly ByteFormat _byteFormat;
        private readonly ByteFormat _bitFormat;

        internal Hx711Reader(GpioController gpioController, Hx711Options options, int pinDout, int pinPD_Sck, object readLock)
        {
            _gpioController = gpioController;
            _options = options;
            _pinDout = pinDout;
            _pinPD_Sck = pinPD_Sck;
            _readLock = readLock;

            // According to the Hx711 Datasheet, order of bits inside each byte is Msb so you shouldn't need to modify it.
            // Docs say "... starting with the Msb bit first ..."
            // page 4
            // https://html.alldatasheet.com/html-pdf/1132222/AVIA/Hx711/573/4/Hx711.html
            _bitFormat = ByteFormat.Msb;

            // Some Hx711 manufacturers return bytes in Lsb, but most in Msb.
            if (options.UseByteLittleEndian)
            {
                _byteFormat = ByteFormat.Lsb;
            }
            else
            {
                _byteFormat = ByteFormat.Msb;
            }
        }

        /// <summary>
        /// Read a weight value from Hx711, how accurate depends on the number of reading passed
        /// </summary>
        /// <param name="numberOfReads">Number of readings to take from which to average, to get a more accurate value.</param>
        /// <param name="offsetFromZero">Offset value from 0</param>
        /// <returns>Return a weight read</returns>
        /// <exception cref="ArgumentException">Throw if number of reads have invalid value</exception>
        public int Read(int numberOfReads = 3, int offsetFromZero = 0)
        {
            // Make sure we've been asked to take a rational amount of samples.
            if (numberOfReads <= 0)
            {
                throw new ArgumentException(message: "Param value must be greater than zero!", nameof(numberOfReads));
            }

            // If we're only average across one value, just read it and return it.
            if (numberOfReads == 1)
            {
                return CalculateNetValue(ReadInt(), offsetFromZero);
            }

            // If we're averaging across a low amount of values, just take the
            // median.
            if (numberOfReads < 5)
            {
                return CalculateNetValue(ReadMedian(numberOfReads), offsetFromZero);
            }

            return CalculateNetValue(ReadAverage(numberOfReads), offsetFromZero);
        }

        /// <summary>
        /// Calculate net value
        /// </summary>
        /// <param name="value">Gross value read from Hx711</param>
        /// <param name="offset">Offset value from 0</param>
        /// <returns>Return net value read</returns>
        private static int CalculateNetValue(int value, int offset)
        {
            return value - offset;
        }

        /// <summary>
        /// Hx711 Channel and gain factor are set by number of bits read
        /// after 24 data bits.
        /// </summary>
        /// <param name="mode">Current Hx711 mode</param>
        /// <returns>Number of extrabit after 24 bit</returns>
        /// <exception cref="ArgumentOutOfRangeException">Throw if mode value is invalid.</exception>
        /// <remarks>Look table "Table 3 Input Channel and Gain Selection" in doc page 4
        /// https://html.alldatasheet.com/html-pdf/1132222/AVIA/Hx711/457/4/Hx711.html</remarks>
        private static int CalculateExtraBitByMode(Hx711Mode mode)
        {
            switch (mode)
            {
                case Hx711Mode.ChannelAGain128:
                    return 1;
                case Hx711Mode.ChannelBGain32:
                    return 2;
                case Hx711Mode.ChannelAGain64:
                    return 3;
                default:
                    throw new ArgumentOutOfRangeException(paramName: nameof(mode), message: "Unknow Hx711 mode.");
            }
        }

        /// <summary>
        /// The output 24 bits of data is in 2's complement format. Convert it to int.
        /// </summary>
        /// <param name="inputValue">24 bit in 2' complement format</param>
        /// <returns>Int converted</returns>
        private static int ConvertFromTwosComplement24bit(int inputValue)
        {
            // Docs says
            // "When input differential signal goes out of the 24-bit range,
            // the output data will be saturated at 800000h (MIN) or 7FFFFFh (MAX),
            // until the input signal comes back to the input range.", page 4
            // https://html.alldatasheet.com/html-pdf/1132222/AVIA/Hx711/457/4/Hx711.html

            // 24 bit in 2's complement only 23 are a value if
            // the number is negative. 0xFFFFFF >> 1 = 0x7FFFFF
            // Mask to take true value
            const int MaxValue = 0x7FFFFF;
            // Mask to take sign bit
            const int BitSign = 0x800000;

            return -(inputValue & BitSign) + (inputValue & MaxValue);
        }

        /// <summary>
        /// Check if Hx711 is ready
        /// </summary>
        private bool IsOutputDataReady()
        {
            // Doc says "When output data is not ready for retrieval, digital output
            // pin DOUT is high.
            // ...
            // When DOUT goes to low, it indicates data is ready for retrieval", page 4
            // https://html.alldatasheet.com/html-pdf/1132222/AVIA/Hx711/457/4/Hx711.html
            var valueRead = _gpioController.Read(_pinDout);
            return valueRead != PinValue.High;
        }

        /// <summary>
        /// A avarage-based read method, might help when getting random value spikes
        /// </summary>
        /// <param name="numberOfReads">Number of readings to take from which to average, to get a more accurate value.</param>
        /// <returns>Return a weight read</returns>
        private int ReadAverage(int numberOfReads)
        {
            // If we're taking a lot of samples, we'll collect them in a list, remove
            // the outliers, then take the mean of the remaining set.
            var valueList = new List<int>(numberOfReads);

            for (int x = 0; x < numberOfReads; x++)
            {
                valueList.Add(ReadInt());
            }

            valueList.Sort();

            // We'll be trimming 20% of outlier samples from top and bottom of collected set.
            int trimAmount = Convert.ToInt32(Math.Round(valueList.Count * 0.2));

            // Trim the edge case values.
            valueList = valueList.Skip(trimAmount).Take(valueList.Count - (trimAmount * 2)).ToList();

            // Return the mean of remaining samples.
            return Convert.ToInt32(Math.Round(valueList.Average()));
        }

        /// <summary>
        /// A median-based read method, might help when getting random value spikes for unknown or CPU-related reasons
        /// </summary>
        /// <param name="numberOfReads">Number of readings to take from which to average, to get a more accurate value.</param>
        /// <returns>Return a weight read</returns>
        private int ReadMedian(int numberOfReads)
        {
            var valueList = new List<int>(numberOfReads);

            for (int x = 0; x < numberOfReads; x++)
            {
                valueList.Add(ReadInt());
            }

            valueList.Sort();

            // If times is odd we can just take the centre value.
            if ((numberOfReads & 0x1) == 0x1)
            {
                return valueList[valueList.Count / 2];
            }
            else
            {
                // If times is even we have to take the arithmetic mean of
                // the two middle values.
                var midpoint = valueList.Count / 2;
                return (valueList[midpoint] + valueList[midpoint + 1]) / 2;
            }
        }

        /// <summary>
        /// Read a weight value from Hx711
        /// </summary>
        /// <returns>Return a weight read</returns>
        private int ReadInt()
        {
            // Get a sample from the Hx711 in the form of raw bytes.
            var dataBytes = ReadRawBytes();

            // Join the raw bytes into a single 24bit 2s complement value.
            int twosComplementValue = (dataBytes[0] << 16)
                | (dataBytes[1] << 8)
                | dataBytes[2];

            // Convert from 24bit twos-complement to a signed value.
            int signedIntValue = ConvertFromTwosComplement24bit(twosComplementValue);

            // Return the sample value we've read from the Hx711.
            return signedIntValue;
        }

        /// <summary>
        /// Read one value from Hx711
        /// </summary>
        /// <returns>Return bytes read</returns>
        private byte[] ReadRawBytes()
        {
            // Wait for and get the Read Lock, incase another thread is already
            // driving the Hx711 serial interface.
            lock (_readLock)
            {
                // Doc says "Serial clock input PD_SCK shold be low", page
                // https://html.alldatasheet.com/html-pdf/1132222/AVIA/Hx711/457/4/Hx711.html
                _gpioController.Write(_pinPD_Sck, PinValue.Low);

                // Wait until Hx711 is ready for us to read a sample.
                while (!IsOutputDataReady())
                {
                    DelayHelper.DelayMicroseconds(microseconds: 1, allowThreadYield: true);
                }

                // Read three bytes (24bit) of data from the Hx711.
                var firstByte = ReadNextByte();
                var secondByte = ReadNextByte();
                var thirdByte = ReadNextByte();

                // Reading extra bit
                for (int i = 0; i < CalculateExtraBitByMode(_options.Mode); i++)
                {
                    // Clock a bit out of the Hx711 and throw it away.
                    _ = ReadNextBit();
                }

                // Depending on how we're configured, return an orderd list of raw byte
                // values.
                return _byteFormat == ByteFormat.Lsb
                    ? (new[] { thirdByte, secondByte, firstByte })
                    : (new[] { firstByte, secondByte, thirdByte });

                // Release the Read Lock, now that we've finished driving the Hx711
                // serial interface.
            }
        }

        /// <summary>
        /// Read bits and build the byte
        /// </summary>
        /// <returns>Byte readed by Hx711</returns>
        private byte ReadNextByte()
        {
            byte byteValue = 0;

            // Read bits and build the byte from top, or bottom, depending
            // on whether we are in Msb or Lsb bit mode.
            for (int x = 0; x < 8; x++)
            {
                if (_bitFormat == ByteFormat.Msb)
                {
                    byteValue <<= 1;
                    byteValue |= ReadNextBit();
                }
                else
                {
                    byteValue >>= 1;
                    byteValue |= (byte)(ReadNextBit() * 0x80);
                }
            }

            return byteValue;
        }

        /// <summary>
        /// Read next bit by send a signal to Hx711
        /// </summary>
        /// <returns>Return bit read from Hx711</returns>
        private byte ReadNextBit()
        {
            // Clock Hx711 Digital Serial Clock (PD_SCK). DOUT will be
            // ready 1µs after PD_SCK rising edge, so we sample after
            // lowering PD_SCL, when we know DOUT will be stable.
            _gpioController.Write(_pinPD_Sck, PinValue.High);
            _gpioController.Write(_pinPD_Sck, PinValue.Low);
            var value = _gpioController.Read(_pinDout);

            return value == PinValue.High ? (byte)1 : (byte)0;
        }
    }
}
