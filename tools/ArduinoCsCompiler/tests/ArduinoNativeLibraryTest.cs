// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Device.I2c;
using System.Text;
using System.Threading;
using ArduinoCsCompiler;
using ArduinoCsCompiler.Runtime;
using Iot.Device.Arduino;
using Iot.Device.CharacterLcd;
using Xunit;

namespace Iot.Device.Arduino.Tests
{
    /// <summary>
    /// Tests native library functions for the IL Executor
    /// </summary>
    [Collection("SingleClientOnly")]
    [Trait("feature", "firmata")]
    [Trait("requires", "hardware")]
    public class ArduinoNativeLibraryTest : ArduinoTestBase, IClassFixture<FirmataTestFixture>
    {
        private const int MaxTestMemoryUsage = 400000;

        public ArduinoNativeLibraryTest(FirmataTestFixture fixture)
            : base(fixture)
        {
            Compiler.ClearAllData(true, true);
            CompilerSettings = new CompilerSettings()
            {
                CreateKernelForFlashing = false,
                LaunchProgramFromFlash = false,
                UseFlashForKernel = false,
            };
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

            public static int RunBlink(int pin, int delay)
            {
                var gpioController = new GpioController(new ArduinoNativeGpioDriver());
                SimpleLedBinding blink = new SimpleLedBinding(gpioController, pin, delay);
                blink.Loop();
                return 1;
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
        }

        [Fact]
        public void RunBlinkWithGpioController()
        {
            ExecuteComplexProgramSuccess<Func<int, int, int>>(SimpleLedBinding.RunBlink, false, 6, 1000);
        }

        [Fact]
        public void ExpectArrayIndexOutOfBounds()
        {
            ExecuteComplexProgramCausesException<Func<int, int>, IndexOutOfRangeException>(typeof(ArduinoNativeLibraryTest), OutOfBoundsCheck, 10);
        }

        [Fact]
        public void ExpectCustomException()
        {
            ExecuteComplexProgramCausesException<Func<int, int>, NotImplementedException>(typeof(ArduinoNativeLibraryTest), NotImplemented, 10);
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

        private static int NotImplemented(int index)
        {
            throw new NotImplementedException("This method is not implemented on purpose");
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

        private static int IsLittleEndianTest()
        {
            return BitConverter.IsLittleEndian ? 1 : 0;
        }

        public static int MethodCallOnGenericTest()
        {
            var obj1 = new ClassWithGenericParameter<int>(2);
            MiniAssert.That(obj1.CompareTo(2) == 0);
            MiniAssert.That(obj1.CompareTo(3) == -1);

            var obj2 = new ClassWithGenericParameter<string>("Test");
            MiniAssert.That(obj2.CompareTo("Test") == 0);

            return 1;
        }

        public static int MethodCallOnValueType()
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

        [Fact]
        public void ClassWith64BitFieldTest()
        {
            ExecuteComplexProgramSuccess<Func<int>>(ClassWith64BitField.ClassMain, true);
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

            public static int ClassMain()
            {
                var instance = new ClassWith64BitField();
                return instance.GetResult();
            }

            public int GetResult()
            {
                return (int)(_field1 + _field2);
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

        [Fact]
        public void EventHandlerTest()
        {
            ExecuteComplexProgramSuccess<Func<int>>(ClassWithAnEvent.Test3, true);
        }

        [Fact]
        public void OverridingObjectEqualsWorksTest()
        {
            ExecuteComplexProgramSuccess<Func<int>>(ClassThatOverridesObjectEquals.Test1, true);
        }

        [Fact]
        public void OverridingObjectEqualsInderivedClassWorksTest()
        {
            ExecuteComplexProgramSuccess<Func<int>>(ClassThatDoesNotOverrideObjectEquals.Test2, true);
        }

        [Fact]
        public void EqualityDoesNotReturnTrueIfTypeIsNotSame()
        {
            ExecuteComplexProgramSuccess<Func<int>>(ClassThatDoesNotOverrideObjectEquals.Test3, true);
        }

        [Fact]
        public void HashSetTest()
        {
            ExecuteComplexProgramSuccess<Func<int>>(CollectionsTest.TestHashSet, true);
        }

        [Fact]
        public void DictionaryTest()
        {
            ExecuteComplexProgramSuccess<Func<int>>(CollectionsTest.DictionaryTest, true);
        }

        [Fact]
        public void ReflectionTest()
        {
            ExecuteComplexProgramSuccess<Func<int>>(CollectionsTest.ReflectionTest, true);
        }

        [Fact]
        public void CreateInstanceTest()
        {
            CompilerSettings s = new CompilerSettings()
            {
                CreateKernelForFlashing = false,
                LaunchProgramFromFlash = false,
                UseFlashForProgram = false,
                AutoRestartProgram = false,
                MaxMemoryUsage = 100_000,
            };

            ExecuteComplexProgramSuccess<Func<int>>(CollectionsTest.CreateInstanceTest, true, s);
        }

        public class ClassThatOverridesObjectEquals : IEquatable<ClassThatOverridesObjectEquals>
        {
            private readonly int _a;

            public ClassThatOverridesObjectEquals(int a)
            {
                _a = a;
            }

            public static int Test1()
            {
                var c1 = new ClassThatOverridesObjectEquals(42);
                var c2 = new ClassThatOverridesObjectEquals(42);
                MiniAssert.False(ReferenceEquals(c1, c2));
                MiniAssert.That(c1.Equals(c2));
                MiniAssert.That(c1.GetHashCode() == 42);
                object o1 = c1;
                object o2 = c2;
                MiniAssert.That(o2.Equals(o1));
                return 1;
            }

            public bool Equals(ClassThatOverridesObjectEquals? other)
            {
                if (ReferenceEquals(null, other))
                {
                    return false;
                }

                if (ReferenceEquals(this, other))
                {
                    return true;
                }

                return _a == other._a;
            }

            public override bool Equals(object? obj)
            {
                if (ReferenceEquals(null, obj))
                {
                    return false;
                }

                if (ReferenceEquals(this, obj))
                {
                    return true;
                }

                if (obj.GetType() != GetType())
                {
                    return false;
                }

                return Equals((ClassThatOverridesObjectEquals)obj);
            }

            public override int GetHashCode()
            {
                return _a;
            }
        }

        /// <summary>
        /// This class inherits from the above, but does not itself override Equals. Still the value equality should be called
        /// </summary>
        public class ClassThatDoesNotOverrideObjectEquals : ClassThatOverridesObjectEquals
        {
            public ClassThatDoesNotOverrideObjectEquals(int a)
            : base(a)
            {
            }

            public static int Test2()
            {
                var c1 = new ClassThatDoesNotOverrideObjectEquals(42);
                var c2 = new ClassThatDoesNotOverrideObjectEquals(42);
                MiniAssert.False(ReferenceEquals(c1, c2));
                MiniAssert.That(c1.Equals(c2));
                MiniAssert.That(c1.GetHashCode() == 42);
                object o1 = c1;
                object o2 = c2;
                MiniAssert.That(o2.Equals(o1));
                return 1;
            }

            public static int Test3()
            {
                var c1 = new ClassThatDoesNotOverrideObjectEquals(42);
                var c2 = new ClassThatOverridesObjectEquals(42);
                MiniAssert.False(ReferenceEquals(c1, c2));
                MiniAssert.That(c1.Equals(c2)); // Because calls IEqualityComparer<ClassThatDoesNotOverrideObjectEquals>.Equal(), which does not an exact type test
                MiniAssert.False(((object)c1).Equals(c2));
                object o1 = c1;
                object o2 = c2;
                MiniAssert.False(o2.Equals(o1));
                return 1;
            }
        }

        public class ClassWithAnEvent
        {
            // The code generated for simple delegates is quite different from what is required for events (which may have multiple targets)
            public Func<int>? SimpleDelegate;

            public event Action<int>? SimpleEvent;

            private int _myValue;

            public ClassWithAnEvent()
            {
                SimpleEvent = null;
            }

            public static int Test1()
            {
                ClassWithAnEvent ev = new ClassWithAnEvent();
                ev.SimpleDelegate = StaticNonVoidMethod;
                int result = ev.FireDelegate();
                MiniAssert.That(result == 1);
                return result;
            }

            public static int Test2()
            {
                ClassWithAnEvent ev = new ClassWithAnEvent();
                ev._myValue = 2;
                ev.SimpleDelegate = ev.NonVoidMethod;
                int result = ev.FireDelegate();
                MiniAssert.That(result == 2);
                return result - 1;
            }

            public static int Test3()
            {
                ClassWithAnEvent ev = new ClassWithAnEvent();
                ev._myValue = 3;
                ev.SimpleEvent += ev.EventHandler;
                ev.FireEvent(1);
                ev.SimpleEvent -= ev.EventHandler;
                ev.FireEvent(27);
                return ev._myValue;
            }

            public static int StaticNonVoidMethod()
            {
                return 1;
            }

            public int NonVoidMethod()
            {
                return _myValue;
            }

            public void EventHandler(int data)
            {
                _myValue = data;
            }

            public int FireDelegate()
            {
                if (SimpleDelegate != null)
                {
                    return SimpleDelegate.Invoke();
                }

                return -1;
            }

            public void FireEvent(int data)
            {
                if (SimpleEvent != null)
                {
                    SimpleEvent.Invoke(data);
                }
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

        /// <summary>
        /// The implementation of these classes uses quite a few low-level functions of the runtime, and they're used in GpioController.
        /// Therefore do some explicit function testing
        /// </summary>
        public class CollectionsTest
        {
            public static int TestHashSet()
            {
                HashSet<int> mySet = new HashSet<int>();
                mySet.Add(1);
                mySet.Add(2);
                mySet.Add(1);
                MiniAssert.That(mySet.Count == 2);
                MiniAssert.That(mySet.Contains(2));
                mySet.Remove(1);
                MiniAssert.That(mySet.Count == 1);
                return 1;
            }

            public static int DictionaryTest()
            {
                Dictionary<int, PinValue?> dict = new Dictionary<int, PinValue?>();
                dict.Add(2, null);
                MiniAssert.That(dict[2] == null);
                dict.Add(5, null);
                MiniAssert.That(dict.TryGetValue(5, out var value) && value.HasValue == false);
                dict[5] = PinValue.Low;
                MiniAssert.That(dict.TryGetValue(5, out value));
                if (value.HasValue)
                {
                    MiniAssert.That(value.Value == PinValue.Low);
                }
                else
                {
                    MiniAssert.That(false, "Value cannot be retrieved");
                }

                dict.Remove(5);
                MiniAssert.That(dict.ContainsKey(2));
                MiniAssert.False(dict.ContainsKey(5));
                return 1;
            }

            /// <summary>
            /// Tests some supported reflection methods (These are implemented, because they're required by the runtime itself)
            /// </summary>
            /// <returns>Returns 1 on success</returns>
            public static int ReflectionTest()
            {
                // First with a "simple" generic type (only one type argument)
                List<bool> list = new List<bool>();
                var type = list.GetType();
                MiniAssert.That(type.IsGenericType);
                var args = list.GetType().GetGenericArguments();
                MiniAssert.That(args.Length == 1);
                MiniAssert.That(args[0] == typeof(bool));
                var main = type.GetGenericTypeDefinition();
                MiniAssert.That(main == typeof(List<>));
                Type listTypeReconstructed = typeof(List<>).MakeGenericType(typeof(bool));
                MiniAssert.That(listTypeReconstructed == type);

                // Some special cases
                var test = main.GetGenericTypeDefinition();
                MiniAssert.That(test == typeof(List<>));

                var test2 = main.GetGenericArguments();
                MiniAssert.That(test2[0] != null); // This is not really defined

                // Then the more complex case with a complex generic type
                Dictionary<int, string> dictionary = new Dictionary<int, string>();
                var args2 = dictionary.GetType().GetGenericArguments();

                MiniAssert.That(args2.Length == 2);
                MiniAssert.That(args2[0] == typeof(int));
                MiniAssert.That(args2[1] == typeof(string));

                var main2 = dictionary.GetType().GetGenericTypeDefinition();
                MiniAssert.That(main2 == typeof(Dictionary<,>));
                var dictionaryReconstructed = typeof(Dictionary<,>).MakeGenericType(typeof(int), typeof(string));
                MiniAssert.That(dictionaryReconstructed == dictionary.GetType());
                return 1;
            }

            public static int CreateInstanceTest()
            {
                // This is a bit stupid for now: We need to explicitly reference the type, or it won't be included in the execution set.
                // TODO: Add possibility to explicitly include a class in the execution set
                List<int> aList = new List<int>(2);
                GC.KeepAlive(aList);
                Type t = typeof(List<>).MakeGenericType(typeof(int));
                var instance = (List<int>?)Activator.CreateInstance(t, 5);
                MiniAssert.That(instance != null);
                MiniAssert.That(instance!.Capacity >= 5);
                return 1;
            }
        }
    }
}
