// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using ArduinoCsCompiler;
using Xunit;
using Xunit.Sdk;

namespace Iot.Device.Arduino.Tests
{
    [Collection("SingleClientOnly")]
    [Trait("feature", "firmata")]
    [Trait("requires", "hardware")]
    public sealed class FirmataIlExecutorTests : ArduinoTestBase, IClassFixture<FirmataTestFixture>, IDisposable
    {
        public FirmataIlExecutorTests(FirmataTestFixture fixture)
        : base(fixture)
        {
            Compiler.ClearAllData(true, false);
        }

        private void LoadCodeMethod<T1, T2, T3>(Type testClass, string methodName, T1 a, T2 b, T3 expectedResult, CompilerSettings? settings = null, bool executeLocally = true)
        {
            var methods = testClass.GetMethods().Where(x => x.Name == methodName).ToList();
            var method = methods.Single();

            if (settings == null)
            {
                settings = new CompilerSettings()
                {
                    CreateKernelForFlashing = false,
                    UseFlashForKernel = false
                };
                // settings.AdditionalSuppressions.Add("System.Number");
                settings.AdditionalSuppressions.Add("System.SR");
            }

            if (executeLocally)
            {
                // First execute the method locally, so we don't have an error in the test
                var uncastedResult = method.Invoke(null, new object[]
                {
                    a!, b!
                });
                if (uncastedResult == null)
                {
                    throw new InvalidOperationException("Void methods not supported here");
                }

                T3 result1 = (T3)uncastedResult;
                Assert.Equal(expectedResult, result1);
            }

            ErrorManager.Clear();
            var set = Compiler.PrepareAndRunExecutionSet(method, settings);

            // This always aborts when debugging tests, preventing that we can get stack dumps., so use a looong timeout for that
            CancellationTokenSource cs = new CancellationTokenSource(System.Diagnostics.Debugger.IsAttached ? -1 : (int)TimeSpan.FromSeconds(60).TotalMilliseconds);

            var remoteMethod = set.MainEntryPoint;

            object[] data = new object[0];
            MethodState state = MethodState.Stopped;

            // for (int i = 0; i < 10; i++)
            {
                // This assertion fails on a timeout
                Assert.True(remoteMethod.Invoke(cs.Token, a!, b!));

                Assert.True(remoteMethod.GetMethodResults(set, out data, out state));

                // The task has terminated (do this after the above, otherwise the test will not show an exception)
                Assert.Equal(MethodState.Stopped, remoteMethod.State);
            }

            // The only result is from the end of the method
            Assert.Equal(MethodState.Stopped, state);
            Assert.Single(data);

            Assert.True(ErrorManager.NumErrors == 0, "There were compilation errors");

            T3 result = (T3)data[0];
            Assert.Equal(expectedResult, result);
            remoteMethod.Dispose();
        }

        private void LoadCodeMethodNoCheck<T1, T2>(string methodName, T1 a, T2 b, CompilerSettings? settings = null)
        {
            var methods = typeof(TestMethods).GetMethods().Where(x => x.Name == methodName).ToList();
            var method = methods.Single();

            if (settings == null)
            {
                settings = new CompilerSettings()
                {
                    CreateKernelForFlashing = false,
                    UseFlashForKernel = false
                };
                settings.AdditionalSuppressions.Add("System.Number");
                settings.AdditionalSuppressions.Add("System.SR");
            }

            var set = Compiler.PrepareAndRunExecutionSet(method, settings);

            CancellationTokenSource cs = new CancellationTokenSource(TimeSpan.FromSeconds(20));

            using var remoteMethod = set.MainEntryPoint;

            // This assertion fails on a timeout
            Assert.True(remoteMethod.Invoke(cs.Token, a!, b!));

            Assert.True(remoteMethod.GetMethodResults(set, out object[] data, out MethodState state));

            // The task has terminated (do this after the above, otherwise the test will not show an exception)
        }

        [Fact]
        public void MainMethodMustBeStatic()
        {
            Assert.Throws<InvalidOperationException>(() => Compiler.PrepareAndRunExecutionSet<Action>(MainMethodMustBeStatic, new CompilerSettings()));
        }

        [Theory]
        [InlineData(nameof(TestMethods.Equal), 2, 2, true)]
        [InlineData(nameof(TestMethods.Equal), 2000, 1999, false)]
        [InlineData(nameof(TestMethods.Equal), -1, -1, true)]
        [InlineData(nameof(TestMethods.SmallerS), 1, 2, true)]
        [InlineData(nameof(TestMethods.SmallerOrEqualS), 7, 20, true)]
        [InlineData(nameof(TestMethods.SmallerOrEqualS), 7, 7, true)]
        [InlineData(nameof(TestMethods.SmallerOrEqualS), 21, 7, false)]
        [InlineData(nameof(TestMethods.GreaterS), 0x12345678, 0x12345677, true)]
        [InlineData(nameof(TestMethods.GreaterOrEqualS), 2, 2, true)]
        [InlineData(nameof(TestMethods.GreaterOrEqualS), 787878, 787877, true)]
        [InlineData(nameof(TestMethods.GreaterThanConstantS), 2701, 0, true)]
        [InlineData(nameof(TestMethods.ComparisonOperators1), 1, 2, true)]
        [InlineData(nameof(TestMethods.ComparisonOperators2), 100, 200, true)]
        [InlineData(nameof(TestMethods.SmallerS), -1, 1, true)]
        [InlineData(nameof(TestMethods.Unequal), 2, 2, false)]
        [InlineData(nameof(TestMethods.SmallerOrEqualS), -2, -1, true)]
        [InlineData(nameof(TestMethods.SmallerOrEqualS), -2, -2, true)]
        public void TestBooleanOperation(string methodName, int argument1, int argument2, bool expected)
        {
            var settings = new CompilerSettings()
            {
                CreateKernelForFlashing = false,
                UseFlashForKernel = false,
                SkipIterativeCompletion = true
            };

            LoadCodeMethod(typeof(TestMethods), methodName, argument1, argument2, expected, settings);
        }

        [Theory]
        [InlineData(nameof(TestMethods.AddS), 10, 20, 30)]
        [InlineData(nameof(TestMethods.AddCheckedS), 0x8888, 0x2222, 0xAAAA)]
        [InlineData(nameof(TestMethods.MulCheckedS), 0x8888, 0x1, 0x8888)]
        [InlineData(nameof(TestMethods.AddS), 10, -5, 5)]
        [InlineData(nameof(TestMethods.AddS), -5, -2, -7)]
        [InlineData(nameof(TestMethods.SubS), 10, 2, 8)]
        [InlineData(nameof(TestMethods.SubS), 10, -2, 12)]
        [InlineData(nameof(TestMethods.SubS), -2, -2, 0)]
        [InlineData(nameof(TestMethods.SubS), -4, 1, -5)]
        [InlineData(nameof(TestMethods.MulS), 4, 6, 24)]
        [InlineData(nameof(TestMethods.MulS), -2, -2, 4)]
        [InlineData(nameof(TestMethods.MulS), -2, 2, -4)]
        [InlineData(nameof(TestMethods.DivS), 10, 5, 2)]
        [InlineData(nameof(TestMethods.DivS), 10, 20, 0)]
        [InlineData(nameof(TestMethods.DivS), -10, 2, -5)]
        [InlineData(nameof(TestMethods.DivS), -10, -2, 5)]
        [InlineData(nameof(TestMethods.ModS), 10, 2, 0)]
        [InlineData(nameof(TestMethods.ModS), 11, 2, 1)]
        [InlineData(nameof(TestMethods.ModS), -11, 2, -1)]
        [InlineData(nameof(TestMethods.LshS), 8, 1, 16)]
        [InlineData(nameof(TestMethods.RshS), 8, 1, 4)]
        [InlineData(nameof(TestMethods.RshS), -8, 1, -4)]
        [InlineData(nameof(TestMethods.AndS), 0xF0, 0x1F, 0x10)]
        [InlineData(nameof(TestMethods.AndS), 0xF0, 0x00, 0x00)]
        [InlineData(nameof(TestMethods.OrS), 0xF0, 0x0F, 0xFF)]
        [InlineData(nameof(TestMethods.NotS), 0xF0, 0, -241)]
        [InlineData(nameof(TestMethods.NegS), -256, 0, 256)]
        [InlineData(nameof(TestMethods.XorS), 0x0F, 0x03, 12)]
        [InlineData(nameof(TestMethods.RshUnS), -8, 1, 2147483644)]
        public void TestArithmeticOperationSigned(string methodName, int argument1, int argument2, int expected)
        {
            var settings = new CompilerSettings()
            {
                CreateKernelForFlashing = false,
                UseFlashForKernel = false,
                SkipIterativeCompletion = true
            };

            settings.AdditionalSuppressions.Add("System.SR");
            LoadCodeMethod(typeof(TestMethods), methodName, argument1, argument2, expected, settings);
        }

        [Theory]
        [InlineData(nameof(TestMethods.AddCheckedS), 0x7fff_fff0, 0x7f99_9999)]
        [InlineData(nameof(TestMethods.MulCheckedS), 0x7fff_fff0, 0x7f00_9999)]
        [InlineData(nameof(TestMethods.SubCheckedU), 10, 20)]
        public void TestArithmeticOperationSignedWithOverflow(string methodName, int argument1, int argument2)
        {
            // Don't execute locally, it's expected to throw there, too.
            Assert.Throws<OverflowException>(() => LoadCodeMethodNoCheck(methodName, argument1, argument2));
        }

        [Theory]
        [InlineData(nameof(TestMethods.AddF), 10, 20, 30)]
        [InlineData(nameof(TestMethods.AddF), 10, -5, 5)]
        [InlineData(nameof(TestMethods.AddF), -5, -2, -7)]
        [InlineData(nameof(TestMethods.SubF), 10, 2, 8)]
        [InlineData(nameof(TestMethods.SubF), 10, -2, 12)]
        [InlineData(nameof(TestMethods.SubF), -2.0f, -2.0f, 0)]
        [InlineData(nameof(TestMethods.SubF), -4, 1, -5)]
        [InlineData(nameof(TestMethods.MulF), 4, 2.5f, 10.0)]
        [InlineData(nameof(TestMethods.MulF), -2, -2, 4)]
        [InlineData(nameof(TestMethods.MulF), -2, 2, -4)]
        [InlineData(nameof(TestMethods.DivF), 1.0f, 5.0f, 0.2f)]
        [InlineData(nameof(TestMethods.DivF), 10, 20, 0.5)]
        [InlineData(nameof(TestMethods.DivF), -10, 2, -5)]
        [InlineData(nameof(TestMethods.DivF), -10, -2, 5)]
        [InlineData(nameof(TestMethods.ModF), 10, 2, 0)]
        [InlineData(nameof(TestMethods.ModF), 11, 2, 1)]
        [InlineData(nameof(TestMethods.ModF), -11, 2, -1)]
        [InlineData(nameof(TestMethods.LoadFloatConstant), 0.0, 0.0, 2.0)] // tests the LDC.R4 instruction
        public void TestArithmeticOperationSignedFloat(string methodName, float argument1, float argument2, float expected)
        {
            LoadCodeMethod(typeof(TestMethods), methodName, argument1, argument2, expected);
        }

        [Theory]
        [InlineData(nameof(TestMethods.AddD), 10, 20.0, 30.0)]
        [InlineData(nameof(TestMethods.AddD), 10, -5, 5)]
        [InlineData(nameof(TestMethods.AddD), -5, -2, -7)]
        [InlineData(nameof(TestMethods.SubD), 10, 2, 8)]
        [InlineData(nameof(TestMethods.SubD), 10, -2, 12)]
        [InlineData(nameof(TestMethods.SubD), -2.0f, -2.0, 0)]
        [InlineData(nameof(TestMethods.SubD), -4, 1, -5)]
        [InlineData(nameof(TestMethods.MulD), 4, 2.5f, 10)]
        [InlineData(nameof(TestMethods.MulD), -2, -2, 4)]
        [InlineData(nameof(TestMethods.MulD), -2, 2, -4)]
        [InlineData(nameof(TestMethods.DivD), 1.0f, 5.0f, 0.2)]
        [InlineData(nameof(TestMethods.DivD), 10, 20, 0.5)]
        [InlineData(nameof(TestMethods.DivD), -10, 2, -5)]
        [InlineData(nameof(TestMethods.DivD), -10, -2, 5)]
        [InlineData(nameof(TestMethods.ModD), 10, 2, 0)]
        [InlineData(nameof(TestMethods.ModD), 11, 2, 1)]
        [InlineData(nameof(TestMethods.ModD), -11, 2, -1)]
        [InlineData(nameof(TestMethods.Truncate), 1.11, 0, 1)]
        [InlineData(nameof(TestMethods.Truncate), -1.11, 0, -1)]
        [InlineData(nameof(TestMethods.LoadDoubleConstant), 0.0, 0.0, 2.0)] // tests the LDC.R8 instruction
        public void TestArithmeticOperationSignedDouble(string methodName, double argument1, double argument2, double expected)
        {
            var settings = new CompilerSettings()
            {
                CreateKernelForFlashing = false,
                UseFlashForKernel = false,
                SkipIterativeCompletion = true
            };

            LoadCodeMethod(typeof(TestMethods), methodName, argument1, argument2, expected, settings);
        }

        [Theory]
        [InlineData(nameof(TestMethods.AddU), 10u, 20u, 30u)]
        [InlineData(nameof(TestMethods.AddU), 10u, -5u, 5u)]
        [InlineData(nameof(TestMethods.AddU), -5u, -2u, -7u)]
        [InlineData(nameof(TestMethods.SubU), 10u, 2u, 8u)]
        [InlineData(nameof(TestMethods.SubU), 10u, -2u, 12u)]
        [InlineData(nameof(TestMethods.SubU), -2u, -2u, 0u)]
        [InlineData(nameof(TestMethods.SubU), -4u, 1u, -5u)]
        [InlineData(nameof(TestMethods.MulU), 4u, 6u, 24u)]
        [InlineData(nameof(TestMethods.DivU), 10u, 5u, 2u)]
        [InlineData(nameof(TestMethods.DivU), 10u, 20u, 0u)]
        [InlineData(nameof(TestMethods.ModU), 10u, 2u, 0u)]
        [InlineData(nameof(TestMethods.ModU), 11u, 2u, 1u)]
        [InlineData(nameof(TestMethods.RshU), 8u, 1u, 4u)]
        [InlineData(nameof(TestMethods.RshU), -8u, 1u, 2147483644)]
        [InlineData(nameof(TestMethods.AndU), 0xF0u, 0x1Fu, 0x10u)]
        [InlineData(nameof(TestMethods.AndU), 0xF0u, 0x00u, 0x00u)]
        [InlineData(nameof(TestMethods.OrU), 0xF0u, 0x0Fu, 0xFFu)]
        [InlineData(nameof(TestMethods.NotU), 0xF0u, 0u, 0xFFFFFF0Fu)]
        [InlineData(nameof(TestMethods.XorU), 0x0Fu, 0x03u, 12)]
        [InlineData(nameof(TestMethods.RshUnU), -8u, 1, 0x7FFFFFFCu)]
        public void TestArithmeticOperationUnsigned(string methodName, Int64 argument1, Int64 argument2, Int64 expected)
        {
            var settings = new CompilerSettings()
            {
                CreateKernelForFlashing = false,
                UseFlashForKernel = false,
                SkipIterativeCompletion = true
            };

            // Method signature as above, otherwise the test data conversion fails
            LoadCodeMethod(typeof(TestMethods), methodName, (uint)argument1, (uint)argument2, (uint)expected, settings);
        }

        [Theory]
        [InlineData(nameof(TestMethods.ResultTypesTest), 50, 20, 70)]
        [InlineData(nameof(TestMethods.ResultTypesTest2), 21, -20, 1)]
        public void TestTypeConversions(string methodName, UInt32 argument1, int argument2, Int32 expected)
        {
            LoadCodeMethod(typeof(TestMethods), methodName, argument1, argument2, expected);
        }

        [Theory]
        [InlineData(nameof(TestMethods.IntArrayTest), 4, 1, 3)]
        [InlineData(nameof(TestMethods.IntArrayTest), 10, 2, 3)]
        [InlineData(nameof(TestMethods.CharArrayTest), 10, 2, 'C')]
        [InlineData(nameof(TestMethods.CharArrayTest), 10, 0, 'A')]
        [InlineData(nameof(TestMethods.ByteArrayTest), 10, 0, 255)]
        [InlineData(nameof(TestMethods.BoxedArrayTest), 5, 2, 7)]
        [InlineData(nameof(TestMethods.StaggedArrayTest), 5, 7, (int)'3')]
        public void ArrayTests(string methodName, Int32 argument1, Int32 argument2, Int32 expected)
        {
            LoadCodeMethod(typeof(TestMethods), methodName, argument1, argument2, expected);
        }

        [Theory]
        [InlineData(nameof(TestMethods.StructCtorBehaviorTest1), 5, 1, 2)]
        [InlineData(nameof(TestMethods.StructCtorBehaviorTest2), 5, 1, 5)]
        [InlineData(nameof(TestMethods.StructMethodCall1), 66, 33, -99)]
        [InlineData(nameof(TestMethods.StructMethodCall2), 66, 33, -66)]
        [InlineData(nameof(TestMethods.StructArray), 5, 2, 10)]
        [InlineData(nameof(TestMethods.StructInterfaceCall1), 10, 2, 8)]
        [InlineData(nameof(TestMethods.StructInterfaceCall2), 10, 2, 10)]
        [InlineData(nameof(TestMethods.StructInterfaceCall3), 15, 3, 12)]
        public void StructTests(string methodName, Int32 argument1, Int32 argument2, Int32 expected)
        {
            LoadCodeMethod(typeof(TestMethods), methodName, argument1, argument2, expected);
        }

        [Theory]
        [InlineData(nameof(TestMethods.LargeStructCtorBehaviorTest1), 5, 1, 4)]
        [InlineData(nameof(TestMethods.LargeStructCtorBehaviorTest2), 5, 1, 5)]
        [InlineData(nameof(TestMethods.LargeStructMethodCall2), 66, 33, 66)]
        [InlineData(nameof(TestMethods.LargeStructArray), 5, 1, 10)]
        [InlineData(nameof(TestMethods.LargeStructAsInterface1), 5, 5, 1)]
        [InlineData(nameof(TestMethods.LargeStructList1), 1, 2, 1)]
        [InlineData(nameof(TestMethods.LargeStructList2), 1, 2, 1)]
        public void LargeStructTest(string methodName, Int32 argument1, Int32 argument2, Int32 expected)
        {
            LoadCodeMethod(typeof(TestMethods), methodName, argument1, argument2, expected);
        }

        [Theory]
        [InlineData(nameof(TestMethods.CastClassTest), 0, 0, 1)]
        [InlineData(nameof(TestMethods.UseShortArgument), -5, 6, 1)]
        public void CastTest(string methodName, Int32 argument1, Int32 argument2, Int32 expected)
        {
            LoadCodeMethod(typeof(TestMethods), methodName, argument1, argument2, expected, new CompilerSettings() { CreateKernelForFlashing = false, UseFlashForKernel = false });
        }

        [Theory]
        [InlineData(nameof(TestMethods.SpanImplementationBehavior), 5, 1, 1)]
        public void SpanTest(string methodName, Int32 argument1, Int32 argument2, Int32 expected)
        {
            var settings = new CompilerSettings()
            {
                CreateKernelForFlashing = false,
                UseFlashForKernel = false,
                SkipIterativeCompletion = true
            };

            LoadCodeMethod(typeof(TestMethods), methodName, argument1, argument2, expected, settings);
        }

        [Theory]
        [InlineData(nameof(TestMethods.CallByValueIntTest), 5, 10, 1)]
        [InlineData(nameof(TestMethods.CallByValueShortTest), 5, 10, 1)]
        [InlineData(nameof(TestMethods.CallByValueObjectTest), 5, 10, 1)]
        public void ByRefTest(string methodName, Int32 argument1, Int32 argument2, Int32 expected)
        {
            LoadCodeMethod(typeof(TestMethods), methodName, argument1, argument2, expected);
        }

        [Theory]
        [InlineData(nameof(TestMethods.SimpleEnumHasValues))]
        [InlineData(nameof(TestMethods.EnumGetValues1))]
        [InlineData(nameof(TestMethods.EnumGetValues2))]
        public void EnumTest(string methodName)
        {
            LoadCodeMethod(typeof(TestMethods), methodName, 0, 0, 1);
        }

        [Fact]
        public void EnumsHaveNames()
        {
            var compilerSettings = new CompilerSettings()
            {
                AutoRestartProgram = false,
                CreateKernelForFlashing = false,
                LaunchProgramFromFlash = false,
                UseFlashForProgram = false
            };

            LoadCodeMethod(typeof(TestMethods), nameof(TestMethods.EnumsHaveNames), 0, 0, 1, compilerSettings, false);
        }

        [Theory]
        [InlineData(nameof(TestMethods.DoubleToString))]
        [InlineData(nameof(TestMethods.DoubleToString2))]
        public void DoubleToStringTest(string name)
        {
            var compilerSettings = new CompilerSettings()
            {
                AutoRestartProgram = false,
                CreateKernelForFlashing = false,
                LaunchProgramFromFlash = false,
                UseFlashForProgram = true
            };

            LoadCodeMethod(typeof(TestMethods), name, 20.23, 202.1, 20.23, compilerSettings);
        }

        /// <summary>
        /// The tests of this group try to expose a problem that was once detected and fixed.
        /// </summary>
        [Theory]
        [InlineData(nameof(TestMethods.IntToString1), 20304)]
        [InlineData(nameof(TestMethods.IntToString2), 20304)]
        [InlineData(nameof(TestMethods.IntToString3), -20304)]
        [InlineData(nameof(TestMethods.DictionaryTest1), 0)]
        [InlineData(nameof(TestMethods.DictionaryTest2), 0)]
        [InlineData(nameof(TestMethods.LcdCharacterEncodingTest1), 0)]
        [InlineData(nameof(TestMethods.LcdCharacterEncodingTest2), 0)]
        [InlineData(nameof(TestMethods.StringInterpolation), 0)]
        [InlineData(nameof(TestMethods.UseStringlyTypedDictionary), 1)]
        [InlineData(nameof(TestMethods.UnitsNetTemperatureTest), 0)]
        [InlineData(nameof(TestMethods.StringEncoding), 0)]
        [InlineData(nameof(TestMethods.PrivateImplementationDetailsUsedCorrectly), 0)]
        public void BrokenImplementationBehaviorValidation(string methodName, int arg1)
        {
            var compilerSettings = new CompilerSettings()
            {
                AutoRestartProgram = false,
                CreateKernelForFlashing = false,
                LaunchProgramFromFlash = false,
                UseFlashForProgram = true
            };

            LoadCodeMethod(typeof(TestMethods), methodName, arg1, 0, 1, compilerSettings);
        }

        [Fact]
        public void ValidateTestMethods()
        {
            Assert.Equal(1, TestMethods.UseStringlyTypedDictionary(1, 2));
        }

        [Theory]
        [InlineData(nameof(TestMethods.IterateOverArray1), 1)]
        [InlineData(nameof(TestMethods.IterateOverArray2), 1)]
        [InlineData(nameof(TestMethods.IterateOverArray3), 1)]
        public void IteratorProblems(string methodName, int arg1)
        {
            var compilerSettings = new CompilerSettings()
            {
                AutoRestartProgram = false,
                CreateKernelForFlashing = false,
                LaunchProgramFromFlash = false,
                UseFlashForProgram = true
            };

            LoadCodeMethod(typeof(TestMethods), methodName, arg1, 0, 1, compilerSettings);
        }

        /// <summary>
        /// Tests how merging similar generic methods can safe memory. The byte code for the implementation of List{object} and List{string}
        /// should be equivalent
        /// </summary>
        [Theory]
        [InlineData(nameof(TestMethods.UnsafeAsCanBeMerged), 1)]
        [InlineData(nameof(TestMethods.UnsafeSizeOf), 1)]
        public void CanMergeSimilarGenericMethods(string methodName, int arg1)
        {
            var compilerSettings = new CompilerSettings()
            {
                AutoRestartProgram = false,
                CreateKernelForFlashing = false,
                LaunchProgramFromFlash = false,
                UseFlashForProgram = true
            };

            LoadCodeMethod(typeof(TestMethods), methodName, arg1, 0, 1, compilerSettings);
        }

        [Theory]
        [InlineData(nameof(TestMethods.NormalTryCatchNoException), 1)]
        [InlineData(nameof(TestMethods.NormalTryFinallyNoException), 1)]
        [InlineData(nameof(TestMethods.NormalTryCatchWithException), 1)]
        [InlineData(nameof(TestMethods.NormalTryCatchWithException2), 1)]
        [InlineData(nameof(TestMethods.NormalTryFinallyWithReturn), 1)]
        [InlineData(nameof(TestMethods.NormalTryFinallyWithCodeAfterFinally), 2)]
        [InlineData(nameof(TestMethods.NormalTryFinallyWithException), 2)]
        [InlineData(nameof(TestMethods.UsingHandlers), 2)]
        [InlineData(nameof(TestMethods.FinallyInDifferentMethod), 2)]
        [InlineData(nameof(TestMethods.TryBlockInCatch), 0)]
        [InlineData(nameof(TestMethods.TryBlockInFinally), 0)]
        [InlineData(nameof(TestMethods.FinallyInDifferentBlock), 0)]
        public void ExceptionHandling(string methodName, int arg1)
        {
            var compilerSettings = new CompilerSettings()
            {
                AutoRestartProgram = false,
                CreateKernelForFlashing = false,
                LaunchProgramFromFlash = false,
                UseFlashForProgram = true
            };

            LoadCodeMethod(typeof(TestMethods), methodName, arg1, 0, 1, compilerSettings);
        }

        [Theory]
        [InlineData(nameof(TestMethods.TryCatchDivideByZeroException), 0)]
        [InlineData(nameof(TestMethods.TryCatchIndexOutOfRangeException), 10)]
        public void ExceptionHandlingForBuiltinErrors(string methodName, int arg1)
        {
            var compilerSettings = new CompilerSettings()
            {
                AutoRestartProgram = false,
                CreateKernelForFlashing = false,
                LaunchProgramFromFlash = false,
                UseFlashForProgram = false
            };

            LoadCodeMethod(typeof(TestMethods), methodName, arg1, 0, 1, compilerSettings);
        }

        [Theory]
        [InlineData(nameof(TestMethods.StringContains), 1)]
        [InlineData(nameof(TestMethods.StringStartsWith), 1)]
        public void StringTest(string methodName, Int32 expected)
        {
            LoadCodeMethod(typeof(TestMethods), methodName, 0, 0, expected);
        }

        [Theory]
        [InlineData(nameof(ThreadingTests.StartAndStopThread), 0, 0, 1)]
        [InlineData(nameof(ThreadingTests.DiningPhilosophers), 0, 0, 1)]
        [InlineData(nameof(ThreadingTests.UseThreadStatic), 0, 0, 1)]
        [InlineData(nameof(ThreadingTests.UseThreadStaticInSystem), 10, 5, 1)]
        [InlineData(nameof(ThreadingTests.UseArrayPool), 0, 0, 1)]
        // Not yet reliable - the task handling seems to still have some bugs, but it's difficult to find out
        // what's going on behind the scenes here.
        // [InlineData(nameof(ThreadingTests.AsyncAwait), 0, 0, 1)]
        [InlineData(nameof(ThreadingTests.TestTask), 0, 0, 1)]
        public void SimpleThreading(string methodName, Int32 a, Int32 b, Int32 expected)
        {
            // No exclusions for this test
            var settings = new CompilerSettings()
            {
                CreateKernelForFlashing = false,
                UseFlashForKernel = false
            };

            LoadCodeMethod(typeof(ThreadingTests), methodName, a, b, expected, settings);
        }

        /// <summary>
        /// Checks that the emulated runtime version is the same as the one on the host
        /// </summary>
        [Fact]
        public void VerifyRuntimeVersion()
        {
            var settings = new CompilerSettings()
            {
                CreateKernelForFlashing = false,
                UseFlashForKernel = false
            };

            // We can't pass on string types as arguments to the runtime at this time
            LoadCodeMethod(typeof(TestMethods), nameof(TestMethods.CompareRuntimeVersion), 0, 0, 1, settings);
        }
    }
}
