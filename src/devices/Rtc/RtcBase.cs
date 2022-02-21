// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Rtc
{
    /// <summary>
    /// Real time clock (RTC)
    /// </summary>
    public abstract class RtcBase : IDisposable
    {
        private TimeZoneInfo _timeZone;

        /// <summary>
        /// The Device's raw <see cref="System.DateTime"/>.
        /// The caller must be aware of the device's time zone.
        /// The behavior of the <see cref="System.DateTime.Kind"/> property is implementation-dependent (typically it is ignored)
        /// </summary>
        public virtual DateTime RtcDateTime
        {
            get => ReadTime();
            set => SetTime(value);
        }

        /// <summary>
        /// Set or retrieves the current date/time. This property returns a <see cref="DateTimeOffset"/> and
        /// is therefore correct regardless of the current time zone (when <see cref="LocalTimeZone"/> is set correctly).
        /// </summary>
        public DateTimeOffset DateTime
        {
            get
            {
                var now = RtcDateTime;
                return new DateTimeOffset(now.Ticks, LocalTimeZone.GetUtcOffset(now));
            }
            set
            {
                var clockNow = new DateTime((value.UtcDateTime + LocalTimeZone.GetUtcOffset(value)).Ticks, DateTimeKind.Local);
                RtcDateTime = clockNow;
            }
        }

        /// <summary>
        /// Gets or sets the time zone this instance will operate in.
        /// Defaults to the local time zone from the system.
        /// Changing this property will not change the time on the real time clock,
        /// but instead affect the return value of <see cref="DateTime"/>
        /// </summary>
        public virtual TimeZoneInfo LocalTimeZone
        {
            get => _timeZone;
            set => _timeZone = value;
        }

        /// <summary>
        /// Creates an instance of this base class
        /// </summary>
        protected RtcBase()
        {
            _timeZone = TimeZoneInfo.Local;
        }

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

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the RtcBase and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">True to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
        }
    }
}
