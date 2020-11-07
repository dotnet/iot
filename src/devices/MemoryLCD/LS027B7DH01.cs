// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Device.Gpio;
using System.Device.Spi;

namespace Iot.Device.MemoryLcd
{
    /// <summary>
    /// Memory LCD model LS027B7DH01
    /// </summary>
    public class LS027B7DH01 : LSxxxB7DHxx
    {
        /// <summary>
        /// Create a <see cref="LS027B7DH01"/> device
        /// </summary>
        /// <param name="spi">SPI controller</param>
        /// <param name="gpio">GPIO controller</param>
        /// <param name="shouldDispose">True to dispose the Gpio Controller</param>
        /// <param name="scs">Chip select signal</param>
        /// <param name="disp">Display ON/OFF signal</param>
        /// <param name="extcomin">External COM inversion signal input</param>
        /// <param name="pinNumberingScheme">The numbering scheme used to represent pins provided by GPIO controller</param>
        public LS027B7DH01(SpiDevice spi, GpioController gpio = null, bool shouldDispose = true, int scs = -1, int disp = -1, int extcomin = -1, PinNumberingScheme pinNumberingScheme = PinNumberingScheme.Logical)
            : base(spi, gpio, shouldDispose, scs, disp, extcomin, pinNumberingScheme)
        {
        }

        /// <inheritdoc/>
        public override int PixelWidth { get; } = 400;

        /// <inheritdoc/>
        public override int PixelHeight { get; } = 240;
    }
}
