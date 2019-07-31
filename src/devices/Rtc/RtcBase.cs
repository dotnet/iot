// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.Rtc
{
    public abstract class RtcBase : IDisposable
    {
        /// <summary>
        /// The Device's <see cref="System.DateTime"/>
        /// </summary>
        public virtual DateTime DateTime { get => ReadTime(); set => SetTime(value); }

        /// <summary>
        /// Set the device time
        /// </summary>
        /// <param name="time">Time</param>
        protected abstract void SetTime(DateTime time);

        /// <summary>
        /// Read time from the device
        /// </summary>
        /// <returns>Time from the device</returns>
        protected abstract DateTime ReadTime();

        /// <summary>
        /// Cleanup
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) { }
    }
}
