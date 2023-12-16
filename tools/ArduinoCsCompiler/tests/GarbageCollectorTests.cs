// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArduinoCsCompiler.Runtime;
using Iot.Device.Arduino;
using Xunit;

namespace Iot.Device.Arduino.Tests
{
    [Collection("SingleClientOnly")]
    [Trait("feature", "firmata")]
    [Trait("requires", "hardware")]
    public class GarbageCollectorTests : ArduinoTestBase, IClassFixture<FirmataTestFixture>, IDisposable
    {
        public GarbageCollectorTests(FirmataTestFixture fixture)
            : base(fixture)
        {
            Compiler.ClearAllData(true, false);
            CompilerSettings.CreateKernelForFlashing = false;
        }

        [Fact]
        public void AllocateSimpleClass()
        {
            ExecuteComplexProgramSuccess<Func<int>>(SimpleClass1.SimpleTest1, true);
        }

        [Fact]
        public void SimpleGcStatistics()
        {
            ExecuteComplexProgramSuccess<Func<int>>(SimpleClass1.SimpleGcStatistics, false);
        }

        [Fact]
        public void SimpleGcFunctions()
        {
            ExecuteComplexProgramSuccess<Func<int>>(SimpleClass1.GcFunctions, true);
        }

        internal class SimpleClass1
        {
            private int _i1;
            private object _obj;
            private object[] _objArray;

            public SimpleClass1(int i1)
            {
                _i1 = i1;
                _obj = new object();
                _objArray = new object[]
                {
                    i1, "ABC", PinValue.High
                };
            }

            public static int SimpleTest1()
            {
                SimpleClass1? cls = new SimpleClass1(2);
                MiniAssert.That(cls._i1 == 2);
                MiniAssert.NotNull(cls._obj);
                MiniAssert.NotNull(cls._objArray);
                MiniAssert.NotNull(cls._objArray[1]);
                int i = (int)cls._objArray[0];
                MiniAssert.That(i == 2);
                cls = null;
                GC.Collect();
                MiniAssert.IsNull(cls);
                cls = new SimpleClass1(3);
                MiniAssert.That(cls._i1 == 3);
                GC.Collect(2, GCCollectionMode.Forced, true, true);
                GC.KeepAlive(cls);
                return 1;
            }

            public static int SimpleGcStatistics()
            {
                SimpleClass1? cls = new SimpleClass1(4);
                SimpleClass1 cls2 = cls;
                cls = null;
                GC.Collect(2);
                MiniAssert.That(GC.GetTotalAllocatedBytes(false) > 0);
                var statistics = GC.GetGCMemoryInfo(GCKind.Any);
                MiniAssert.That(statistics.HeapSizeBytes > 4096);
                MiniAssert.That(statistics.Generation == 0);
                MiniAssert.That(statistics.TotalAvailableMemoryBytes > 0);
                MiniAssert.That(statistics.Compacted == false);
                MiniAssert.That(cls2._i1 == 4);
                return 1;
            }

            public static int GcFunctions()
            {
                SimpleClass1? cls = new SimpleClass1(4);
                SimpleClass1 cls2 = cls;
                cls = null;
                MiniAssert.That(GC.MaxGeneration >= 0);
                GC.SuppressFinalize(cls2);
                return 1;
            }
        }
    }
}
