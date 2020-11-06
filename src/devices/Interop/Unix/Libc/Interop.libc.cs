// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Disable these StyleCop rules for this file, as we are using native names here.
#pragma warning disable SA1300 // Element should begin with upper-case letter

using System;
using System.Runtime.InteropServices;

internal partial class Interop
{
    [DllImport(LibcLibrary, SetLastError = true)]
    internal static extern int ioctl(int fd, int request, IntPtr argp);

    [DllImport(LibcLibrary, SetLastError = true)]
    internal static extern int open([MarshalAs(UnmanagedType.LPStr)] string pathname, FileOpenFlags flags);

    [DllImport(LibcLibrary)]
    internal static extern int close(int fd);

    [DllImport(LibcLibrary, SetLastError = true)]
    internal static extern IntPtr mmap(IntPtr addr, int length, MemoryMappedProtections prot, MemoryMappedFlags flags, int fd, int offset);

    [DllImport(LibcLibrary)]
    internal static extern int munmap(IntPtr addr, int length);
}

internal enum FileOpenFlags
{
    O_RDONLY = 0x00,
    O_RDWR = 0x02,
    O_NONBLOCK = 0x800,
    O_SYNC = 0x101000
}

[Flags]
internal enum MemoryMappedProtections
{
    PROT_NONE = 0x0,
    PROT_READ = 0x1,
    PROT_WRITE = 0x2,
    PROT_EXEC = 0x4
}

[Flags]
internal enum MemoryMappedFlags
{
    MAP_SHARED = 0x01,
    MAP_PRIVATE = 0x02,
    MAP_FIXED = 0x10
}
