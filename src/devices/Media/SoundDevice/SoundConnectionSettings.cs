// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Media
{
    /// <summary>
    /// The connection settings of a sound device.
    /// </summary>
    public class SoundConnectionSettings
    {
        /// <summary>
        /// The playback device name of the sound device is connected to.
        /// </summary>
        public string PlaybackDeviceName { get; set; } = "default";

        /// <summary>
        /// The recording device name of the sound device is connected to.
        /// </summary>
        public string RecordingDeviceName { get; set; } = "default";

        /// <summary>
        /// The mixer device name of the sound device is connected to.
        /// </summary>
        public string MixerDeviceName { get; set; } = "default";

        /// <summary>
        /// The sample rate of recording.
        /// </summary>
        public uint RecordingSampleRate { get; set; } = 8000;

        /// <summary>
        /// The channels of recording.
        /// </summary>
        public ushort RecordingChannels { get; set; } = 2;

        /// <summary>
        /// The bits per sample of recording.
        /// </summary>
        public ushort RecordingBitsPerSample { get; set; } = 16;
    }
}
