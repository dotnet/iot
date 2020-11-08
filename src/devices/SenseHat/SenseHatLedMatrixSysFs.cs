// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;
using System.Device.Gpio;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.CompilerServices;

namespace Iot.Device.SenseHat
{
    /// <summary>
    /// SenseHAT - LED matrix (using Linux driver)
    /// </summary>
    public class SenseHatLedMatrixSysFs : SenseHatLedMatrix
    {
        private const string SenseHatDeviceName = "RPi-Sense FB";

        // Pixel length in binary representation
        private const int PixelLength = 2; // RGB565

        private FileStream _deviceFile;

        /// <summary>
        /// Constructs SenseHatLedMatrixSysFs instance
        /// </summary>
        public SenseHatLedMatrixSysFs()
        {
            string? device = GetSenseHatDevice();

            if (device is null)
            {
                throw new InvalidOperationException("Sense HAT not found. Ensure device is enabled in config.txt.");
            }

            _deviceFile = new FileStream(device, FileMode.Open, FileAccess.ReadWrite);
            Fill(Color.Black);
        }

        /// <inheritdoc/>
        public override void Write(ReadOnlySpan<Color> colors)
        {
            if (colors.Length != NumberOfPixels)
            {
                throw new ArgumentException($"`{nameof(colors)}` must have exactly {NumberOfPixels} elements.");
            }

            StartWritingColors();

            for (int i = 0; i < NumberOfPixels; i++)
            {
                WriteColor(colors[i]);
            }

            EndWriting();
        }

        /// <inheritdoc/>
        public override void Fill(Color color = default(Color))
        {
            StartWritingColors();

            for (int i = 0; i < NumberOfPixels; i++)
            {
                WriteColor(color);
            }

            EndWriting();
        }

        /// <inheritdoc/>
        public override void SetPixel(int x, int y, Color color)
        {
            if (x < 0 || x >= NumberOfPixelsPerRow)
            {
                throw new ArgumentOutOfRangeException(nameof(x));
            }

            if (y < 0 || y >= NumberOfPixelsPerColumn)
            {
                throw new ArgumentOutOfRangeException(nameof(y));
            }

            StartWritingColor(x, y);
            WriteColor(color);
            EndWriting();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void StartWritingColors() =>
            _deviceFile.Seek(0, SeekOrigin.Begin);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void StartWritingColor(int x, int y) =>
            _deviceFile.Seek(PositionToIndex(x, y) * PixelLength, SeekOrigin.Begin);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EndWriting() =>
            _deviceFile.Flush();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WriteColor(Color color)
        {
            // Writes color in RGB565 format
            byte r = (byte)(color.R >> 3);
            byte g = (byte)(color.G >> 2);
            byte b = (byte)(color.B >> 3);
            ushort col = (ushort)((r << 11) | (g << 5) | b);
            Span<byte> encoded = stackalloc byte[2]
            {
                (byte)(col & 0xff),
                (byte)(col >> 8)
            };

            _deviceFile.Write(encoded);
        }

        private static string? GetSenseHatDevice()
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

        /// <inheritdoc/>
        public override void Dispose()
        {
            _deviceFile?.Dispose();
            _deviceFile = null!;
        }
    }
}
