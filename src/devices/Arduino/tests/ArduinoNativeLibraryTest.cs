using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Text;
using System.Threading;
using Iot.Device.Arduino;
using Xunit;

namespace Iot.Device.Arduino.Tests
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

            public static int RunBlink(int pin, int delay)
            {
                var gpioController = new GpioController(PinNumberingScheme.Logical, new ArduinoNativeGpioDriver());
                SimpleLedBinding blink = new SimpleLedBinding(gpioController, pin, delay);
                blink.Loop();
                return 1;
            }
        }

        private void ExecuteComplexProgramSuccess<T>(Type mainClass, T mainEntryPoint, params object[] args)
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

            long memoryUsage = exec.EstimateRequiredMemory();
            Assert.True(memoryUsage < 31000, $"Expected memory usage: {memoryUsage} bytes");

            var task = exec.EntryPoint;
            task.InvokeAsync(args);

            task.WaitForResult();

            Assert.True(task.GetMethodResults(exec, out var returnCodes, out var state));
            Assert.NotEmpty(returnCodes);
            Assert.Equal(1, returnCodes[0]);
            _compiler.ClearAllData(true);
        }

        private void ExecuteComplexProgramCausesException<T, TException>(Type mainClass, T mainEntryPoint, params object[] args)
            where TException : Exception
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

            long memoryUsage = exec.EstimateRequiredMemory();
            Assert.True(memoryUsage < 10000, $"Expected memory usage: {memoryUsage} bytes");

            var task = exec.EntryPoint;
            task.InvokeAsync(args);

            task.WaitForResult();
            MethodState state = MethodState.Running;
            Assert.Throws<TException>(() => task.GetMethodResults(exec, out var returnCodes, out state));
            Assert.Equal(MethodState.Aborted, state);
            _compiler.ClearAllData(true);
        }

        [Fact]
        public void RunBlinkWithGpioController()
        {
            ExecuteComplexProgramSuccess<Func<int, int, int>>(typeof(SimpleLedBinding), SimpleLedBinding.RunBlink, 6, 1000);
        }

        [Fact]
        public void ExpectArrayIndexOutOfBounds()
        {
            ExecuteComplexProgramCausesException<Func<int, int>, ArgumentOutOfRangeException>(typeof(ArduinoNativeLibraryTest), OutOfBoundsCheck, 10);
        }

        [Fact]
        public void ExpectDivideByZero()
        {
            ExecuteComplexProgramCausesException<Func<int, int>, DivideByZeroException>(typeof(ArduinoNativeLibraryTest), DivideByZero, 0);
        }

        [Fact]
        public void ExpectOutOfMemory()
        {
            ExecuteComplexProgramCausesException<Func<int, int>, OutOfMemoryException>(typeof(ArduinoNativeLibraryTest), OutOfMemory, (1 << 31) + (1 << 30));
        }

        private int OutOfBoundsCheck(int index)
        {
            int[] array = new int[2];
            return array[index];
        }

        private int DivideByZero(int zero)
        {
            return 10 / zero;
        }

        private int OutOfMemory(int sizeToAllocate)
        {
            int[] array = new int[sizeToAllocate];
            return array.Length;
        }

        [Fact]
        public void GetDataFromStaticByteField()
        {
            ExecuteComplexProgramSuccess<Func<int, int, int>>(typeof(ClassWithStaticByteField), ClassWithStaticByteField.GetFirstByte, 0, 0);
        }

        /// <summary>
        /// This test not only tests the value of BitConverter.IsLittleEndian but also whether accessing a static
        /// field of a class with a native implementation works
        /// </summary>
        [Fact]
        public void CpuIsLittleEndian()
        {
            ExecuteComplexProgramSuccess<Func<int>>(typeof(ArduinoNativeLibraryTest), IsLittleEndianTest);
        }

        private int IsLittleEndianTest()
        {
            return BitConverter.IsLittleEndian ? 1 : 0;
        }

        [Fact]
        public void ClassWith64BitFieldTest()
        {
            ExecuteComplexProgramSuccess<Func<int>>(typeof(ClassWith64BitField), ClassWith64BitField.ClassMain);
        }

        public int MethodCallOnValueType()
        {
            // Tests the simple cases, where the type is known. When it is not (generics), and a CONSTRAINED. prefix comes into play,
            // things get worse
            int i = 32;
            // Virtual instance method (This is weird, The IL for this method shows the signature as "newslot virtual final", which is a bit pointless, unless
            // the method is called via an interface.
            if (i.CompareTo(11) == 0)
            {
                return 0;
            }

            // Virtual instance method
            int hash = i.GetHashCode();
            if (hash == 0)
            {
                return 0;
            }

            // Now this is a virtual call
            IComparable<int> i2 = i;
            if (i2.CompareTo(12) == 0)
            {
                return 0;
            }

            DateTime dt = new DateTime(2020, 01, 20);
            // Non-virtual method call on a value type
            dt = dt.AddDays(1.0f);
            if (dt.DayOfYear != 20)
            {
                return dt.DayOfYear;
            }

            return 1;
        }

        [Fact]
        public void MethodCallOnValueTypeTest()
        {
            ExecuteComplexProgramSuccess<Func<int>>(typeof(ArduinoNativeLibraryTest), MethodCallOnValueType);
        }

        public class ClassWith64BitField
        {
            private int _field1;
            private long _field2;

            public ClassWith64BitField()
            {
                _field1 = -1;
                _field2 = 2;
            }

            public int GetResult()
            {
                return (int)(_field1 + _field2);
            }

            public static int ClassMain()
            {
                var instance = new ClassWith64BitField();
                return instance.GetResult();
            }
        }

        public class ClassWithStaticByteField
        {
            private static byte[] _byteData =
            {
                1, 2, 3, 4, 5, 6
            };

            public ClassWithStaticByteField()
            {
            }

            public byte[] ByteData
            {
                get
                {
                    return _byteData;
                }
            }

            /// <summary>
            /// The two input arguments are expected to be 0. This is just to make sure the tests can reuse the same infrastructure
            /// </summary>
            public static int GetFirstByte(int index, int extraIndex)
            {
                return _byteData[index + extraIndex];
            }
        }

        [Fact]
        public void GetDataFromClassWithStaticField2()
        {
            ExecuteComplexProgramSuccess<Func<int, int, int>>(typeof(ClassWithStaticField2), ClassWithStaticField2.GetFirstByte, 0, 0);
        }

        public class ClassWithStaticField2
        {
            private static object?[] _byteData = new object?[]
            {
                new object(), new HashSet<int>(), null,
            };

            public ClassWithStaticField2()
            {
            }

            /// <summary>
            /// The two input arguments are expected to be 0. This is just to make sure the tests can reuse the same infrastructure
            /// </summary>
            public static int GetFirstByte(int index, int extraIndex)
            {
                if (_byteData[index + extraIndex] != null)
                {
                    return 1;
                }

                return 0;
            }
        }

        [Fact]
        public void GetDataFromStaticIntField()
        {
            ExecuteComplexProgramSuccess<Func<int, int, int>>(typeof(ClassWithStaticIntField), ClassWithStaticIntField.GetFirst, 7, 0);
        }

        public class ClassWithStaticIntField
        {
            private static int[] _intData =
            {
                7, 2, 3, 4, 5, 6, 4711, 1, 80000,
            };

            public ClassWithStaticIntField()
            {
            }

            public int[] IntData
            {
                get
                {
                    return _intData;
                }
            }

            /// <summary>
            /// The two input arguments are expected to be 0. This is just to make sure the tests can reuse the same infrastructure
            /// </summary>
            public static int GetFirst(int index, int extraIndex)
            {
                return _intData[index + extraIndex];
            }
        }

    }
}
