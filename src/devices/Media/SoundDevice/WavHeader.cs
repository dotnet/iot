// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Media
{
    /// <summary>
    /// Wave header
    /// </summary>
    public struct WavHeader
    {
        /// <summary>
        /// The wave chunk header
        /// </summary>
        public WavHeaderChunk Chunk { get; set; }

        /// <summary>
        /// The format
        /// </summary>
        public char[] Format { get; set; }

        /// <summary>
        /// First sub chunk
        /// </summary>
        public WavHeaderChunk SubChunk1 { get; set; }

        /// <summary>
        /// Audio format
        /// </summary>
        public ushort AudioFormat { get; set; }

        /// <summary>
        /// Number of channels
        /// </summary>
        public ushort NumChannels { get; set; }

        /// <summary>
        /// Sample rate
        /// </summary>
        public uint SampleRate { get; set; }

        /// <summary>
        /// Byte rate
        /// </summary>
        public uint ByteRate { get; set; }

        /// <summary>
        /// Block alignment
        /// </summary>
        public ushort BlockAlign { get; set; }

        /// <summary>
        /// Bits per sample
        /// </summary>
        public ushort BitsPerSample { get; set; }

        /// <summary>
        /// Second sub chunk
        /// </summary>
        public WavHeaderChunk SubChunk2 { get; set; }
    }
}
