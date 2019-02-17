// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;
using System.Device.Gpio;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.CompilerServices; 

namespace Iot.Device.SenseHat
{
    public class SenseHatLedMatrix : IDisposable
    {
        public const int NumberOfPixels = 64;
        public const int NumberOfPixelsPerRow = 8;

        // does not need to be public since it should not be used
        private const int NumberOfPixelsPerColumn = 8;

        private const string SenseHatDeviceName = "RPi-Sense FB";

        // Pixel length in binary representation
        private const int PixelLength = 2; // RGB565
        private FileStream _deviceFile;

        public SenseHatLedMatrix()
        {
            string device = GetSenseHatDevice();

            if (device == null)
            {
                throw new InvalidOperationException("Sense HAT not found. Ensure device is enabled in config.txt.");
            }

            _deviceFile = new FileStream(device, FileMode.Open, FileAccess.ReadWrite);
            Clear(Color.Black);
        }

        public void Write(ReadOnlySpan<Color> colors)
        {
            if (colors.Length != NumberOfPixels)
                throw new ArgumentException($"`{nameof(colors)}` must have exactly {NumberOfPixels} elements.");

            StartWritingColors();

            for (int i = 0; i < NumberOfPixels; i++)
            {
                WriteColor(colors[i]);
            }

            EndWriting();
        }

        public void Clear(Color color)
        {
            StartWritingColors();

            for (int i = 0; i < NumberOfPixels; i++)
            {
                WriteColor(color);
            }

            EndWriting();
        }

        public void SetPixel(int x, int y, Color color)
        {
            StartWritingColor(x, y);
            WriteColor(color);
            EndWriting();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (int x, int y) IndexToPosition(int index)
        {
            return (index % NumberOfPixelsPerRow, index / NumberOfPixelsPerRow);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int PositionToIndex(int x, int y)
        {
            Debug.Assert(x >= 0 && x < NumberOfPixelsPerRow);
            Debug.Assert(y >= 0 && y < NumberOfPixelsPerColumn);
            return x + y * NumberOfPixelsPerRow;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void StartWritingColors()
        {
            _deviceFile.Seek(0, SeekOrigin.Begin);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void StartWritingColor(int x, int y)
        {
            _deviceFile.Seek(PositionToIndex(x, y) * PixelLength, SeekOrigin.Begin);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EndWriting()
        {
            _deviceFile.Flush();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WriteColor(Color color)
        {
            // Writes color in RGB565 format
            byte r = (byte)(color.R >> 3);
            byte g = (byte)(color.G >> 2);
            byte b = (byte)(color.B >> 3);
            ushort col = (ushort)((r << 11) | (g << 5) | b);
            Span<byte> encoded = stackalloc byte[2] { (byte)(col & 0xff), (byte)(col >> 8) };

            _deviceFile.Write(encoded);
        }

        private static string GetSenseHatDevice()
        {
            foreach (string dev in Directory.EnumerateFileSystemEntries("/sys/class/graphics/", "fb*"))
            {
                string devName = Path.Combine(dev, "name");
                if (File.Exists(devName) && File.ReadAllText(devName).Trim() == SenseHatDeviceName)
                {
                    return Path.Combine("/dev/", Path.GetFileName(dev));
                }
            }

            return null;
        }

        public void Dispose()
        {
            _deviceFile?.Dispose();
            _deviceFile = null;
        }
    }
}
