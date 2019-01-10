// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;
using System.Device.Gpio;
using System.Runtime.InteropServices;

internal partial class Interop
{
    private const string LibgpiodLibrary = "libgpiod";

    /// <summary>
    ///     Create a new gpiochip iterator.
    ///     <see href="https://git.kernel.org/pub/scm/libs/libgpiod/libgpiod.git/tree/include/gpiod.h#n1275">here</see>.
    /// </summary>
    /// <returns>Pointer to a new chip iterator object or NULL if an error occurred.</returns>
    [DllImport(LibgpiodLibrary, SetLastError = true)]
    internal static extern SafeChipIteratorHandle gpiod_chip_iter_new();

    /// <summary>
    ///     Release all resources allocated for the gpiochip iterator and close the most recently opened gpiochip(if any).
    ///     <see href="https://git.kernel.org/pub/scm/libs/libgpiod/libgpiod.git/tree/include/gpiod.h#n1282">here</see>.
    /// </summary>
    /// <param name="iter">The gpiochip iterator object</param>
    [DllImport(LibgpiodLibrary)]
    internal static extern void gpiod_chip_iter_free(IntPtr iter);

    /// <summary>
    ///     Release all resources allocated for the gpiochip iterator but don't close the most recently opened gpiochip (if any).
    ///     <see href="https://git.kernel.org/pub/scm/libs/libgpiod/libgpiod.git/tree/include/gpiod.h#n1293">here</see>.
    /// </summary>
    /// <param name="iter">The gpiochip iterator object</param>
    [DllImport(LibgpiodLibrary)]
    internal static extern void gpiod_chip_iter_free_noclose(SafeChipIteratorHandle iter);

    /// <summary>
    ///     Get the next gpiochip handle.
    ///     <see href="https://git.kernel.org/pub/scm/libs/libgpiod/libgpiod.git/tree/include/gpiod.h#n1303">here</see>.
    /// </summary>
    /// <param name="iter">The gpiochip iterator object</param>
    [DllImport(LibgpiodLibrary)]
    internal static extern SafeChipHandle gpiod_chip_iter_next(SafeChipIteratorHandle iter);

    /// <summary>
    ///     Get the number of GPIO lines exposed by this chip.
    ///     <see href="https://git.kernel.org/pub/scm/libs/libgpiod/libgpiod.git/tree/include/gpiod.h#n464">here</see>.
    /// </summary>
    /// <param name="chip">The GPIO chip handle.</param>
    /// <returns>Number of GPIO lines.</returns>
    [DllImport(LibgpiodLibrary)]
    internal static extern IntPtr gpiod_chip_name(SafeChipHandle chip);

    /// <summary>
    ///     Close a GPIO chip handle and release all allocated resources.
    ///     <see href="https://git.kernel.org/pub/scm/libs/libgpiod/libgpiod.git/tree/include/gpiod.h#n443">here</see>.
    /// </summary>
    /// <param name="chip">The GPIO chip pointer</param>
    [DllImport(LibgpiodLibrary)]
    internal static extern void gpiod_chip_close(IntPtr chip);

    /// <summary>
    ///     Get the number of GPIO lines exposed by this chip.
    ///     <see href="https://git.kernel.org/pub/scm/libs/libgpiod/libgpiod.git/tree/include/gpiod.h#n464">here</see>.
    /// </summary>
    /// <param name="chip">The GPIO chip handle.</param>
    /// <returns>Number of GPIO lines.</returns>
    [DllImport(LibgpiodLibrary)]
    internal static extern int gpiod_chip_num_lines(SafeChipHandle chip);

    /// <summary>
    ///     Get the handle to the GPIO line at given offset.
    ///     <see href="https://git.kernel.org/pub/scm/libs/libgpiod/libgpiod.git/tree/include/gpiod.h#n473">here</see>.
    /// </summary>
    /// <param name="chip">The GPIO chip handle</param>
    /// <param name="offset">The offset of the GPIO line</param>
    /// <returns>Handle to the GPIO line or NULL if an error occured.</returns>
    [DllImport(LibgpiodLibrary, SetLastError = true)]
    internal static extern SafeLineHandle gpiod_chip_get_line(SafeChipHandle chip, int offset);

    /// <summary>
    ///     Read the GPIO line direction setting.
    ///     <see href="https://git.kernel.org/pub/scm/libs/libgpiod/libgpiod.git/tree/include/gpiod.h#n691">here</see>.
    /// </summary>
    /// <param name="line">GPIO line handle</param>
    /// <returns>GPIOD_DIRECTION_INPUT or GPIOD_DIRECTION_OUTPUT.</returns>
    [DllImport(LibgpiodLibrary)]
    internal static extern int gpiod_line_direction(SafeLineHandle line);

    /// <summary>
    ///     Reserve a single line, set the direction to input.
    ///     <see href="https://git.kernel.org/pub/scm/libs/libgpiod/libgpiod.git/tree/include/gpiod.h#n823">here</see>.
    /// </summary>
    /// <param name="line">GPIO line handle</param>
    /// <param name="consumer">Name of the consumer.</param>
    /// <returns>0 if the line was properly reserved, -1 on failure.</returns>
    [DllImport(LibgpiodLibrary)]
    internal static extern int gpiod_line_request_input(SafeLineHandle line, string consumer);

    /// <summary>
    ///     Reserve a single line, set the direction to output.
    ///     <see href="https://git.kernel.org/pub/scm/libs/libgpiod/libgpiod.git/tree/include/gpiod.h#n833">here</see>.
    /// </summary>
    /// <param name="line">GPIO line handle</param>
    /// <param name="consumer">Name of the consumer.</param>
    /// <param name="default_val">Initial line value</param>
    /// <returns>0 if the line was properly reserved, -1 on failure.</returns>
    [DllImport(LibgpiodLibrary)]
    internal static extern int gpiod_line_request_output(SafeLineHandle line, string consumer, int default_val);

    /// <summary>
    ///     Set the value of a single GPIO line.
    ///     <see href="https://git.kernel.org/pub/scm/libs/libgpiod/libgpiod.git/tree/include/gpiod.h#n1107">here</see>.
    /// </summary>
    /// <param name="line">GPIO line handle</param>
    /// <param name="value">New value.</param>
    /// <returns>0 if the operation succeeds. In case of an error this routine returns -1 and sets the last error number.</returns>
    [DllImport(LibgpiodLibrary)]
    internal static extern int gpiod_line_set_value(SafeLineHandle line, int value);

    /// <summary>
    ///     Read current value of a single GPIO line.
    ///     <see href="https://git.kernel.org/pub/scm/libs/libgpiod/libgpiod.git/tree/include/gpiod.h#n1084">here</see>.
    /// </summary>
    /// <param name="line">GPIO line handle</param>
    /// <returns>0 or 1 if the operation succeeds. On error this routine returns -1 and sets the last error number.</returns>
    [DllImport(LibgpiodLibrary, SetLastError = true)]
    internal static extern int gpiod_line_get_value(SafeLineHandle line);

    /// <summary>
    ///     Release a previously reserved line.
    ///     <see href="https://git.kernel.org/pub/scm/libs/libgpiod/libgpiod.git/tree/include/gpiod.h#n1045">here</see>.
    /// </summary>
    /// <param name="line">GPIO line handle</param>
    [DllImport(LibgpiodLibrary)]
    internal static extern void gpiod_line_release(SafeLineHandle lineHandle);

    /// <summary>
    ///     Request all event type notifications on a single line.
    ///     <see href="https://git.kernel.org/pub/scm/libs/libgpiod/libgpiod.git/tree/include/gpiod.h#n860">here</see>.
    /// </summary>
    /// <param name="line">GPIO line handle</param>
    /// <param name="consumer">Name of the consumer.</param>
    /// <returns>0 the operation succeeds, -1 on failure.</returns>
    [DllImport(LibgpiodLibrary, SetLastError = true)]
    internal static extern int gpiod_line_request_both_edges_events(SafeLineHandle line, string consumer);

    /// <summary>
    ///     Wait for an event on a single line.
    ///     <see href="https://git.kernel.org/pub/scm/libs/libgpiod/libgpiod.git/tree/include/gpiod.h#n1156">here</see>.
    /// </summary>
    /// <param name="line">GPIO line handle</param>
    /// <param name="timeout">Wait time limit.</param>
    /// <returns>0 if wait timed out, -1 if an error occurred, 1 if an event occurred.</returns>
    [DllImport(LibgpiodLibrary, SetLastError = true)]
    internal static extern int gpiod_line_event_wait(SafeLineHandle line, ref timespec timeout);

    /// <summary>
    ///     Read the last event from the GPIO line.
    ///     <see href="https://git.kernel.org/pub/scm/libs/libgpiod/libgpiod.git/tree/include/gpiod.h#n1179">here</see>.
    /// </summary>
    /// <param name="line">GPIO line handle</param>
    /// <param name="eventInfo">Buffer to which the event data will be copied.</param>
    /// <returns>0 if the event was read correctly, -1 on error.</returns>
    [DllImport(LibgpiodLibrary, SetLastError = true)]
    internal static extern int gpiod_line_event_read(SafeLineHandle line, out gpiod_line_event eventInfo);

    /// <summary>
    ///     Wait for events on a single GPIO line.
    ///     <see href="https://git.kernel.org/pub/scm/libs/libgpiod/libgpiod.git/tree/include/gpiod.h#n262">here</see>.
    /// </summary>
    /// <param name="device">Name, path, number or label of the gpiochip</param>
    /// <param name="offset">GPIO line offset to monitor</param>
    /// <param name="active_low">The active state of this line - true if low.</param>
    /// <param name="consumer">Name of the consumer.</param>
    /// <param name="timeout">Maximum wait time for each iteration.</param>
    /// <param name="poll_cb">Callback function to call when waiting for events</param>
    /// <param name="event_cb">Callback function to call for each line event.</param>
    /// <param name="data">User data passed to the callback.</param>
    /// <returns>0 if no errors were encountered, -1 if an error occurred</returns>
    [DllImport(LibgpiodLibrary, SetLastError = true)]
    internal static extern int gpiod_ctxless_event_loop(string device, uint offset,
        bool active_low, string consumer, ref timespec timeout,
        [MarshalAs(UnmanagedType.FunctionPtr)]gpiod_ctxless_event_poll_cb poll_cb,
        [MarshalAs(UnmanagedType.FunctionPtr)]gpiod_ctxless_event_handle_cb event_cb, IntPtr data);
}

[StructLayout(LayoutKind.Sequential)]
internal struct gpiod_ctxless_event_poll_fd
{
    private int fd;
    private bool eventOccured;
}

internal delegate int gpiod_ctxless_event_poll_cb(uint number_of_lines, gpiod_ctxless_event_poll_fd fd, ref timespec timeout, IntPtr data);

/// <summary>
///     Simple event callack signature. This callback is called by the ctxless event loop functions for each GPIO event
///     <see href="https://git.kernel.org/pub/scm/libs/libgpiod/libgpiod.git/tree/include/gpiod.h#n203">here</see>.
/// </summary>
/// <param name="eventType">
///     <code>
///         enum {  // Event types that can be passed to the ctxless event callback.
///             GPIOD_CTXLESS_EVENT_CB_TIMEOUT = 1,   // Waiting for events timed out.
///	            GPIOD_CTXLESS_EVENT_CB_RISING_EDGE,   // Rising edge event occured.
///	            GPIOD_CTXLESS_EVENT_CB_FALLING_EDGE,  // Falling edge event occured.
///	       };
///     </code>
/// </param>
/// <param name="lineOffset">GPIO line offset being monitored</param>
/// <param name="timeout">Event timestamp</param>
/// <param name="data">pointer to user data</param>
/// <returns>
///     <code>
///         enum {   //Return status values that the ctxless event callback can return.
///             GPIOD_CTXLESS_EVENT_CB_RET_ERR = -1,  // Stop processing events and indicate an error.
///         	GPIOD_CTXLESS_EVENT_CB_RET_OK = 0,    // Continue processing events.
///     	    GPIOD_CTXLESS_EVENT_CB_RET_STOP = 1,  // Stop processing events.
///         };
///     </code>
/// </returns>
internal delegate int gpiod_ctxless_event_handle_cb(int eventType, uint lineOffset, ref timespec timeout, IntPtr data);

internal struct gpiod_line_event
{
    internal timespec ts;
    internal int event_type;
}

[StructLayout(LayoutKind.Sequential)]
internal struct timespec
{
    internal IntPtr tv_sec;
    internal IntPtr tv_nsec;

    internal timespec(IntPtr seconds, IntPtr nanoSeconds)
    {
        tv_sec = seconds;
        tv_nsec = nanoSeconds;
    }
}