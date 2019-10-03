// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;

namespace Iot.Device.Media
{
    /// <summary>
    /// The communications channel to a sound device.
    /// </summary>
    public abstract partial class SoundDevice : IDisposable
    {
        /// <summary>
        /// Create a communications channel to a sound device running on Unix.
        /// </summary>
        /// <param name="settings">The connection settings of a sound device.</param>
        /// <returns>A communications channel to a sound device running on Unix.</returns>
        public static SoundDevice Create(SoundConnectionSettings settings) => new UnixSoundDevice(settings);

        /// <summary>
        /// The connection settings of the sound device.
        /// </summary>
        public abstract SoundConnectionSettings Settings { get; }

        /// <summary>
        /// The playback volume of the sound device.
        /// </summary>
        public abstract long PlaybackVolume { get; set; }

        /// <summary>
        /// The playback mute of the sound device.
        /// </summary>
        public abstract bool PlaybackMute { get; set; }

        /// <summary>
        /// The recording volume of the sound device.
        /// </summary>
        public abstract long RecordingVolume { get; set; }

        /// <summary>
        /// The recording mute of the sound device.
        /// </summary>
        public abstract bool RecordingMute { get; set; }

        /// <summary>
        /// Play WAV file.
        /// </summary>
        /// <param name="wavPath">WAV file path.</param>
        public abstract void Play(string wavPath);

        /// <summary>
        /// Play WAV file.
        /// </summary>
        /// <param name="wavStream">WAV stream.</param>
        public abstract void Play(Stream wavStream);

        /// <summary>
        /// Sound recording.
        /// </summary>
        /// <param name="recordTimeSeconds">Recording duration(In seconds).</param>
        /// <param name="outputFilePath">Recording save path.</param>
        public abstract void Record(uint recordTimeSeconds, string outputFilePath);

        /// <summary>
        /// Sound recording.
        /// </summary>
        /// <param name="recordTimeSeconds">Recording duration(In seconds).</param>
        /// <param name="outputStream">Recording save stream.</param>
        public abstract void Record(uint recordTimeSeconds, Stream outputStream);

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the SoundDevice and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">True to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing) { }
    }
}
