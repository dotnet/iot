// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable SA1307 // Accessible fields should begin with upper-case letter
#pragma warning disable CS8981 // Type name only contains lower-cased ascii characters

using System;
using System.Runtime.InteropServices;

internal partial class Interop
{
    /// <summary>
    /// Waits for one of a set of file descriptors to become ready.
    /// </summary>
    /// <param name="fds">Array of descriptors to watch.</param>
    /// <param name="nfds">Number of entries in <paramref name="fds"/>. This is a nfds_t, which is pointer sized.</param>
    /// <param name="timeout">Milliseconds to wait, or -1 to block indefinitely.</param>
    /// <remarks>
    /// poll() is used rather than ppoll(): its timeout is a plain int, so unlike a struct timespec it
    /// carries no dependency on how the C library was compiled (see _TIME_BITS).
    /// </remarks>
    [DllImport(LibcLibrary, SetLastError = true)]
    internal static extern int poll([In, Out] pollfd[] fds, nuint nfds, int timeout);
}

/// <summary>
/// struct pollfd. Fixed width on every ABI: two ints and two shorts.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct pollfd
{
    public int fd;
    public PollFlags events;
    public PollFlags revents;
}

[Flags]
internal enum PollFlags : short
{
    None = 0,
    POLLIN = 0x001,
    POLLPRI = 0x002,
    POLLERR = 0x008,
    POLLHUP = 0x010,
    POLLNVAL = 0x020
}
