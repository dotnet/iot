// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Iot.Device.Arduino;
using Microsoft.Extensions.Logging;

namespace ArduinoCsCompiler;

internal class ExecRun : Run<ExecOptions>
{
    private ArduinoBoard? _board;

    public ExecRun(ExecOptions execOptions)
        : base(execOptions)
    {
    }

    public override bool RunCommand()
    {
        if (!ConnectToBoard(CommandLineOptions, out _board))
        {
            return false;
        }

        var compiler = new MicroCompiler(_board, true);

        if (!compiler.QueryBoardCapabilities(true, out var caps))
        {
            return false;
        }

        if (CommandLineOptions.Stop)
        {
            compiler.KillTask(null);
            Logger.LogInformation("All tasks terminated");
        }
        else
        {
            Logger.LogError("No subcommand given - nothing was done");
        }

        return true;
    }

    protected override void Dispose(bool disposing)
    {
        _board?.Dispose();
        _board = null;

        base.Dispose(disposing);
    }
}
