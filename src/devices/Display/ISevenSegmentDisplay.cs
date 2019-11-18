// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.Display
{
    /// <summary>
    /// Represents a 7-Segment display that can display multiple digits
    /// </summary>
    public interface ISevenSegmentDisplay
    {
        /// <summary>
        /// Gets the number of digits supported by the display
        /// </summary>
        int NumberOfDigits { get; }

        /// <summary>
        /// Gets or sets a single digit's segments by id
        /// </summary>
        /// <param name="address">address of digit</param>
        Segment this[int address] { get; set; }

        /// <summary>
        /// Write a series of digits to the display buffer
        /// </summary>
        /// <param name="digits">a list of digits represented in segments</param>
        /// <param name="startAddress">Address to start writing from</param>
        void Write(ReadOnlySpan<Segment> digits, int startAddress = 0);

        /// <summary>
        /// Write a series of characters to the display buffer
        /// </summary>
        /// <param name="characters">a list of characters represented in fonts</param>
        /// <param name="startAddress">Address to start writing from</param>
        void Write(ReadOnlySpan<Font> characters, int startAddress = 0);
    }
}
