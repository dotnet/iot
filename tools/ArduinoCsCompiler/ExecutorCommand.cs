// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#pragma warning disable CS1591
namespace ArduinoCsCompiler
{
    public enum ExecutorCommand : byte
    {
        None = 0,
        DeclareMethod = 1,
        LoadIl = 3,
        StartTask = 4,
        ResetExecutor = 5,
        KillTask = 6,
        MethodSignature = 7,
        ClassDeclaration = 8,
        ClassDeclarationEnd = 9, // Last part of class declaration
        ConstantData = 10,
        Interfaces = 11,
        CopyToFlash = 12,

        WriteFlashHeader = 13,
        CheckFlashVersion = 14,
        EraseFlash = 15,

        SetConstantMemorySize = 16,
        SpecialTokenList = 17,
        ExceptionClauses = 18,
        GlobalMetadata = 19,

        QueryHardware = 20,

        ConditionalBreakpointHit = 30,
        BreakpointHit = 31,
        Variables = 32,
        DebuggerCommand = 35,

        Reply = 0x7d,
        Nack = 0x7e,
        Ack = 0x7f,
    }
}
