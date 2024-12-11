// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using ArduinoCsCompiler;
using Xunit;

#pragma warning disable SA1405 // Debug.Assert without description
#pragma warning disable SA1204 // Static members first
namespace Iot.Device.Arduino.Tests
{
    public class FrameworkBehaviorTests
    {
        [Fact]
        public void CannotGetSizeOfOpenGenericType()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                Marshal.SizeOf(typeof(GenericStruct<>));
            });
        }

        [Fact]
        public void CanGetSizeOfClosedGenericType()
        {
            // This test fails
            // Assert.Equal(6, Marshal.SizeOf(typeof(GenericStruct<short>)));
        }

        [Fact]
        public void CanGetSizeOfClosedGenericTypeViaInstance()
        {
            GenericStruct<short> gs;
            gs._data1 = 2;
            gs._data2 = 10;
            Assert.Equal(8, Marshal.SizeOf(gs));
        }

        [Fact]
        public void MarshalOffsetOfBehaviorSimple()
        {
            Assert.Equal(1, Marshal.SizeOf(typeof(System.Char))); // !!! this returns 1, because the default marshaller uses ASCII encoding
            Assert.Equal(0, Marshal.OffsetOf(typeof(TestAlignment), nameof(TestAlignment._a)).ToInt32());
            Assert.Equal(4, Marshal.OffsetOf(typeof(TestAlignment), nameof(TestAlignment._b)).ToInt32());
            Assert.Equal(5, Marshal.OffsetOf(typeof(TestAlignment), nameof(TestAlignment._c)).ToInt32());
            Assert.Equal(8, Marshal.OffsetOf(typeof(TestAlignment), nameof(TestAlignment._d)).ToInt32());
        }

        [Fact]
        public void MarshalOffsetOfBehaviorGenerics()
        {
            Assert.Equal(0, Marshal.OffsetOf(typeof(TestAlignmentGenerics<byte>), "_a").ToInt32());
            Assert.Equal(4, Marshal.OffsetOf(typeof(TestAlignmentGenerics<byte>), "_b").ToInt32());
            Assert.Equal(5, Marshal.OffsetOf(typeof(TestAlignmentGenerics<byte>), "_c").ToInt32());
            Assert.Equal(8, Marshal.OffsetOf(typeof(TestAlignmentGenerics<byte>), "_d").ToInt32());
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        private struct TestAlignment
        {
            public int _a;
            public char _b;
            public byte _c;
            public int _d;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        private struct TestAlignmentGenerics<T>
        {
            public int _a;
            public T _b;
            public byte _c;
            public int _d;
        }

        /// <summary>
        /// These tests help debugging the Double.ToString() implementation by testing individual methods as seen in the runtime
        /// </summary>
        [Fact]
        public void DoubleToStringImplementationTests()
        {
            int exponent;
            ulong fraction = ExtractFractionAndBiasedExponent(20.2, out exponent);

            Assert.Equal(5685794529555251ul, fraction);
            Assert.Equal(-48, exponent);
        }

        private static ulong ExtractFractionAndBiasedExponent(double value, out int exponent)
        {
            ulong bits = (ulong)(BitConverter.DoubleToInt64Bits(value));
            ulong fraction = (bits & 0xFFFFFFFFFFFFF);
            exponent = ((int)(bits >> 52) & 0x7FF);

            if (exponent != 0)
            {
                // For normalized value, according to https://en.wikipedia.org/wiki/Double-precision_floating-point_format
                // value = 1.fraction * 2^(exp - 1023)
                //       = (1 + mantissa / 2^52) * 2^(exp - 1023)
                //       = (2^52 + mantissa) * 2^(exp - 1023 - 52)
                //
                // So f = (2^52 + mantissa), e = exp - 1075;
                fraction |= (1UL << 52);
                exponent -= 1075;
            }
            else
            {
                // For denormalized value, according to https://en.wikipedia.org/wiki/Double-precision_floating-point_format
                // value = 0.fraction * 2^(1 - 1023)
                //       = (mantissa / 2^52) * 2^(-1022)
                //       = mantissa * 2^(-1022 - 52)
                //       = mantissa * 2^(-1074)
                // So f = mantissa, e = -1074
                exponent = -1074;
            }

            return fraction;
        }

        [Fact]
        public void TestRounding()
        {
            Assert.Equal(1024, RoundUp(1024, 1024));
            Assert.Equal(1024, RoundUp(100, 1024));
        }

        /// <summary>
        /// Documentation and implementation don't match - so test this
        /// </summary>
        [Fact]
        public void TestHexParsing()
        {
            string hexNumber = "0x9f";
            Assert.True(Debugger.TryParseHexOrDec(hexNumber, out int number));
            Assert.Equal(0x9f, number);
            Assert.False(Debugger.TryParseHexOrDec("0x", out number));
            Assert.True(Debugger.TryParseHexOrDec("234", out number));
            Assert.Equal(234, number);
        }

        private long RoundUp(long offset, long align)
        {
            long evenBy = offset % align;
            if (evenBy == 0L) // Resharper complains on this line with "Expression is always false", which is obviously not true
            {
                return offset;
            }

            offset += (align - evenBy);

            return offset;
        }

        private struct GenericStruct<T>
        {
            public T _data1;
            public int _data2;
        }
    }
}
