// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Gui
{
    /// <summary>
    /// Mouse buttons
    /// </summary>
    [Flags]
    public enum MouseButton
    {
        /// <summary>
        /// No button pressed
        /// </summary>
        None = 0,

        /// <summary>
        /// The left button is pressed
        /// </summary>
        Left = 1,

        /// <summary>
        /// The right mouse button is pressed
        /// </summary>
        Right = 2,

        /// <summary>
        /// The middle mouse button is pressed
        /// </summary>
        Middle = 4
    }
}
