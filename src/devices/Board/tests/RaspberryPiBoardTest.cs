// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Device.I2c;
using System.Device.Spi;
using System.Text;
using Xunit;

namespace Iot.Device.Board.Tests
{
    /// <summary>
    /// Tests for Raspberry specific board features.
    /// Note: These tests do not need actual Raspberry hardware.
    /// </summary>
    public class RaspberryPiBoardTest
    {
        [Fact]
        public void GetDefaultSpiPins()
        {
            RaspberryPiBoard b = new RaspberryPiBoard();
            var pins = b.GetDefaultPinAssignmentForSpi(new SpiConnectionSettings(0, -1));
            Assert.Equal(9, pins[0]);
            Assert.Equal(10, pins[1]);
            Assert.Equal(11, pins[2]);
        }

        [Fact]
        public void GetDefaultI2cPinsLogicalNumbering()
        {
            RaspberryPiBoard b = new RaspberryPiBoard();
            var pins = b.GetDefaultPinAssignmentForI2c(0);
            Assert.Equal(0, pins[0]);
            Assert.Equal(1, pins[1]);
            pins = b.GetDefaultPinAssignmentForI2c(1);
            Assert.Equal(2, pins[0]);
            Assert.Equal(3, pins[1]);
        }
    }
}
