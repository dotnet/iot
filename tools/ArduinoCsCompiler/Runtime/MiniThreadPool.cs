// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading;
using Iot.Device.Arduino;

namespace ArduinoCsCompiler.Runtime
{
    [ArduinoReplacement(typeof(ThreadPool), IncludingPrivates = true)]
    internal sealed class MiniThreadPool
    {
        public static bool UseWindowsThreadPool
        {
            [ArduinoImplementation]
            get
            {
                return false;
            }
        }

    }
}
