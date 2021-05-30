using System;
using Xunit;

namespace Iot.Device.Arduino.Tests
{
    public class ArduinoTestBase : IDisposable, IClassFixture<FirmataTestFixture>
    {
        private readonly FirmataTestFixture _fixture;
        private readonly ArduinoCsCompiler _compiler;
        private readonly ArduinoBoard _board;

        public ArduinoTestBase(FirmataTestFixture fixture)
        {
            _fixture = fixture;
            Assert.NotNull(_fixture.Board);
            _board = _fixture.Board!;
            _compiler = new ArduinoCsCompiler(_fixture.Board!, true);
        }

        protected ArduinoCsCompiler Compiler => _compiler;

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
            ExecuteComplexProgramSuccess<T>(mainEntryPoint, executeLocally, _fixture.DefaultCompilerSettings, args);
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

            var exec = _compiler.CreateExecutionSet(mainEntryPoint, settings);

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
            var exec = _compiler.CreateExecutionSet(mainEntryPoint, _fixture.DefaultCompilerSettings);

            long memoryUsage = exec.EstimateRequiredMemory();
            Assert.True(memoryUsage < _fixture.DefaultCompilerSettings.MaxMemoryUsage, $"Expected memory usage: {memoryUsage} bytes");

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
