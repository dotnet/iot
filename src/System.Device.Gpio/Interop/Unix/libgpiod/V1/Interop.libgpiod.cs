// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Disable these StyleCop rules for this file, as we are using native names here.
#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable SA1307 // Accessible fields should begin with upper-case letter

using System;
using System.Device.Gpio;
using System.Device.Gpio.Libgpiod.V1;
using System.Runtime.InteropServices;
#if NET6_0_OR_GREATER
using System.Runtime.Loader;
using System.Reflection;
#endif
// Since this is only used on Linux, and in C on Linux sizeof(long) == sizeof(void*) this is a valid alias.
using NativeLong = System.IntPtr;

internal partial class Interop
{
    internal static partial class LibgpiodV1
    {
#if NET6_0_OR_GREATER
        private const string LibgpiodLibrary = "libgpiod.so.2";
#else
        private const string LibgpiodLibrary = "libgpiod";
#endif
        internal static IntPtr InvalidHandleValue;

        static LibgpiodV1()
        {
            InvalidHandleValue = new IntPtr(-1);
#if NET6_0_OR_GREATER
            Assembly currentAssembly = typeof(LibgpiodV1).Assembly;

            AssemblyLoadContext.GetLoadContext(currentAssembly)!.ResolvingUnmanagedDll += (assembly, libgpiodName) =>
            {
                if (assembly != currentAssembly || libgpiodName != LibgpiodLibrary)
                {
                    return IntPtr.Zero;
                }

                // If loading the 2.x libgpiod failed, we may be running in a Linux distribution that has the 1.x
                // version installed, so we try to load that instead.
                if (NativeLibrary.TryLoad("libgpiod.so.1", out IntPtr handle))
                {
                    return handle;
                }

                return IntPtr.Zero;
            };
#endif
        }

        /// <summary>
        /// Release all resources allocated for the gpiochip iterator but does not close the most recently opened gpiochip
        /// </summary>
        /// <param name="iter">The gpiochip iterator object</param>
        [DllImport(LibgpiodLibrary)]
        internal static extern void gpiod_chip_iter_free_noclose(IntPtr iter);

        /// <summary>
        /// Start an iteration over the list of gpio chips.
        /// </summary>
        [DllImport(LibgpiodLibrary)]
        internal static extern IntPtr gpiod_chip_iter_new();

        /// <summary>
        /// Iterate over a chip iterator. Does not close any previously opened chips.
        /// </summary>
        /// <param name="handle">Handle of a chip iterator</param>
        /// <returns>The next chip handle or null if at the end of the iteration</returns>
        [DllImport(LibgpiodLibrary)]
        internal static extern IntPtr gpiod_chip_iter_next_noclose(SafeChipIteratorHandle handle);

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
        /// <returns>Handle to the GPIO line or <see langword="null" /> if an error occurred.</returns>
        [DllImport(LibgpiodLibrary, SetLastError = true)]
        internal static extern IntPtr gpiod_chip_get_line(SafeChipHandle chip, int offset);

        /// <summary>
        /// Reserve a single line, set the direction to input.
        /// </summary>
        /// <param name="line">GPIO line handle</param>
        /// <param name="consumer">Name of the consumer.</param>
        /// <returns>0 if the line was properly reserved, -1 on failure.</returns>
        [DllImport(LibgpiodLibrary, SetLastError = true)]
        internal static extern int gpiod_line_request_input(IntPtr line, string consumer);

        /// <summary>
        /// Reserve a single line, set the direction to input with flags
        /// </summary>
        /// <param name="line">GPIO line handle</param>
        /// <param name="consumer">Name of the consumer.</param>
        /// <param name="flags">Additional request flags.</param>
        /// <returns>0 if the line was properly reserved, -1 on failure.</returns>
        [DllImport(LibgpiodLibrary, SetLastError = true)]
        internal static extern int gpiod_line_request_input_flags(IntPtr line, string consumer, int flags);

        /// <summary>
        /// Reserve a single line, set the direction to output.
        /// </summary>
        /// <param name="line">GPIO line handle</param>
        /// <param name="consumer">Name of the consumer.</param>
        /// <param name="default_val">Initial value of the line</param>
        /// <returns>0 if the line was properly reserved, -1 on failure.</returns>
        [DllImport(LibgpiodLibrary, SetLastError = true)]
        internal static extern int gpiod_line_request_output(IntPtr line, string consumer, int default_val);

        /// <summary>
        /// Set the value of a single GPIO line.
        /// </summary>
        /// <param name="line">GPIO line handle</param>
        /// <param name="value">New value.</param>
        /// <returns>0 if the operation succeeds. In case of an error this routine returns -1 and sets the last error number.</returns>
        [DllImport(LibgpiodLibrary)]
        internal static extern int gpiod_line_set_value(IntPtr line, int value);

        /// <summary>
        /// Read current value of a single GPIO line.
        /// </summary>
        /// <param name="line">GPIO line handle</param>
        /// <returns>0 or 1 if the operation succeeds. On error this routine returns -1 and sets the last error number.</returns>
        [DllImport(LibgpiodLibrary, SetLastError = true)]
        internal static extern int gpiod_line_get_value(IntPtr line);

        /// <summary>
        /// Check if line is no used (not set as Input or Output, not listening events).
        /// </summary>
        /// <param name="line">GPIO line handle</param>
        /// <returns>false if pin is used as Input/Output or Listening an event, true if it is free</returns>
        [DllImport(LibgpiodLibrary)]
        internal static extern bool gpiod_line_is_free(IntPtr line);

        /// <summary>
        /// Release a previously reserved line.
        /// </summary>
        /// <param name="lineHandle">GPIO line handle</param>
        /// <remarks>
        /// This does NOT invalidate the line handle. This only releases the lock, so that a gpiod_line_request_input/gpiod_line_request_output can be called again.
        /// </remarks>
        [DllImport(LibgpiodLibrary)]
        internal static extern void gpiod_line_release(IntPtr lineHandle);

        /// <summary>
        /// Get the direction of the pin (input or output)
        /// </summary>
        /// <param name="lineHandle">GPIO line handle</param>
        /// <returns>1 for input, 2 for output</returns>
        [DllImport(LibgpiodLibrary)]
        internal static extern int gpiod_line_direction(IntPtr lineHandle);

        /// <summary>
        /// Read the GPIO line bias setting.
        /// </summary>
        /// <param name="lineHandle">GPIO line handle</param>
        /// <returns>GPIOD_LINE_BIAS_PULL_UP (3), GPIOD_LINE_BIAS_PULL_DOWN (4), GPIOD_LINE_BIAS_DISABLE (2) or GPIOD_LINE_BIAS_UNKNOWN (1). </returns>
        [DllImport(LibgpiodLibrary)]
        internal static extern int gpiod_line_bias(IntPtr lineHandle);

        /// <summary>
        /// Request all event type notifications on a single line.
        /// </summary>
        /// <param name="line">GPIO line handle</param>
        /// <param name="consumer">Name of the consumer.</param>
        /// <returns>0 the operation succeeds, -1 on failure.</returns>
        [DllImport(LibgpiodLibrary, SetLastError = true)]
        internal static extern int gpiod_line_request_both_edges_events(IntPtr line, string consumer);

        /// <summary>
        /// Wait for an event on a single line.
        /// </summary>
        /// <param name="line">GPIO line handle</param>
        /// <param name="timeout">The TimeSpec to wait for before timing out</param>
        /// <returns>0 if wait timed out, -1 if an error occurred, 1 if an event occurred.</returns>
        [DllImport(LibgpiodLibrary, SetLastError = true)]
        internal static extern WaitEventResult gpiod_line_event_wait(IntPtr line, ref TimeSpec timeout);

        /// <summary>
        /// Read the last event from the GPIO line.
        /// </summary>
        /// <param name="line">GPIO line handle</param>
        /// <param name="gpioEvent">Reference to the gpio event that was detected</param>
        /// <returns>1 if rising edge event occurred, 2 on falling edge, -1 on error.</returns>
        [DllImport(LibgpiodLibrary, SetLastError = true)]
        internal static extern int gpiod_line_event_read(IntPtr line, ref GpioLineEvent gpioEvent);

        /// <summary>
        /// Open a gpiochip by number.
        /// </summary>
        /// <returns>GPIO chip pointer handle or NULL if an error occurred.</returns>
        [DllImport(LibgpiodLibrary, SetLastError = true)]
        internal static extern IntPtr gpiod_chip_open_by_number(int number);

        /// <summary>
        /// Get the API version of the library as a human-readable string.
        /// </summary>
        /// <returns>Human-readable string containing the library version.</returns>
        [DllImport(LibgpiodLibrary, SetLastError = true)]
        internal static extern IntPtr gpiod_version_string();

        [DllImport(LibgpiodLibrary)]
        internal static extern IntPtr gpiod_chip_name(SafeChipHandle chip);

        [DllImport(LibgpiodLibrary)]
        internal static extern IntPtr gpiod_chip_label(SafeChipHandle chip);
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
