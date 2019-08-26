// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;
using System.Device.Gpio;
using System.Runtime.InteropServices;
// Since this is only used on Linux, and in C on Linux sizeof(long) == sizeof(void*) this is a valid alias.
using NativeLong = System.IntPtr;

internal partial class Interop
{
    internal partial class libgpiod
    {
        private const string LibgpiodLibrary = "libgpiod";
        internal static IntPtr InvalidHandleValue = new IntPtr(-1);

        /// <summary>
        /// Release all resources allocated for the gpiochip iterator and close the most recently opened gpiochip(if any).
        /// </summary>
        /// <param name="iter">The gpiochip iterator object</param>
        [DllImport(LibgpiodLibrary)]
        internal static extern void gpiod_chip_iter_free(IntPtr iter);

        /// <summary>
        /// Close a GPIO chip handle and release all allocated resources.
        /// </summary>
        /// <param name="chip">The GPIO chip pointer</param>
        [DllImport(LibgpiodLibrary)]
        internal static extern void gpiod_chip_close(IntPtr chip);

        /// <summary>
        /// Get the number of GPIO lines exposed by this chip.
        /// </summary>
        /// <param name="chip">The GPIO chip handle.</param>
        /// <returns>Number of GPIO lines.</returns>
        [DllImport(LibgpiodLibrary)]
        internal static extern int gpiod_chip_num_lines(SafeChipHandle chip);

        /// <summary>
        /// Get the handle to the GPIO line at given offset.
        /// </summary>
        /// <param name="chip">The GPIO chip handle</param>
        /// <param name="offset">The offset of the GPIO line</param>
        /// <returns>Handle to the GPIO line or <see langword="null" /> if an error occured.</returns>
        [DllImport(LibgpiodLibrary, SetLastError = true)]
        internal static extern SafeLineHandle gpiod_chip_get_line(SafeChipHandle chip, int offset);

        /// <summary>
        /// Reserve a single line, set the direction to input.
        /// </summary>
        /// <param name="line">GPIO line handle</param>
        /// <param name="consumer">Name of the consumer.</param>
        /// <returns>0 if the line was properly reserved, -1 on failure.</returns>
        [DllImport(LibgpiodLibrary, SetLastError = true)]
        internal static extern int gpiod_line_request_input(SafeLineHandle line, string consumer);

        /// <summary>
        /// Reserve a single line, set the direction to output.
        /// </summary>
        /// <param name="line">GPIO line handle</param>
        /// <param name="consumer">Name of the consumer.</param>
        /// <returns>0 if the line was properly reserved, -1 on failure.</returns>
        [DllImport(LibgpiodLibrary, SetLastError = true)]
        internal static extern int gpiod_line_request_output(SafeLineHandle line, string consumer);

        /// <summary>
        /// Set the value of a single GPIO line.
        /// </summary>
        /// <param name="line">GPIO line handle</param>
        /// <param name="value">New value.</param>
        /// <returns>0 if the operation succeeds. In case of an error this routine returns -1 and sets the last error number.</returns>
        [DllImport(LibgpiodLibrary)]
        internal static extern int gpiod_line_set_value(SafeLineHandle line, int value);

        /// <summary>
        /// Read current value of a single GPIO line.
        /// </summary>
        /// <param name="line">GPIO line handle</param>
        /// <returns>0 or 1 if the operation succeeds. On error this routine returns -1 and sets the last error number.</returns>
        [DllImport(LibgpiodLibrary, SetLastError = true)]
        internal static extern int gpiod_line_get_value(SafeLineHandle line);

        /// <summary>
        /// Check if line is no used (not set as Input or Output, not listening events).
        /// </summary>
        /// <param name="line">GPIO line handle</param>
        /// <returns>false if pin is used as Input/Output or Listening an event, true if it is free</returns>
        [DllImport(LibgpiodLibrary)]
        internal static extern bool gpiod_line_is_free(SafeLineHandle line);

        /// <summary>
        /// Release a previously reserved line.
        /// </summary>
        /// <param name="lineHandle">GPIO line handle</param>
        [DllImport(LibgpiodLibrary)]
        internal static extern void gpiod_line_release(IntPtr lineHandle);

        /// <summary>
        /// Request all event type notifications on a single line.
        /// </summary>
        /// <param name="line">GPIO line handle</param>
        /// <param name="consumer">Name of the consumer.</param>
        /// <returns>0 the operation succeeds, -1 on failure.</returns>
        [DllImport(LibgpiodLibrary, SetLastError = true)]
        internal static extern int gpiod_line_request_both_edges_events(SafeLineHandle line, string consumer);

        /// <summary>
        /// Wait for an event on a single line.
        /// </summary>
        /// <param name="line">GPIO line handle</param>
        /// <param name="timeout">The TimeSpec to wait for before timing out</param>
        /// <returns>0 if wait timed out, -1 if an error occurred, 1 if an event occurred.</returns>
        [DllImport(LibgpiodLibrary, SetLastError = true)]
        internal static extern WaitEventResult gpiod_line_event_wait(SafeLineHandle line, ref TimeSpec timeout);

        /// <summary>
        /// Read the last event from the GPIO line.
        /// </summary>
        /// <param name="line">GPIO line handle</param>
        /// <param name="gpioEvent">Reference to the gpio event that was detected</param>
        /// <returns>1 if rising edge event occured, 2 on falling edge, -1 on error.</returns>
        [DllImport(LibgpiodLibrary, SetLastError = true)]
        internal static extern int gpiod_line_event_read(SafeLineHandle line, ref GpioLineEvent gpioEvent);

        /// <summary>
        /// Open a gpiochip by number.
        /// </summary>
        /// <returns>GPIO chip pointer handle or NULL if an error occurred.</returns>
        [DllImport(LibgpiodLibrary, SetLastError = true)]
        internal static extern SafeChipHandle gpiod_chip_open_by_number(int number);
    }
}

internal struct GpioLineEvent
{
    public TimeSpec ts;
    public int event_type;
}

internal struct TimeSpec
{
    public NativeLong TvSec;
    public NativeLong TvNsec;
}
