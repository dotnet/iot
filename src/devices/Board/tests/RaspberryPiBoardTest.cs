// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Device.I2c;
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
        /// <summary>
        /// This shouldn't map anything in either direction
        /// </summary>
        [Fact]
        public void PinMappingIsReversibleLogical()
        {
            RaspberryPiBoard b = new RaspberryPiBoard(PinNumberingScheme.Logical);
            for (int i = 0; i < b.PinCount; i++)
            {
                int mapped = b.ConvertPinNumber(i, PinNumberingScheme.Logical, PinNumberingScheme.Logical);
                int reverse = b.ConvertPinNumber(i, PinNumberingScheme.Logical, PinNumberingScheme.Logical);
                Assert.Equal(reverse, mapped);
                Assert.Equal(mapped, i);
            }
        }

        /// <summary>
        /// The mapping is not 1:1, but must be reversible
        /// </summary>
        [Fact]
        public void PinMappingIsReversibleBoard()
        {
            RaspberryPiBoard b = new RaspberryPiBoard(PinNumberingScheme.Board);
            Assert.NotEqual(3, b.ConvertPinNumber(3, PinNumberingScheme.Board, PinNumberingScheme.Logical));
            for (int i = 0; i < b.PinCount; i++)
            {
                int mapped = b.ConvertPinNumber(i, PinNumberingScheme.Logical, PinNumberingScheme.Board);
                int reverse = b.ConvertPinNumber(mapped, PinNumberingScheme.Board, PinNumberingScheme.Logical);
                Assert.Equal(i, reverse);
            }
        }

        [Fact]
        public void GetDefaultI2cPinsBoardNumbering()
        {
            RaspberryPiBoard b = new RaspberryPiBoard(PinNumberingScheme.Board);
            var pins = b.GetDefaultPinAssignmentForI2c(new I2cConnectionSettings(0, 0));
            Assert.Equal(27, pins[0]);
            Assert.Equal(28, pins[1]);
            pins = b.GetDefaultPinAssignmentForI2c(new I2cConnectionSettings(1, 0));
            Assert.Equal(3, pins[0]);
            Assert.Equal(5, pins[1]);
        }

        [Fact]
        public void GetDefaultI2cPinsLogicalNumbering()
        {
            RaspberryPiBoard b = new RaspberryPiBoard(PinNumberingScheme.Logical);
            var pins = b.GetDefaultPinAssignmentForI2c(new I2cConnectionSettings(0, 0));
            Assert.Equal(0, pins[0]);
            Assert.Equal(1, pins[1]);
            pins = b.GetDefaultPinAssignmentForI2c(new I2cConnectionSettings(1, 0));
            Assert.Equal(2, pins[0]);
            Assert.Equal(3, pins[1]);
        }
    }
}
