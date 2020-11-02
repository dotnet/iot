// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Media
{
    internal struct WavHeader
    {
        public WavHeaderChunk Chunk { get; set; }

        public char[] Format { get; set; }

        public WavHeaderChunk SubChunk1 { get; set; }

        public ushort AudioFormat { get; set; }

        public ushort NumChannels { get; set; }

        public uint SampleRate { get; set; }

        public uint ByteRate { get; set; }

        public ushort BlockAlign { get; set; }

        public ushort BitsPerSample { get; set; }

        public WavHeaderChunk SubChunk2 { get; set; }
    }
}
