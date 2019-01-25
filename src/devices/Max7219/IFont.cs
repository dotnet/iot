// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;

namespace Iot.Device.Max7219
{
    /// <summary>
    /// A font contains one list of bytes per character which can be written to the matrix to represent the character.
    /// </summary>
    /// <remarks>
    /// Each character consists of a list of bytes where a single byte represents a column of the display.
    /// </remarks>
    /// 
    /// <example>
    /// This example shows how the 'A' char could by encoded:
    /// <code>
    /// var aBytes = new byte[] {
    ///     0b1111100, 
    ///     0b1111110, 
    ///     0b0010011, 
    ///     0b0010011, 
    ///     0b1111110, 
    ///     0b1111100, 
    ///     0b0000000, 
    ///     0b0000000 
    /// };
    /// </code>
    /// 
    /// </example>
    public interface IFont {

        /// <summary>
        /// Returns a list of bytes for a given character to be written to a matrix.
        /// </summary>
        IReadOnlyList<byte> this[char chr]
        {
            get;
        }
    }
}