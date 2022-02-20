// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

/// <summary>
/// Interop methods for Windows and Unix (P/Invoke call declarations)
/// This class is internal by default, but gets promoted to public within the device binding projects
/// </summary>
partial class Interop
{
    private const string LibcLibrary = "libc";
    private const string AlsaLibrary = "libasound";
}
