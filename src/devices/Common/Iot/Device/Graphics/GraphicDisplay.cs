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
        /// Clears the screen to black
        /// </summary>
        public virtual void ClearScreen()
        {
            var bmp = GetBackBufferCompatibleImage();
            bmp.Clear(Color.Black);
            DrawBitmap(bmp);
            bmp.Dispose();
        }

        /// <summary>
        /// Send a bitmap to the display buffer.
        /// </summary>
        /// <param name="bm">The bitmap to be sent to the display controller.</param>
        public abstract void DrawBitmap(BitmapImage bm);

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
