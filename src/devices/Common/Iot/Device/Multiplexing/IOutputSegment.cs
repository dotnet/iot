// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Threading;

namespace Iot.Device.Multiplexing
{
    /// <summary>
    /// Interface that abstracts multiplexing over a segment of outputs.
    /// </summary>
    public interface IOutputSegment
    {
        /// <summary>
        /// Length of segment (number of outputs)
        /// </summary>
        int Length { get; }

        /// <summary>
        /// Writes a PinValue to a multiplexed output.
        /// Does not perform a latch.
        /// </summary>
        void Write(int output, PinValue value);

        /// <summary>
        /// Writes an int (processed as one of more bytes) to a multiplexed output.
        /// Written one byte at a time, left to right. Least significant bit will written to index 0.
        /// Does not perform a latch.
        /// </summary>
        void Write(int value);

        /// <summary>
        /// Turns off all outputs.
        /// Performs a latch.
        /// </summary>
        void Clear();

        /// <summary>
        /// Displays segment until token receives a cancellation signal, possibly due to a specificated duration.
        /// As appropriate for a given implementation, performs a latch.
        /// </summary>
        void Display(TimeSpan time);
    }
}
