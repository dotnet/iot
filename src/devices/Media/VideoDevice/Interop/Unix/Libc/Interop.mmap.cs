// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.InteropServices;

internal partial class Interop
{
    [DllImport(LibcLibrary, SetLastError = true)]
    internal static extern IntPtr mmap(IntPtr addr, int length, MemoryMappedProtections prot, MemoryMappedFlags flags, int fd, int offset);
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
