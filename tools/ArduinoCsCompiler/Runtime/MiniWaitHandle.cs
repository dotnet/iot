// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ArduinoCsCompiler.Runtime
{
    [ArduinoReplacement(typeof(WaitHandle), IncludingPrivates = true)]
    internal class MiniWaitHandle
    {
        [ArduinoImplementation("WaitHandleWaitOneCore")]
        public static int WaitOneCore(IntPtr waitHandle, int millisecondsTimeout)
        {
            throw new NotImplementedException();
        }
    }
}
