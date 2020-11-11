// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace Iot.Device.Media
{
    /// <summary>
    /// The communications channel to a video device.
    /// </summary>
    public abstract partial class VideoDevice
    {
        /// <summary>
        /// Convert YUV(YUV444) to RGB format.
        /// </summary>
        /// <param name="stream">YUV stream.</param>
        /// <returns>RGB format colors.</returns>
        public static Color[] YuvToRgb(Stream stream)
        {
            int y, u, v;

            List<Color> colors = new List<Color>();
            while (stream.Position != stream.Length)
            {
                y = stream.ReadByte();
                u = stream.ReadByte();
                v = stream.ReadByte();

                colors.Add(YuvToRgb(y, u, v));
            }

            return colors.ToArray();
        }

        /// <summary>
        /// Convert YUYV(YUV422) to RGB format.
        /// </summary>
        /// <param name="stream">YUYV stream.</param>
        /// <returns>RGB format colors.</returns>
        public static Color[] YuyvToRgb(Stream stream)
        {
            int y0, u, y1, v;

            List<Color> colors = new List<Color>();
            while (stream.Position != stream.Length)
            {
                y0 = stream.ReadByte();
                u = stream.ReadByte();
                y1 = stream.ReadByte();
                v = stream.ReadByte();

                colors.Add(YuvToRgb(y0, u, v));
                colors.Add(YuvToRgb(y1, u, v));
            }

            return colors.ToArray();
        }

        /// <summary>
        /// Convert YV12(YUV420) to RGB format.
        /// </summary>
        /// <param name="stream">YV12 stream.</param>
        /// <param name="size">Image size in the stream.</param>
        /// <returns>RGB format colors.</returns>
        public static Color[] Yv12ToRgb(Stream stream, (uint Width, uint Height) size)
        {
            int y0, u, v;
            int width = (int)size.Width, height = (int)size.Height;
            int total = width * height;
            int shift, vShift = total / 4;

            byte[] yuv = new byte[stream.Length];
            stream.Read(yuv, 0, yuv.Length);

            List<Color> colors = new List<Color>();
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    shift = (y / 2) * (width / 2) + (x / 2);

                    y0 = yuv[y * width + x];
                    u = yuv[total + shift];
                    v = yuv[total + shift + vShift];

                    colors.Add(YuvToRgb(y0, u, v));
                }
            }

            return colors.ToArray();
        }

        /// <summary>
        /// Convert NV12(YUV420) to RGB format.
        /// </summary>
        /// <param name="stream">NV12 stream.</param>
        /// <param name="size">Image size in the stream.</param>
        /// <returns>RGB format colors.</returns>
        public static Color[] Nv12ToRgb(Stream stream, (uint Width, uint Height) size)
        {
            int y0, u, v;
            int width = (int)size.Width, height = (int)size.Height;
            int total = width * height;
            int shift;

            byte[] yuv = new byte[stream.Length];
            stream.Read(yuv, 0, yuv.Length);

            List<Color> colors = new List<Color>();
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    shift = y / 2 * width + x - x % 2;

                    y0 = yuv[y * width + x];
                    u = yuv[total + shift];
                    v = yuv[total + shift + 1];

                    colors.Add(YuvToRgb(y0, u, v));
                }
            }

            return colors.ToArray();
        }

        /// <summary>
        /// Convert RGB format to bitmap
        /// </summary>
        /// <param name="size">Image size in the RGB data.</param>
        /// <param name="colors">RGB data.</param>
        /// <param name="format">Bitmap pixel format</param>
        /// <returns>Bitmap</returns>
        public static Bitmap RgbToBitmap((uint Width, uint Height) size, Color[] colors, System.Drawing.Imaging.PixelFormat format = System.Drawing.Imaging.PixelFormat.Format24bppRgb)
        {
            int width = (int)size.Width, height = (int)size.Height;

            Bitmap pic = new Bitmap(width, height, format);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    pic.SetPixel(x, y, colors[y * width + x]);
                }
            }

            return pic;
        }

        /// <summary>
        /// Convert single YUV pixel to RGB color.
        /// </summary>
        /// <param name="y">Y</param>
        /// <param name="u">U</param>
        /// <param name="v">V</param>
        /// <returns>RGB color.</returns>
        private static Color YuvToRgb(int y, int u, int v)
        {
            byte r = (byte)(y + 1.4075 * (v - 128));
            byte g = (byte)(y - 0.3455 * (u - 128) - (0.7169 * (v - 128)));
            byte b = (byte)(y + 1.7790 * (u - 128));

            return Color.FromArgb(r, g, b);
        }
    }
}
