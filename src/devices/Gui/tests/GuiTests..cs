// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.InteropServices;
using Iot.Device.Graphics;
using Iot.Device.Graphics.SkiaSharpAdapter;
using Xunit;

namespace Iot.Device.Gui.Tests
{
    public class ScreenCaptureTests
    {
        [Fact]
        public void TestCapture()
        {
            SkiaSharpAdapter.Register();
            var screenCapture = new ScreenCapture();
            var os = Environment.OSVersion;
            if (os.Platform == PlatformID.Win32NT || os.Platform == PlatformID.Unix)
            {
                var size = screenCapture.ScreenSize();
                Assert.True(size.Width > 0);
                Assert.True(size.Height > 0);
                BitmapImage img = screenCapture.GetScreenContents();
                Assert.NotNull(img);
                Assert.Equal(size.Width, img.Width);
                Assert.Equal(size.Height, img.Height);
            }
            else
            {
                Assert.Throws<PlatformNotSupportedException>(() => screenCapture.GetScreenContents());
                Assert.Throws<PlatformNotSupportedException>(() => screenCapture.ScreenSize());
            }
        }
    }
}
