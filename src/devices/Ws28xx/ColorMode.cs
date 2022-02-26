// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Ws28xx
{
    /// <summary>
    /// Defines the order of the values on the line.
    /// </summary>
    public enum ColorMode
    {
        /// <summary>
        /// Green first, then red, then blue.
        /// </summary>
        GRB,

        /// <summary>
        /// Red first, then green, then blue.
        /// </summary>
        RGB,
    }
}
