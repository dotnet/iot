// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace ArduinoCsCompiler.Tests;

public interface IGetNext<T>
    where T : IGetNext<T>
{
    static abstract T operator ++(T other);
}

public struct RepeatSequence : IGetNext<RepeatSequence>
{
    private const char Ch = 'A';
    public string Text = new string(Ch, 1);

    public RepeatSequence()
    {
    }

    public static RepeatSequence operator ++(RepeatSequence other)
        => other with { Text = other.Text + Ch };

    public static void TestMethod()
    {
        RepeatSequence rs = new RepeatSequence();
        rs++;
    }

    public override string ToString() => Text;
}
