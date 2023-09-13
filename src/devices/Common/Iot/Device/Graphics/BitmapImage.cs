// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;

namespace Iot.Device.Graphics
{
    /// <summary>
    /// An abstract class for a bitmap image, to be implemented by an image provider.
    /// This class is also the factory to create new bitmaps with the current image factory.
    /// The image factory must implement <see cref="IImageFactory"/> and must be registered using a call to
    /// <see cref="RegisterImageFactory"/>.
    /// </summary>
    /// <remarks>
    /// This class does not have an implementation within Iot.Device.Bindings.dll. You can use the Iot.Device.Bindings.SkiaSharpAdapter.dll
    /// as a supported operating-system independent implementation of this class or write your own implementation.
    /// </remarks>
    public abstract class BitmapImage : IDisposable
    {
        /// <summary>
        /// The currently registered image factory
        /// </summary>
        private static IImageFactory? s_currentFactory;

        /// <summary>
        /// Initializes a <see cref="T:Iot.Device.Graphics.BitmapImage" /> instance with the specified data, width, height and stride.
        /// </summary>
        /// <param name="width">Width of the image</param>
        /// <param name="height">Height of the image</param>
        /// <param name="stride">Number of bytes per row</param>
        /// <param name="pixelFormat">The pixel format of the data</param>
        protected BitmapImage(int width, int height, int stride, PixelFormat pixelFormat)
        {
            Width = width;
            Height = height;
            Stride = stride;
            PixelFormat = pixelFormat;
        }

        /// <summary>
        /// Width of the image
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// Height of the image
        /// </summary>
        public int Height { get; }

        /// <summary>
        /// Number of bytes per row
        /// </summary>
        public int Stride { get; }

        /// <summary>
        /// The format of the image
        /// </summary>
        public PixelFormat PixelFormat { get; }

        /// <summary>
        /// Accesses the pixel at the given position
        /// </summary>
        /// <param name="x">Pixel X position</param>
        /// <param name="y">Pixel Y position</param>
        public Color this[int x, int y]
        {
            get
            {
                return GetPixel(x, y);
            }
            set
            {
                SetPixel(x, y, value);
            }
        }

        /// <summary>
        /// Register an image factory.
        /// </summary>
        /// <param name="factory">The image factory to register</param>
        public static void RegisterImageFactory(IImageFactory factory)
        {
            s_currentFactory = factory;
        }

        /// <summary>
        /// Creates a bitmap using the active factory. This requires an implementation to be registered.
        /// See <see cref="BitmapImage"/> for details.
        /// </summary>
        /// <param name="width">Width of the image, in pixels</param>
        /// <param name="height">Height of the image, in pixels</param>
        /// <param name="pixelFormat">Desired pixel format</param>
        /// <returns>A new bitmap with the provided size</returns>
        public static BitmapImage CreateBitmap(int width, int height, PixelFormat pixelFormat)
        {
            VerifyFactoryAvailable();
            return s_currentFactory!.CreateBitmap(width, height, pixelFormat);
        }

        private static void VerifyFactoryAvailable()
        {
            if (s_currentFactory == null)
            {
                throw new InvalidOperationException("No image factory registered. Call BitmapImage.RegisterImageFactory() with a suitable implementation first. Consult the documentation for further information");
            }
        }

        /// <summary>
        /// Create a bitmap from a file. This requires an implementation to be registered.
        /// See <see cref="BitmapImage"/> for details.
        /// </summary>
        /// <param name="filename">The file to load</param>
        /// <returns>A bitmap</returns>
        public static BitmapImage CreateFromFile(string filename)
        {
            VerifyFactoryAvailable();
            using var s = new FileStream(filename, FileMode.Open);
            return s_currentFactory!.CreateFromStream(s);
        }

        /// <summary>
        /// Create a bitmap from an open stream. This requires an implementation to be registered.
        /// See <see cref="BitmapImage"/> for details.
        /// </summary>
        /// <param name="data">The data stream</param>
        /// <returns>A bitmap</returns>
        public static BitmapImage CreateFromStream(Stream data)
        {
            VerifyFactoryAvailable();
            return s_currentFactory!.CreateFromStream(data);
        }

        /// <summary>
        /// Clears the image to specific color
        /// </summary>
        /// <param name="color">Color to clear the image. Defaults to black.</param>
        public virtual void Clear(Color color = default)
        {
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    SetPixel(x, y, color);
                }
            }
        }

        /// <summary>
        /// Sets pixel at specific position
        /// </summary>
        /// <param name="x">X coordinate of the pixel</param>
        /// <param name="y">Y coordinate of the pixel</param>
        /// <param name="color">Color to set the pixel to</param>
        /// <remarks>The use of <see cref="SetPixel"/> and <see cref="GetPixel"/> is usually slow. For fast image updates, grab the underlying
        /// raw buffer by calling <see cref="AsByteSpan"/> or use the <see cref="GetDrawingApi"/> method and use high-level drawing functions.</remarks>
        public abstract void SetPixel(int x, int y, Color color);

        /// <summary>
        /// Gets the color of the pixel at the given position
        /// </summary>
        /// <param name="x">X coordinate of the pixel</param>
        /// <param name="y">Y coordinate of the pixel</param>
        /// <returns>The color of the pixel</returns>
        /// <remarks>The use of <see cref="SetPixel"/> and <see cref="GetPixel"/> is usually slow. For fast image updates, grab the underlying
        /// raw buffer by calling <see cref="AsByteSpan"/> or use the <see cref="GetDrawingApi"/> method and use high-level drawing functions.</remarks>
        public abstract Color GetPixel(int x, int y);

        /// <summary>
        /// Return the data pointer as a raw span of bytes
        /// </summary>
        /// <returns>A span of bytes</returns>
        public abstract Span<byte> AsByteSpan();

        /// <summary>
        /// Saves this bitmap to a file
        /// </summary>
        /// <param name="filename">The filename to save it to</param>
        /// <param name="fileType">The filetype to use.</param>
        /// <remarks>
        /// Generally, the method is not checking that the filename extension matches the file type provided.
        /// </remarks>
        public void SaveToFile(string filename, ImageFileType fileType)
        {
            using (var fs = new FileStream(filename, FileMode.CreateNew))
            {
                SaveToStream(fs, fileType);
            }
        }

        /// <summary>
        /// Save the image to a stream
        /// </summary>
        /// <param name="stream">The stream to save the data to</param>
        /// <param name="format">The image format</param>
        public abstract void SaveToStream(Stream stream, ImageFileType format);

        /// <summary>
        /// Disposes this instance
        /// </summary>
        /// <param name="disposing">True if disposing, false if called from finalizer</param>
        protected abstract void Dispose(bool disposing);

        /// <summary>
        /// Disposes this instance. Correctly disposing instance of this class is important to prevent memory leaks or overload of the garbage collector.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Returns an abstraction interface for drawing to this bitmap.
        /// </summary>
        public abstract IGraphics GetDrawingApi();
    }
}
