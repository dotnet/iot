// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ArduinoCsCompiler.Runtime
{
    [ArduinoReplacement(typeof(FileLoadException), false, IncludingPrivates = true)]
    internal class MiniFileLoadException
    {
        [ArduinoImplementation(CompareByParameterNames = true)]
        public static void GetFileLoadExceptionMessage(Int32 hResult, StringHandleOnStack retString)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation(CompareByParameterNames = true)]
        public static void GetMessageForHR(int hresult, StringHandleOnStack retString)
        {
            throw new NotImplementedException();
        }

        internal unsafe ref struct StringHandleOnStack
        {
            private void* _ptr;

            internal StringHandleOnStack(ref string? s)
            {
                _ptr = Unsafe.AsPointer(ref s);
            }
        }
    }
}
