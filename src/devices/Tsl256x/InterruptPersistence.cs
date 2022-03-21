// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Tsl256x
{
    /// <summary>
    /// Interrupt Persistence Select
    /// </summary>
    public enum InterruptPersistence : byte
    {
        /// <summary>Every ADC cycle generates interrupt</summary>
        EveryAdc = 0,

        /// <summary>Any value outside of threshold range</summary>
        AnyValueOutsideThreshold,

        /// <summary>2 integration time periods out of range</summary>
        OutOfRange02IntegrationTimePeriods,

        /// <summary>3 integration time periods out of range</summary>
        OutOfRange03IntegrationTimePeriods,

        /// <summary>4 integration time periods out of range</summary>
        OutOfRange04IntegrationTimePeriods,

        /// <summary>5 integration time periods out of range</summary>
        OutOfRange05IntegrationTimePeriods,

        /// <summary>6 integration time periods out of range</summary>
        OutOfRange06IntegrationTimePeriods,

        /// <summary>7 integration time periods out of range</summary>
        OutOfRange07IntegrationTimePeriods,

        /// <summary>8 integration time periods out of range</summary>
        OutOfRange08IntegrationTimePeriods,

        /// <summary>9 integration time periods out of range</summary>
        OutOfRange09IntegrationTimePeriods,

        /// <summary>10 integration time periods out of range</summary>
        OutOfRange10IntegrationTimePeriods,

        /// <summary>11 integration time periods out of range</summary>
        OutOfRange11IntegrationTimePeriods,

        /// <summary>12 integration time periods out of range</summary>
        OutOfRange12IntegrationTimePeriods,

        /// <summary>13 integration time periods out of range</summary>
        OutOfRange13IntegrationTimePeriods,

        /// <summary>14 integration time periods out of range</summary>
        OutOfRange14IntegrationTimePeriods,

        /// <summary>15 integration time periods out of range</summary>
        OutOfRange15IntegrationTimePeriods,
    }
}
