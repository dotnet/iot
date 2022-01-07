// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

/// <summary>
/// Interop methods for Windows and Unix (P/Invoke call declarations)
/// </summary>
#if BUILDING_IOT_DEVICE_BINDINGS
    internal
#else
public
#endif
partial class Interop
{
    private const string LibcLibrary = "libc";
    private const string AlsaLibrary = "libasound";
}
