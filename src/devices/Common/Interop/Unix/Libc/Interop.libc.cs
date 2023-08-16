// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Disable these StyleCop rules for this file, as we are using native names here.
#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable CS1591 // Public member is not documented - These members are not public in the final package

using System;
using System.IO;
using System.Runtime.InteropServices;

partial class Interop
{
    public enum FileOpenFlags
    {
        O_RDONLY = 0x00,
        O_WRONLY = 0x01,
        O_RDWR = 0x02,
        O_NONBLOCK = 0x800,
        O_SYNC = 0x101000
    }

    public enum MemoryMappedProtections
    {
        PROT_NONE = 0x0,
        PROT_READ = 0x1,
        PROT_WRITE = 0x2,
        PROT_EXEC = 0x4
    }

    [Flags]
    public enum MemoryMappedFlags
    {
        MAP_SHARED = 0x01,
        MAP_PRIVATE = 0x02,
        MAP_FIXED = 0x10
    }

    [DllImport(LibcLibrary, SetLastError = true)]
    public static extern int ioctl(int fd, int request, IntPtr argp);

    [DllImport(LibcLibrary, SetLastError = true)]
    public static extern int ioctl(int fd, int request, int intArg);

    [DllImport(LibcLibrary, SetLastError = true)]
    public static extern int open([MarshalAs(UnmanagedType.LPStr)] string pathname, FileOpenFlags flags);

    [DllImport(LibcLibrary)]
    public static extern int write(int fd, IntPtr data, int length);

    [DllImport(LibcLibrary)]
    public static extern int close(int fd);

    [DllImport(LibcLibrary, SetLastError = true)]
    public static extern IntPtr mmap(IntPtr addr, int length, MemoryMappedProtections prot, MemoryMappedFlags flags, int fd, int offset);

    [DllImport(LibcLibrary)]
    public static extern int munmap(IntPtr addr, int length);

    public static void ioctlv(int fd, int request, IntPtr argp)
    {
        int result = ioctl(fd, request, argp);
        if (result != 0)
        {
            throw new IOException($"IOCTL request {request} failed: {result}");
        }
    }

    public static void ioctlv(int fd, int request, int arg)
    {
        int result = ioctl(fd, request, arg);
        if (result != 0)
        {
            throw new IOException($"IOCTL request {request} failed: {result}");
        }
    }
}
