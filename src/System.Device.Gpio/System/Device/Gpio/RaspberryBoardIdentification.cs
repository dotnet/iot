using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace System.Device.Gpio
{
    /// <summary>
    /// Identification of Raspberry Pi board models
    /// </summary>
    internal class RaspberryBoardIdentification
    {
        /// <summary>
        /// The Raspberry Pi model.
        /// </summary>
        public enum Model
        {
            /// <summary>
            /// Unknown model.
            /// </summary>
            Unknown,

            /// <summary>
            /// Raspberry Model A.
            /// </summary>
            RaspberryPiA,

            /// <summary>
            /// Model A+.
            /// </summary>
            RaspberryPiAPlus,

            /// <summary>
            /// Model B rev1.
            /// </summary>
            RaspberryPiBRev1,

            /// <summary>
            /// Model B rev2.
            /// </summary>
            RaspberryPiBRev2,

            /// <summary>
            /// Model B+.
            /// </summary>
            RaspberryPiBPlus,

            /// <summary>
            /// Compute module.
            /// </summary>
            RaspberryPiComputeModule,

            /// <summary>
            /// Pi 2 Model B.
            /// </summary>
            RaspberryPi2B,

            /// <summary>
            /// Pi Zero.
            /// </summary>
            RaspberryPiZero,

            /// <summary>
            /// Pi Zero W.
            /// </summary>
            RaspberryPiZeroW,

            /// <summary>
            /// Pi 3 Model B.
            /// </summary>
            RaspberryPi3B,

            /// <summary>
            /// Pi 3 Model B+.
            /// </summary>
            RaspberryPi3BPlus,

            /// <summary>
            /// Compute module 3.
            /// </summary>
            RaspberryPiComputeModule3,

            /// <summary>
            /// Pi 4 all versions.
            /// </summary>
            RaspberryPi4,
        }

        /// <summary>
        /// The board processor.
        /// </summary>
        public enum Processor
        {
            /// <summary>
            /// Processor is unknown.
            /// </summary>
            Unknown,

            /// <summary>
            /// Processor is a BCM2708.
            /// </summary>
            Bcm2708,

            /// <summary>
            /// Processor is a BCM2709.
            /// </summary>
            Bcm2709,

            /// <summary>
            /// Processor is a BCM2710.
            /// </summary>
            Bcm2710,

            /// <summary>
            /// Processor is a11.
            /// </summary>
            Bcm2711,
        }

        #region Fields

        private readonly Dictionary<string, string> _settings;

        private RaspberryBoardIdentification(Dictionary<string, string> settings)
        {
            _settings = settings;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Get board model from firmware revision
        /// See http://www.raspberrypi-spy.co.uk/2012/09/checking-your-raspberry-pi-board-version/ for information.
        /// </summary>
        /// <returns></returns>
        internal Model GetBoardModel()
        {
            var firmware = Firmware;
            switch (firmware & 0xFFFF)
            {
                case 0x2:
                case 0x3:
                    return Model.RaspberryPiBRev1;

                case 0x4:
                case 0x5:
                case 0x6:
                case 0xd:
                case 0xe:
                case 0xf:
                    return Model.RaspberryPiBRev2;

                case 0x7:
                case 0x8:
                case 0x9:
                    return Model.RaspberryPiA;

                case 0x10:
                    return Model.RaspberryPiBPlus;

                case 0x11:
                    return Model.RaspberryPiComputeModule;

                case 0x12:
                    return Model.RaspberryPiAPlus;

                case 0x1040:
                case 0x1041:
                    return Model.RaspberryPi2B;

                case 0x0092:
                case 0x0093:
                    return Model.RaspberryPiZero;

                case 0x00C1:
                    return Model.RaspberryPiZeroW;

                case 0x2082:
                    return Model.RaspberryPi3B;

                case 0x20D3:
                    return Model.RaspberryPi3BPlus;

                case 0x20A0:
                    return Model.RaspberryPiComputeModule3;

                case 0x3111:
                    return Model.RaspberryPi4;

                default:
                    return Model.Unknown;
            }
        }

        /// <summary>
        /// Gets the processor name.
        /// </summary>
        /// <value>
        /// The name of the processor.
        /// </value>
        public string ProcessorName
        {
            get
            {
                _settings.TryGetValue("Hardware", out string hardware);
                return hardware;
            }
        }

        /// <summary>
        /// Gets the board firmware version.
        /// </summary>
        public int Firmware
        {
            get
            {
                if (_settings.TryGetValue("Revision", out string revision)
                    && !string.IsNullOrEmpty(revision)
                    && int.TryParse(revision, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int firmware))
                {
                    return firmware;
                }

                return 0;
            }
        }

        /// <summary>
        /// Gets the serial number.
        /// </summary>
        public string SerialNumber
        {
            get
            {
                if (_settings.TryGetValue("Serial", out string serial)
                    && !string.IsNullOrEmpty(serial))
                {
                    return serial;
                }

                return string.Empty;
            }
        }

        /// <summary>
        /// Gets a value indicating whether board is overclocked.
        /// </summary>
        /// <value>
        ///   <c>true</c> if board is overclocked; otherwise, <c>false</c>.
        /// </value>
        public bool IsOverclocked
        {
            get
            {
                var firmware = Firmware;
                return (firmware & 0xFFFF0000) != 0;
            }
        }

        #endregion

        #region Private Helpers

        private Processor LoadProcessor(Model model)
        {
            switch (model)
            {
                case Model.RaspberryPiA:
                case Model.RaspberryPiAPlus:
                case Model.RaspberryPiBRev1:
                case Model.RaspberryPiBRev2:
                case Model.RaspberryPiBPlus:
                case Model.RaspberryPiComputeModule:
                case Model.RaspberryPiZero:
                case Model.RaspberryPiZeroW:
                    return Processor.Bcm2708;
                case Model.RaspberryPi2B:
                // TBC: B3(+) should be a BCM2710 processor ...
                case Model.RaspberryPi3B:
                case Model.RaspberryPi3BPlus:
                case Model.RaspberryPiComputeModule3:
                    return Processor.Bcm2709;
                case Model.RaspberryPi4:
                    return Processor.Bcm2711;
                default:
                    return Processor.Unknown;
            }
        }

        /// <summary>
        /// Detect the board CPU information from /proc/cpuinfo
        /// </summary>
        /// <returns>
        /// The <see cref="RaspberryBoardIdentification"/>.
        /// </returns>
        internal static RaspberryBoardIdentification LoadBoard()
        {
            try
            {
                const string filePath = "/proc/cpuinfo";

                var cpuInfo = File.ReadAllLines(filePath);
                var settings = new Dictionary<string, string>();
                var suffix = string.Empty;

                foreach (var line in cpuInfo)
                {
                    var separator = line.IndexOf(':');

                    if (!string.IsNullOrWhiteSpace(line) && separator > 0)
                    {
                        var key = line.Substring(0, separator).Trim();
                        var val = line.Substring(separator + 1).Trim();
                        if (string.Equals(key, "processor", StringComparison.InvariantCultureIgnoreCase))
                        {
                            suffix = "." + val;
                        }

                        settings.Add(key + suffix, val);
                    }
                    else
                    {
                        suffix = string.Empty;
                    }
                }

                return new RaspberryBoardIdentification(settings);
            }
            catch
            {
                return new RaspberryBoardIdentification(new Dictionary<string, string>());
            }
        }
        #endregion
    }
}
