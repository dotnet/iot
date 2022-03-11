// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Iot.Device.Arduino;

namespace ArduinoCsCompiler.Runtime
{
    /// <summary>
    /// This class is here for reference. The actual implementation is directly in the runtime.
    /// </summary>
    // [ArduinoReplacement("System.ByReference<T>", "System.Private.CoreLib.dll", false)]
    internal ref struct MiniByReference<T>
    {
        [ArduinoImplementation]
        public MiniByReference(ref T value)
        {
            throw new NotImplementedException();
        }

        public ref T Value
        {
            [ArduinoImplementation]
            get
            {
                throw new NotImplementedException();
            }
        }
    }
}
