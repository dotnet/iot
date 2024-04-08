// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace EmulatedTag
{
    /// <summary>
    /// The state of the card.
    /// </summary>
    public enum CardStatus
    {
        /// <summary>Card is in released state.</summary>
        Released = 0,

        /// <summary>Car is in activated state.</summary>
        Activated,

        /// <summary>Card is on delected state.</summary>
        Deselected
    }
}
