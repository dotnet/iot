using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Arduino.Tests;
using Xunit;

namespace Iot.Device.Arduino.Tests
{
    public sealed class FirmataIlExecutorTests : IClassFixture<FirmataTestFixture>, IDisposable
    {
        private FirmataTestFixture _fixture;
        private ArduinoCsCompiler _compiler;

        public FirmataIlExecutorTests(FirmataTestFixture fixture)
        {
            _fixture = fixture;
            _compiler = new ArduinoCsCompiler(fixture.Board, true);
            _compiler.ClearAllData(true);
        }

        public void Dispose()
        {
            _compiler.Dispose();
        }

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

        private void LoadCodeMethod<T1, T2, T3>(Type type, string methodName, T1 a, T2 b, T3 expectedResult)
        {
            var methods = type.GetMethods().Where(x => x.Name == methodName).ToList();
            Assert.Single(methods);
            var set = _compiler.CreateExecutionSet();
            _compiler.PrepareLowLevelInterface(set);
            CancellationTokenSource cs = new CancellationTokenSource(TimeSpan.FromSeconds(20));
            var method = _compiler.AddSimpleMethod(set, methods[0]);

            // First execute the method locally, so we don't have an error in the test
            T3 result = (T3)methods[0].Invoke(null, new object[] { a!, b! });
            Assert.Equal(expectedResult, result);

            // This assertion fails on a timeout
            Assert.True(method.Invoke(cs.Token, a!, b!));

            // The task has terminated
            Assert.Equal(MethodState.Stopped, method.State);

            Assert.True(method.GetMethodResults(set, out object[] data, out MethodState state));
            // The only result is from the end of the method
            Assert.Equal(MethodState.Stopped, state);
            Assert.Single(data);

            result = (T3)data[0];
            Assert.Equal(expectedResult, result);
            method.Dispose();
        }

        [Theory]
        [InlineData("Equal", 2, 2, true)]
        [InlineData("Equal", 2000, 1999, false)]
        [InlineData("Equal", -1, -1, true)]
        [InlineData("SmallerS", 1, 2, true)]
        [InlineData("SmallerOrEqualS", 7, 20, true)]
        [InlineData("SmallerOrEqualS", 7, 7, true)]
        [InlineData("SmallerOrEqualS", 21, 7, false)]
        [InlineData("GreaterS", 0x12345678, 0x12345677, true)]
        [InlineData("GreaterOrEqualS", 2, 2, true)]
        [InlineData("GreaterOrEqualS", 787878, 787877, true)]
        [InlineData("GreaterThanConstantS", 2701, 0, true)]
        [InlineData("ComparisonOperators1", 1, 2, true)]
        [InlineData("ComparisonOperators2", 100, 200, true)]
        [InlineData("SmallerS", -1, 1, true)]
        [InlineData("Unequal", 2, 2, false)]
        [InlineData("SmallerOrEqualS", -2, -1, true)]
        [InlineData("SmallerOrEqualS", -2, -2, true)]
        public void TestBooleanOperation(string methodName, int argument1, int argument2, bool expected)
        {
            LoadCodeMethod(GetType(), methodName, argument1, argument2, expected);
        }

        [Theory]
        [InlineData("AddS", 10, 20, 30)]
        [InlineData("AddS", 10, -5, 5)]
        [InlineData("AddS", -5, -2, -7)]
        [InlineData("SubS", 10, 2, 8)]
        [InlineData("SubS", 10, -2, 12)]
        [InlineData("SubS", -2, -2, 0)]
        [InlineData("SubS", -4, 1, -5)]
        [InlineData("MulS", 4, 6, 24)]
        [InlineData("MulS", -2, -2, 4)]
        [InlineData("MulS", -2, 2, -4)]
        [InlineData("DivS", 10, 5, 2)]
        [InlineData("DivS", 10, 20, 0)]
        [InlineData("DivS", -10, 2, -5)]
        [InlineData("DivS", -10, -2, 5)]
        [InlineData("ModS", 10, 2, 0)]
        [InlineData("ModS", 11, 2, 1)]
        [InlineData("ModS", -11, 2, -1)]
        [InlineData("LshS", 8, 1, 16)]
        [InlineData("RshS", 8, 1, 4)]
        [InlineData("RshS", -8, 1, -4)]
        [InlineData("AndS", 0xF0, 0x1F, 0x10)]
        [InlineData("AndS", 0xF0, 0x00, 0x00)]
        [InlineData("OrS", 0xF0, 0x0F, 0xFF)]
        [InlineData("NotS", 0xF0, 0, -241)]
        [InlineData("NegS", -256, 0, 256)]
        [InlineData("XorS", 0x0F, 0x03, 12)]
        [InlineData("RshUnS", -8, 1, 2147483644)]
        public void TestArithmeticOperationSigned(string methodName, int argument1, int argument2, int expected)
        {
            LoadCodeMethod(GetType(), methodName, argument1, argument2, expected);
        }

        [Theory]
        [InlineData("AddU", 10u, 20u, 30u)]
        [InlineData("AddU", 10u, -5u, 5u)]
        [InlineData("AddU", -5u, -2u, -7u)]
        [InlineData("SubU", 10u, 2u, 8u)]
        [InlineData("SubU", 10u, -2u, 12u)]
        [InlineData("SubU", -2u, -2u, 0u)]
        [InlineData("SubU", -4u, 1u, -5u)]
        [InlineData("MulU", 4u, 6u, 24u)]
        [InlineData("DivU", 10u, 5u, 2u)]
        [InlineData("DivU", 10u, 20u, 0u)]
        [InlineData("ModU", 10u, 2u, 0u)]
        [InlineData("ModU", 11u, 2u, 1u)]
        [InlineData("RshU", 8u, 1u, 4u)]
        [InlineData("RshU", -8u, 1u, 2147483644)]
        [InlineData("AndU", 0xF0u, 0x1Fu, 0x10u)]
        [InlineData("AndU", 0xF0u, 0x00u, 0x00u)]
        [InlineData("OrU", 0xF0u, 0x0Fu, 0xFFu)]
        [InlineData("NotU", 0xF0u, 0u, 0xFFFFFF0Fu)]
        [InlineData("XorU", 0x0Fu, 0x03u, 12)]
        [InlineData("RshUnU", -8u, 1, 0x7FFFFFFCu)]
        public void TestArithmeticOperationUnsigned(string methodName, Int64 argument1, Int64 argument2, Int64 expected)
        {
            // Method signature as above, otherwise the test data conversion fails
            LoadCodeMethod(GetType(), methodName, (uint)argument1, (uint)argument2, (uint)expected);
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

        [Theory]
        [InlineData("ResultTypesTest", 50, 20, 70)]
        public void TestTypeConversions(string methodName, UInt32 argument1, int argument2, Int32 expected)
        {
            LoadCodeMethod(GetType(), methodName, argument1, argument2, expected);
        }

        [Theory]
        [InlineData("IntArrayTest", 4, 1, 3)]
        [InlineData("IntArrayTest", 10, 2, 3)]
        [InlineData("CharArrayTest", 10, 2, 'C')]
        [InlineData("CharArrayTest", 10, 0, 'A')]
        [InlineData("ByteArrayTest", 10, 0, 255)]
        public void ArrayTests(string methodName, Int32 argument1, Int32 argument2, Int32 expected)
        {
            LoadCodeMethod(GetType(), methodName, argument1, argument2, expected);
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
            array[3] = 'C';
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
    }
}
