// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Iot.Device.Graphics;

namespace Iot.Device.Gui
{
    public partial class ScreenCapture
    {
        [SuppressMessage("Interoperability", "CA1416", Justification = "Only used on windows, see call site")]
        private static unsafe BitmapImage GetScreenContentsWindows(Rectangle area)
        {
            try
            {
                using (Bitmap bitmap = new Bitmap(area.Width, area.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
                {
                    using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bitmap))
                    {
                        g.CopyFromScreen(new System.Drawing.Point(area.Left, area.Top), System.Drawing.Point.Empty, new System.Drawing.Size(area.Width, area.Height));
                    }

                    // For some reason, we need to swap R and B here. Strange...
                    var bmd = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
                    int totalPixels = bmd.Width * bmd.Height;
                    Span<uint> pixels = new Span<uint>(bmd.Scan0.ToPointer(), totalPixels);
                    for (int pix = 0; pix < totalPixels; pix++)
                    {
                        uint origColor = pixels[pix];
                        // Swap byte 3 with 1
                        uint r = (origColor << 16) & 0x00FF0000;
                        uint b = (origColor >> 16) & 0x000000FF;
                        uint ag = (origColor & 0xFF00FF00); // A and G stay where they are
                        pixels[pix] = r | ag | b;
                    }

                    bitmap.UnlockBits(bmd);

                    var image = Converters.ToBitmapImage(bitmap);
                    return image;
                }
            }
            catch (Win32Exception x)
            {
                throw new NotSupportedException($"Unable to take a screenshot: {x.Message}", x);
            }
        }

        private Rectangle ScreenSizeWindows()
        {
            return new Rectangle(Interop.GetSystemMetrics(Interop.SystemMetric.SM_XVIRTUALSCREEN),
                Interop.GetSystemMetrics(Interop.SystemMetric.SM_YVIRTUALSCREEN),
                Interop.GetSystemMetrics(Interop.SystemMetric.SM_CXVIRTUALSCREEN),
                Interop.GetSystemMetrics(Interop.SystemMetric.SM_CYVIRTUALSCREEN));
        }
    }
}
