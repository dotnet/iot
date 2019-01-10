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
        /// Controller that takes in a numbering scheme. Will default to use the driver that best applies given the platform the program is running on.
        /// </summary>
        /// <param name="pinNumberingScheme">The numbering scheme used to represent pins on the board.</param>
        public GpioController(PinNumberingScheme pinNumberingScheme)
            : this(pinNumberingScheme, GetBestDriverForBoard())
        {
        }

        /// <summary>
        /// Private method that tries to get the best applicable driver for the board you are running in.
        /// </summary>
        /// <returns>A driver which works on the current running board.</returns>
        private static GpioDriver GetBestDriverForBoard()
        {
            string[] cpuInfoLines = File.ReadAllLines(CpuInfoPath);
            Regex regex = new Regex(@"Hardware\s*:\s*(.*)");
            foreach (string cpuInfoLine in cpuInfoLines)
            {
                Match match = regex.Match(cpuInfoLine);
                if (match.Success)
                {
                    if (match.Groups.Count > 1)
                    {
                        if (match.Groups[1].Value == RaspberryPiHardware)
                        {
                            Console.WriteLine("raspberry driver");
                            return new RaspberryPi3Driver();
                        }
                        // Commenting out as HummingBoard driver is not implemented yet, will be added back after implementation 
                        // https://github.com/dotnet/iot/issues/76                
                        //if (match.Groups[1].Value == HummingBoardHardware)
                        //{
                        //    return new HummingBoardDriver();
                        //} 
                        return UnixDriver.InitUnixDriver();
                    }
                }
            }
            return UnixDriver.InitUnixDriver();
        }
    }
}
