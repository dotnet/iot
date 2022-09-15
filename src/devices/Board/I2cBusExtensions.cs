// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.I2c;
using System.IO;
using System.Text;

namespace Iot.Device.Board
{
    /// <summary>
    /// Contains extension methods that operate on the I2c bus
    /// </summary>
    public static class I2cBusExtensions
    {
        /// <summary>
        /// Performs a scan on the I2C bus, returning the addresses for all connected devices.
        /// </summary>
        /// <param name="bus">The bus to scan</param>
        /// <param name="lowest">The lowest address to scan. Default 0x03</param>
        /// <param name="highest">The highest address to scan. Default 0x77</param>
        /// <returns>A list of bus addresses that are in use, an empty list if no device was found</returns>
        /// <remarks>This method should never throw an exception. Bus scanning may interfere with normal device operation,
        /// so this should not be done while devices are being used.</remarks>
        public static List<int> PerformBusScan(this I2cBus bus, int lowest = 0x3, int highest = 0x77)
        {
            List<int> ret = new List<int>();
            for (int addr = lowest; addr <= highest; addr++)
            {
                try
                {
                    using (I2cDevice device = bus.CreateDevice(addr))
                    {
                        device.ReadByte(); // Success means that this does not throw an exception
                        ret.Add(addr);
                    }
                }
                catch (Exception x) when (x is IOException || x is UnauthorizedAccessException)
                {
                    // Ignore and continue
                }
            }

            return ret;
        }

        /// <summary>
        /// Performs a scan on the I2C bus, returning the addresses for all connected devices.
        /// </summary>
        /// <param name="bus">The bus to scan</param>
        /// <param name="progress">Progress feedback provider. Receives scan progress in percent</param>
        /// <param name="lowestAddress">The lowest address to scan. Default 0x03</param>
        /// <param name="highestAddress">The highest address to scan. Default 0x77</param>
        /// <returns>A list of bus addresses that are in use, an empty list if no device was found</returns>
        /// <remarks>This method should never throw an exception. Bus scanning may interfere with normal device operation,
        /// so this should not be done while devices are being used.</remarks>
        public static (List<int> FoundDevices, int LowestAddress, int HighestAddress) PerformBusScan(this I2cBus bus, IProgress<float>? progress, int lowestAddress = 0x3, int highestAddress = 0x77)
        {
            if (lowestAddress < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(lowestAddress));
            }

            if (highestAddress < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(highestAddress));
            }

            List<int> addresses = new();
            int numberOfDevicesToScan = highestAddress - lowestAddress;
            float currentPercentage = 0;
            float stepPerDevice = 100.0f / numberOfDevicesToScan;

            if (numberOfDevicesToScan <= 0)
            {
                return (addresses, 0, 0);
            }

            for (int addr = lowestAddress; addr <= highestAddress; addr++)
            {
                try
                {
                    using (I2cDevice device = bus.CreateDevice(addr))
                    {
                        device.ReadByte(); // Success means that this does not throw an exception
                        addresses.Add(addr);
                    }
                }
                catch (Exception x) when (x is IOException || x is UnauthorizedAccessException || x is TimeoutException)
                {
                    // Ignore and continue
                }

                currentPercentage += stepPerDevice;
                if (progress != null)
                {
                    progress.Report(currentPercentage);
                }
            }

            if (progress != null)
            {
                progress.Report(100.0f);
            }

            return (addresses, lowestAddress, highestAddress);
        }

        /// <summary>
        /// Converts the output of <see cref="PerformBusScan(I2cBus, IProgress{float}, int, int)"/> into a user-readable table, corresponding to the output of the standard
        /// linux command i2cdetect.
        /// </summary>
        /// <param name="data">The result of a bus scan</param>
        /// <returns>A table in form of a multiline string for the first 127 possible devices. An empty field represents an address that was not tested, a -- means no device
        /// found and a number means there was a device at that address</returns>
        public static string ToUserReadableTable(this (List<int> FoundDevices, int LowestAddress, int HighestAddress) data)
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append("     0  1  2  3  4  5  6  7  8  9  a  b  c  d  e  f");
            stringBuilder.Append(Environment.NewLine);

            for (int startingRowAddress = 0; startingRowAddress < 128; startingRowAddress += 16)
            {
                stringBuilder.Append($"{startingRowAddress:x2}: ");  // Beginning of row.

                for (int rowAddress = 0; rowAddress < 16; rowAddress++)
                {
                    int deviceAddress = startingRowAddress + rowAddress;

                    // Skip the unwanted addresses.
                    if (deviceAddress < data.LowestAddress || deviceAddress > data.HighestAddress)
                    {
                        stringBuilder.Append("   ");
                        continue;
                    }

                    if (data.FoundDevices.Contains(deviceAddress))
                    {
                        stringBuilder.Append($"{deviceAddress:x2} ");
                    }
                    else
                    {
                        stringBuilder.Append("-- ");
                    }

                }

                stringBuilder.Append(Environment.NewLine);
            }

            return stringBuilder.ToString();
        }
    }
}
