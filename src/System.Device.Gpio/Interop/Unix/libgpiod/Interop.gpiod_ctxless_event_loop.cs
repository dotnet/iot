// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;
using System.Device.Gpio;
using System.Runtime.InteropServices;

internal partial class Interop
{
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
