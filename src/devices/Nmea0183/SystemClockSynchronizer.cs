// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Iot.Device.Common;
using Iot.Device.Nmea0183.Sentences;
using Iot.Device.Rtc;
using Microsoft.Extensions.Logging;

namespace Iot.Device.Nmea0183
{
    /// <summary>
    /// Synchronizes the system clock to the NMEA data stream
    /// </summary>
    public class SystemClockSynchronizer : NmeaSinkAndSource
    {
        private int _numberOfValidMessagesSeen;
        private ILogger _logger;

        static SystemClockSynchronizer()
        {
            RequiredAccuracy = TimeSpan.FromSeconds(20);
        }

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        public SystemClockSynchronizer()
            : base("System Clock Synchronizer")
        {
            _numberOfValidMessagesSeen = 0;
            _logger = this.GetCurrentClassLogger();
        }

        /// <summary>
        /// The time delta that triggers a resync. Setting this to a low value may cause permantent clock updates
        /// if messages are delayed.
        /// </summary>
        public static TimeSpan RequiredAccuracy
        {
            get;
            set;
        }

        /// <inheritdoc />
        public override void StartDecode()
        {
            // Nothing to do, this component is only a sink
        }

        /// <inheritdoc />
        public override void SendSentence(NmeaSinkAndSource source, NmeaSentence sentence)
        {
            if (sentence is TimeDate zda)
            {
                // Wait a few seconds, so that we're not looking at messages from the cache
                _numberOfValidMessagesSeen++;
                if (_numberOfValidMessagesSeen > 10)
                {
                    TimeSpan delta = (zda.DateTime.UtcDateTime - DateTime.UtcNow);
                    if (Math.Abs(delta.TotalSeconds) > RequiredAccuracy.TotalSeconds)
                    {
                        // The time message seems valid, but it is off by more than 10 seconds from what the system clock
                        // says. Synchronize.
                        SetTime(zda.DateTime.UtcDateTime);
                        _numberOfValidMessagesSeen = -50; // Don't try to often.
                    }
                }
            }
        }

        private void SetTime(DateTime dt)
        {
            try
            {
                _logger.LogInformation($"About to synchronize clock from {DateTime.UtcNow} to {dt}");
                SystemClock.SetSystemTimeUtc(dt);
            }
            catch (Exception e) when (e is UnauthorizedAccessException || e is IOException)
            {
                _logger.LogError(e, "Unable to set system time");
                return;
            }

            _logger.LogInformation($"Successfully set time. System time is now {DateTime.UtcNow}");
        }

        /// <inheritdoc />
        public override void StopDecode()
        {
        }
    }
}
