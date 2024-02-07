// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Globalization;
using Iot.Device.Arduino;

namespace ArduinoCsCompiler.Runtime
{
    [ArduinoReplacement(typeof(CompareInfo), false, IncludingPrivates = true)]
    internal class MiniCompareInfo
    {
        [ArduinoImplementation]
        public int IcuCompareString(ReadOnlySpan<char> string1, ReadOnlySpan<char> string2, CompareOptions options)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation]
        private unsafe int IcuIndexOfCore(ReadOnlySpan<char> source, ReadOnlySpan<char> target, CompareOptions options, int* matchLengthPtr, bool fromBeginning)
        {
            throw new NotImplementedException();
        }
    }
}
