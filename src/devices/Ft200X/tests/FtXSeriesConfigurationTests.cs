// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Linq;
using System.Runtime.InteropServices;
using Iot.Device.FtCommon;
using Xunit;

namespace Iot.Device.Ft200X.Tests
{
    public class FtXSeriesConfigurationTests
    {
        [Fact]
        public void ClonePreservesAllFieldsAndCopiesEntireDescriptorBuffers()
        {
            byte[] bytes = Enumerable.Range(1, 56).Select(i => (byte)i).ToArray();
            var original = new FtXSeriesConfiguration { Data = MemoryMarshal.Read<FtXSeriesEeprom>(bytes) };
            Array.Fill(original.Manufacturer, (byte)0x80);
            Array.Fill(original.ManufacturerId, (byte)0x81);
            Array.Fill(original.Description, (byte)0x82);
            Array.Fill(original.SerialNumber, (byte)0x83);
            original.Manufacturer[1] = 0;
            original.ManufacturerId[1] = 0;
            original.Description[1] = 0;
            original.SerialNumber[1] = 0;

            FtXSeriesConfiguration clone = original.Clone();

            Assert.NotSame(original, clone);
            Assert.Equal(original.Data, clone.Data);
            Assert.True(original.HasSameContents(clone));
            Assert.NotSame(original.Manufacturer, clone.Manufacturer);
            Assert.NotSame(original.ManufacturerId, clone.ManufacturerId);
            Assert.NotSame(original.Description, clone.Description);
            Assert.NotSame(original.SerialNumber, clone.SerialNumber);
            Assert.Equal(original.Manufacturer, clone.Manufacturer);
            Assert.Equal(original.ManufacturerId, clone.ManufacturerId);
            Assert.Equal(original.Description, clone.Description);
            Assert.Equal(original.SerialNumber, clone.SerialNumber);

            FtXSeriesEeprom data = clone.Data;
            data.Cbus0++;
            clone.Data = data;
            clone.Manufacturer[0]++;
            clone.ManufacturerId[0]++;
            clone.Description[0]++;
            clone.SerialNumber[0]++;

            Assert.Equal(MemoryMarshal.Read<FtXSeriesEeprom>(bytes), original.Data);
            Assert.Equal((byte)0x80, original.Manufacturer[0]);
            Assert.Equal((byte)0x81, original.ManufacturerId[0]);
            Assert.Equal((byte)0x82, original.Description[0]);
            Assert.Equal((byte)0x83, original.SerialNumber[0]);
        }

        [Fact]
        public void ComparisonIgnoresDescriptorBytesAfterNullTerminators()
        {
            var original = new FtXSeriesConfiguration();
            FtXSeriesConfiguration clone = original.Clone();
            clone.Manufacturer[1] = 1;
            clone.ManufacturerId[1] = 2;
            clone.Description[1] = 3;
            clone.SerialNumber[1] = 4;

            original.Validate();
            clone.Validate();
            Assert.True(original.HasSameContents(clone));
        }
    }
}
