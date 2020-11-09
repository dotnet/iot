// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.Gpio;
using System.Device.Spi;

namespace Iot.Device.MemoryLcd
{
    /// <summary>
    /// Memory LCD model LS013B7DH03
    /// </summary>
    public class LS013B7DH03 : LSxxxB7DHxx
    {
        /// <summary>
        /// Create a <see cref="LS013B7DH03"/> device
        /// </summary>
        /// <param name="spi">SPI controller</param>
        /// <param name="gpio">GPIO controller</param>
        /// <param name="shouldDispose">True to dispose the Gpio Controller</param>
        /// <param name="scs">Chip select signal</param>
        /// <param name="disp">Display ON/OFF signal</param>
        /// <param name="extcomin">External COM inversion signal input</param>
        public LS013B7DH03(SpiDevice spi, GpioController? gpio = null, bool shouldDispose = true, int scs = -1, int disp = -1, int extcomin = -1)
            : base(spi, gpio, shouldDispose, scs, disp, extcomin)
        {
        }

        /// <inheritdoc/>
        public override int PixelWidth { get; } = 128;

        /// <inheritdoc/>
        public override int PixelHeight { get; } = 128;
    }
}
