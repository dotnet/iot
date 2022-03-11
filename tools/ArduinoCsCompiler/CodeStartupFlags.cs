// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace ArduinoCsCompiler
{
    /// <summary>
    /// Flags to configure startup settings for firmware code
    /// </summary>
    [Flags]
    public enum CodeStartupFlags
    {
        /// <summary>
        /// No special flags
        /// </summary>
        None,

        /// <summary>
        /// Automatically restart the code after a crash
        /// </summary>
        AutoRestartAfterCrash = 1,
    }
}
