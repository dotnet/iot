// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Text;

namespace System.Device
{
    /// <summary>
    /// Interface to support querying component information from a class
    /// (declared internal for now, since it doesn't need exposing as long as the callers operate on the actual type of the components)
    /// </summary>
    internal interface IQueryComponentInformation
    {
        /// <summary>
        /// Query information about a component and it's children.
        /// </summary>
        /// <returns>A tree of <see cref="ComponentInformation"/> instances.</returns>
        public ComponentInformation QueryComponentInformation();
    }
}
