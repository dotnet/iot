// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Spi;
using System.Drawing;

namespace Iot.Device.Apa102
{
    /// <summary>
    /// Driver for APA102. A double line transmission integrated control LED
    /// </summary>
    public class Apa102 : LedStrip
    {
        private SpiDevice _spiDevice;
        private byte[] _buffer;

        /// <summary>
        /// Initializes a new instance of the APA102 device.
        /// </summary>
        /// <param name="spiDevice">The SPI device used for communication.</param>
        /// <param name="length">Number of LEDs</param>
        public Apa102(SpiDevice spiDevice, int length):base(length)
        {
            _spiDevice = spiDevice ?? throw new ArgumentNullException(nameof(spiDevice));
            _buffer = new byte[(length + 2) * 4];

            _buffer.AsSpan(0, 4).Fill(0x00); // start frame
            _buffer.AsSpan((length + 1) * 4, 4).Fill(0xFF); // end frame
        }

        /// <summary>
        /// Update color data to LEDs
        /// </summary>
        public override void Flush()
        {
            for (int i = 0; i < Pixels.Length; i++)
            {
                Span<byte> pixel = _buffer.AsSpan((i + 1) * 4);
                pixel[0] = (byte)((Pixels[i].A >> 3) | 0b11100000); // global brightness (alpha)
                pixel[1] = FixGamma(Pixels[i].B); // blue
                pixel[2] = FixGamma(Pixels[i].G); // green
                pixel[3] = FixGamma(Pixels[i].R); // red
            }

            _spiDevice.Write(_buffer);
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            _spiDevice?.Dispose();
            _spiDevice = null!;
            _buffer = null!;

            base.Dispose();
        }
    }
}
