// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Max44009
{
    public enum IntegrationTime
    {
        /// <summary>
        /// 800ms
        /// </summary>
        TIM_800 = 0b000,
        /// <summary>
        /// 400ms
        /// </summary>
        TIM_400 = 0b001,
        /// <summary>
        /// 200ms
        /// </summary>
        TIM_200 = 0b010,
        /// <summary>
        /// 100ms
        /// </summary>
        TIM_100 = 0b011,
        /// <summary>
        /// 50ms
        /// </summary>
        TIM_050 = 0b100,
        /// <summary>
        /// 25ms
        /// </summary>
        TIM_025 = 0b101,
        /// <summary>
        /// 12.5ms
        /// </summary>
        TIM_012_5 = 0b110,
        /// <summary>
        /// 6.25ms
        /// </summary>
        TIM_006_25 = 0b111
    }
}
