// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System;
using System.Reflection;
using Iot.Device.Vcnl4040.Internal;
using Xunit;

namespace Iot.Device.Vcnl4040.Tests.Internal
{
    public class RegisterTest
    {
        protected const byte InitialLowByte = 0x00;
        protected const byte InitialLowByteInv = 0xff;
        protected const byte InitialHighByte = 0x00;
        protected const byte InitialHighByteInv = 0xff;
        protected const byte UnmodifiedLowByte = 0x55;
        protected const byte UnmodifiedLowByteInv = 0xaa;
        protected const byte UnmodifiedHighByte = 0x55;
        protected const byte UnmodifiedHighByteInv = 0xaa;

        internal static void PropertyWriteTest<TReg, TVal>(byte initialRegisterLowByte,
                                                           byte initialRegisterHighByte,
                                                           TVal testValue,
                                                           byte expectedLowByte,
                                                           byte expectedHighByte,
                                                           byte commandCode,
                                                           string registerPropertyName,
                                                           bool registerReadsBeforeWriting)
            where TReg : Register
        {
            var testDevice = new I2cTestDevice();
            // enqueue data for initial read operation to load register with device data
            testDevice.DataToRead.Enqueue(initialRegisterLowByte);
            testDevice.DataToRead.Enqueue(initialRegisterHighByte);

            // enqueue data for read as part of write
            if (registerReadsBeforeWriting)
            {
                testDevice.DataToRead.Enqueue(initialRegisterLowByte);
                testDevice.DataToRead.Enqueue(initialRegisterHighByte);
            }

            // Instantiate the class, reead register from device, set the property and write back to device
            TReg reg = (TReg)Activator.CreateInstance(typeof(TReg), testDevice)!;
            reg.Read();

            PropertyInfo property = typeof(TReg).GetProperty(registerPropertyName)!;
            property.SetValue(reg, testValue);
            reg.Write();

            int expectedNumberOfBytesWritten = 0;
            if (registerReadsBeforeWriting)
            {
                // expect 5 bytes to be written: 1x command code for read, 1x command code for read in write, 1x command code for actual write, 2x data for write
                expectedNumberOfBytesWritten = 5;
            }
            else
            {
                // expect 3 bytes to be written: 1x command code for actual write, 2x data for write
                expectedNumberOfBytesWritten = 4;
            }

            Assert.Equal(expectedNumberOfBytesWritten, testDevice.DataWritten.Count);
            // command code from Register.Read call above
            Assert.Equal(commandCode, testDevice.DataWritten.Dequeue());
            // command code if Register.Write perform a read before altering register content (for those registers with flags)
            if (registerReadsBeforeWriting)
            {
                Assert.Equal(commandCode, testDevice.DataWritten.Dequeue());
            }

            Assert.Equal(commandCode, testDevice.DataWritten.Dequeue());
            Assert.Equal(expectedLowByte, testDevice.DataWritten.Dequeue());
            Assert.Equal(expectedHighByte, testDevice.DataWritten.Dequeue());
        }
    }
}
