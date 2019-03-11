// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.Mcp25xxx.Register;
using Iot.Device.Mcp25xxx.Register.CanControl;
using Xunit;
using static Iot.Device.Mcp25xxx.Register.CanControl.CanCtrl;

namespace Iot.Device.Mcp25xxx.Tests.Register.CanControl
{
    public class CanCtrlTests
    {
        [Fact]
        public void Get_Address()
        {
            Assert.Equal(Address.CanCtrl, new CanCtrl(ClkOutPinPrescaler.ClockDivideBy1, false, false, false, OperationMode.NormalOperation).Address);
        }

        [Theory]
        [InlineData(ClkOutPinPrescaler.ClockDivideBy1, false, false, false, OperationMode.NormalOperation, 0b0000_0000)]
        [InlineData(ClkOutPinPrescaler.ClockDivideBy2, false, false, false, OperationMode.NormalOperation, 0b0000_0001)]
        [InlineData(ClkOutPinPrescaler.ClockDivideBy4, false, false, false, OperationMode.NormalOperation, 0b0000_0010)]
        [InlineData(ClkOutPinPrescaler.ClockDivideBy8, false, false, false, OperationMode.NormalOperation, 0b0000_0011)]
        [InlineData(ClkOutPinPrescaler.ClockDivideBy1, true, false, false, OperationMode.NormalOperation, 0b0000_0100)]
        [InlineData(ClkOutPinPrescaler.ClockDivideBy1, false, true, false, OperationMode.NormalOperation, 0b0000_1000)]
        [InlineData(ClkOutPinPrescaler.ClockDivideBy1, false, false, true, OperationMode.NormalOperation, 0b0001_0000)]
        [InlineData(ClkOutPinPrescaler.ClockDivideBy1, false, false, false, OperationMode.Sleep, 0b0010_0000)]
        [InlineData(ClkOutPinPrescaler.ClockDivideBy1, false, false, false, OperationMode.Loopback, 0b0100_0000)]
        [InlineData(ClkOutPinPrescaler.ClockDivideBy1, false, false, false, OperationMode.ListenOnly, 0b0110_0000)]
        [InlineData(ClkOutPinPrescaler.ClockDivideBy1, false, false, false, OperationMode.Configuration, 0b1000_0000)]
        public void From_To_Byte(ClkOutPinPrescaler clkPre,
            bool clkEn,
            bool osm,
            bool abat,
            OperationMode reqOp,
            byte expectedByte)
        {
            var canCtrl = new CanCtrl(expectedByte);
            Assert.Equal(clkPre, canCtrl.ClkPre);
            Assert.Equal(clkEn, canCtrl.ClkEn);
            Assert.Equal(osm, canCtrl.Osm);
            Assert.Equal(abat, canCtrl.Abat);
            Assert.Equal(reqOp, canCtrl.ReqOp);

            Assert.Equal(expectedByte, new CanCtrl(clkPre, clkEn, osm, abat, reqOp).ToByte());
        }
    }
}
