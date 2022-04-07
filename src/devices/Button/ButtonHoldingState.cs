// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Button
{
    /// <summary>
    /// The different states of a button that is being held.
    /// </summary>
    public enum ButtonHoldingState
    {
        /// <summary>Button holding started.</summary>
        Started,

        /// <summary>Button holding completed.</summary>
        Completed,

        /// <summary>Button holding cancelled.</summary>
        Canceled,
    }
}
