// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.InteropServices;

internal partial class Interop
{
    [DllImport(AlsaLibrary)]
    internal static extern IntPtr snd_strerror(int errnum);

    // <param name="pcm">snd_pcm_t**</param>
    [DllImport(AlsaLibrary)]
    internal static extern int snd_pcm_open(ref IntPtr pcm, string name, snd_pcm_stream_t stream, int mode);

    [DllImport(AlsaLibrary)]
    internal static extern int snd_pcm_start(IntPtr pcm);

    [DllImport(AlsaLibrary)]
    internal static extern int snd_pcm_pause(IntPtr pcm, int enable);

    [DllImport(AlsaLibrary)]
    internal static extern int snd_pcm_resume(IntPtr pcm);

    [DllImport(AlsaLibrary)]
    internal static extern int snd_pcm_drain(IntPtr pcm);

    [DllImport(AlsaLibrary)]
    internal static extern int snd_pcm_drop(IntPtr pcm);

    [DllImport(AlsaLibrary)]
    internal static extern int snd_pcm_close(IntPtr pcm);

    [DllImport(AlsaLibrary)]
    internal static extern int snd_pcm_recover(IntPtr pcm, int err, int silent);

    [DllImport(AlsaLibrary)]
    internal static extern int snd_pcm_writei(IntPtr pcm, IntPtr buffer, ulong size);

    [DllImport(AlsaLibrary)]
    internal static extern int snd_pcm_readi(IntPtr pcm, IntPtr buffer, ulong size);

    [DllImport(AlsaLibrary)]
    internal static extern int snd_pcm_set_params(IntPtr pcm, snd_pcm_format_t format, snd_pcm_access_t access, uint channels, uint rate, int soft_resample, uint latency);

    // <param name="params">snd_pcm_hw_params_t**</param>
    [DllImport(AlsaLibrary)]
    internal static extern int snd_pcm_hw_params_malloc(ref IntPtr @params);

    [DllImport(AlsaLibrary)]
    internal static extern int snd_pcm_hw_params_any(IntPtr pcm, IntPtr @params);

    [DllImport(AlsaLibrary)]
    internal static extern int snd_pcm_hw_params_set_access(IntPtr pcm, IntPtr @params, snd_pcm_access_t access);

    [DllImport(AlsaLibrary)]
    internal static extern int snd_pcm_hw_params_set_format(IntPtr pcm, IntPtr @params, snd_pcm_format_t val);

    // <param name="val">1 for mono and 2 for stereo</param>
    [DllImport(AlsaLibrary)]
    internal static extern int snd_pcm_hw_params_set_channels(IntPtr pcm, IntPtr @params, uint val);

    [DllImport(AlsaLibrary)]
    internal unsafe static extern int snd_pcm_hw_params_set_rate_near(IntPtr pcm, IntPtr @params, uint* val, int* dir);

    [DllImport(AlsaLibrary)]
    internal static extern int snd_pcm_hw_params(IntPtr pcm, IntPtr @params);

    // <param name="frames">snd_pcm_uframes_t*</param>
    [DllImport(AlsaLibrary)]
    internal unsafe static extern int snd_pcm_hw_params_get_period_size(IntPtr @params, ulong* frames, int* dir);

    [DllImport(AlsaLibrary)]
    internal unsafe static extern int snd_pcm_hw_params_set_period_size_near(IntPtr pcm, IntPtr @params, ulong* frames, int* dir);

    // <param name="mixer">snd_mixer_t**</param>
    [DllImport(AlsaLibrary)]
    internal static extern int snd_mixer_open(ref IntPtr mixer, int mode);

    [DllImport(AlsaLibrary)]
    internal static extern int snd_mixer_close(IntPtr mixer);

    [DllImport(AlsaLibrary)]
    internal static extern int snd_mixer_attach(IntPtr mixer, string name);

    [DllImport(AlsaLibrary)]
    internal static extern int snd_mixer_load(IntPtr mixer);

    [DllImport(AlsaLibrary)]
    internal static extern int snd_mixer_selem_register(IntPtr mixer, IntPtr options, IntPtr classp);

    // <returns>snd_mixer_elem_t*</returns>
    [DllImport(AlsaLibrary)]
    internal static extern IntPtr snd_mixer_first_elem(IntPtr mixer);

    [DllImport(AlsaLibrary)]
    internal static extern IntPtr snd_mixer_elem_next(IntPtr elem);

    // <param name="elem">snd_mixer_elem_t*</param>
    [DllImport(AlsaLibrary)]
    internal static extern string snd_mixer_selem_get_name(IntPtr elem);

    [DllImport(AlsaLibrary)]
    internal static extern void snd_mixer_selem_id_alloca(IntPtr ptr);

    // <param name="mixer">snd_mixer_elem_t*</param>
    [DllImport(AlsaLibrary)]
    internal unsafe static extern int snd_mixer_selem_get_playback_volume(IntPtr elem, snd_mixer_selem_channel_id channel, long* value);

    [DllImport(AlsaLibrary)]
    internal static extern int snd_mixer_selem_set_playback_volume(IntPtr elem, snd_mixer_selem_channel_id channel, long value);

    [DllImport(AlsaLibrary)]
    internal static extern int snd_mixer_selem_set_playback_volume_all(IntPtr elem, long value);

    [DllImport(AlsaLibrary)]
    internal static extern int snd_mixer_selem_set_playback_switch_all(IntPtr elem, int value);

    [DllImport(AlsaLibrary)]
    internal unsafe static extern int snd_mixer_selem_get_playback_volume_range(IntPtr elem, long* min, long* max);

    [DllImport(AlsaLibrary)]
    internal static extern int snd_mixer_selem_set_playback_volume_range(IntPtr elem, long min, long max);

    [DllImport(AlsaLibrary)]
    internal unsafe static extern int snd_mixer_selem_get_capture_volume(IntPtr elem, snd_mixer_selem_channel_id channel, long* value);

    [DllImport(AlsaLibrary)]
    internal static extern int snd_mixer_selem_set_capture_volume(IntPtr elem, snd_mixer_selem_channel_id channel, long value);

    [DllImport(AlsaLibrary)]
    internal static extern int snd_mixer_selem_set_capture_volume_all(IntPtr elem, long value);

    [DllImport(AlsaLibrary)]
    internal static extern int snd_mixer_selem_set_capture_switch_all(IntPtr elem, int value);

    [DllImport(AlsaLibrary)]
    internal unsafe static extern int snd_mixer_selem_get_capture_volume_range(IntPtr elem, long* min, long* max);

    [DllImport(AlsaLibrary)]
    internal static extern int snd_mixer_selem_set_capture_volume_range(IntPtr elem, long min, long max);
}
