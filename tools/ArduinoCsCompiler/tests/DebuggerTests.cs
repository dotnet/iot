// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using ArduinoCsCompiler;
using Xunit;

namespace Iot.Device.Arduino.Tests;

public class DebuggerTests
{
    [Theory]
    [InlineData(42, 0x123456)]
    [InlineData(1234, 0x654321)]
    public void UnknownStackFramePreservesTaskIdAndMethodToken(int taskId, int methodToken)
    {
        // Decoding an unknown token does not invoke the compiler or contact a board.
        var executionSet = new ExecutionSet(null!, new CompilerSettings());

        var frames = Debugger.DecodeStackTrace(executionSet, taskId, new List<int> { 0, 0, methodToken, 7 });

        var frame = Assert.Single(frames);
        Assert.Null(frame.Method);
        Assert.Equal(taskId, frame.TaskId);
        Assert.Equal(methodToken, frame.RemoteToken);
    }
}
