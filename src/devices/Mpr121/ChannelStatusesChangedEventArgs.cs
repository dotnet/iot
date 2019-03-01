// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Iot.Device.Mpr121
{
    /// <summary>
    /// Represents the arguments of event rising when the channel statuses have been changed.
    /// </summary>
    public class ChannelStatusesChangedEventArgs : EventArgs
    {
        /// <summary>
        /// The channel statuses.
        /// </summary>
        public IReadOnlyDictionary<Channels, bool> ChannelStatuses { get; private set; }

        /// <summary>
        /// Initialize event arguments.
        /// </summary>
        /// <param name="channelStatuses">The channel statuses.</param>
        public ChannelStatusesChangedEventArgs(IReadOnlyDictionary<Channels, bool> channelStatuses) : base()
        {
            ChannelStatuses = channelStatuses;
        }
    }
}