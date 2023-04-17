// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace ArduinoCsCompiler.Runtime
{
    /// <summary>
    /// This exception is thrown by <see cref="MiniAssert"/>
    /// </summary>
    public class MiniAssertionException : Exception
    {
        public MiniAssertionException()
        {
        }

        public MiniAssertionException(string message)
            : base(message)
        {
        }

        public MiniAssertionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
