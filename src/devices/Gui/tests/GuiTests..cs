// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.InteropServices;
using Iot.Device.Graphics;
using Iot.Device.Graphics.SkiaSharpAdapter;
using Xunit;
using Xunit.Abstractions;

namespace Iot.Device.Gui.Tests
{
    public class ScreenCaptureTests
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public ScreenCaptureTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void TestCapture()
        {
            var os = Environment.OSVersion;
            if (os.Platform == PlatformID.Unix)
            {
                try
                {
                    DoRunTest();
                }
                catch (DllNotFoundException x)
                {
                    _testOutputHelper.WriteLine($"Cannot run test, because libX11.so is missing: {x}");
                }
                catch (NotSupportedException y)
                {
                    // This exception is thrown e.g. when no display is available
                    _testOutputHelper.WriteLine($"Cannot run test: {y}");
                }
            }
            else if (os.Platform == PlatformID.Win32NT)
            {
                DoRunTest();
            }
            else if (os.Platform == PlatformID.MacOSX)
            {
                DoRunTest();
            }
        }

        private static void DoRunTest()
        {
            SkiaSharpAdapter.Register();
            var os = Environment.OSVersion;
            var screenCapture = new ScreenCapture();
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
