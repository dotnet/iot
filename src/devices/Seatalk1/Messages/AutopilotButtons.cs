// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Seatalk1.Messages
{
    /// <summary>
    /// The buttons of the auto-pilot controller (more or less standardized between manufacturers).
    /// Note that for many functions, multiple buttons must be pressed simultaneously, therefore this is a flag enum.
    /// </summary>
    [Flags]
    public enum AutopilotButtons
    {
        /// <summary>
        /// No button was pressed
        /// </summary>
        None = 0,

        /// <summary>
        /// The -1 button was pressed
        /// </summary>
        MinusOne = 1,

        /// <summary>
        /// The -10 button was pressed
        /// </summary>
        MinusTen = 2,

        /// <summary>
        /// The +1 button was pressed
        /// </summary>
        PlusOne = 4,

        /// <summary>
        /// The +10 button was pressed
        /// </summary>
        PlusTen = 8,

        /// <summary>
        /// The Auto button was pressed
        /// </summary>
        Auto = 16,

        /// <summary>
        /// The Standby button was pressed
        /// </summary>
        StandBy = 32,

        /// <summary>
        /// The Disp button was pressed (not always present)
        /// </summary>
        Disp = 128,

        /// <summary>
        /// The button(s) where pressed longer
        /// </summary>
        LongPress = 256,
    }
}
