// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Seatalk1.Messages
{
    public record AutopilotCalibrationParameterMessage : SeatalkMessage
    {
        public override byte CommandByte => 0x88;
        public override byte ExpectedLength => 0x06;

        public AutopilotCalibrationParameter Parameter
        {
            get;
            init;
        }

        public int CurrentSetting
        {
            get;
            init;
        }

        public int MinValue
        {
            get;
            init;
        }

        public int MaxValue
        {
            get;
            init;
        }

        public override SeatalkMessage CreateNewMessage(IReadOnlyList<byte> data)
        {
            int param = data[2];
            int value = Convert.ToInt32(data[3]);
            int max = Convert.ToInt32(data[4]);
            int min = Convert.ToInt32(data[5]);
            return new AutopilotCalibrationParameterMessage()
            {
                Parameter = (AutopilotCalibrationParameter)param,
                CurrentSetting = value,
                MinValue = min,
                MaxValue = max,
            };
        }

        public override byte[] CreateDatagram()
        {
            throw new NotImplementedException();
        }
    }
}
