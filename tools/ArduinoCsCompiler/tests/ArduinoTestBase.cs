// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using ArduinoCsCompiler;
using Iot.Device.Common;
using Xunit;

namespace Iot.Device.Arduino.Tests
{
    public class ArduinoTestBase : IDisposable, IClassFixture<FirmataTestFixture>
    {
        private readonly FirmataTestFixture _fixture;
        private readonly MicroCompiler _compiler;
        private readonly ArduinoBoard _board;

        public ArduinoTestBase(FirmataTestFixture fixture)
        {
            _fixture = fixture;
            Assert.NotNull(_fixture.Board);
            _board = _fixture.Board!;
            CompilerSettings = new CompilerSettings()
            {
                CreateKernelForFlashing = true,
                UseFlashForKernel = true,
                UseFlashForProgram = false,
                MaxMemoryUsage = 350_000
            };

            ErrorManager.Logger = this.GetCurrentClassLogger();

            _compiler = new MicroCompiler(_fixture.Board!, true);

            if (!_compiler.QueryBoardCapabilities(false, out IlCapabilities data))
            {
                throw new NotSupportedException("No valid IL execution firmware found on board");
            }

            Debug.WriteLine(data.ToString());
        }

        public CompilerSettings CompilerSettings
        {
            get;
            set;
        }

        protected MicroCompiler Compiler => _compiler;

        protected FirmataTestFixture Fixture => _fixture;

        protected ArduinoBoard Board => _board;

        protected virtual void Dispose(bool disposing)
        {
            _compiler.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void ExecuteComplexProgramSuccess<T>(T mainEntryPoint, bool executeLocally, params object[] args)
            where T : Delegate
        {
            ExecuteComplexProgramSuccess<T>(mainEntryPoint, executeLocally, CompilerSettings, args);
        }

        protected void ExecuteComplexProgramSuccess<T>(T mainEntryPoint, bool executeLocally, CompilerSettings settings, params object[] args)
            where T : Delegate
        {
            // Execute function locally, if possible (to compare behavior)
            if (executeLocally)
            {
                object? result = mainEntryPoint.DynamicInvoke(args);
                int returnValue = (int)result!;
                Assert.Equal(1, returnValue);
            }

            var exec = _compiler.PrepareAndRunExecutionSet(mainEntryPoint, settings);

            // long memoryUsage = exec.EstimateRequiredMemory();
            // Assert.True(memoryUsage < settings.MaxMemoryUsage, $"Expected memory usage: {memoryUsage} bytes");
            var task = exec.MainEntryPoint;
            task.InvokeAsync(args);

            task.WaitForResult();

            Assert.True(task.GetMethodResults(exec, out var returnCodes, out var state));
            Assert.NotEmpty(returnCodes);
            Assert.Equal(1, returnCodes[0]);
            _compiler.ClearAllData(true);
        }

        protected void ExecuteComplexProgramCausesException<T, TException>(Type mainClass, T mainEntryPoint, params object[] args)
            where TException : Exception
            where T : Delegate
        {
            // These operations should be combined into one, to simplify usage (just provide the main entry point,
            // and derive everything required from there)
            _compiler.ClearAllData(true);
            var exec = _compiler.PrepareAndRunExecutionSet(mainEntryPoint, CompilerSettings);

            long memoryUsage = exec.EstimateRequiredMemory();
            var task = exec.MainEntryPoint;
            task.InvokeAsync(args);

            task.WaitForResult();
            MethodState state = MethodState.Running;
            Assert.Throws<TException>(() => task.GetMethodResults(exec, out var returnCodes, out state));
            Assert.Equal(MethodState.Aborted, state);
            _compiler.ClearAllData(true);
        }
    }
}
