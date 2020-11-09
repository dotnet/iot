// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.Gpio;
using System.Device.Spi;

namespace Iot.Device.MemoryLcd
{
    /// <summary>
    /// Memory LCD model LS013B7DH05
    /// </summary>
    public class LS013B7DH05 : LSxxxB7DHxx
    {
        /// <summary>
        /// Create a <see cref="LS013B7DH05"/> device
        /// </summary>
        /// <param name="spi">SPI controller<br/><b>ChipSelectLineActiveState</b> must be <b>HIGH</b> or use <paramref name="gpio"/> and <paramref name="chipSelect"/>. See Datasheet 6-3</param>
        /// <param name="gpio">GPIO controller</param>
        /// <param name="shouldDispose">True to dispose the Gpio Controller</param>
        /// <param name="chipSelect">Chip select signal<br/>-1 when using SPI chipSelect line</param>
        /// <param name="display">Display ON/OFF signal</param>
        /// <param name="externalCom">External COM inversion signal input</param>
        public LS013B7DH05(SpiDevice spi, GpioController? gpio = null, bool shouldDispose = true, int chipSelect = -1, int display = -1, int externalCom = -1)
            : base(spi, gpio, shouldDispose, chipSelect, display, externalCom)
        {
        }

        /// <inheritdoc/>
        public override int PixelWidth { get; } = 144;

        /// <inheritdoc/>
        public override int PixelHeight { get; } = 168;
    }
}
