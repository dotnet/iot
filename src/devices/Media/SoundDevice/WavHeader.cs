// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Media
{
    internal struct WavHeader
    {
        public char[] ChunkId { get; set; }

        public uint ChunkSize { get; set; }

        public char[] Format { get; set; }

        public char[] Subchunk1ID { get; set; }

        public uint Subchunk1Size { get; set; }

        public ushort AudioFormat { get; set; }

        public ushort NumChannels { get; set; }

        public uint SampleRate { get; set; }

        public uint ByteRate { get; set; }

        public ushort BlockAlign { get; set; }

        public ushort BitsPerSample { get; set; }

        public char[] Subchunk2Id { get; set; }

        public uint Subchunk2Size { get; set; }
    }
}
