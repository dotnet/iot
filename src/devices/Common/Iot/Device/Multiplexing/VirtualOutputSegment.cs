// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Threading;
using System.Threading.Tasks;

namespace Iot.Device.Multiplexing.Utility
{
    /// <summary>
    /// Interface that abstracts multiplexing over a segment of outputs.
    /// </summary>
    public sealed class VirtualOutputSegment : IOutputSegment
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
        public PinValue this[int index]
        {
            get => _values[index];
            set => _values[index] = value;
        }

        /// <summary>
        /// Writes a PinValue to a virtual segment.
        /// Does not display output.
        /// </summary>
        public void Write(int output, PinValue value)
        {
            _values[output] = value;
        }

        /// <summary>
        /// Writes discrete underlying bits to a virtual segment.
        /// Writes each bit, left to right. Least significant bit will written to index 0.
        /// Does not display output.
        /// </summary>
        public void Write(byte value)
            => WriteByteAsValues(value, 0);

        /// <summary>
        /// Writes discrete underlying bits to a virtual output.
        /// Writes each byte, left to right. Least significant bit will written to index 0.
        /// Does not display output.
        /// </summary>
        public void Write(ReadOnlySpan<byte> value)
        {
            // Scenarios
            // values can be shorter than byteLength e.g. (1 * 8) - 16 = -8 < 0
            // values can be longer than byteLength e.g. (3 * 8) - 16 = 8 > 0
            // values can be same as byteLength e.g. (2 * 8) - 16 = 0
            if (value.Length * 8 > _length)
            {
                throw new ArgumentException($"The bytes provided exceed the length of the {nameof(IOutputSegment)}.");
            }

            for (int i = 0; i < value.Length; i++)
            {
                int offset = i * 8;
                WriteByteAsValues(value[i], offset);
            }
        }

        private void WriteByteAsValues(byte value, int offset)
        {
            for (int i = 0; i < 8; i++)
            {
                // create mask to determine value of bit
                // starts left-most and ends up right-most (after 8th interation)
                // 0b_1000_0000 is used; other algorithms use 128. They are the same value.
                int data = (0b_1000_0000 >> i) & value;
                int index = offset + 7 - i;
                _values[index] = data;
            }
        }

        /// <summary>
        /// Writes a Low PinValue to all outputs.
        /// Performs a latch.
        /// </summary>
        public void TurnOffAll()
        {
            for (int i = 0; i < _length; i++)
            {
                _values[i] = PinValue.Low;
            }
        }

        /// <summary>
        /// Displays current state of segment.
        /// Segment is displayed at least until token receives a cancellation signal, possibly due to a specified duration expiring.
        /// </summary>
        public void Display(CancellationToken token) => token.WaitHandle.WaitOne();

        /// <summary>
        /// Displays current state of segment.
        /// Segment is displayed at least until token receives a cancellation signal, possibly due to a specified duration expiring.
        /// </summary>
        public Task DisplayAsync(CancellationToken token) => Task.Delay(Timeout.Infinite, token);

        /// <summary>
        /// Disposes any native resources.
        /// </summary>
        public void Dispose()
        {
        }
    }
}
