// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.CharacterLcd
{
    /// <summary>
    /// Controls what happens when the cursor moves beyond the end of the display (both horizontally or vertically).
    /// </summary>
    public enum LineWrapMode
    {
        /// <summary>
        /// The cursor will stay beyond the edge of the screen.
        /// Any further write attempts will not do anything until the cursor is moved or the display cleared.
        /// A newline will move to the next line, unless already on the last line of the display.
        /// </summary>
        Truncate = 0,

        /// <summary>
        /// Wraps to the next line if the end of the line is reached.
        /// </summary>
        Wrap = 1,

        /// <summary>
        /// Attempts to wrap at word borders when the text does not fit on the (remaining part of) the line.
        /// </summary>
        WordWrap = 2,
    }
}
