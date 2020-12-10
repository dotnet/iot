// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Buffers.Binary;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Iot.Device.Media
{
    /// <summary>
    /// Represents a communications channel to a sound device running on Unix.
    /// </summary>
    internal class UnixSoundDevice : SoundDevice
    {
        private IntPtr _playbackPcm;
        private IntPtr _recordingPcm;
        private IntPtr _mixer;
        private IntPtr _elem;
        private int _errorNum;

        private static readonly object s_playbackInitializationLock = new object();
        private static readonly object s_recordingInitializationLock = new object();
        private static readonly object s_mixerInitializationLock = new object();

        private uint _recordingTotalTimeSeconds;
        private bool _record;
        private Thread? _recordingThread;

        /// <summary>
        /// The connection settings of the sound device.
        /// </summary>
        public override SoundConnectionSettings Settings { get; }

        /// <summary>
        /// The playback volume of the sound device.
        /// </summary>
        public override long PlaybackVolume
        {
            get => GetPlaybackVolume();
            set
            {
                SetPlaybackVolume(value);
            }
        }

        // The lib do not have a method of get all channels mute state.
        private bool _playbackMute;

        /// <summary>
        /// The playback mute of the sound device.
        /// </summary>
        public override bool PlaybackMute
        {
            get => _playbackMute;
            set
            {
                SetPlaybackMute(value);
                _playbackMute = value;
            }
        }

        /// <summary>
        /// The recording volume of the sound device.
        /// </summary>
        public override long RecordingVolume
        {
            get => GetRecordingVolume();
            set => SetRecordingVolume(value);
        }

        private bool _recordingMute;

        /// <summary>
        /// The recording mute of the sound device.
        /// </summary>
        public override bool RecordingMute
        {
            get => _recordingMute;
            set
            {
                SetRecordingMute(value);
                _recordingMute = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnixSoundDevice"/> class that will use the specified settings to communicate with the sound device.
        /// </summary>
        /// <param name="settings">The connection settings of a sound device.</param>
        /// <param name="unmute">Unmute the device if true</param>
        /// <remarks>Some device do not support to be unmuted, if you try so, it will raise an exception and dispose the device. In this case, you should set the unmute parameter to false</remarks>
        public UnixSoundDevice(SoundConnectionSettings settings, bool unmute = true)
        {
            Settings = settings;

            if (unmute)
            {
                PlaybackMute = false;
                RecordingMute = false;
            }
        }

        /// <summary>
        /// Play WAV file.
        /// </summary>
        /// <param name="wavPath">WAV file path.</param>
        public override void Play(string wavPath)
        {
            using FileStream fs = File.Open(wavPath, FileMode.Open);

            Play(fs);
        }

        /// <summary>
        /// Play WAV file.
        /// </summary>
        /// <param name="wavStream">WAV stream.</param>
        public override void Play(Stream wavStream)
        {
            IntPtr @params = new IntPtr();
            int dir = 0;
            WavHeader header = ReadWavHeader(wavStream);

            try
            {
                OpenPlaybackPcm();
                PcmInitialize(_playbackPcm, header, ref @params, ref dir);
                WriteStream(wavStream, header, ref @params, ref dir);
                ClosePlaybackPcm();
            }
            finally
            {
                Dispose();
            }
        }

        /// <summary>
        /// Sound recording.
        /// </summary>
        /// <param name="recordTimeSeconds">Recording duration(In seconds).</param>
        /// <param name="outputFilePath">Recording save path.</param>
        public override void Record(uint recordTimeSeconds, string outputFilePath)
        {
            using (FileStream fs = File.Open(outputFilePath, FileMode.Create))
            {
                Record(recordTimeSeconds, fs);
            }
        }

        /// <summary>
        /// Sound recording.
        /// </summary>
        /// <param name="recordTimeSeconds">Recording duration(In seconds).</param>
        /// <param name="outputStream">Recording save stream.</param>
        public override void Record(uint recordTimeSeconds, Stream outputStream)
        {
            IntPtr @params = new IntPtr();
            int dir = 0;

            var header = CreateHeader(recordTimeSeconds);

            WriteWavHeader(outputStream, header);
            try
            {
                OpenRecordingPcm();
                PcmInitialize(_recordingPcm, header, ref @params, ref dir);
                ReadStream(outputStream, header, ref @params, ref dir);
                CloseRecordingPcm();
            }
            finally
            {
                // We won't do anything on purpose
            }
        }

        private WavHeader CreateHeader(uint recordTimeSeconds)
        {
            WavHeaderChunk chunk = new WavHeaderChunk
            {
                ChunkId = new[] { 'R', 'I', 'F', 'F' },
                ChunkSize = recordTimeSeconds * Settings.RecordingSampleRate * Settings.RecordingBitsPerSample * Settings.RecordingChannels / 8 + 36
            };
            WavHeaderChunk subChunk1 = new WavHeaderChunk
            {
                ChunkId = new[] { 'f', 'm', 't', ' ' },
                ChunkSize = 16
            };
            WavHeaderChunk subChunk2 = new WavHeaderChunk
            {
                ChunkId = new[] { 'd', 'a', 't', 'a' },
                ChunkSize = recordTimeSeconds * Settings.RecordingSampleRate * Settings.RecordingBitsPerSample * Settings.RecordingChannels / 8
            };

            WavHeader header = new WavHeader
            {
                Chunk = chunk,
                Format = new[] { 'W', 'A', 'V', 'E' },
                SubChunk1 = subChunk1,
                AudioFormat = 1,
                NumChannels = Settings.RecordingChannels,
                SampleRate = Settings.RecordingSampleRate,
                ByteRate = Settings.RecordingSampleRate * Settings.RecordingBitsPerSample * Settings.RecordingChannels / 8,
                BlockAlign = (ushort)(Settings.RecordingBitsPerSample * Settings.RecordingChannels / 8),
                BitsPerSample = Settings.RecordingBitsPerSample,
                SubChunk2 = subChunk2
            };

            return header;
        }

        public override void StartRecording(string outputFilePath)
        {
            Record(outputFilePath);
        }

        public override void StopRecording()
        {
            _record = false;
            _recordingThread!.Join();
        }

        private void Record(string outputFilePath)
        {
            _recordingThread = new Thread(() =>
            {
                IntPtr @params = new IntPtr();
                int dir = 0;
                _recordingTotalTimeSeconds = 0;
                var header = CreateHeader(1);
                using (var recordingFile = File.Open(outputFilePath, FileMode.Create))
                {
                    try
                    {
                        OpenRecordingPcm();
                        PcmInitialize(_recordingPcm, header, ref @params, ref dir);
                        _record = true;
                        while (_record)
                        {
                            _recordingTotalTimeSeconds++;
                            ReadStream(recordingFile, header, ref @params, ref dir);
                        }

                        CloseRecordingPcm();
                    }
                    finally
                    {
                        Dispose();
                    }

                    // Write header with total time
                    header = CreateHeader(_recordingTotalTimeSeconds);
                    recordingFile.Position = 0;
                    WriteWavHeader(recordingFile, header);
                    recordingFile.Flush();
                    recordingFile.Close();
                    recordingFile.Dispose();
                }
            });
            _recordingThread.Start();
        }

        public override void WriteWavHeader(Stream wavStream, WavHeader header)
        {
            byte[] writeBuffer2 = new byte[2];
            byte[] writeBuffer4 = new byte[4];

            writeBuffer4 = Encoding.ASCII.GetBytes(header.Chunk.ChunkId);
            wavStream.Write(writeBuffer4);

            BinaryPrimitives.WriteUInt32LittleEndian(writeBuffer4, header.Chunk.ChunkSize);
            wavStream.Write(writeBuffer4);

            writeBuffer4 = Encoding.ASCII.GetBytes(header.Format);
            wavStream.Write(writeBuffer4);

            writeBuffer4 = Encoding.ASCII.GetBytes(header.SubChunk1.ChunkId);
            wavStream.Write(writeBuffer4);

            BinaryPrimitives.WriteUInt32LittleEndian(writeBuffer4, header.SubChunk1.ChunkSize);
            wavStream.Write(writeBuffer4);

            BinaryPrimitives.WriteUInt16LittleEndian(writeBuffer2, header.AudioFormat);
            wavStream.Write(writeBuffer2);

            BinaryPrimitives.WriteUInt16LittleEndian(writeBuffer2, header.NumChannels);
            wavStream.Write(writeBuffer2);

            BinaryPrimitives.WriteUInt32LittleEndian(writeBuffer4, header.SampleRate);
            wavStream.Write(writeBuffer4);

            BinaryPrimitives.WriteUInt32LittleEndian(writeBuffer4, header.ByteRate);
            wavStream.Write(writeBuffer4);

            BinaryPrimitives.WriteUInt16LittleEndian(writeBuffer2, header.BlockAlign);
            wavStream.Write(writeBuffer2);

            BinaryPrimitives.WriteUInt16LittleEndian(writeBuffer2, header.BitsPerSample);
            wavStream.Write(writeBuffer2);

            writeBuffer4 = Encoding.ASCII.GetBytes(header.SubChunk2.ChunkId);
            wavStream.Write(writeBuffer4);

            BinaryPrimitives.WriteUInt32LittleEndian(writeBuffer4, header.SubChunk2.ChunkSize);
            wavStream.Write(writeBuffer4);
        }

        private WavHeader ReadWavHeader(Stream wavStream)
        {
            byte[] readBuffer2 = new byte[2];
            byte[] readBuffer4 = new byte[4];

            WavHeaderChunk chunk = new WavHeaderChunk();
            WavHeaderChunk subChunk1 = new WavHeaderChunk();
            WavHeaderChunk subChunk2 = new WavHeaderChunk();

            WavHeader header = new WavHeader();

            wavStream.Read(readBuffer4, 0, readBuffer4.Length);
            chunk.ChunkId = Encoding.ASCII.GetString(readBuffer4).ToCharArray();

            wavStream.Read(readBuffer4, 0, readBuffer4.Length);
            chunk.ChunkSize = BinaryPrimitives.ReadUInt32LittleEndian(readBuffer4);

            wavStream.Read(readBuffer4, 0, readBuffer4.Length);
            header.Format = Encoding.ASCII.GetString(readBuffer4).ToCharArray();

            wavStream.Read(readBuffer4, 0, readBuffer4.Length);
            subChunk1.ChunkId = Encoding.ASCII.GetString(readBuffer4).ToCharArray();

            wavStream.Read(readBuffer4, 0, readBuffer4.Length);
            subChunk1.ChunkSize = BinaryPrimitives.ReadUInt32LittleEndian(readBuffer4);

            wavStream.Read(readBuffer2, 0, readBuffer2.Length);
            header.AudioFormat = BinaryPrimitives.ReadUInt16LittleEndian(readBuffer2);

            wavStream.Read(readBuffer2, 0, readBuffer2.Length);
            header.NumChannels = BinaryPrimitives.ReadUInt16LittleEndian(readBuffer2);

            wavStream.Read(readBuffer4, 0, readBuffer4.Length);
            header.SampleRate = BinaryPrimitives.ReadUInt32LittleEndian(readBuffer4);

            wavStream.Read(readBuffer4, 0, readBuffer4.Length);
            header.ByteRate = BinaryPrimitives.ReadUInt32LittleEndian(readBuffer4);

            wavStream.Read(readBuffer2, 0, readBuffer2.Length);
            header.BlockAlign = BinaryPrimitives.ReadUInt16LittleEndian(readBuffer2);

            wavStream.Read(readBuffer2, 0, readBuffer2.Length);
            header.BitsPerSample = BinaryPrimitives.ReadUInt16LittleEndian(readBuffer2);

            wavStream.Read(readBuffer4, 0, readBuffer4.Length);
            subChunk2.ChunkId = Encoding.ASCII.GetString(readBuffer4).ToCharArray();

            wavStream.Read(readBuffer4, 0, readBuffer4.Length);
            subChunk2.ChunkSize = BinaryPrimitives.ReadUInt32LittleEndian(readBuffer4);

            header.Chunk = chunk;
            header.SubChunk1 = subChunk1;
            header.SubChunk2 = subChunk2;

            return header;
        }

        private unsafe void WriteStream(Stream wavStream, WavHeader header, ref IntPtr @params, ref int dir)
        {
            ulong frames, bufferSize;

            fixed (int* dirP = &dir)
            {
                _errorNum = Interop.snd_pcm_hw_params_get_period_size(@params, &frames, dirP);
                ThrowIfError("Can not get period size.");
            }

            bufferSize = frames * header.BlockAlign;
            // In Interop, the frames is defined as ulong. But actucally, the value of bufferSize won't be too big.
            byte[] readBuffer = new byte[(int)bufferSize];

            fixed (byte* buffer = readBuffer)
            {
                while (wavStream.Read(readBuffer, 0, readBuffer.Length) != 0)
                {
                    _errorNum = Interop.snd_pcm_writei(_playbackPcm, (IntPtr)buffer, frames);
                    ThrowIfError("Can not write data to the device.");
                }
            }
        }

        private unsafe void ReadStream(Stream outputStream, WavHeader header, ref IntPtr @params, ref int dir)
        {
            ulong frames, bufferSize;

            fixed (int* dirP = &dir)
            {
                _errorNum = Interop.snd_pcm_hw_params_get_period_size(@params, &frames, dirP);
                ThrowIfError("Can not get period size.");
            }

            bufferSize = frames * header.BlockAlign;
            byte[] readBuffer = new byte[(int)bufferSize];

            fixed (byte* buffer = readBuffer)
            {
                for (int i = 0; i < (int)(header.SubChunk2.ChunkSize / bufferSize); i++)
                {
                    _errorNum = Interop.snd_pcm_readi(_recordingPcm, (IntPtr)buffer, frames);
                    ThrowIfError("Can not read data from the device.");

                    outputStream.Write(readBuffer, 0, readBuffer.Length);
                }
            }

            outputStream.Flush();
        }

        private unsafe void PcmInitialize(IntPtr pcm, WavHeader header, ref IntPtr @params, ref int dir)
        {
            _errorNum = Interop.snd_pcm_hw_params_malloc(out @params);
            ThrowIfError("Can not allocate parameters object.");

            _errorNum = Interop.snd_pcm_hw_params_any(pcm, @params);
            ThrowIfError("Can not fill parameters object.");

            _errorNum = Interop.snd_pcm_hw_params_set_access(pcm, @params, snd_pcm_access_t.SND_PCM_ACCESS_RW_INTERLEAVED);
            ThrowIfError("Can not set access mode.");

            _errorNum = (int)(header.BitsPerSample / 8) switch
            {
                1 => Interop.snd_pcm_hw_params_set_format(pcm, @params, snd_pcm_format_t.SND_PCM_FORMAT_U8),
                2 => Interop.snd_pcm_hw_params_set_format(pcm, @params, snd_pcm_format_t.SND_PCM_FORMAT_S16_LE),
                3 => Interop.snd_pcm_hw_params_set_format(pcm, @params, snd_pcm_format_t.SND_PCM_FORMAT_S24_LE),
                _ => throw new Exception("Bits per sample error. Please reset the value of RecordingBitsPerSample."),
            };
            ThrowIfError("Can not set format.");

            _errorNum = Interop.snd_pcm_hw_params_set_channels(pcm, @params, header.NumChannels);
            ThrowIfError("Can not set channel.");

            uint val = header.SampleRate;
            fixed (int* dirP = &dir)
            {
                _errorNum = Interop.snd_pcm_hw_params_set_rate_near(pcm, @params, &val, dirP);
                ThrowIfError("Can not set rate.");
            }

            _errorNum = Interop.snd_pcm_hw_params(pcm, @params);
            ThrowIfError("Can not set hardware parameters.");
        }

        private unsafe void SetPlaybackVolume(long volume)
        {
            OpenMixer();

            // The snd_mixer_selem_set_playback_volume_all method in Raspberry Pi is invalid.
            // So here we adjust the volume by setting the left and right channels separately.
            _errorNum = Interop.snd_mixer_selem_set_playback_volume(_elem, snd_mixer_selem_channel_id.SND_MIXER_SCHN_FRONT_LEFT, volume);
            _errorNum = Interop.snd_mixer_selem_set_playback_volume(_elem, snd_mixer_selem_channel_id.SND_MIXER_SCHN_FRONT_RIGHT, volume);
            ThrowIfError("Set playback volume error.");

            CloseMixer();
        }

        private unsafe long GetPlaybackVolume()
        {
            long volumeLeft, volumeRight;

            OpenMixer();

            _errorNum = Interop.snd_mixer_selem_get_playback_volume(_elem, snd_mixer_selem_channel_id.SND_MIXER_SCHN_FRONT_LEFT, &volumeLeft);
            _errorNum = Interop.snd_mixer_selem_get_playback_volume(_elem, snd_mixer_selem_channel_id.SND_MIXER_SCHN_FRONT_RIGHT, &volumeRight);
            ThrowIfError("Get playback volume error.");

            CloseMixer();

            return (volumeLeft + volumeRight) / 2;
        }

        private unsafe void SetRecordingVolume(long volume)
        {
            OpenMixer();

            _errorNum = Interop.snd_mixer_selem_set_capture_volume(_elem, snd_mixer_selem_channel_id.SND_MIXER_SCHN_FRONT_LEFT, volume);
            _errorNum = Interop.snd_mixer_selem_set_capture_volume(_elem, snd_mixer_selem_channel_id.SND_MIXER_SCHN_FRONT_RIGHT, volume);
            ThrowIfError("Set recording volume error.");

            CloseMixer();
        }

        private unsafe long GetRecordingVolume()
        {
            long volumeLeft, volumeRight;

            OpenMixer();

            _errorNum = Interop.snd_mixer_selem_get_capture_volume(_elem, snd_mixer_selem_channel_id.SND_MIXER_SCHN_FRONT_LEFT, &volumeLeft);
            _errorNum = Interop.snd_mixer_selem_get_capture_volume(_elem, snd_mixer_selem_channel_id.SND_MIXER_SCHN_FRONT_RIGHT, &volumeRight);
            ThrowIfError("Get recording volume error.");

            CloseMixer();

            return (volumeLeft + volumeRight) / 2;
        }

        private void SetPlaybackMute(bool isMute)
        {
            OpenMixer();

            _errorNum = Interop.snd_mixer_selem_set_playback_switch_all(_elem, isMute ? 0 : 1);
            ThrowIfError("Set playback mute error.");

            CloseMixer();
        }

        private void SetRecordingMute(bool isMute)
        {
            OpenMixer();

            _errorNum = Interop.snd_mixer_selem_set_playback_switch_all(_elem, isMute ? 0 : 1);
            ThrowIfError("Set recording mute error.");

            CloseMixer();
        }

        private void OpenPlaybackPcm()
        {
            if (_playbackPcm != default)
            {
                return;
            }

            lock (s_playbackInitializationLock)
            {
                _errorNum = Interop.snd_pcm_open(out _playbackPcm, Settings.PlaybackDeviceName, snd_pcm_stream_t.SND_PCM_STREAM_PLAYBACK, 0);
                ThrowIfError("Can not open playback device.");
            }
        }

        private void ClosePlaybackPcm()
        {
            if (_playbackPcm != default)
            {
                _errorNum = Interop.snd_pcm_drain(_playbackPcm);
                ThrowIfError("Drop playback device error.");

                _errorNum = Interop.snd_pcm_close(_playbackPcm);
                ThrowIfError("Close playback device error.");

                _playbackPcm = default;
            }
        }

        private void OpenRecordingPcm()
        {
            if (_recordingPcm != default)
            {
                return;
            }

            lock (s_recordingInitializationLock)
            {
                _errorNum = Interop.snd_pcm_open(out _recordingPcm, Settings.RecordingDeviceName, snd_pcm_stream_t.SND_PCM_STREAM_CAPTURE, 0);
                ThrowIfError("Can not open recording device.");
            }
        }

        private void CloseRecordingPcm()
        {
            if (_recordingPcm != default)
            {
                _errorNum = Interop.snd_pcm_drain(_recordingPcm);
                ThrowIfError("Drop recording device error.");

                _errorNum = Interop.snd_pcm_close(_recordingPcm);
                ThrowIfError("Close recording device error.");

                _recordingPcm = default;
            }
        }

        private void OpenMixer()
        {
            if (_mixer != default)
            {
                return;
            }

            lock (s_mixerInitializationLock)
            {
                _errorNum = Interop.snd_mixer_open(out _mixer, 0);
                ThrowIfError("Can not open sound device mixer.");

                _errorNum = Interop.snd_mixer_attach(_mixer, Settings.MixerDeviceName);
                ThrowIfError("Can not attach sound device mixer.");

                _errorNum = Interop.snd_mixer_selem_register(_mixer, IntPtr.Zero, IntPtr.Zero);
                ThrowIfError("Can not register sound device mixer.");

                _errorNum = Interop.snd_mixer_load(_mixer);
                ThrowIfError("Can not load sound device mixer.");

                _elem = Interop.snd_mixer_first_elem(_mixer);
            }
        }

        private void CloseMixer()
        {
            if (_mixer != default)
            {
                _errorNum = Interop.snd_mixer_close(_mixer);
                ThrowIfError("Close sound device mixer error.");

                _mixer = default;
                _elem = default;
            }
        }

        protected override void Dispose(bool disposing)
        {
            ClosePlaybackPcm();
            CloseRecordingPcm();
            CloseMixer();

            base.Dispose(disposing);
        }

        private void ThrowIfError(string message)
        {
            if (_errorNum < 0)
            {
                int code = _errorNum;
                string? errorMsg = Marshal.PtrToStringAnsi(Interop.snd_strerror(_errorNum));

                Dispose();
                throw new Exception($"{message}\nError {code}. {errorMsg}.");
            }
        }
    }
}
