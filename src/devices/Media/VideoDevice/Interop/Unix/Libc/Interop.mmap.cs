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

/// <summary>
/// The BCM GPIO registers expose the data/direction/interrupt/etc functionality of pins.
/// Each register is 64 bits, where each bit represents a logical register number.
/// 
/// For example, writing HIGH to register 20 would translate to (registerViewPointer).GPSET[0] | (1U &lt;&lt; 20).
/// </summary>
[StructLayout(LayoutKind.Explicit)]
internal unsafe struct RegisterView
{
    ///<summary>GPIO Function Select, 6x32 bits, R/W.</summary>
    [FieldOffset(0x00)]
    public fixed uint GPFSEL[6];

    ///<summary>GPIO Pin Output Set, 2x32 bits, W.</summary>
    [FieldOffset(0x1C)]
    public fixed uint GPSET[2];

    ///<summary>GPIO Pin Output Clear, 2x32 bits, W.</summary>
    [FieldOffset(0x28)]
    public fixed uint GPCLR[2];

    ///<summary>GPIO Pin Level, 2x32 bits, R.</summary>
    [FieldOffset(0x34)]
    public fixed uint GPLEV[2];

    ///<summary>GPIO Pin Event Detect Status, 2x32 bits, R/W.</summary>
    [FieldOffset(0x40)]
    public fixed uint GPEDS[2];

    ///<summary>GPIO Pin Rising Edge Detect Enable, 2x32 bits, R/W.</summary>
    [FieldOffset(0x4C)]
    public fixed uint GPREN[2];

    ///<summary>GPIO Pin Falling Edge Detect Enable, 2x32 bits, R/W.</summary>
    [FieldOffset(0x58)]
    public fixed uint GPFEN[2];

    ///<summary>GPIO Pin High Detect Enable, 2x32 bits, R/W.</summary>
    [FieldOffset(0x64)]
    public fixed uint GPHEN[2];

    ///<summary>GPIO Pin Low Detect Enable, 2x32 bits, R/W.</summary>
    [FieldOffset(0x70)]
    public fixed uint GPLEN[2];

    ///<summary>GPIO Pin Async. Rising Edge Detect, 2x32 bits, R/W.</summary>
    [FieldOffset(0x7C)]
    public fixed uint GPAREN[2];

    ///<summary>GPIO Pin Async. Falling Edge Detect, 2x32 bits, R/W.</summary>
    [FieldOffset(0x88)]
    public fixed uint GPAFEN[2];

    ///<summary>GPIO Pin Pull-up/down Enable, 32 bits, R/W.</summary>
    [FieldOffset(0x94)]
    public uint GPPUD;

    ///<summary>GPIO Pin Pull-up/down Enable Clock, 2x32 bits, R/W.</summary>
    [FieldOffset(0x98)]
    public fixed uint GPPUDCLK[2];
}
