// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Device.Gpio.Drivers;
using System.IO;
using System.Text.RegularExpressions;

namespace System.Device.Gpio
{
    public sealed partial class GpioController
    {
        private const string CpuInfoPath = "/proc/cpuinfo";
        private const string RaspberryPiHardware = "BCM2835";
        private const string HummingBoardHardware = @"Freescale i.MX6 Quad/DualLite (Device Tree)";

        /// <summary>
        /// Initializes a new instance of the <see cref="GpioController"/> class that will use the specified numbering scheme.
        /// The controller will default to use the driver that best applies given the platform the program is executing on.
        /// </summary>
        /// <param name="numberingScheme">The numbering scheme used to represent pins provided by the controller.</param>
        public GpioController(PinNumberingScheme numberingScheme)
            : this(numberingScheme, GetBestDriverForBoard())
        {
        }

        /// <summary>
        /// Attempt to get the best applicable driver for the board the program is executing on.
        /// </summary>
        /// <returns>A driver that works with the board the program is executing on.</returns>
        public static GpioDriver GetBestDriverForBoard()
        {
            string[] cpuInfoLines = File.ReadAllLines(CpuInfoPath);
            Regex regexHw = new Regex(@"Hardware\s*:\s*(.*)");
            Regex regexRev = new Regex(@"Revision\s*:\s*(.*)");
            for (int i = 0; i < cpuInfoLines.Length; i++)
            {
                string cpuInfoLine = cpuInfoLines[i];
                Match match = regexHw.Match(cpuInfoLine);
                if (match.Success)
                {
                    if (match.Groups.Count > 1)
                    {
                        if (match.Groups[1].Value == RaspberryPiHardware)
                        {
                            if (i + 1 < cpuInfoLines.Length)
                            {
                                string revisionLine = cpuInfoLines[i + 1];
                                match = regexRev.Match(revisionLine);
                                if (match.Success && match.Groups.Count > 1)
                                {
                                    string rev = match.Groups[1].Value;
                                    if (rev.EndsWith("03111"))
                                    {
                                        // The Pi4 reports the same CPU (although that's technically wrong) but different board revisions.
                                        // The currently known revisions are 0xa03111, 0xb03111, 0xc03111 for the models with 1GB, 2GB and 4GB of ram, respectively. 
                                        return new RaspberryPi4Driver();
                                    }
                                }
                            }

                            // Assume Pi3 otherwise
                            return new RaspberryPi3Driver();
                        }
                        // Commenting out as HummingBoard driver is not implemented yet, will be added back after implementation 
                        // https://github.com/dotnet/iot/issues/76                
                        //if (match.Groups[1].Value == HummingBoardHardware)
                        //{
                        //    return new HummingBoardDriver();
                        //} 
                        return UnixDriver.Create();
                    }
                }
            }
            return UnixDriver.Create();
        }
    }
}
