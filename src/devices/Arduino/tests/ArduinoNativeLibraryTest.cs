using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Text;
using System.Threading;
using Iot.Device.Arduino;
using Xunit;

namespace Arduino.Tests
{
    /// <summary>
    /// Tests native library functions for the IL Executor
    /// </summary>
    public class ArduinoNativeLibraryTest : IClassFixture<FirmataTestFixture>, IDisposable
    {
        private FirmataTestFixture _fixture;
        private ArduinoCsCompiler _compiler;

        public ArduinoNativeLibraryTest(FirmataTestFixture fixture)
        {
            _fixture = fixture;
            _compiler = new ArduinoCsCompiler(fixture.Board, true);
            _compiler.ClearAllData(true);
        }

        public void Dispose()
        {
            _compiler.Dispose();
        }

        public class SimpleLedBinding
        {
            private readonly GpioController _controller;
            private int _ledPin;
            private int _delay;

            public SimpleLedBinding(GpioController controller, int pin, int delay)
            {
                _controller = controller;
                _controller.OpenPin(pin);
                _controller.SetPinMode(pin, PinMode.Output);
                _ledPin = pin;
                _delay = delay;
            }

            public void Loop()
            {
                for (int i = 0; i < 2; i++)
                {
                    _controller.Write(_ledPin, 1);
                    Thread.Sleep(_delay);
                    _controller.Write(_ledPin, 0);
                    Thread.Sleep(_delay);
                }
            }

            public static void RunBlink(int pin, int delay)
            {
                var gpioController = new GpioController(PinNumberingScheme.Logical, new ArduinoNativeGpioDriver());
                SimpleLedBinding blink = new SimpleLedBinding(gpioController, pin, delay);
                blink.Loop();
            }
        }

        private void ExecuteComplexProgramSuccess<T>(Type mainClass, T mainEntryPoint)
            where T : Delegate
        {
            // These operations should be combined into one, to simplify usage (just provide the main entry point,
            // and derive everything required from there)
            _compiler.ClearAllData(true);
            var exec = _compiler.PrepareProgram(mainClass, mainEntryPoint);
            try
            {
                exec.Load();
            }
            catch (Exception)
            {
                _compiler.ClearAllData(true);
                throw;
            }

            var task = exec.EntryPoint;
            task.InvokeAsync(6, 1000);

            task.WaitForResult();

            Assert.True(task.GetMethodResults(exec, out _, out var state));
            _compiler.ClearAllData(true);
        }

        [Fact]
        public void RunBlinkWithGpioController()
        {
            ExecuteComplexProgramSuccess<Action<int, int>>(typeof(SimpleLedBinding), SimpleLedBinding.RunBlink);
        }
    }
}
