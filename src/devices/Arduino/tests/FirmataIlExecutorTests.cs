using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Arduino.Tests;
using Microsoft.VisualBasic.CompilerServices;
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
            Assert.NotNull(_fixture.Board);
            _compiler = new ArduinoCsCompiler(_fixture.Board!, true);
            _compiler.ClearAllData(true, false);
        }

        public void Dispose()
        {
            _compiler.Dispose();
        }

        private void LoadCodeMethod<T1, T2, T3>(string methodName, T1 a, T2 b, T3 expectedResult, CompilerSettings? settings = null)
        {
            var methods = typeof(TestMethods).GetMethods().Where(x => x.Name == methodName).ToList();
            var method = methods.Single();

            if (settings == null)
            {
                settings = new CompilerSettings()
                {
                    CreateKernelForFlashing = false, UseFlashForKernel = false
                };
                settings.AdditionalSuppressions.Add("System.Number");
                settings.AdditionalSuppressions.Add("System.SR");
            }

            var set = _compiler.CreateExecutionSet(methods[0], settings);

            CancellationTokenSource cs = new CancellationTokenSource(TimeSpan.FromSeconds(20));

            // First execute the method locally, so we don't have an error in the test
            var uncastedResult = method.Invoke(null, new object[] { a!, b! });
            if (uncastedResult == null)
            {
                throw new InvalidOperationException("Void methods not supported here");
            }

            T3 result = (T3)uncastedResult;
            Assert.Equal(expectedResult, result);

            var remoteMethod = set.MainEntryPoint;

            // This assertion fails on a timeout
            Assert.True(remoteMethod.Invoke(cs.Token, a!, b!));

            Assert.True(remoteMethod.GetMethodResults(set, out object[] data, out MethodState state));

            // The task has terminated (do this after the above, otherwise the test will not show an exception)
            Assert.Equal(MethodState.Stopped, remoteMethod.State);

            // The only result is from the end of the method
            Assert.Equal(MethodState.Stopped, state);
            Assert.Single(data);

            result = (T3)data[0];
            Assert.Equal(expectedResult, result);
            remoteMethod.Dispose();
        }

        [Fact]
        public void MainMethodMustBeStatic()
        {
            Assert.Throws<InvalidOperationException>(() => _compiler.CreateExecutionSet<Action>(MainMethodMustBeStatic, new CompilerSettings()));
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
            LoadCodeMethod(methodName, argument1, argument2, expected);
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
            LoadCodeMethod(methodName, argument1, argument2, expected);
        }

        [Theory]
        [InlineData("AddF", 10, 20, 30)]
        [InlineData("AddF", 10, -5, 5)]
        [InlineData("AddF", -5, -2, -7)]
        [InlineData("SubF", 10, 2, 8)]
        [InlineData("SubF", 10, -2, 12)]
        [InlineData("SubF", -2.0f, -2.0f, 0)]
        [InlineData("SubF", -4, 1, -5)]
        [InlineData("MulF", 4, 2.5f, 10.0)]
        [InlineData("MulF", -2, -2, 4)]
        [InlineData("MulF", -2, 2, -4)]
        [InlineData("DivF", 1.0f, 5.0f, 0.2f)]
        [InlineData("DivF", 10, 20, 0.5)]
        [InlineData("DivF", -10, 2, -5)]
        [InlineData("DivF", -10, -2, 5)]
        [InlineData("ModF", 10, 2, 0)]
        [InlineData("ModF", 11, 2, 1)]
        [InlineData("ModF", -11, 2, -1)]
        [InlineData("LoadFloatConstant", 0.0, 0.0, 2.0)] // tests the LDC.R4 instruction
        public void TestArithmeticOperationSignedFloat(string methodName, float argument1, float argument2, float expected)
        {
            LoadCodeMethod(methodName, argument1, argument2, expected);
        }

        [Theory]
        [InlineData("AddD", 10, 20.0, 30.0)]
        [InlineData("AddD", 10, -5, 5)]
        [InlineData("AddD", -5, -2, -7)]
        [InlineData("SubD", 10, 2, 8)]
        [InlineData("SubD", 10, -2, 12)]
        [InlineData("SubD", -2.0f, -2.0, 0)]
        [InlineData("SubD", -4, 1, -5)]
        [InlineData("MulD", 4, 2.5f, 10)]
        [InlineData("MulD", -2, -2, 4)]
        [InlineData("MulD", -2, 2, -4)]
        [InlineData("DivD", 1.0f, 5.0f, 0.2)]
        [InlineData("DivD", 10, 20, 0.5)]
        [InlineData("DivD", -10, 2, -5)]
        [InlineData("DivD", -10, -2, 5)]
        [InlineData("ModD", 10, 2, 0)]
        [InlineData("ModD", 11, 2, 1)]
        [InlineData("ModD", -11, 2, -1)]
        [InlineData("LoadDoubleConstant", 0.0, 0.0, 2.0)] // tests the LDC.R8 instruction
        public void TestArithmeticOperationSignedDouble(string methodName, double argument1, double argument2, double expected)
        {
            LoadCodeMethod(methodName, argument1, argument2, expected);
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
            LoadCodeMethod(methodName, (uint)argument1, (uint)argument2, (uint)expected);
        }

        [Theory]
        [InlineData("ResultTypesTest", 50, 20, 70)]
        [InlineData("ResultTypesTest2", 21, -20, 1)]
        public void TestTypeConversions(string methodName, UInt32 argument1, int argument2, Int32 expected)
        {
            LoadCodeMethod(methodName, argument1, argument2, expected);
        }

        [Theory]
        [InlineData("IntArrayTest", 4, 1, 3)]
        [InlineData("IntArrayTest", 10, 2, 3)]
        [InlineData("CharArrayTest", 10, 2, 'C')]
        [InlineData("CharArrayTest", 10, 0, 'A')]
        [InlineData("ByteArrayTest", 10, 0, 255)]
        [InlineData("BoxedArrayTest", 5, 2, 7)]
        [InlineData("StaggedArrayTest", 5, 7, (int)'3')]
        public void ArrayTests(string methodName, Int32 argument1, Int32 argument2, Int32 expected)
        {
            LoadCodeMethod(methodName, argument1, argument2, expected);
        }

        [Theory]
        [InlineData("StructCtorBehaviorTest1", 5, 1, 2)]
        [InlineData("StructCtorBehaviorTest2", 5, 1, 5)]
        [InlineData("StructMethodCall1", 66, 33, -99)]
        [InlineData("StructMethodCall2", 66, 33, -66)]
        [InlineData("StructArray", 5, 2, 10)]
        [InlineData("StructInterfaceCall1", 10, 2, 8)]
        [InlineData("StructInterfaceCall2", 10, 2, 10)]
        [InlineData("StructInterfaceCall3", 15, 3, 12)]
        public void StructTests(string methodName, Int32 argument1, Int32 argument2, Int32 expected)
        {
            LoadCodeMethod(methodName, argument1, argument2, expected);
        }

        [Theory]
        [InlineData("LargeStructCtorBehaviorTest1", 5, 1, 4)]
        [InlineData("LargeStructCtorBehaviorTest2", 5, 1, 5)]
        [InlineData("LargeStructMethodCall2", 66, 33, 66)]
        [InlineData("LargeStructArray", 5, 1, 10)]
        public void LargeStructTest(string methodName, Int32 argument1, Int32 argument2, Int32 expected)
        {
            LoadCodeMethod(methodName, argument1, argument2, expected);
        }

        [Theory]
        [InlineData("CastClassTest", 0, 0, 1)]
        public void CastTest(string methodName, Int32 argument1, Int32 argument2, Int32 expected)
        {
            LoadCodeMethod(methodName, argument1, argument2, expected, new CompilerSettings() { CreateKernelForFlashing = true, UseFlashForKernel = true });
        }

        [Theory]
        [InlineData("SpanImplementationBehavior", 5, 1, 1)]
        public void SpanTest(string methodName, Int32 argument1, Int32 argument2, Int32 expected)
        {
            LoadCodeMethod(methodName, argument1, argument2, expected);
        }
    }
}
