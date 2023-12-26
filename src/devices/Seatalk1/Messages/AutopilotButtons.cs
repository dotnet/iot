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
        None = 0,
        MinusOne = 1,
        MinusTen = 2,
        PlusOne = 4,
        PlusTen = 8,
        Auto = 16,
        StandBy = 32,
        Track = 64,
        Disp = 128,
        LongPress = 256,
    }
}
