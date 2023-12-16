// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Board
{
    internal interface IDeviceManager : IDisposable
    {
        /// <summary>
        /// Returns the list of pins that are currently in use by this manager (excluding pins that are closed)
        /// </summary>
        /// <returns>A set of pin numbers</returns>
        public IReadOnlyCollection<int> GetActiveManagedPins();

        /// <summary>
        /// Returns the component information of this manager
        /// </summary>
        /// <returns>An instance of <see cref="ComponentInformation"/>.</returns>
        public ComponentInformation QueryComponentInformation();
    }
}
