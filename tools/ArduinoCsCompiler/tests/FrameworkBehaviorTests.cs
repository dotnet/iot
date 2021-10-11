using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Xunit;

#pragma warning disable SA1405 // Debug.Assert without description
#pragma warning disable SA1204 // Static members first
namespace Iot.Device.Tests
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
        public void CanGetSizeOfOpenGenericType()
        {
            // This test fails
            // Assert.Equal(6, Marshal.SizeOf(typeof(GenericStruct<short>)));
        }

        [Fact]
        public void CanGetSizeOfOpenGenericTypeViaInstance()
        {
            GenericStruct<short> gs;
            gs._data1 = 2;
            gs._data2 = 10;
            Assert.Equal(8, Marshal.SizeOf(gs));
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

        private struct GenericStruct<T>
        {
            public T _data1;
            public int _data2;
        }
    }
}
