// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Media
{
    internal struct WavHeaderChunk
    {
        public char[] ChunkId { get; set; }

        public uint ChunkSize { get; set; }
    }
}
