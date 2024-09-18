// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Iot.Device.Graphics;
using static Iot.Device.Gui.InteropGui;

namespace Iot.Device.Gui
{
    public partial class ScreenCapture
    {
        private IntPtr _display;

        private void InitLinux()
        {
            _display = XOpenDisplay();
            if (_display == IntPtr.Zero)
            {
                throw new NotSupportedException("Unable to open display");
            }
        }

        private BitmapImage GetScreenContentsLinux(Rectangle area)
        {
            var root = XDefaultRootWindow(_display);

            IntPtr rawImage = XGetImage(_display, root, area.Left, area.Top, (UInt32)area.Width, (UInt32)area.Height, AllPlanes, ZPixmap);

            XImage? image = Marshal.PtrToStructure<XImage>(rawImage);

            if (image == null)
            {
                throw new NotSupportedException("Unable to get screen image pointer");
            }

            var resultImage = BitmapImage.CreateBitmap(area.Width, area.Height, PixelFormat.Format32bppXrgb);
            Span<int> targetImage = MemoryMarshal.Cast<byte, int>(resultImage.AsByteSpan());

            nuint red_mask = image.red_mask;
            nuint green_mask = image.green_mask;
            nuint blue_mask = image.blue_mask;

            for (int x = 0; x < area.Width; x++)
            {
                for (int y = 0; y < area.Height; y++)
                {
                    UInt32 pixel = XGetPixel(rawImage, x, y);

                    // Swap R and B (similar to windows, but since the display of directly loaded images is right, I still think the error is here)
                    nuint red = pixel & blue_mask;
                    nuint green = (pixel & green_mask) >> 8;
                    nuint blue = (pixel & red_mask) >> 16;

                    var color = Color.FromArgb(255, (int)red, (int)green, (int)blue);
                    targetImage[area.Width * y + x] = color.ToArgb();
                }
            }

            XDestroyImage(rawImage);
            return resultImage!;
        }

        private Rectangle ScreenSizeLinux()
        {
            var root = XDefaultRootWindow(_display);
            InteropGui.XWindowAttributes gwa = default;

            XGetWindowAttributes(_display, root, ref gwa);
            int width = gwa.width;
            int height = gwa.height;
            return new Rectangle(0, 0, width, height);
        }

    }
}
