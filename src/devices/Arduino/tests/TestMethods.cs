using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Iot.Device.Arduino.Runtime;
using UnitsNet.Units;

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
    }
}
