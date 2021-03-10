// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Threading;
using System.Threading.Tasks;

namespace Iot.Device.Multiplexing
{
    /// <summary>
    /// Abstracts a segment of outputs from multiplexing sources (like a shift register).
    /// </summary>
    public interface IOutputSegment : IDisposable
    {
        /// <summary>
        /// Length of segment (number of outputs)
        /// </summary>
        int Length { get; }

        /// <summary>
        /// Segment values.
        /// </summary>
        PinValue this[int index] { get; set; }

        /// <summary>
        /// Writes a PinValue to a virtual segment.
        /// Does not display output until calling Display() or Display(CancellationToken ct) methods.
        /// </summary>
        void Write(int index, PinValue value);

        /// <summary>
        /// Writes discrete underlying bits to a virtual segment.
        /// Writes each bit, left to right. Least significant bit will written to index 0.
        /// Does not display output.
        /// </summary>
        void Write(byte value);

        /// <summary>
        /// Writes discrete underlying bits to a virtual output.
        /// Writes each byte, left to right. Least significant bit will written to index 0.
        /// Does not display output.
        /// </summary>
        void Write(ReadOnlySpan<byte> value);

        /// <summary>
        /// Turns off all outputs.
        /// </summary>
        void TurnOffAll();

        /// <summary>
        /// Displays current state of segment.
        /// Segment is displayed at least until token receives a cancellation signal, possibly due to a specified duration expiring.
        /// </summary>
        void Display(CancellationToken token);

        /// <summary>
        /// Displays current state of segment.
        /// Segment is displayed at least until token receives a cancellation signal, possibly due to a specified duration expiring.
        /// </summary>
        Task DisplayAsync(CancellationToken token);
    }
}
