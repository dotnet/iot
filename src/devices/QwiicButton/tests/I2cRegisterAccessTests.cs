// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using QwiicButton.Tests;
using Xunit;

namespace Iot.Device.QwiicButton.Tests
{
    public class I2cRegisterAccessTests
    {
        #region WriteRegister Tests

        [Theory]
        [InlineData(55, 12, true, new byte[] { 55, 12 })]
        [InlineData(55, 12, false, new byte[] { 55, 12 })]
        public void WriteRegister_GivenAddressAndSByteData_WhenUsingEndianness_ThenWritesExpectedArray(
            byte registerAddress,
            sbyte data,
            bool useLittleEndian,
            byte[] expected)
        {
            RunWriteTest(registerAddress, data, useLittleEndian, expected);
        }

        [Theory]
        [InlineData(55, 12, true, new byte[] { 55, 12 })]
        [InlineData(55, 12, false, new byte[] { 55, 12 })]
        public void WriteRegister_GivenAddressAndByteData_WhenUsingEndianness_ThenWritesExpectedArray(
            byte registerAddress,
            byte data,
            bool useLittleEndian,
            byte[] expected)
        {
            RunWriteTest(registerAddress, data, useLittleEndian, expected);
        }

        [Theory]
        [InlineData(55, 6528, true, new byte[] { 55, 128, 25 })]
        [InlineData(55, 6528, false, new byte[] { 55, 25, 128 })]
        public void WriteRegister_GivenAddressAndShortData_WhenUsingEndianness_ThenWritesExpectedArray(
            byte registerAddress,
            short data,
            bool useLittleEndian,
            byte[] expected)
        {
            RunWriteTest(registerAddress, data, useLittleEndian, expected);
        }

        [Theory]
        [InlineData(55, 65280, true, new byte[] { 55, 0, 255 })]
        [InlineData(55, 65280, false, new byte[] { 55, 255, 0 })]
        public void WriteRegister_GivenAddressAndUShortData_WhenUsingEndianness_ThenWritesExpectedArray(
            byte registerAddress,
            ushort data,
            bool useLittleEndian,
            byte[] expected)
        {
            RunWriteTest(registerAddress, data, useLittleEndian, expected);
        }

        [Theory]
        [InlineData(55, -214718364, true, new byte[] { 55, 100, 168, 51, 243 })]
        [InlineData(55, -214718364, false, new byte[] { 55, 243, 51, 168, 100 })]
        public void WriteRegister_GivenAddressAndIntData_WhenUsingEndianness_ThenWritesExpectedArray(
            byte registerAddress,
            int data,
            bool useLittleEndian,
            byte[] expected)
        {
            RunWriteTest(registerAddress, data, useLittleEndian, expected);
        }

        [Theory]
        [InlineData(55, 4278255360, true, new byte[] { 55, 0, 255, 0, 255 })]
        [InlineData(55, 4278255360, false, new byte[] { 55, 255, 0, 255, 0 })]
        public void WriteRegister_GivenAddressAndUIntData_WhenUsingEndianness_ThenWritesExpectedArray(
            byte registerAddress,
            uint data,
            bool useLittleEndian,
            byte[] expected)
        {
            RunWriteTest(registerAddress, data, useLittleEndian, expected);
        }

        [Theory]
        [InlineData(55, 280379743338240, true, new byte[] { 55, 0, 255, 0, 255, 0, 255, 0, 0 })]
        [InlineData(55, 280379743338240, false, new byte[] { 55, 0, 0, 255, 0, 255, 0, 255, 0 })]
        public void WriteRegister_GivenAddressAndLongData_WhenUsingEndianness_ThenWritesExpectedArray(
            byte registerAddress,
            long data,
            bool useLittleEndian,
            byte[] expected)
        {
            RunWriteTest(registerAddress, data, useLittleEndian, expected);
        }

        [Theory]
        [InlineData(55, 18374966859414961920, true, new byte[] { 55, 0, 255, 0, 255, 0, 255, 0, 255 })]
        [InlineData(55, 18374966859414961920, false, new byte[] { 55, 255, 0, 255, 0, 255, 0, 255, 0 })]
        public void WriteRegister_GivenAddressAndULongData_WhenUsingEndianness_ThenWritesExpectedArray(
            byte registerAddress,
            ulong data,
            bool useLittleEndian,
            byte[] expected)
        {
            RunWriteTest(registerAddress, data, useLittleEndian, expected);
        }

        [Fact]
        public void WriteRegister_GivenAddress_WhenUsingUnsupportedDataType_ThenThrowsException()
        {
            // Arrange
            var deviceMock = new I2cDeviceMock();
            var sut = new I2cRegisterAccess(deviceMock);

            // Act + Assert
            Assert.Throws<InvalidOperationException>(() => sut.WriteRegister(55, char.MaxValue, true));
        }

        #endregion

        #region ReadRegister Tests

        [Theory]
        [InlineData(55, new byte[] { 240 }, true, -16)]
        [InlineData(55, new byte[] { 240 }, false, -16)]
        public void ReadRegister_GivenAddress_WhenGettingDataFromDeviceAndUsingEndianness_ThenReturnsExpectedSByte(
            byte registerAddress,
            byte[] dataFromDevice,
            bool useLittleEndian,
            sbyte expected)
        {
            RunReadTest(registerAddress, dataFromDevice, useLittleEndian, expected);
        }

        [Theory]
        [InlineData(55, new byte[] { 198 }, true, 198)]
        [InlineData(55, new byte[] { 198 }, false, 198)]
        public void ReadRegister_GivenAddress_WhenGettingDataFromDeviceAndUsingEndianness_ThenReturnsExpectedByte(
            byte registerAddress,
            byte[] dataFromDevice,
            bool useLittleEndian,
            byte expected)
        {
            RunReadTest(registerAddress, dataFromDevice, useLittleEndian, expected);
        }

        [Theory]
        [InlineData(55, new byte[] { 128, 25 }, true, 6528)]
        [InlineData(55, new byte[] { 25, 128 }, false, 6528)]
        public void ReadRegister_GivenAddress_WhenGettingDataFromDeviceAndUsingEndianness_ThenReturnsExpectedShort(
            byte registerAddress,
            byte[] dataFromDevice,
            bool useLittleEndian,
            short expected)
        {
            RunReadTest(registerAddress, dataFromDevice, useLittleEndian, expected);
        }

        [Theory]
        [InlineData(55, new byte[] { 0, 255 }, true, 65280)]
        [InlineData(55, new byte[] { 255, 0 }, false, 65280)]
        public void ReadRegister_GivenAddress_WhenGettingDataFromDeviceAndUsingEndianness_ThenReturnsExpectedUShort(
            byte registerAddress,
            byte[] dataFromDevice,
            bool useLittleEndian,
            ushort expected)
        {
            RunReadTest(registerAddress, dataFromDevice, useLittleEndian, expected);
        }

        [Theory]
        [InlineData(55, new byte[] { 100, 168, 51, 243 }, true, -214718364)]
        [InlineData(55, new byte[] { 243, 51, 168, 100 }, false, -214718364)]
        public void ReadRegister_GivenAddress_WhenGettingDataFromDeviceAndUsingEndianness_ThenReturnsExpectedInt(
            byte registerAddress,
            byte[] dataFromDevice,
            bool useLittleEndian,
            int expected)
        {
            RunReadTest(registerAddress, dataFromDevice, useLittleEndian, expected);
        }

        [Theory]
        [InlineData(55, new byte[] { 0, 255, 0, 255 }, true, 4278255360)]
        [InlineData(55, new byte[] { 255, 0, 255, 0 }, false, 4278255360)]
        public void ReadRegister_GivenAddress_WhenGettingDataFromDeviceAndUsingEndianness_ThenReturnsExpectedUInt(
            byte registerAddress,
            byte[] dataFromDevice,
            bool useLittleEndian,
            uint expected)
        {
            RunReadTest(registerAddress, dataFromDevice, useLittleEndian, expected);
        }

        [Theory]
        [InlineData(55, new byte[] { 0, 255, 0, 255, 0, 255, 0, 0 }, true, 280379743338240)]
        [InlineData(55, new byte[] { 0, 0, 255, 0, 255, 0, 255, 0 }, false, 280379743338240)]
        public void ReadRegister_GivenAddress_WhenGettingDataFromDeviceAndUsingEndianness_ThenReturnsExpectedLong(
            byte registerAddress,
            byte[] dataFromDevice,
            bool useLittleEndian,
            long expected)
        {
            RunReadTest(registerAddress, dataFromDevice, useLittleEndian, expected);
        }

        [Theory]
        [InlineData(55, new byte[] { 0, 255, 0, 255, 0, 255, 0, 255 }, true, 18374966859414961920)]
        [InlineData(55, new byte[] { 255, 0, 255, 0, 255, 0, 255, 0 }, false, 18374966859414961920)]
        public void ReadRegister_GivenAddress_WhenGettingDataFromDeviceAndUsingEndianness_ThenReturnsExpectedULong(
            byte registerAddress,
            byte[] dataFromDevice,
            bool useLittleEndian,
            ulong expected)
        {
            RunReadTest(registerAddress, dataFromDevice, useLittleEndian, expected);
        }

        [Fact]
        public void ReadRegister_GivenAddress_WhenRequestingUnsupportedDataType_ThenThrowsException()
        {
            // Arrange
            var deviceMock = new I2cDeviceMock();
            var sut = new I2cRegisterAccess(deviceMock);
            deviceMock.ReadBuffer = new byte[0];

            // Act + Assert
            Assert.Throws<InvalidOperationException>(() => sut.ReadRegister<char>(55, true));
        }

        #endregion

        private static void RunWriteTest<T>(byte registerAddress, T data, bool useLittleEndian, byte[] expected)
            where T : struct
        {
            // Arrange
            var deviceMock = new I2cDeviceMock();
            var sut = new I2cRegisterAccess(deviceMock);

            // Act
            sut.WriteRegister(registerAddress, data, useLittleEndian);

            // Assert
            Assert.Equal(deviceMock.WriteBuffer, expected);
        }

        private static void RunReadTest<T>(byte registerAddress, byte[] dataFromDevice, bool useLittleEndian, T expected)
            where T : struct
        {
            // Arrange
            var deviceMock = new I2cDeviceMock();
            var sut = new I2cRegisterAccess(deviceMock);
            deviceMock.ReadBuffer = dataFromDevice;

            // Act
            var actual = sut.ReadRegister<T>(registerAddress, useLittleEndian);

            // Assert
            Assert.Equal(deviceMock.WriteBuffer, new[] { registerAddress });
            Assert.Equal(expected, actual);
        }
    }
}
