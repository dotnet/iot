// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Device.I2c;
using System.Device.Spi;

namespace Iot.Device.Graphics.SkiaSharpAdapter
{
    /// <summary>
    /// Image factory registry helper.
    /// </summary>
    public static class SkiaSharpAdapter
    {
        /// <summary>
        /// Registers this factory as the default image factory.
        /// Call this method once at startup to use SkiaSharp as image backend.
        /// </summary>
        public static void Register()
        {
            BitmapImage.RegisterImageFactory(new SkiaSharpImageFactory());
        }
    }
}
