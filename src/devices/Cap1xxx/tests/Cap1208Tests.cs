// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;
using Xunit;

namespace Iot.Device.Cap1xxx.Tests
{
    public sealed class Cap1208Tests : IDisposable
    {
        private readonly SimulatedCap1208 _simulatedCap1208;
        private readonly Cap1208 _cap1208;

        public Cap1208Tests()
        {
            _simulatedCap1208 = new SimulatedCap1208();
            _cap1208 = new Cap1208(_simulatedCap1208);
        }

        public void Dispose()
        {
            _cap1208.Dispose();
        }

        [Fact]
        public void Constructor_DoesNotChangeTheConfiguration()
        {
            Assert.Empty(_simulatedCap1208.Writes);
        }

        [Theory]
        [InlineData(0x50, 0x5D)] // CAP1188
        [InlineData(0x6B, 0x00)]
        public void Constructor_ThrowsIfTheDeviceIsNotACap1208(byte productId, byte manufacturerId)
        {
            using var otherDevice = new SimulatedCap1208(productId, manufacturerId);
            Assert.Throws<IOException>(() => new Cap1208(otherDevice));
        }

        [Fact]
        public void Constructor_ThrowsIfTheDeviceIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new Cap1208(null!));
        }

        [Fact]
        public void ReadTouchedInputs_ReturnsNoneWithoutTouches()
        {
            Assert.Equal(SensorInputs.None, _cap1208.ReadTouchedInputs());
            Assert.Empty(_simulatedCap1208.Writes);
        }

        [Fact]
        public void ReadTouchedInputs_ReturnsTheTouchedInputs()
        {
            _simulatedCap1208.Touch(SensorInputs.Input5 | SensorInputs.Input2);

            Assert.Equal(SensorInputs.Input5 | SensorInputs.Input2, _cap1208.ReadTouchedInputs());
        }

        [Fact]
        public void ReadTouchedInputs_ReturnsATouchReleasedBeforeTheRead()
        {
            _simulatedCap1208.Touch(SensorInputs.Input1);
            _simulatedCap1208.Release(SensorInputs.Input1);

            Assert.Equal(SensorInputs.Input1, _cap1208.ReadTouchedInputs());
            Assert.Equal(SensorInputs.None, _cap1208.ReadTouchedInputs());
        }

        [Fact]
        public void ReadTouchedInputs_KeepsTheInputsThatAreStillTouched()
        {
            _simulatedCap1208.Touch(SensorInputs.Input3 | SensorInputs.Input8);
            _simulatedCap1208.Release(SensorInputs.Input8);

            Assert.Equal(SensorInputs.Input3 | SensorInputs.Input8, _cap1208.ReadTouchedInputs());
            Assert.Equal(SensorInputs.Input3, _cap1208.ReadTouchedInputs());

            _simulatedCap1208.Release(SensorInputs.Input3);
            Assert.Equal(SensorInputs.Input3, _cap1208.ReadTouchedInputs());
        }

        [Fact]
        public void ReadTouchedInputs_ClearsOnlyTheInterruptBit()
        {
            // Standby (STBY, bit 5) and an interrupt pending
            _simulatedCap1208.RegisterMap[0x00].WriteRegister(0x21);
            _simulatedCap1208.Writes.Clear();

            _cap1208.ReadTouchedInputs();

            Assert.Equal(new[] { ((byte)0x00, (byte)0x20) }, _simulatedCap1208.Writes);
            Assert.Equal(0x20, _simulatedCap1208.RegisterMap[0x00].ReadRegister());
        }

        [Fact]
        public void MultipleTouchBlocking_IsOnByDefault()
        {
            Assert.True(_cap1208.MultipleTouchBlocking);
        }

        [Fact]
        public void MultipleTouchBlocking_ChangesOnlyItsBit()
        {
            // MULT_BLK_EN and B_MULT_T = 01b (2 touches)
            _simulatedCap1208.RegisterMap[0x2A].WriteRegister(0x84);

            _cap1208.MultipleTouchBlocking = false;
            Assert.Equal(0x04, _simulatedCap1208.RegisterMap[0x2A].ReadRegister());
            Assert.False(_cap1208.MultipleTouchBlocking);

            _cap1208.MultipleTouchBlocking = true;
            Assert.Equal(0x84, _simulatedCap1208.RegisterMap[0x2A].ReadRegister());
            Assert.True(_cap1208.MultipleTouchBlocking);
        }

        [Fact]
        public void Sensitivity_Is32xByDefault()
        {
            Assert.Equal(Sensitivity.Multiplier32, _cap1208.Sensitivity);
        }

        [Theory]
        [InlineData(Sensitivity.Multiplier128, 0x0F)]
        [InlineData(Sensitivity.Multiplier2, 0x6F)]
        [InlineData(Sensitivity.Multiplier1, 0x7F)]
        public void Sensitivity_ChangesOnlyTheDeltaSenseBits(Sensitivity sensitivity, byte expectedRegister)
        {
            // The power-on value 0x2F has BASE_SHIFT = 1111b, which must not change
            _cap1208.Sensitivity = sensitivity;

            Assert.Equal(expectedRegister, _simulatedCap1208.RegisterMap[0x1F].ReadRegister());
            Assert.Equal(sensitivity, _cap1208.Sensitivity);
        }

        [Fact]
        public void Sensitivity_ThrowsForAnInvalidValue()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _cap1208.Sensitivity = (Sensitivity)8);
        }

        [Fact]
        public void Recalibrate_CalibratesAllTheInputsByDefault()
        {
            _cap1208.Recalibrate();

            Assert.Equal(new[] { ((byte)0x26, (byte)0xFF) }, _simulatedCap1208.Writes);
        }

        [Fact]
        public void Recalibrate_CalibratesTheGivenInputs()
        {
            _cap1208.Recalibrate(SensorInputs.Input1 | SensorInputs.Input8);

            Assert.Equal(new[] { ((byte)0x26, (byte)0x81) }, _simulatedCap1208.Writes);
        }

        [Fact]
        public void Dispose_DisposesTheI2cDevice()
        {
            _cap1208.Dispose();

            Assert.Throws<ObjectDisposedException>(() => _simulatedCap1208.ReadByte());
        }
    }
}
