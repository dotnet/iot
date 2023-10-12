// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Iot.Device.Camera.Settings;

namespace Iot.Device.Camera;

/// <summary>
/// Controls an external process
/// </summary>
public class ProcessRunner : IDisposable
{
    private readonly ProcessSettings _processSettings;
    private CancellationTokenSource _cts;
    private Process? _process = null;

    /// <summary>
    /// Creates an instance of the ProcessRunner by getting the
    /// Process Settings
    /// </summary>
    public ProcessRunner(ProcessSettings processSettings)
    {
        _cts = new CancellationTokenSource();
        _processSettings = processSettings;
    }

    /// <summary>
    /// The PID of the process that is running or null
    /// </summary>
    public int? PId => _process?.Id;

    /// <summary>
    /// Dispose the active run process
    /// </summary>
    public void Dispose()
    {
        _cts.Cancel();
        if (_process != null && !_process.WaitForExit(5000))
        {
            throw new Exception($"Process '{_processSettings.Filename}' is hung");
        }
    }

    /// <summary>
    /// Kill the process
    /// </summary>
    public void Kill()
    {
        try
        {
            if (_process != null && !_process.HasExited)
            {
                _process.Kill(true);
            }
        }
        catch (Exception)
        {
        }
    }

    /// <summary>
    /// Execute the process with a number of arguments. The target
    /// Stream received the stdout of the process
    /// </summary>
    public Task ExecuteAsync(string[] arguments, Stream target)
    {
        var argsString = string.Join(' ', arguments);
        return ExecuteAsync(argsString, target);
    }

    /// <summary>
    /// Execute the process with a number of arguments. The target
    /// Stream received the stdout of the process
    /// </summary>
    public async Task ExecuteAsync(string argsString, Stream target)
    {
        var processStartInfo = BuildOptions(argsString);
        _process = new Process();
        _process.EnableRaisingEvents = true;
        _process.StartInfo = processStartInfo;
        _process.Start();

        var br = new BinaryReader(_process.StandardOutput.BaseStream);
        await br.BaseStream.CopyToAsync(target, _processSettings.BufferSize, _cts.Token);
        // _process.Dispose();
        _process.WaitForExit(1000);
    }

    /// <summary>
    /// Execute the process with the given arguments and
    /// returns the output as a string, decoded as UTF8
    /// </summary>
    public async Task<string> ExecuteReadOutputAsStringAsync(string[] arguments)
    {
        var argsString = string.Join(' ', arguments);
        return await ExecuteReadOutputAsStringAsync(argsString);
    }

    /// <summary>
    /// Execute the process with the given arguments and
    /// returns the output as a string, decoded as UTF8
    /// </summary>
    public async Task<string> ExecuteReadOutputAsStringAsync(string argsString)
    {
        using var ms = new MemoryStream();
        await ExecuteAsync(argsString, ms);
        ms.Seek(0, SeekOrigin.Begin);

        using StreamReader sr = new(ms, Encoding.UTF8);
        return await sr.ReadToEndAsync();
    }

    /// <summary>
    /// Runs the execute on a separate thread
    /// </summary>
    public Task<Task> ContinuousRunAsync(string[] arguments, Stream target)
    {
        var argsString = string.Join(' ', arguments);
        return ContinuousRunAsync(argsString, target);
    }

    /// <summary>
    /// Runs the execute on a separate thread
    /// </summary>
    public async Task<Task> ContinuousRunAsync(string argsString, Stream target)
    {
        return await Task.Factory.StartNew(async () =>
        {
            await ExecuteAsync(argsString, target);
        }, _cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
    }

    /*
    public async Task ExecuteAsync(string argsString, PipeWriter target)
    {
        _cts = new CancellationTokenSource();

        var folder = Directory.GetCurrentDirectory();
        var processStartInfo = BuildOptions(argsString);
        _process = new Process();
        // _process.EnableRaisingEvents = true;
        _process.StartInfo = processStartInfo;
        _process.Start();

        var br = new BinaryReader(_process.StandardOutput.BaseStream);

        // the copy to pipe will exit only if the stream ends or the cancellation token is triggered
        // When using a webcam, the stream will exit only if the device is closed
        await br.BaseStream.CopyToAsync(target, _cts.Token);
        _process.Dispose();
    }
    */

    private ProcessStartInfo BuildOptions(string arguments)
    {
        var psi = new ProcessStartInfo();
        psi.FileName = _processSettings.Filename;
        psi.Arguments = arguments;
        psi.UseShellExecute = false;
        psi.RedirectStandardOutput = true;
        psi.RedirectStandardError = true;
        psi.CreateNoWindow = true;
        psi.WorkingDirectory = _processSettings.WorkingDirectory ?? Directory.GetCurrentDirectory();

        return psi;
    }
}
