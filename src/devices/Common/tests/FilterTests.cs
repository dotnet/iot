// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Iot.Device.Common;
using Xunit;

namespace Common.Tests
{
    public class FilterTests
    {
        [Fact]
        public void SimpleCase()
        {
            var filter1 = new TimeSliceFilter<double>(TimeSpan.MaxValue, TimeSliceFilter<double>.AverageFilter);
            filter1.Add(1);
            filter1.Add(2);
            double result = filter1.CurrentValue();
            Assert.Equal(1.5, result, 0.01);
        }

        [Fact]
        public void ElementsAreRemovedAfterTimeout()
        {
            var filter1 = new TimeSliceFilter<double>(TimeSpan.Zero, TimeSliceFilter<double>.AverageFilter);
            filter1.Add(1);
            Thread.Sleep(100);
            filter1.CurrentValue();
            filter1.MaxAge = TimeSpan.FromMinutes(1);
            filter1.Add(2);
            double result = filter1.CurrentValue();
            Assert.Equal(2.0, result, 0.01);
        }
    }
}
