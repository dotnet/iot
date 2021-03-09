// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Threading;

namespace Iot.Device.Multiplexing.Utility
{
    /// <summary>
    /// Interface that abstracts multiplexing over a segment of outputs.
    /// </summary>
    public class VirtualOutputSegment : IOutputSegment
    {
        private readonly int _length;
        private readonly PinValue[] _values;

        /// <summary>
        /// A virtual implementation of IOutputSegment that manages the values of a set of virtual outputs.
        /// This type is intended as a helper to be used in IOutputSegment implementations.
        /// </summary>
        /// <param name="length">The number of outputs in the segment.</param>
        public VirtualOutputSegment(int length)
        {
            _length = length;
            _values = new PinValue[_length];
        }

        /// <summary>
        /// Length of segment (number of outputs).
        /// </summary>
        public int Length => _length;

        /// <summary>
        /// Segment values.
        /// </summary>
        public PinValue this[int index] => _values[index];

        /// <summary>
        /// Writes a PinValue to a virtual output.
        /// Does not latch.
        /// </summary>
        public void Write(int output, PinValue value)
        {
            _values[output] = value;
        }

        /// <summary>
        /// Writes a byte to a virtual output.
        /// Does not latch.
        /// </summary>
        public void Write(int value)
        {
            for (int i = (_length / 8) - 1; i > 0; i--)
            {
                int shift = i * 8;
                int downShiftedValue = value >> shift;
                WriteByteAsValues((byte)downShiftedValue, shift);
            }

            WriteByteAsValues((byte)value, 0);

            void WriteByteAsValues(byte value, int offset)
            {
                for (int i = 0; i < 8; i++)
                {
                    // create mask to determine value of bit
                    // starts left-most and ends up right-most (after 8th interation)
                    // 0b_1000_0000 is used; other algorithms use 128. They are the same value.
                    int data = (0b_1000_0000 >> i) & value;
                    int index = offset + 8 - i - 1;
                    _values[index] = data;
                }
            }
        }

        /// <summary>
        /// Writes a Low PinValue to all outputs.
        /// Performs a latch.
        /// </summary>
        public void Clear()
        {
            for (int i = 0; i < _length; i++)
            {
                _values[i] = PinValue.Low;
            }
        }

        /// <summary>
        /// Displays segment until token receives a cancellation signal, possibly due to a specificated duration.
        /// As appropriate for a given implementation, performs a latch.
        /// </summary>
        public void Display(CancellationToken token) => token.WaitHandle.WaitOne();

        /// <summary>
        /// Disposes any native resources.
        /// </summary>
        public void Dispose()
        {
        }
    }
}
