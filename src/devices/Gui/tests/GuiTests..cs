// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.InteropServices;
using Xunit;

namespace Iot.Device.Gui.Tests
{
    public class ScreenCaptureTests
    {
        [Fact]
        public void TestWindows()
        {
            var screenCapture = new ScreenCapture();
            if (OperatingSystem.IsWindows() || OperatingSystem.IsLinux())
            {
                var size = screenCapture.ScreenSize();
                Assert.True(size.Width > 0);
                Assert.True(size.Height > 0);
                var img = screenCapture.GetScreenContents();
                Assert.NotNull(img);
                Assert.Equal(size.Width, img!.Width);
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
