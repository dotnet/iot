// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Text;

namespace System.Device
{
    /// <summary>
    /// Interface to support querying component information from a class
    /// </summary>
    public interface IQueryComponentInformation
    {
        /// <summary>
        /// Query information about a component and it's children.
        /// </summary>
        /// <param name="onlyActive">True to return only active components, false to also list possible alternatives or inactive drivers</param>
        /// <returns>A tree of <see cref="ComponentInformation"/> instances.</returns>
        public ComponentInformation QueryComponentInformation(bool onlyActive);
    }
}
