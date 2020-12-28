using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Arduino.Tests
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
            Assert.Equal(6, Marshal.SizeOf(typeof(GenericStruct<short>)));
        }

        [Fact]
        public void CanGetSizeOfOpenGenericTypeViaInstance()
        {
            GenericStruct<short> gs;
            gs._data1 = 2;
            gs._data2 = 10;
            Assert.Equal(8, Marshal.SizeOf(gs));
        }

        private struct GenericStruct<T>
        {
            public T _data1;
            public int _data2;
        }
    }
}
