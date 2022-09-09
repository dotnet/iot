// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Vl53L1X
{
    /// <summary>
    /// The timing budget for the device
    /// </summary>
    public enum TimingBudget : ushort
    {
        /// <summary>
        /// Unknown budget or the budget is not configured
        /// </summary>
        BudgetUnknown = 0,

        /// <summary>
        /// Budget of 15 ms
        /// </summary>
        Budget15 = 15,

        /// <summary>
        /// Budget of 20 ms
        /// </summary>
        Budget20 = 20,

        /// <summary>
        /// Budget of 33 ms
        /// </summary>
        Budget33 = 33,

        /// <summary>
        /// Budget of 50 ms
        /// </summary>
        Budget50 = 50,

        /// <summary>
        /// Budget of 100 ms
        /// </summary>
        Budget100 = 100,

        /// <summary>
        /// Budget of 200 ms
        /// </summary>
        Budget200 = 200,

        /// <summary>
        /// Budget of 500 ms
        /// </summary>
        Budget500 = 500,
    }
}
