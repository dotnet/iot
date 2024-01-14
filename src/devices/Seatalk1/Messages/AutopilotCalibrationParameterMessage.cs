// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Seatalk1.Messages
{
    /// <summary>
    /// Message generated when autopilot is in calibration mode.
    /// </summary>
    public record AutopilotCalibrationParameterMessage : SeatalkMessage
    {
        /// <inheritdoc />
        public override byte CommandByte => 0x88;

        /// <inheritdoc />
        public override byte ExpectedLength => 0x06;

        /// <summary>
        /// The parameter in this message. Defines the meaning of the other fields.
        /// </summary>
        public AutopilotCalibrationParameter Parameter
        {
            get;
            init;
        }

        /// <summary>
        /// The value of the parameter
        /// </summary>
        public int CurrentSetting
        {
            get;
            init;
        }

        /// <summary>
        /// Minimum allowed value for the setting
        /// </summary>
        public int MinValue
        {
            get;
            init;
        }

        /// <summary>
        /// Maximum allowed value for the setting
        /// </summary>
        public int MaxValue
        {
            get;
            init;
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public override byte[] CreateDatagram()
        {
            byte[] data = new byte[ExpectedLength];
            data[0] = CommandByte;
            data[1] = (byte)(ExpectedLength - 3);
            data[2] = (byte)Parameter;
            data[3] = (byte)CurrentSetting;
            data[4] = (byte)MaxValue;
            data[5] = (byte)MinValue;
            return data;
        }
    }
}
