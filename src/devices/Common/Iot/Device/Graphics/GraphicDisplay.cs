// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Graphics
{
    /// <summary>
    /// An interface for representing graphic displays
    /// </summary>
    public abstract class GraphicDisplay : MarshalByRefObject, IDisposable
    {
        /// <summary>
        /// The width of the display, in pixels
        /// </summary>
        public abstract int ScreenWidth { get; }

        /// <summary>
        /// The height of the display, in pixels
        /// </summary>
        public abstract int ScreenHeight { get; }

        /// <summary>
        /// The native display format of the display (the number of colors it can actually display)
        /// </summary>
        public abstract PixelFormat NativePixelFormat { get; }

        /// <summary>
        /// Checks whether the <see cref="DrawBitmap(BitmapImage)"/> method can convert the given format into the native format.
        /// </summary>
        /// <param name="format">The format to convert</param>
        /// <returns>True if yes, false if no</returns>
        public abstract bool CanConvertFromPixelFormat(PixelFormat format);

        /// <summary>
        /// Returns an image that can act as back buffer (has a supported image format and the right size).
        /// Updating this bitmap does not change the screen. Use <see cref="DrawBitmap(Iot.Device.Graphics.BitmapImage)"/> to show the bitmap/>
        /// </summary>
        /// <returns></returns>
        public abstract BitmapImage GetBackBufferCompatibleImage();

        /// <summary>
        /// Send a bitmap to the Ili9341 display.
        /// </summary>
        /// <param name="bm">The bitmap to be sent to the display controller.</param>
        public void DrawBitmap(BitmapImage bm)
        {
            int width = (int)ScreenWidth;
            if (width > bm.Width)
            {
                width = bm.Width;
            }

            int height = (int)ScreenHeight;
            if (height > bm.Height)
            {
                height = bm.Height;
            }

            DrawBitmap(bm, new Point(0, 0), new Rectangle(0, 0, width, height), true);
        }

        /// <summary>
        /// Send a bitmap to the Ili9341 display specifying the starting position and destination clipping rectangle.
        /// </summary>
        /// <param name="bm">The bitmap to be sent to the display controller note that only Pixel Format Format32bppArgb is supported.</param>
        /// <param name="updateRect">A rectangle that defines where in the display the bitmap is written. Note that no scaling is done.</param>
        public void DrawBitmap(BitmapImage bm, Rectangle updateRect)
        {
            DrawBitmap(bm, new Point(updateRect.X, updateRect.Y), updateRect, true);
        }

        /// <summary>
        /// Copies the given bitmap to the back buffer and optionally updates the screen directly
        /// </summary>
        /// <param name="bm">The bitmap to draw</param>
        /// <param name="sourcePoint">A coordinate point in the source bitmap where copying starts from.</param>
        /// <param name="destinationRect">A rectangle that defines where in the display the bitmap is written. No scaling is done.</param>
        /// <param name="update">True to immediately send the updated backbuffer to the screen</param>
        public abstract void DrawBitmap(BitmapImage bm, Point sourcePoint, Rectangle destinationRect, bool update);

        /// <summary>
        /// Updates the display with the current screen buffer.
        /// <param name="forceFull">Forces a full update, otherwise only changed screen contents are updated (if that feature is supported)</param>
        /// </summary>
        public abstract void SendFrame(bool forceFull);

        /// <summary>
        /// Updates the display with the current screen buffer.
        /// </summary>
        public void SendFrame()
        {
            SendFrame(false);
        }

        /// <summary>
        /// Standard dispose pattern
        /// </summary>
        /// <param name="disposing">True if disposing, false if in finalizer</param>
        protected virtual void Dispose(bool disposing)
        {
            // Nothing to do here
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
