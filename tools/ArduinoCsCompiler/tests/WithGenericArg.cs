// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Arduino.Tests
{
    /// <summary>
    /// Needs to be outside the testing class, because it needs to be directly in the namespace for the test
    /// </summary>
    internal class WithGenericArg<T>
    {
        private T _data;

        public WithGenericArg(T data)
        {
            _data = data;
        }

        public class Internal
        {
            public int Foo()
            {
                return 0;
            }
        }

        public T2 TestGenericArg<T2>(T2 input)
        {
            return input;
        }

        public class Internal2<T2>
        {
            public T2? _data2;

            public Internal2(T2? data2)
            {
                _data2 = data2;
            }

            public T2? Foo2()
            {
                return _data2;
            }
        }
    }
}
