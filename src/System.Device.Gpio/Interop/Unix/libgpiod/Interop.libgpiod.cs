// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;
using System.Device.Gpio;
using System.Runtime.InteropServices;

internal partial class Interop
{
    private const string LibgpiodLibrary = "System.Device.Gpio.Native";
    internal static IntPtr InvalidHandleValue = new IntPtr(-1);

    /// <summary>
    /// Create a new gpiochip iterator.
    /// </summary>
    /// <returns>Pointer to a new chip iterator object or <see langword="null" /> if an error occurred.</returns>
    [DllImport(LibgpiodLibrary, SetLastError = true)]
    internal static extern SafeChipIteratorHandle GetChipIterator();

    /// <summary>
    /// Release all resources allocated for the gpiochip iterator and close the most recently opened gpiochip(if any).
    /// </summary>
    /// <param name="iter">The gpiochip iterator object</param>
    [DllImport(LibgpiodLibrary)]
    internal static extern void FreeChipIterator(IntPtr iter);

    /// <summary>
    /// Release all resources allocated for the gpiochip iterator but don't close the most recently opened gpiochip (if any).
    /// </summary>
    /// <param name="iter">The gpiochip iterator object</param>
    [DllImport(LibgpiodLibrary)]
    internal static extern void FreeChipIteratorNoCloseCurrentChip(SafeChipIteratorHandle iter);

    /// <summary>
    /// Get the next gpiochip handle.
    /// </summary>
    /// <param name="iter">The gpiochip iterator object</param>
    [DllImport(LibgpiodLibrary, SetLastError = true)]
    internal static extern SafeChipHandle GetNextChipFromChipIterator(SafeChipIteratorHandle iter);

    /// <summary>
    /// Close a GPIO chip handle and release all allocated resources.
    /// </summary>
    /// <param name="chip">The GPIO chip pointer</param>
    [DllImport(LibgpiodLibrary)]
    internal static extern void CloseChip(IntPtr chip);

    /// <summary>
    /// Get the number of GPIO lines exposed by this chip.
    /// </summary>
    /// <param name="chip">The GPIO chip handle.</param>
    /// <returns>Number of GPIO lines.</returns>
    [DllImport(LibgpiodLibrary)]
    internal static extern int GetNumberOfLines(SafeChipHandle chip);

    /// <summary>
    /// Get the handle to the GPIO line at given offset.
    /// </summary>
    /// <param name="chip">The GPIO chip handle</param>
    /// <param name="offset">The offset of the GPIO line</param>
    /// <returns>Handle to the GPIO line or <see langword="null" /> if an error occured.</returns>
    [DllImport(LibgpiodLibrary, SetLastError = true)]
    internal static extern SafeLineHandle GetChipLineByOffset(SafeChipHandle chip, int offset);

    /// <summary>
    /// Read the GPIO line direction setting.
    /// </summary>
    /// <param name="line">GPIO line handle</param>
    /// <returns>1 if INPUT otherwise 2 (OUTPUT).</returns>
    [DllImport(LibgpiodLibrary)]
    internal static extern int GetLineDirection(SafeLineHandle line);

    /// <summary>
    /// Reserve a single line, set the direction to input.
    /// </summary>
    /// <param name="line">GPIO line handle</param>
    /// <param name="consumer">Name of the consumer.</param>
    /// <returns>0 if the line was properly reserved, -1 on failure.</returns>
    [DllImport(LibgpiodLibrary)]
    internal static extern int RequestLineInput(SafeLineHandle line, string consumer);

    /// <summary>
    /// Reserve a single line, set the direction to output.
    /// </summary>
    /// <param name="line">GPIO line handle</param>
    /// <param name="consumer">Name of the consumer.</param>
    /// <returns>0 if the line was properly reserved, -1 on failure.</returns>
    [DllImport(LibgpiodLibrary)]
    internal static extern int RequestLineOutput(SafeLineHandle line, string consumer);

    /// <summary>
    /// Set the value of a single GPIO line.
    /// </summary>
    /// <param name="line">GPIO line handle</param>
    /// <param name="value">New value.</param>
    /// <returns>0 if the operation succeeds. In case of an error this routine returns -1 and sets the last error number.</returns>
    [DllImport(LibgpiodLibrary)]
    internal static extern int SetGpiodLineValue(SafeLineHandle line, int value);

    /// <summary>
    /// Read current value of a single GPIO line.
    /// </summary>
    /// <param name="line">GPIO line handle</param>
    /// <returns>0 or 1 if the operation succeeds. On error this routine returns -1 and sets the last error number.</returns>
    [DllImport(LibgpiodLibrary, SetLastError = true)]
    internal static extern int GetGpiodLineValue(SafeLineHandle line);

    /// <summary>
    /// Check if line is no used (not set as Input or Output, not listening events).
    /// </summary>
    /// <param name="line">GPIO line handle</param>
    /// <returns>false if pin is used as Input/Output or Listening an event, true if it is free</returns>
    [DllImport(LibgpiodLibrary)]
    internal static extern bool LineIsFree(SafeLineHandle line);

    /// <summary>
    /// Release a previously reserved line.
    /// </summary>
    /// <param name="line">GPIO line handle</param>
    [DllImport(LibgpiodLibrary)]
    internal static extern void ReleaseGpiodLine(IntPtr lineHandle);

    /// <summary>
    /// Request all event type notifications on a single line.
    /// </summary>
    /// <param name="line">GPIO line handle</param>
    /// <param name="consumer">Name of the consumer.</param>
    /// <returns>0 the operation succeeds, -1 on failure.</returns>
    [DllImport(LibgpiodLibrary, SetLastError = true)]
    internal static extern int RequestBothEdgesEventForLine(SafeLineHandle line, string consumer);

    /// <summary>
    /// Wait for an event on a single line.
    /// </summary>
    /// <param name="line">GPIO line handle</param>
    /// <returns>0 if wait timed out, -1 if an error occurred, 1 if an event occurred.</returns>
    [DllImport(LibgpiodLibrary, SetLastError = true)]
    internal static extern WaitEventResult WaitForEventOnLine(SafeLineHandle line);

    /// <summary>
    /// Read the last event from the GPIO line.
    /// </summary>
    /// <param name="line">GPIO line handle</param>
    /// <returns>1 if rising edge event occured, 2 on falling edge, -1 on error.</returns>
    [DllImport(LibgpiodLibrary, SetLastError = true)]
    internal static extern int ReadEventForLine(SafeLineHandle line);

    /// <summary>
    /// Open a gpiochip by number.
    /// </summary>
    /// <returns>GPIO chip pointer handle or NULL if an error occurred.</returns>
    [DllImport(LibgpiodLibrary, SetLastError = true)]
    internal static extern SafeChipHandle OpenChipByNumber(int number);
}
