// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Media
{
    /// <summary>
    /// Wave header chunk
    /// </summary>
    public struct WavHeaderChunk
    {
        /// <summary>
        /// The chunk id of the wave header chunk
        /// </summary>
        public char[] ChunkId { get; set; }

        /// <summary>
        /// The size of the wave header chunk
        /// </summary>
        public uint ChunkSize { get; set; }
    }
}
