// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace ArduinoCsCompiler
{
    public enum MethodState
    {
        Stopped = 0,
        Aborted = 1,
        Running = 2,
        Killed = 3,
        ConnectionError = 4,
        Debugging = 5,
    }
}
