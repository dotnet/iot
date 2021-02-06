using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Device.I2c;
using System.Text;
using System.Threading;
using Iot.Device.Arduino;
using Iot.Device.CharacterLcd;
using Xunit;

namespace Iot.Device.Arduino.Tests
{
    /// <summary>
    /// Tests native library functions for the IL Executor
    /// </summary>
    public class ArduinoNativeLibraryTest : IClassFixture<FirmataTestFixture>, IDisposable
    {
        private const int MaxTestMemoryUsage = 200000;
        private FirmataTestFixture _fixture;
        private ArduinoCsCompiler _compiler;

        public ArduinoNativeLibraryTest(FirmataTestFixture fixture)
        {
            _fixture = fixture;
            _compiler = new ArduinoCsCompiler(fixture.Board, true);
            _compiler.ClearAllData(true, false);
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

        private void ExecuteComplexProgramSuccess<T>(T mainEntryPoint, bool executeLocally, params object[] args)
            where T : Delegate
        {
            // Execute function locally, if possible (to compare behavior)
            if (executeLocally)
            {
                object? result = mainEntryPoint.DynamicInvoke(args);
                int returnValue = (int)result!;
                Assert.Equal(1, returnValue);
            }

            var exec = _compiler.CreateExecutionSet(mainEntryPoint);

            long memoryUsage = exec.EstimateRequiredMemory();
            Assert.True(memoryUsage < MaxTestMemoryUsage, $"Expected memory usage: {memoryUsage} bytes");

            var task = exec.MainEntryPoint;
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
            var exec = _compiler.CreateExecutionSet(mainEntryPoint);

            long memoryUsage = exec.EstimateRequiredMemory();
            Assert.True(memoryUsage < MaxTestMemoryUsage, $"Expected memory usage: {memoryUsage} bytes");

            var task = exec.MainEntryPoint;
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
            ExecuteComplexProgramSuccess<Func<int, int, int>>(SimpleLedBinding.RunBlink, false, 6, 1000);
        }

        [Fact]
        public void DisplayHelloWorld()
        {
            ExecuteComplexProgramSuccess<Func<int>>(UseI2cDisplay.Run, false);
        }

        [Fact]
        public void ExpectArrayIndexOutOfBounds()
        {
            ExecuteComplexProgramCausesException<Func<int, int>, IndexOutOfRangeException>(typeof(ArduinoNativeLibraryTest), OutOfBoundsCheck, 10);
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

        private static int OutOfBoundsCheck(int index)
        {
            int[] array = new int[2];
            return array[index];
        }

        private static int DivideByZero(int zero)
        {
            return 10 / zero;
        }

        private static int OutOfMemory(int sizeToAllocate)
        {
            int[] array = new int[sizeToAllocate];
            return array.Length;
        }

        [Fact]
        public void GetDataFromStaticByteField()
        {
            ExecuteComplexProgramSuccess<Func<int, int, int>>(ClassWithStaticByteField.GetFirstByte, true, 0, 0);
        }

        /// <summary>
        /// This test not only tests the value of BitConverter.IsLittleEndian but also whether accessing a static
        /// field of a class with a native implementation works
        /// </summary>
        [Fact]
        public void CpuIsLittleEndian()
        {
            ExecuteComplexProgramSuccess<Func<int>>(IsLittleEndianTest, true);
        }

        private int IsLittleEndianTest()
        {
            return BitConverter.IsLittleEndian ? 1 : 0;
        }

        [Fact]
        public void ClassWith64BitFieldTest()
        {
            ExecuteComplexProgramSuccess<Func<int>>(ClassWith64BitField.ClassMain, true);
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
            if (dt.DayOfYear != 21)
            {
                return dt.DayOfYear;
            }

            return 1;
        }

        public int MethodCallOnGenericTest()
        {
            var obj1 = new ClassWithGenericParameter<int>(2);
            MiniAssert.That(obj1.CompareTo(2) == 0);
            MiniAssert.That(obj1.CompareTo(3) == -1);

            var obj2 = new ClassWithGenericParameter<string>("Test");
            MiniAssert.That(obj2.CompareTo("Test") == 0);

            return 1;
        }

        [Fact]
        public void MethodCallOnValueTypeTest()
        {
            ExecuteComplexProgramSuccess<Func<int>>(MethodCallOnValueType, true);
        }

        [Fact]
        public void MethodCallOnGenericClass()
        {
            ExecuteComplexProgramSuccess<Func<int>>(MethodCallOnGenericTest, true);
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
            ExecuteComplexProgramSuccess<Func<int, int, int>>(ClassWithStaticField2.GetFirstByte, true, 0, 0);
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
            ExecuteComplexProgramSuccess<Func<int, int, int>>(ClassWithStaticIntField.GetFirst, true, 7, 0);
        }

        [Fact]
        public void SimpleDelegateTest()
        {
            ExecuteComplexProgramSuccess<Func<int>>(ClassWithAnEvent.Test1, true);
        }

        [Fact]
        public void NonStaticDelegateTest()
        {
            ExecuteComplexProgramSuccess<Func<int>>(ClassWithAnEvent.Test2, true);
        }

        public class ClassWithAnEvent
        {
            public Func<int>? RegisterHere;

            private int _myValue;

            public ClassWithAnEvent()
            {
            }

            public static int Test1()
            {
                ClassWithAnEvent ev = new ClassWithAnEvent();
                ev.RegisterHere = StaticNonVoidMethod;
                int result = ev.FireEvent();
                MiniAssert.That(result == 1);
                return result;
            }

            public static int Test2()
            {
                ClassWithAnEvent ev = new ClassWithAnEvent();
                ev._myValue = 2;
                ev.RegisterHere = ev.NonVoidMethod;
                int result = ev.FireEvent();
                MiniAssert.That(result == 2);
                return result - 1;
            }

            public static int StaticNonVoidMethod()
            {
                return 1;
            }

            public int NonVoidMethod()
            {
                return _myValue;
            }

            public int FireEvent()
            {
                if (RegisterHere != null)
                {
                    return RegisterHere.Invoke();
                }

                return -1;
            }
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

        public class ClassWithGenericParameter<T>
            where T : IComparable<T>
        {
            private T _a;

            public ClassWithGenericParameter(T a)
            {
                _a = a;
            }

            public int CompareTo(T other)
            {
                return _a.CompareTo(other);
            }
        }

        public class UseI2cDisplay
        {
            public static int Run()
            {
                using I2cDevice i2cDevice = new ArduinoNativeI2cDevice(new I2cConnectionSettings(1, 0x27));
                using LcdInterface lcdInterface = LcdInterface.CreateI2c(i2cDevice, false);
                using Hd44780 hd44780 = new Lcd2004(lcdInterface);
                hd44780.UnderlineCursorVisible = false;
                hd44780.BacklightOn = true;
                hd44780.DisplayOn = true;
                hd44780.Clear();
                hd44780.Write("Hello World!");
                return 1;
            }
        }
    }
}
