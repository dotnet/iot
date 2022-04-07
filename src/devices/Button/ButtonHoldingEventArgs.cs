// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Button
{
    /// <summary>
    /// Button holding event arguments.
    /// </summary>
    public class ButtonHoldingEventArgs : EventArgs
    {
        /// <summary>
        /// Button holding state.
        /// </summary>
        public ButtonHoldingState HoldingState { get; set; }
    }
}
