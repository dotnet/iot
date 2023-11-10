// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System;
using System.Reflection;
using Iot.Device.Vcnl4040.Infrastructure;
using Xunit;

namespace Iot.Device.Vcnl4040.Tests
{
    public class RegisterTest
    {
        internal static void PropertyWriteTest<TReg, TVal>(byte initialRegisterLowByte,
                                                           byte initialRegisterHighByte,
                                                           TVal testValue,
                                                           byte expectedLowByte,
                                                           byte expectedHighByte,
                                                           byte commandCode,
                                                           string registerPropertyName,
                                                           int expectedNumberOfBytesWritten,
                                                           bool readBeforeWrite)
            where TReg : Register
        {
            var testDevice = new I2cTestDevice();
            I2cInterface testBus = new(testDevice);
            // enqueue data for initial read operation to load register with device data
            if (readBeforeWrite)
            {
                testDevice.DataToRead.Enqueue(initialRegisterLowByte);
                testDevice.DataToRead.Enqueue(initialRegisterHighByte);
            }

            // enqueue data for read as part of write
            testDevice.DataToRead.Enqueue(initialRegisterLowByte);
            testDevice.DataToRead.Enqueue(initialRegisterHighByte);

            // Instantiate the class, reead register from device, set the property and write back to device
            TReg reg = (TReg)Activator.CreateInstance(typeof(TReg), testBus)!;
            if (readBeforeWrite)
            {
                reg.Read();
            }

            PropertyInfo property = typeof(TReg).GetProperty(registerPropertyName)!;
            property.SetValue(reg, testValue);
            reg.Write();

            Assert.Equal(expectedNumberOfBytesWritten, testDevice.DataWritten.Count);
            if (readBeforeWrite)
            {
                Assert.Equal(commandCode, testDevice.DataWritten.Dequeue());
                Assert.Equal(commandCode, testDevice.DataWritten.Dequeue());
            }

            Assert.Equal(commandCode, testDevice.DataWritten.Dequeue());
            Assert.Equal(expectedLowByte, testDevice.DataWritten.Dequeue());
            Assert.Equal(expectedHighByte, testDevice.DataWritten.Dequeue());
        }
    }
}
