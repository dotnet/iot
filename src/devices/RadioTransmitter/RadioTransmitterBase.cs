// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Text;

namespace Iot.Device.RadioTransmitter
{
    /// <summary>
    /// Base class for radio transmitter.
    /// </summary>
    public abstract class RadioTransmitterBase : IDisposable
    {
        /// <summary>
        /// Radio transmitter FM frequency.
        /// </summary>
        public abstract double Frequency { get; set; }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the RadioTransmitterBase and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">True to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
        }
    }
}
