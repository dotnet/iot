// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Graphics
{
    /// <summary>
    /// Specifies the pixel format of an image.
    /// </summary>
    public enum PixelFormat
    {
        /// <summary>
        /// The format is unspecified.
        /// </summary>
        Unspecified,

        /// <summary>
        /// The standard 32 bit image format
        /// </summary>
        Format32bppArgb,

        /// <summary>
        /// 32 bit image format with ignored alpha
        /// </summary>
        Format32bppXrgb,

        /// <summary>
        /// 16 bit RGB image format (5 bits for red, 6 bits for green and 5 bits for blue)
        /// </summary>
        Format16bppRgb565,

        /// <summary>
        /// 1 bit black and white image
        /// </summary>
        Format1bppBw,
    }
}
