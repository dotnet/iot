// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.IO;

namespace System.Device.Media
{
    public abstract partial class VideoDevice : IDisposable
    {
        /// <summary>
        /// Creates a communications channel to a video device running on Unix.
        /// </summary>
        /// <param name="settings">The connection settings of a video device.</param>
        /// <returns>A communications channel to a video device running on Unix.</returns>
        public static VideoDevice Create(VideoConnectionSettings settings) => new Windows10VideoDevice(settings);
    }
}
