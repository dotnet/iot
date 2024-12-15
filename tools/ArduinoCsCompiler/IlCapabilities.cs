// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using UnitsNet;
using UnitsNet.Units;

namespace ArduinoCsCompiler
{
    public record IlCapabilities
    {
        public Information IntSize
        {
            get;
            init;
        }

        public Information PointerSize
        {
            get;
            init;
        }

        public Information FlashSize
        {
            get;
            init;
        }

        public Information RamSize
        {
            get;
            init;
        }

        public Information FlashUsed
        {
            get;
            init;
        }

        public int ProtocolVersion
        {
            get;
            init;
        }

        public override string ToString()
        {
            return @$"
sizeof(int)  : {IntSize}
sizeof(void*): {PointerSize}
Flash Size   : {FlashSize.ToUnit(InformationUnit.Kibibyte)}
Flash Used   : {FlashUsed.ToUnit(InformationUnit.Kibibyte)}
Ram Size     : {RamSize.ToUnit(InformationUnit.Kibibyte)}";
        }
    }
}
