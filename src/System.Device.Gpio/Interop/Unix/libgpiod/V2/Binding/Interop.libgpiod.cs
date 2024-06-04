// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Disable these StyleCop rules for this file, as we are using native names here.
#pragma warning disable SA1300 // Element should begin with upper-case letter

using System;
using System.Device.Gpio.Libgpiod.V2;
using System.Runtime.InteropServices;

internal partial class Interop
{
    /// <summary>
    /// Binding for libgpiod V2 <see href="https://libgpiod.readthedocs.io/en/latest/modules.html"/>
    /// </summary>
    internal static class LibgpiodV2
    {
        private const string LibgpiodLibrary = "libgpiod.so.3";

        #region Gpio chips

        /// <remarks>IntPtr is used instead of string so the CLR does not free native memory.</remarks>
        [DllImport(LibgpiodLibrary, SetLastError = true, CharSet = CharSet.Auto)]
        public static extern ChipSafeHandle gpiod_chip_open(IntPtr path);

        /// <remarks>IntPtr is used to avoid ObjectDisposed exceptions.</remarks>
        [DllImport(LibgpiodLibrary)]
        public static extern void gpiod_chip_close(IntPtr chip);

        [DllImport(LibgpiodLibrary, SetLastError = true)]
        public static extern ChipInfoSafeHandle gpiod_chip_get_info(ChipSafeHandle chip);

        /// <remarks>IntPtr is used instead of string so the CLR does not free native memory.</remarks>
        [DllImport(LibgpiodLibrary, SetLastError = true)]
        public static extern IntPtr gpiod_chip_get_path(ChipSafeHandle chip);

        [DllImport(LibgpiodLibrary, SetLastError = true)]
        public static extern LineInfoSafeHandle gpiod_chip_get_line_info(ChipSafeHandle chip, uint offset);

        [DllImport(LibgpiodLibrary, SetLastError = true)]
        public static extern LineInfoSafeHandle gpiod_chip_watch_line_info(ChipSafeHandle chip, uint offset);

        [DllImport(LibgpiodLibrary, SetLastError = true)]
        public static extern int gpiod_chip_unwatch_line_info(ChipSafeHandle chip, uint offset);

        [DllImport(LibgpiodLibrary, SetLastError = true)]
        public static extern int gpiod_chip_get_fd(ChipSafeHandle chip);

        [DllImport(LibgpiodLibrary, SetLastError = true)]
        public static extern int gpiod_chip_wait_info_event(ChipSafeHandle chip, long timeout_ns);

        [DllImport(LibgpiodLibrary, SetLastError = true)]
        public static extern LineInfoEventSafeHandle gpiod_chip_read_info_event(ChipSafeHandle chip);

        [DllImport(LibgpiodLibrary, SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int gpiod_chip_get_line_offset_from_name(ChipSafeHandle chip, string name);

        [DllImport(LibgpiodLibrary, SetLastError = true)]
        public static extern LineRequestSafeHandle gpiod_chip_request_lines(ChipSafeHandle chip, RequestConfigSafeHandle req_cfg,
            LineConfigSafeHandle line_cfg);

        #endregion

        #region Chip info

        /// <remarks>IntPtr is used to avoid ObjectDisposed exceptions.</remarks>
        [DllImport(LibgpiodLibrary)]
        public static extern void gpiod_chip_info_free(IntPtr info);

        /// <remarks>IntPtr is used instead of string so the CLR does not free native memory.</remarks>
        [DllImport(LibgpiodLibrary, SetLastError = true)]
        public static extern IntPtr gpiod_chip_info_get_name(ChipInfoSafeHandle info);

        /// <remarks>IntPtr is used instead of string so the CLR does not free native memory.</remarks>
        [DllImport(LibgpiodLibrary, SetLastError = true)]
        public static extern IntPtr gpiod_chip_info_get_label(ChipInfoSafeHandle info);

        [DllImport(LibgpiodLibrary, SetLastError = true)]
        public static extern int gpiod_chip_info_get_num_lines(ChipInfoSafeHandle info);

        #endregion

        #region Line info

        /// <remarks>IntPtr is used to avoid ObjectDisposed exceptions.</remarks>
        [DllImport(LibgpiodLibrary)]
        public static extern void gpiod_line_info_free(IntPtr info);

        [DllImport(LibgpiodLibrary)]
        public static extern LineInfoSafeHandle gpiod_line_info_copy(LineInfoSafeHandle info);

        [DllImport(LibgpiodLibrary, SetLastError = true)]
        public static extern uint gpiod_line_info_get_offset(LineInfoSafeHandle info);

        /// <remarks>IntPtr is used instead of string so the CLR does not free native memory.</remarks>
        [DllImport(LibgpiodLibrary, SetLastError = true)]
        public static extern IntPtr gpiod_line_info_get_name(LineInfoSafeHandle info);

        [DllImport(LibgpiodLibrary, SetLastError = true)]
        public static extern bool gpiod_line_info_is_used(LineInfoSafeHandle info);

        /// <remarks>IntPtr is used instead of string so the CLR does not free native memory.</remarks>
        [DllImport(LibgpiodLibrary, SetLastError = true)]
        public static extern IntPtr gpiod_line_info_get_consumer(LineInfoSafeHandle info);

        [DllImport(LibgpiodLibrary, SetLastError = true)]
        public static extern GpiodLineDirection gpiod_line_info_get_direction(LineInfoSafeHandle info);

        [DllImport(LibgpiodLibrary, SetLastError = true)]
        public static extern GpiodLineEdge gpiod_line_info_get_edge_detection(LineInfoSafeHandle info);

        [DllImport(LibgpiodLibrary, SetLastError = true)]
        public static extern GpiodLineBias gpiod_line_info_get_bias(LineInfoSafeHandle info);

        [DllImport(LibgpiodLibrary, SetLastError = true)]
        public static extern GpiodLineDrive gpiod_line_info_get_drive(LineInfoSafeHandle info);

        [DllImport(LibgpiodLibrary, SetLastError = true)]
        public static extern bool gpiod_line_info_is_active_low(LineInfoSafeHandle info);

        [DllImport(LibgpiodLibrary, SetLastError = true)]
        public static extern bool gpiod_line_info_is_debounced(LineInfoSafeHandle info);

        [DllImport(LibgpiodLibrary, SetLastError = true)]
        public static extern ulong gpiod_line_info_get_debounce_period_us(LineInfoSafeHandle info);

        [DllImport(LibgpiodLibrary, SetLastError = true)]
        public static extern GpiodLineClock gpiod_line_info_get_event_clock(LineInfoSafeHandle info);

        #endregion

        #region Line status watch events

        /// <remarks>IntPtr is used to avoid ObjectDisposed exceptions.</remarks>
        [DllImport(LibgpiodLibrary)]
        public static extern void gpiod_info_event_free(IntPtr @event);

        [DllImport(LibgpiodLibrary, SetLastError = true)]
        public static extern GpiodLineInfoEventType gpiod_info_event_get_event_type(LineInfoEventSafeHandle @event);

        [DllImport(LibgpiodLibrary, SetLastError = true)]
        public static extern ulong gpiod_info_event_get_timestamp_ns(LineInfoEventSafeHandle @event);

        [DllImport(LibgpiodLibrary, SetLastError = true)]
        public static extern LineInfoSafeHandleNotFreeable gpiod_info_event_get_line_info(LineInfoEventSafeHandle @event);

        #endregion

        #region Line settings objects

        [DllImport(LibgpiodLibrary, SetLastError = true)]
        public static extern LineSettingsSafeHandle gpiod_line_settings_new();

        /// <remarks>IntPtr is used to avoid ObjectDisposed exceptions.</remarks>
        [DllImport(LibgpiodLibrary)]
        public static extern void gpiod_line_settings_free(IntPtr settings);

        [DllImport(LibgpiodLibrary)]
        public static extern void gpiod_line_settings_reset(LineSettingsSafeHandle settings);

        [DllImport(LibgpiodLibrary)]
        public static extern LineSettingsSafeHandle gpiod_line_settings_copy(LineSettingsSafeHandle settings);

        [DllImport(LibgpiodLibrary, SetLastError = true)]
        public static extern int gpiod_line_settings_set_direction(LineSettingsSafeHandle settings, GpiodLineDirection direction);

        [DllImport(LibgpiodLibrary, SetLastError = true)]
        public static extern GpiodLineDirection gpiod_line_settings_get_direction(LineSettingsSafeHandle settings);

        [DllImport(LibgpiodLibrary, SetLastError = true)]
        public static extern int gpiod_line_settings_set_edge_detection(LineSettingsSafeHandle settings, GpiodLineEdge edge);

        [DllImport(LibgpiodLibrary, SetLastError = true)]
        public static extern GpiodLineEdge gpiod_line_settings_get_edge_detection(LineSettingsSafeHandle settings);

        [DllImport(LibgpiodLibrary, SetLastError = true)]
        public static extern int gpiod_line_settings_set_bias(LineSettingsSafeHandle settings, GpiodLineBias bias);

        [DllImport(LibgpiodLibrary, SetLastError = true)]
        public static extern GpiodLineBias gpiod_line_settings_get_bias(LineSettingsSafeHandle settings);

        [DllImport(LibgpiodLibrary, SetLastError = true)]
        public static extern int gpiod_line_settings_set_drive(LineSettingsSafeHandle settings, GpiodLineDrive drive);

        [DllImport(LibgpiodLibrary, SetLastError = true)]
        public static extern GpiodLineDrive gpiod_line_settings_get_drive(LineSettingsSafeHandle settings);

        [DllImport(LibgpiodLibrary, SetLastError = true)]
        public static extern int gpiod_line_settings_set_active_low(LineSettingsSafeHandle settings, bool active_low);

        [DllImport(LibgpiodLibrary, SetLastError = true)]
        public static extern bool gpiod_line_settings_get_active_low(LineSettingsSafeHandle settings);

        [DllImport(LibgpiodLibrary, SetLastError = true)]
        public static extern void gpiod_line_settings_set_debounce_period_us(LineSettingsSafeHandle settings, ulong period);

        [DllImport(LibgpiodLibrary, SetLastError = true)]
        public static extern ulong gpiod_line_settings_get_debounce_period_us(LineSettingsSafeHandle settings);

        [DllImport(LibgpiodLibrary, SetLastError = true)]
        public static extern int gpiod_line_settings_set_event_clock(LineSettingsSafeHandle settings, GpiodLineClock event_clock);

        [DllImport(LibgpiodLibrary, SetLastError = true)]
        public static extern GpiodLineClock gpiod_line_settings_get_event_clock(LineSettingsSafeHandle settings);

        [DllImport(LibgpiodLibrary, SetLastError = true)]
        public static extern int gpiod_line_settings_set_output_value(LineSettingsSafeHandle settings, GpiodLineValue value);

        [DllImport(LibgpiodLibrary, SetLastError = true)]
        public static extern GpiodLineValue gpiod_line_settings_get_output_value(LineSettingsSafeHandle settings);

        #endregion

        #region Line configuration objects

        [DllImport(LibgpiodLibrary, SetLastError = true)]
        public static extern LineConfigSafeHandle gpiod_line_config_new();

        /// <remarks>IntPtr is used to avoid ObjectDisposed exceptions.</remarks>
        [DllImport(LibgpiodLibrary)]
        public static extern void gpiod_line_config_free(IntPtr config);

        [DllImport(LibgpiodLibrary)]
        public static extern void gpiod_line_config_reset(LineConfigSafeHandle config);

        [DllImport(LibgpiodLibrary)]
        public static extern void gpiod_line_config_add_line_settings(LineConfigSafeHandle config, uint[] offsets, int num_offsets,
            LineSettingsSafeHandle settings);

        [DllImport(LibgpiodLibrary, SetLastError = true)]
        public static extern LineSettingsSafeHandle gpiod_line_config_get_line_settings(LineConfigSafeHandle config, uint offset);

        [DllImport(LibgpiodLibrary, SetLastError = true)]
        public static extern int gpiod_line_config_set_output_values(LineConfigSafeHandle config, GpiodLineValue[] values, int num_values);

        [DllImport(LibgpiodLibrary, SetLastError = true)]
        public static extern int gpiod_line_config_get_num_configured_offsets(LineConfigSafeHandle config);

        [DllImport(LibgpiodLibrary, SetLastError = true)]
        public static extern int gpiod_line_config_get_configured_offsets(LineConfigSafeHandle config, uint[] offsets, int max_offsets);

        #endregion

        #region Request configuration objects

        [DllImport(LibgpiodLibrary, SetLastError = true)]
        public static extern RequestConfigSafeHandle gpiod_request_config_new();

        /// <remarks>IntPtr is used to avoid ObjectDisposed exceptions.</remarks>
        [DllImport(LibgpiodLibrary)]
        public static extern void gpiod_request_config_free(IntPtr config);

        [DllImport(LibgpiodLibrary, CharSet = CharSet.Auto)]
        public static extern void gpiod_request_config_set_consumer(RequestConfigSafeHandle config, IntPtr consumer);

        [DllImport(LibgpiodLibrary, SetLastError = true)]
        public static extern IntPtr gpiod_request_config_get_consumer(RequestConfigSafeHandle config);

        [DllImport(LibgpiodLibrary)]
        public static extern void gpiod_request_config_set_event_buffer_size(RequestConfigSafeHandle config, int event_buffer_size);

        [DllImport(LibgpiodLibrary, SetLastError = true)]
        public static extern int gpiod_request_config_get_event_buffer_size(RequestConfigSafeHandle config);

        #endregion

        #region Line request operations

        /// <remarks>IntPtr is used to avoid ObjectDisposed exceptions.</remarks>
        [DllImport(LibgpiodLibrary)]
        public static extern void gpiod_line_request_release(IntPtr request);

        [DllImport(LibgpiodLibrary, SetLastError = true)]
        public static extern int gpiod_line_request_get_num_requested_lines(LineRequestSafeHandle request);

        [DllImport(LibgpiodLibrary, SetLastError = true)]
        public static extern int gpiod_line_request_get_requested_offsets(LineRequestSafeHandle request, uint[] offsets, int max_offsets);

        [DllImport(LibgpiodLibrary, SetLastError = true)]
        public static extern GpiodLineValue gpiod_line_request_get_value(LineRequestSafeHandle request, uint offset);

        [DllImport(LibgpiodLibrary, SetLastError = true)]
        public static extern int gpiod_line_request_get_values_subset(LineRequestSafeHandle request, int num_values, uint[] offsets,
            GpiodLineValue[] values);

        [DllImport(LibgpiodLibrary, SetLastError = true)]
        public static extern int gpiod_line_request_get_values(LineRequestSafeHandle request, GpiodLineValue[] values);

        [DllImport(LibgpiodLibrary, SetLastError = true)]
        public static extern int gpiod_line_request_set_value(LineRequestSafeHandle request, uint offset, GpiodLineValue value);

        [DllImport(LibgpiodLibrary, SetLastError = true)]
        public static extern int gpiod_line_request_set_values_subset(LineRequestSafeHandle request, int num_values, uint[] offsets,
            GpiodLineValue[] values);

        [DllImport(LibgpiodLibrary, SetLastError = true)]
        public static extern int gpiod_line_request_set_values(LineRequestSafeHandle request, GpiodLineValue[] values);

        [DllImport(LibgpiodLibrary, SetLastError = true)]
        public static extern int gpiod_line_request_reconfigure_lines(LineRequestSafeHandle request, LineConfigSafeHandle config);

        [DllImport(LibgpiodLibrary, SetLastError = true)]
        public static extern int gpiod_line_request_get_fd(LineRequestSafeHandle request);

        [DllImport(LibgpiodLibrary, SetLastError = true)]
        public static extern int gpiod_line_request_wait_edge_events(LineRequestSafeHandle request, long timeout_ns);

        [DllImport(LibgpiodLibrary, SetLastError = true)]
        public static extern int gpiod_line_request_read_edge_events(LineRequestSafeHandle request, EdgeEventBufferSafeHandle buffer, int max_events);

        #endregion

        #region Line edge events handling

        /// <remarks>IntPtr is used to avoid ObjectDisposed exceptions.</remarks>
        [DllImport(LibgpiodLibrary)]
        public static extern void gpiod_edge_event_free(IntPtr @event);

        [DllImport(LibgpiodLibrary)]
        public static extern EdgeEventSafeHandle gpiod_edge_event_copy(EdgeEventSafeHandle @event);

        [DllImport(LibgpiodLibrary, SetLastError = true)]
        public static extern GpiodEdgeEventType gpiod_edge_event_get_event_type(EdgeEventSafeHandle @event);

        [DllImport(LibgpiodLibrary, SetLastError = true)]
        public static extern ulong gpiod_edge_event_get_timestamp_ns(EdgeEventSafeHandle @event);

        [DllImport(LibgpiodLibrary, SetLastError = true)]
        public static extern uint gpiod_edge_event_get_line_offset(EdgeEventSafeHandle @event);

        [DllImport(LibgpiodLibrary, SetLastError = true)]
        public static extern ulong gpiod_edge_event_get_global_seqno(EdgeEventSafeHandle @event);

        [DllImport(LibgpiodLibrary, SetLastError = true)]
        public static extern ulong gpiod_edge_event_get_line_seqno(EdgeEventSafeHandle @event);

        [DllImport(LibgpiodLibrary, SetLastError = true)]
        public static extern EdgeEventBufferSafeHandle gpiod_edge_event_buffer_new(int capacity);

        [DllImport(LibgpiodLibrary, SetLastError = true)]
        public static extern int gpiod_edge_event_buffer_get_capacity(EdgeEventBufferSafeHandle buffer);

        /// <remarks>IntPtr is used to avoid ObjectDisposed exceptions.</remarks>
        [DllImport(LibgpiodLibrary)]
        public static extern void gpiod_edge_event_buffer_free(IntPtr buffer);

        [DllImport(LibgpiodLibrary, SetLastError = true)]
        public static extern EdgeEventNotFreeable gpiod_edge_event_buffer_get_event(EdgeEventBufferSafeHandle buffer, ulong index);

        [DllImport(LibgpiodLibrary, SetLastError = true)]
        public static extern int gpiod_edge_event_buffer_get_num_events(EdgeEventBufferSafeHandle buffer);

        #endregion

        #region Stuff that didn't fit anywhere else

        [DllImport(LibgpiodLibrary, CharSet = CharSet.Auto)]
        public static extern bool gpiod_is_gpiochip_device(IntPtr path);

        [DllImport(LibgpiodLibrary, SetLastError = true)]
        public static extern IntPtr gpiod_api_version();

        #endregion
    }
}
