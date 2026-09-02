// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#pragma warning disable SA1300 // Element should begin with upper-case letter

using System.Runtime.InteropServices;

internal partial class Interop
{
    /// <summary>
    /// O_CLOEXEC. Same value on every architecture supported by .NET on Linux.
    /// </summary>
    internal const int O_CLOEXEC = 0x80000;

    /// <summary>
    /// Creates a pipe, applying <paramref name="flags"/> atomically.
    /// </summary>
    /// <remarks>
    /// Preferred over pipe() followed by fcntl(F_SETFD): setting close-on-exec afterwards races
    /// with a fork/exec on another thread, which would leak the descriptors into a child process.
    /// </remarks>
    [DllImport(LibcLibrary, SetLastError = true)]
    internal static extern int pipe2(int[] pipefd, int flags);
}
