// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Device.I2c;
using Xunit;

namespace Iot.Device.Pca95x4.Tests
{
    public class Pca95x4Test
    {
        [Fact]
        public void PinCountIsEight()
        {
            using Pca95x4Chip chip = new Pca95x4Chip();
            using Pca95x4 device = new Pca95x4(chip);
            using GpioController controller = new GpioController(device);
            Assert.Equal(8, controller.PinCount);
        }

        [Theory]
        [InlineData(PinMode.Input)]
        [InlineData(PinMode.Output)]
        public void SupportedPinModes(PinMode mode)
        {
            using Pca95x4Chip chip = new Pca95x4Chip();
            using Pca95x4 device = new Pca95x4(chip);
            using GpioController controller = new GpioController(device);
            Assert.True(controller.IsPinModeSupported(0, mode));
        }

        [Theory]
        [InlineData(PinMode.InputPullUp)]
        [InlineData(PinMode.InputPullDown)]
        public void UnsupportedPinModes(PinMode mode)
        {
            using Pca95x4Chip chip = new Pca95x4Chip();
            using Pca95x4 device = new Pca95x4(chip);
            using GpioController controller = new GpioController(device);
            Assert.False(controller.IsPinModeSupported(0, mode));
        }

        [Fact]
        public void SetPinModeUpdatesConfigurationRegister()
        {
            using Pca95x4Chip chip = new Pca95x4Chip();
            using Pca95x4 device = new Pca95x4(chip);
            using GpioController controller = new GpioController(device);

            controller.OpenPin(2, PinMode.Output);
            // A cleared configuration bit designates an output.
            Assert.Equal(0, chip.Registers[(int)Register.Configuration] & (1 << 2));
            Assert.Equal(PinMode.Output, controller.GetPinMode(2));

            controller.SetPinMode(2, PinMode.Input);
            // A set configuration bit designates an input.
            Assert.NotEqual(0, chip.Registers[(int)Register.Configuration] & (1 << 2));
            Assert.Equal(PinMode.Input, controller.GetPinMode(2));
        }

        [Fact]
        public void WriteUpdatesOutputRegister()
        {
            using Pca95x4Chip chip = new Pca95x4Chip();
            using Pca95x4 device = new Pca95x4(chip);
            using GpioController controller = new GpioController(device);

            controller.OpenPin(5, PinMode.Output);
            controller.Write(5, PinValue.High);
            Assert.NotEqual(0, chip.Registers[(int)Register.OutputPort] & (1 << 5));

            controller.Write(5, PinValue.Low);
            Assert.Equal(0, chip.Registers[(int)Register.OutputPort] & (1 << 5));
        }

        [Fact]
        public void ReadReflectsInputRegister()
        {
            using Pca95x4Chip chip = new Pca95x4Chip();
            using Pca95x4 device = new Pca95x4(chip);
            using GpioController controller = new GpioController(device);

            controller.OpenPin(3, PinMode.Input);

            chip.Registers[(int)Register.InputPort] = 1 << 3;
            Assert.Equal(PinValue.High, controller.Read(3));

            chip.Registers[(int)Register.InputPort] = 0x00;
            Assert.Equal(PinValue.Low, controller.Read(3));
        }

        [Fact]
        public void TogglingFlipsOutput()
        {
            using Pca95x4Chip chip = new Pca95x4Chip();
            using Pca95x4 device = new Pca95x4(chip);
            using GpioController controller = new GpioController(device);

            controller.OpenPin(1, PinMode.Output);
            controller.Write(1, PinValue.Low);
            controller.Toggle(1);
            Assert.NotEqual(0, chip.Registers[(int)Register.OutputPort] & (1 << 1));
            controller.Toggle(1);
            Assert.Equal(0, chip.Registers[(int)Register.OutputPort] & (1 << 1));
        }

        [Fact]
        public void RegisterApiStillWorks()
        {
            using Pca95x4Chip chip = new Pca95x4Chip();
            using Pca95x4 device = new Pca95x4(chip);

            device.Write(Register.OutputPort, 0xAA);
            Assert.Equal(0xAA, device.Read(Register.OutputPort));
            Assert.True(device.ReadBit(Register.OutputPort, 1));
            Assert.False(device.ReadBit(Register.OutputPort, 0));
        }

        /// <summary>
        /// Simple in-memory mock that mimics the four PCA95x4 registers.
        /// </summary>
        private sealed class Pca95x4Chip : I2cDevice
        {
            private readonly byte[] _registers = new byte[4];
            private int _address;

            public Pca95x4Chip()
            {
                // At reset the I/Os are configured as inputs.
                _registers[(int)Register.Configuration] = 0xFF;
            }

            public byte[] Registers => _registers;

            public override I2cConnectionSettings ConnectionSettings => new I2cConnectionSettings(1, 0x38);

            public override void WriteByte(byte value) => _address = value;

            public override byte ReadByte() => _registers[_address];

            public override void Write(ReadOnlySpan<byte> buffer)
            {
                _address = buffer[0];
                if (buffer.Length > 1)
                {
                    _registers[_address] = buffer[1];

                    // On real hardware the Input Port reflects the actual pin levels, including
                    // those driven by the Output Port. Mirror that behaviour for output pins.
                    if (_address == (int)Register.OutputPort)
                    {
                        _registers[(int)Register.InputPort] = buffer[1];
                    }
                }
            }

            public override void Read(Span<byte> buffer)
            {
                for (int i = 0; i < buffer.Length; i++)
                {
                    buffer[i] = _registers[_address];
                }
            }

            public override void WriteRead(ReadOnlySpan<byte> writeBuffer, Span<byte> readBuffer) =>
                throw new NotImplementedException();
        }
    }
}
