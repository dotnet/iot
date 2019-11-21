using System;
using System.Collections.Generic;
using System.Text;

namespace Iot.Device.CharacterLcd
{
    /// <summary>
    /// Controls what happens when the cursor moves beyond the end of the display (both horizontally or vertically). 
    /// </summary>
    [Flags]
    public enum LineFeedMode
    {
        /// <summary>
        /// The cursor will stay beyond the edge of the screen. 
        /// Any further write attempts will not do anything until the cursor is moved or the display cleared. 
		/// A newline will move to the next line, unless already on the last line of the display. 
        /// </summary>
        Disable = 0, 

        /// <summary>
        /// Wraps to the next line if the end of the line is reached.
        /// </summary>
        UntilFull = 0x2,

        /// <summary>
        /// Intelligently wraps to the next line, if needed. This tries to wrap at word borders.
        /// </summary>
        Intelligent = 0x6,

        /// <summary>
        /// If this flag is also set, the display scrolls up when the last line is full.
        /// </summary>
        ScrollUp = 0x10,
    }
}
