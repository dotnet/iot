// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Text;

namespace System.Device
{
    /// <summary>
    /// Identifies the state of an instance of <see cref="ComponentInformation"/>
    /// </summary>
    public enum ComponentState
    {
        /// <summary>
        /// The state is unknown
        /// </summary>
        Unknown,

        /// <summary>
        /// The component is active and available
        /// </summary>
        Active,

        /// <summary>
        /// The component is available, but not currently in use
        /// </summary>
        Available,

        /// <summary>
        /// The component has failed or cannot be initialized
        /// </summary>
        Failed
    }
}
