using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArduinoCsCompiler.Runtime;
using Iot.Device.CharacterLcd;
using Iot.Device.Graphics;
using UnitsNet;
using UnitsNet.Units;
using Xunit;

namespace Iot.Device.Arduino.Tests
{
    /// <summary>
    /// The methods in this class are loaded by name. They are NOT unused! See <see cref="FirmataIlExecutorTests"/>.
    /// Having them in a separate class prevents accidentally referencing the test class and its dependencies (i.e. ArduinoBoard) into the execution set.
    /// </summary>
    public class TestMethods
    {
        public static uint AddU(uint a, uint b)
        {
            return a + b;
        }

        public static uint SubU(uint a, uint b)
        {
            return a - b;
        }

        public static uint MulU(uint a, uint b)
        {
            return a * b;
        }

        public static uint DivU(uint a, uint b)
        {
            return a / b;
        }

        public static uint ModU(uint a, uint b)
        {
            return a % b;
        }

        public static uint AndU(uint a, uint b)
        {
            return a & b;
        }

        public static uint OrU(uint a, uint b)
        {
            return a | b;
        }

        public static uint NotU(uint a, uint b)
        {
            return ~a;
        }

        public static uint RshU(uint a, uint b)
        {
            return a >> (int)b;
        }

        public static uint XorU(uint a, uint b)
        {
            return a ^ b;
        }

        public static uint RshUnU(uint a, uint b)
        {
            return a >> (int)b;
        }

        public static bool EqualU(uint a, uint b)
        {
            return a == b;
        }

        public static bool UnequalU(uint a, uint b)
        {
            return (a != b);
        }

        public static bool SmallerU(uint a, uint b)
        {
            return a < b;
        }

        public static bool SmallerOrEqualU(uint a, uint b)
        {
            return a <= b;
        }

        public static bool GreaterU(uint a, uint b)
        {
            return a > b;
        }

        public static bool GreaterOrEqualU(uint a, uint b)
        {
            return a >= b;
        }

        public static bool GreaterThanConstantU(uint a, uint b)
        {
            return a > 2700;
        }

        public static int AddS(int a, int b)
        {
            return a + b;
        }

        public static int AddCheckedS(int a, int b)
        {
            checked
            {
                return a + b;
            }
        }

        public static int MulCheckedS(int a, int b)
        {
            checked
            {
                return a * b;
            }
        }

        public static UInt32 SubCheckedU(UInt32 a, UInt32 b)
        {
            checked
            {
                return a - b;
            }
        }

        public static int SubS(int a, int b)
        {
            return a - b;
        }

        public static int MulS(int a, int b)
        {
            return a * b;
        }

        public static int DivS(int a, int b)
        {
            return a / b;
        }

        public static int ModS(int a, int b)
        {
            return a % b;
        }

        public static float AddF(float a, float b)
        {
            return a + b;
        }

        public static float SubF(float a, float b)
        {
            return a - b;
        }

        public static float MulF(float a, float b)
        {
            return a * b;
        }

        public static float DivF(float a, float b)
        {
            return a / b;
        }

        public static float ModF(float a, float b)
        {
            return a % b;
        }

        public static double AddD(double a, double b)
        {
            return a + b;
        }

        public static double SubD(double a, double b)
        {
            return a - b;
        }

        public static double MulD(double a, double b)
        {
            return a * b;
        }

        public static double DivD(double a, double b)
        {
            return a / b;
        }

        public static double ModD(double a, double b)
        {
            return a % b;
        }

        public static double LoadDoubleConstant(double a, double b)
        {
            double ret = 1.0;
            ret += a;
            ret *= 2.0;
            return ret + a;
        }

        public static float LoadFloatConstant(float a, float b)
        {
            float ret = 1.0f;
            ret += a;
            ret *= 2.0f;
            return ret + a;
        }

        public static int AndS(int a, int b)
        {
            return a & b;
        }

        public static int OrS(int a, int b)
        {
            return a | b;
        }

        public static int NotS(int a, int b)
        {
            return ~a;
        }

        public static int NegS(int a, int b)
        {
            return -a;
        }

        public static int LshS(int a, int b)
        {
            return a << b;
        }

        public static int RshS(int a, int b)
        {
            return a >> b;
        }

        public static int XorS(int a, int b)
        {
            return a ^ b;
        }

        public static int RshUnS(int a, int b)
        {
            return (int)((uint)a >> b);
        }

        public static bool Equal(int a, int b)
        {
            return a == b;
        }

        public static bool Unequal(int a, int b)
        {
            return (a != b);
        }

        public static bool SmallerS(int a, int b)
        {
            return a < b;
        }

        public static bool SmallerOrEqualS(int a, int b)
        {
            return a <= b;
        }

        public static bool GreaterS(int a, int b)
        {
            return a > b;
        }

        public static bool GreaterOrEqualS(int a, int b)
        {
            return a >= b;
        }

        public static bool GreaterThanConstantS(int a, int b)
        {
            return a > 2700;
        }

        /// <summary>
        /// Passes when inputValue1 is 1 and inputValue2 is 2
        /// </summary>
        /// <param name="inputValue1">Must be 1</param>
        /// <param name="inputValue2">Must be 2</param>
        /// <returns></returns>
        public static bool ComparisonOperators1(int inputValue1, int inputValue2)
        {
            if (inputValue2 >= 20)
            {
                return false;
            }

            if (inputValue1 > inputValue2)
            {
                return false;
            }

            if (inputValue2 != 2)
            {
                return false;
            }

            if (inputValue1 <= 0)
            {
                return false;
            }

            return true;
        }

        // inputValue1 = 100, inputValue2 = 200
        public static bool ComparisonOperators2(int inputValue1, int inputValue2)
        {
            int v = inputValue2;
            while (v-- > inputValue1)
            {
                inputValue2++;
            }

            if (v > inputValue2)
            {
                return false;
            }

            v = 1;
            for (int i = 0; i <= 10; i++)
            {
                v += i;
            }

            if (v < 10)
            {
                return false;
            }

            return true;
        }

        public static Int32 ResultTypesTest(UInt32 argument1, Int32 argument2)
        {
            // Checks the result types of mixed-type arithmetic operations
            Int16 a = 10;
            UInt16 b = 20;
            Int32 result = (a + b);
            if (result != 30)
            {
                return 0;
            }

            Int32 a2 = (Int32)argument1;

            return a2 + argument2;
        }

        public static Int32 ResultTypesTest2(UInt32 argument1, Int32 argument2)
        {
            // Checks the result types of mixed-type arithmetic operations
            Int16 a = (Int16)argument1;
            Int32 b = argument2;
            Int32 result = (a + b);
            return result;
        }

        public static int IntArrayTest(int size, int index)
        {
            int[] array = new int[size];
            array[index] = 3;
            array[array.Length - 1] = 2;
            return array[index];
        }

        public static int CharArrayTest(int size, int indexToRetrieve)
        {
            char[] array = new char[size];
            array[0] = 'A';
            array[1] = 'B';
            array[2] = 'C';
            return array[indexToRetrieve];
        }

        public static int ByteArrayTest(int size, int indexToRetrieve)
        {
            byte[] array = new byte[size];
            array[0] = 0xFF;
            array[1] = 1;
            array[3] = 2;
            return array[indexToRetrieve];
        }

        public static int BoxedArrayTest(int size, int indexToRetrieve)
        {
            object[] array = new object[size];
            array[0] = new object();
            array[1] = 2;
            array[2] = 7;
            return (int)array[indexToRetrieve];
        }

        public static int StaggedArrayTest(int size1, int size2)
        {
            char[][] staggedArray = new char[size1][];
            staggedArray[1] = new char[size2];
            staggedArray[1][1] = '3';
            return staggedArray[1][1];
        }

        public static int StructCtorBehaviorTest1(int size, int indexToRetrieve)
        {
            object[] array = new object[size];
            array[0] = new object();
            array[1] = new SmallStruct(2);
            array[2] = 7;

            SmallStruct t = (SmallStruct)array[indexToRetrieve];
            return (int)t.Ticks;
        }

        public static int StructCtorBehaviorTest2(int size, int indexToRetrieve)
        {
            SmallStruct s = default;
            s.Ticks = size;
            return (int)s.Ticks;
        }

        public static int StructMethodCall1(int arg1, int arg2)
        {
            SmallStruct s = default;
            s.Ticks = arg1;
            s.Add(arg2);
            var t = -s;
            return (int)t.Ticks;
        }

        public static int StructMethodCall2(int arg1, int arg2)
        {
            SmallStruct s = default;
            s.Ticks = arg1;
            s.Negate();
            return (int)s.Ticks;
        }

        public static int StructInterfaceCall1(int arg1, int arg2)
        {
            SmallStruct s = new SmallStruct(arg1);
            s.Subtract(arg2);
            return (int)s.Ticks;
        }

        public static int StructInterfaceCall2(int arg1, int arg2)
        {
            // Unlike the above, this one does not return the expected result, since it boxes an instance which is later discarded
            SmallStruct s = new SmallStruct(arg1);
            ISubtractable sub = s;
            sub.Subtract(arg2);
            return (int)s.Ticks;
        }

        public static int StructInterfaceCall3(int arg1, int arg2)
        {
            // This one works, though
            SmallStruct s = new SmallStruct(arg1);
            ISubtractable sub = s;
            sub.Subtract(arg2);
            // Unbox the changed instance
            SmallStruct s2 = (SmallStruct)sub;
            return (int)s2.Ticks;
        }

        public static int StructArray(int size, int indexToRetrieve)
        {
            SmallStruct a = new SmallStruct(2);
            SmallStruct[] array = new SmallStruct[size];
            array[0].Ticks = 5;
            array[1] = a;
            array[2] = new SmallStruct(10);

            a.Ticks = 27;
            if (array[1].Ticks == 27)
            {
                // This shouldn't happen (copying a to the array above should make a copy)
                throw new InvalidProgramException();
            }

            SmallStruct t = array[indexToRetrieve];
            return (int)t.Ticks;
        }

        public static int LargeStructCtorBehaviorTest1(int size, int indexToRetrieve)
        {
            object[] array = new object[size];
            array[0] = new object();
            array[1] = new LargeStruct(2, 3, 4);
            array[2] = 7;

            LargeStruct t = (LargeStruct)array[indexToRetrieve];
            if ((int)array[2] != 7)
            {
                throw new InvalidProgramException();
            }

            return (int)t.D;
        }

        public static int LargeStructCtorBehaviorTest2(int size, int indexToRetrieve)
        {
            LargeStruct s = default;
            s.D = size;
            return (int)s.D;
        }

        public static int LargeStructMethodCall2(int arg1, int arg2)
        {
            LargeStruct s = default;
            s.D = arg1;
            s.Sum();
            s.D = arg2;
            return s.TheSum;
        }

        public static int LargeStructAsInterface1(int arg1, int arg2)
        {
            IDisposable d = new LargeStructWithInterface(arg1, arg2, arg1 + arg2);

            LargeStructWithInterface wi = (LargeStructWithInterface)d;
            MiniAssert.That(wi.A == arg1);
            MiniAssert.That(wi.D == arg1 + arg2);

            d.Dispose();

            // This is unaffected (because the copy was not modified)
            MiniAssert.That(wi.D == arg1 + arg2);

            LargeStructWithInterface wi2 = (LargeStructWithInterface)d;
            MiniAssert.That(wi2.D == -1);
            return 1;
        }

        /// <summary>
        /// There are three different implementations of GetEnumerator(), only 2 do an explicit boxing, one returns a value type
        /// </summary>
        public static int LargeStructList1(int arg1, int arg2)
        {
            List<LargeStructWithInterface> l = new List<LargeStructWithInterface>();
            l.Add(new LargeStructWithInterface(1, 2, 3));
            l.Add(new LargeStructWithInterface(4, 5, 6));
            // This calls List<T>.GetEnumerator() which returns Enumerator (a value type)
            foreach (var e in l)
            {
                MiniAssert.That(e.A != 0);
            }

            MiniAssert.That(l.Count == 2);

            // This calls IEnumerable<T>.GetEnumerator(), which returns an IEnumerator<T>
            var single = l.Single(x => x.A == 1);
            MiniAssert.That(single.A == 1);

            return 1;
        }

        public static int LargeStructList2(int arg1, int arg2)
        {
            List<LargeStructWithInterface> l = new List<LargeStructWithInterface>();
            l.Add(new LargeStructWithInterface(1, 2, 3));
            l.Add(new LargeStructWithInterface(4, 5, 6));

            IEnumerable l2 = l;
            // This calls IEnumerable.GetEnumerator() which returns an untyped IEnumerator
            foreach (LargeStructWithInterface e2 in l2)
            {
                MiniAssert.That(e2.A != 0);
            }

            MiniAssert.That(l.Count == 2);

            return 1;
        }

        public static int LargeStructArray(int size, int indexToRetrieve)
        {
            LargeStruct a = new LargeStruct(2, 10, -1);
            LargeStruct[] array = new LargeStruct[size];
            array[0].D = 5;
            array[1] = a;
            array[2] = new LargeStruct(11, 12, 13);

            a.D = 27;
            if (array[1].D == 27)
            {
                // This shouldn't happen (copying a to the array above should make a copy)
                throw new InvalidProgramException();
            }

            LargeStruct t = array[indexToRetrieve];
            return t.B;
        }

        public static int IterateOverArray1(int arg1, int arg2)
        {
            string[] array = new string[]
            {
                "Hello", "World", "of", "strings"
            };

            foreach (var s in array)
            {
                MiniAssert.NotNull(s);
                MiniAssert.That(s.Length > 1);
            }

            return 1;
        }

        /// <summary>
        /// The code of this method is significantly different from the above. While the above code is optimized to a for loop,
        /// this one uses string[].GetIterator(), which is an implicitly compiler-generated function that exists on arrays, because T[] shall implement IList{T}
        /// </summary>
        public static int IterateOverArray2(int arg1, int arg2)
        {
            string[] array = new string[]
            {
                "Hello", "World", "of", "strings"
            };

            IEnumerable<string> enumerableString = array;

            foreach (var s in enumerableString)
            {
                MiniAssert.NotNull(s);
                MiniAssert.That(s.Length > 1);
            }

            return 1;
        }

        /// <summary>
        /// Same as above, but this time with a value array
        /// </summary>
        public static int IterateOverArray3(int arg1, int arg2)
        {
            int[] array = new int[]
            {
               3, 4, 5, 6
            };

            IEnumerable<int> enumerable = array;

            foreach (var s in enumerable)
            {
                MiniAssert.That(s > 0);
            }

            return 1;
        }

        public static int SpanImplementationBehavior(int a, int b)
        {
            Span<byte> span = stackalloc byte[]
            {
                (byte)a,
                (byte)b,
                (byte)(a + 1),
            };

            MiniAssert.That(span.Length == 3);

            return span[1];
        }

        public static int CastClassTest(int arg1, int arg2)
        {
            SmallBase s = new SmallBase(1);
            IDisposable d = s;

            SmallDerived derived = new SmallDerived(2);
            SmallBase s2 = derived;
            s2.Foo(10);
            MiniAssert.That(s2.A == 11);

            SmallDerived derived2 = (SmallDerived)s2;
            derived2.A = 12;
            MiniAssert.That(s2.A == 12);

            ICloneable c = derived2;
            ICloneable c2 = (ICloneable)c.Clone();
            SmallDerived derived3 = (SmallDerived)c2;
            MiniAssert.That(derived3.A == 12);

            d.Dispose();
            MiniAssert.That(s.A == -1);

            return 1;
        }

        public static int SimpleEnumHasValues(int arg1, int arg2)
        {
            TestEnum t1 = TestEnum.None;
            TestEnum t2 = TestEnum.Two;
            MiniAssert.That(t1 == TestEnum.None);
            MiniAssert.That(t1 != TestEnum.One);
            MiniAssert.That(t1 != t2);
            MiniAssert.That(2 == (int)t2);
            return 1;
        }

        public static int EnumsHaveNames(int arg1, int arg2)
        {
            TestEnum t1 = TestEnum.Three;
            MiniAssert.That("3" == t1.ToString());
            return 1;
        }

        public static int EnumGetValues1(int arg1, int arg2)
        {
            var array = Enum.GetValues(typeof(TestEnum));
            var typedArray = array.Cast<TestEnum>().ToArray();
            MiniAssert.That(typedArray.Count() == 4);
            MiniAssert.That(typedArray[0] == TestEnum.None);
            MiniAssert.That(typedArray[1] == TestEnum.One);
            return 1;
        }

        public static int EnumGetValues2(int arg1, int arg2)
        {
            var array = Enum.GetValues<TestEnum>();
            MiniAssert.That(array.Count() == 4);
            MiniAssert.That(array[0] == TestEnum.None);
            MiniAssert.That(array[1] == TestEnum.One);
            return 1;
        }

        public static double DoubleToString(double arg1, double arg2)
        {
            string result = arg1.ToString("F2") + " °C";
            MiniAssert.That(result == "20.23 °C");
            return arg1;
        }

        public static double DoubleToString2(double arg1, double arg2)
        {
            string result = arg1.ToString();
            MiniAssert.AreEqual("20.23", result);
            return arg1;
        }

        public static int IntToString1(int arg1, int arg2)
        {
            string result = arg1.ToString();
            MiniAssert.That(result == "20304", result);
            result = arg2.ToString();
            MiniAssert.That(result == "0", result);
            return 1;
        }

        public static int IntToString2(int arg1, int arg2)
        {
            string result = $"Argument 1 is {arg1} and argument 2 is {arg2}";
            MiniAssert.That(result == "Argument 1 is 20304 and argument 2 is 0", result);
            return 1;
        }

        public static int IntToString3(int arg1, int arg2)
        {
            string result = $"Argument 1 is {arg1} and argument 2 is {arg2}";
            MiniAssert.That(result == "Argument 1 is -20304 and argument 2 is 0", result);
            return 1;
        }

        /// <summary>
        /// A broken Array.Copy implementation caused this to fail after more than 3 elements had been added
        /// </summary>
        public static int DictionaryTest1(int arg1, int arg2)
        {
            Dictionary<char, byte> charDict = new Dictionary<char, byte>();
            for (char c = 'a'; c <= 'z'; c++)
            {
                charDict.Add(c, (byte)c);
            }

            foreach (var ch in charDict.Keys)
            {
                MiniAssert.That(ch != 0);
                Debug.WriteLine($"Char is {ch}, int {(int)ch}");
            }

            Dictionary<char, byte> copy = new Dictionary<char, byte>(charDict);
            MiniAssert.That(copy['b'] == (byte)'b');
            return 1;
        }

        public static int DictionaryTest2(int arg1, int arg2)
        {
            Dictionary<char, byte> charDict = new Dictionary<char, byte>();
            charDict.Add('°', 0b1101_1111);

            MiniAssert.That(charDict['°'] == 0b1101_1111);
            return 1;
        }

        public static int LcdCharacterEncodingTest1(int arg1, int arg2)
        {
            LcdCharacterEncoding encoding = LcdConsole.CreateEncoding(CultureInfo.CreateSpecificCulture("de-CH"), "A00", '?', 8);
            string testString1 = "Abc";
            MiniAssert.That(3 == encoding.GetByteCount(testString1), "byte count does not match");
            byte[] encoded = encoding.GetBytes(testString1);
            MiniAssert.That((byte)'A' == encoded[0]);
            MiniAssert.That((byte)'b' == encoded[1]);
            MiniAssert.That((byte)'c' == encoded[2]);
            return 1;
        }

        public static int LcdCharacterEncodingTest2(int arg1, int arg2)
        {
            LcdCharacterEncoding encoding = LcdConsole.CreateEncoding(CultureInfo.CreateSpecificCulture("de-CH"), "A00", '?', 8);
            string testString1 = "22.76 °C";
            char symbol = testString1[6];
            MiniAssert.That(symbol == 0xb0);
            MiniAssert.That(8 == encoding.GetByteCount(testString1), "byte count does not match");
            byte[] encoded = encoding.GetBytes(testString1);
            var encodedByte = encoded[6];
            MiniAssert.That(0b1101_1111 == encodedByte, $"Byte is 0x{(int)encodedByte:X4}"); // Place of the ° character in the A00 ROM map (at least something that looks like it)
            MiniAssert.That((byte)'C' == encoded[7]);
            return 1;
        }

        public static int UnitsNetTemperatureTest(int arg1, int arg2)
        {
            Temperature t1 = Temperature.FromDegreesCelsius(0);
            MiniAssert.AreEqual(273.15, t1.Kelvins, 0.01);
            t1 = Temperature.FromDegreesCelsius(20.0);
            MiniAssert.AreEqual(273.15 + 20.0, t1.Kelvins, 0.01);
            return 1;
        }

        public static int NormalTryCatchNoException(int arg1, int arg2)
        {
            int a = 2;
            try
            {
                a = 1;
            }
            catch (NotImplementedException)
            {
                return 78;
            }

            return a;
        }

        public static int NormalTryFinallyNoException(int arg1, int arg2)
        {
            int a = 2;
            try
            {
                a = 3;
            }
            finally
            {
                a = 1;
            }

            return a;
        }

        public static int NormalTryFinallyWithReturn(int arg1, int arg2)
        {
            int a = 1;
            var m = new MyDisposable();
            try
            {
                return a;
            }
            finally
            {
                m.Dispose();
            }
        }

        public static int NormalTryCatchWithException(int arg1, int arg2)
        {
            int a = 2;
            try
            {
                a = 5;
                throw new NotSupportedException("This is an exception");
            }
            catch (NotSupportedException)
            {
                a = 1;
            }

            return a;
        }

        public static int NormalTryCatchWithException2(int arg1, int arg2)
        {
            try
            {
                throw new Win32Exception(arg1);
            }
            catch (Win32Exception x)
            {
                MiniAssert.NotNull(x.Message);
                return x.NativeErrorCode;
            }
        }

        private class SmallBase : IDisposable
        {
            private int _a;
            public SmallBase(int a)
            {
                _a = a;
            }

            public int A
            {
                get => _a;
                set => _a = value;
            }

            public virtual void Foo(int x)
            {
                _a = x;
            }

            protected virtual void Dispose(bool disposing)
            {
                if (disposing)
                {
                    _a = -1;
                }
                else
                {
                    _a = 2;
                }
            }

            public void Dispose()
            {
                Dispose(true);
            }
        }

        private class SmallDerived : SmallBase, ICloneable
        {
            public SmallDerived(int a)
            : base(a)
            {
            }

            public override void Foo(int x)
            {
                A = x + 1;
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);
                A = 5;
            }

            public object Clone()
            {
                return MemberwiseClone();
            }
        }

        public interface ISubtractable
        {
            void Subtract(int amount);
        }

        private struct SmallStruct : ISubtractable
        {
            private Int64 _ticks;

            public SmallStruct(Int64 ticks)
            {
                _ticks = ticks;
            }

            public Int64 Ticks
            {
                get
                {
                    return _ticks;
                }
                set
                {
                    _ticks = value;
                }
            }

            public void Add(int moreTicks)
            {
                _ticks += moreTicks;
            }

            public void Subtract(int ticks)
            {
                _ticks -= ticks;
            }

            public void Negate()
            {
                // stupid implementation, but test case (generates an LDOBJ instruction)
                SmallStruct s2 = -this;
                _ticks = s2.Ticks;
            }

            public static SmallStruct operator -(SmallStruct st)
            {
                return new SmallStruct(-st.Ticks);
            }
        }

        private struct LargeStruct
        {
            private int _a;
            private int _b;
            private long _d;
            private long _sum;

            public LargeStruct(int a, int b, long d)
            {
                _a = a;
                _b = b;
                _d = d;
                _sum = 0;
            }

            public int A => _a;

            public int B => _b;

            public long D
            {
                get
                {
                    return _d;
                }
                set
                {
                    _d = value;
                }
            }

            public int TheSum => (int)_sum;

            public void Sum()
            {
                _sum = _a + _b + _d;
            }
        }

        private struct LargeStructWithInterface : IDisposable
        {
            private int _a;
            private int _b;
            private long _d;
            private long _sum;

            public LargeStructWithInterface(int a, int b, long d)
            {
                _a = a;
                _b = b;
                _d = d;
                _sum = 0;
            }

            public int A => _a;

            public int B => _b;

            public long D
            {
                get
                {
                    return _d;
                }
                set
                {
                    _d = value;
                }
            }

            public long Sum => _sum;

            public void Dispose()
            {
                _sum = -1;
                _d = -1;
            }
        }

        private sealed class MyDisposable : IDisposable
        {
            public MyDisposable()
            {
                IsDisposed = false;
            }

            public bool IsDisposed
            {
                get;
                private set;
            }

            public void Dispose()
            {
                IsDisposed = true;
            }
        }

    }
}
