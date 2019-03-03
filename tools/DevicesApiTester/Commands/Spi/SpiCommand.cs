// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Device.Spi;
using CommandLine;
using DeviceApiTester.Infrastructure;

namespace DeviceApiTester.Commands.Spi
{
    public abstract class SpiCommand : DebuggableCommand
    {
        [Option('b', "bus-id", HelpText = "The bus id the SPI device to connect to", Required = true)]
        public int BusId { get; set; }

        [Option('c', "chip-select-line", HelpText = "The chip select line for the connection to the SPI device", Required = true)]
        public int ChipSelectLine { get; set; }

        [Option('m', "mode", HelpText = "The clock polarity & phase mode to use: { Mode0 | Mode1 | Mode2 | Mode3 }", Required = false, Default = SpiMode.Mode0)]
        public SpiMode Mode { get; set; }

        [Option('l', "data-bit-length", HelpText = "The bit length for data on this connection", Required = false, Default = 8)]
        public int DataBitLength { get; set; }

        [Option('f', "clock-frequency", HelpText = "the clock frequency in Hz for the connection", Required = false, Default = 500_000)]
        public int ClockFrequency { get; set; }

        [Option('d', "device", HelpText = "The SpiDevice to use: { Windows | UnixSysFs }", Required = true)]
        public SpiDriverType Device { get; set; }

        protected SpiDevice CreateSpiDevice(SpiConnectionSettings connectionSettings)
        {
            return DriverFactory.CreateFromEnum<SpiDevice, SpiDriverType>(this.Device, connectionSettings);
        }
    }
}
