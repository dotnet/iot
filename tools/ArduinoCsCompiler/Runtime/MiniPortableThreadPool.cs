// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArduinoCsCompiler.Runtime
{
    internal class MiniPortableThreadPool
    {
        [ArduinoReplacement("System.Threading.PortableThreadPool+WorkerThread", "System.Private.CoreLib.dll", false, typeof(System.String), IncludingPrivates = true)]
        internal class WorkerThread
        {
            public static bool IsIOPending
            {
                // Function was newly added in .NET 6.0.12
                [ArduinoImplementation]
                get
                {
                    return false;
                }
            }
        }
    }
}
