// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Xunit;

namespace Iot.Device.Common.Tests
{
    public class HysteresisTest
    {
        private HysteresisFilter _filter;
        public HysteresisTest()
        {
            _filter = new HysteresisFilter(false);
        }

        [Fact]
        public void NoHysteresis()
        {
            Assert.False(_filter.Output);
            _filter.Update(true);
            Assert.True(_filter.Output);
            _filter.Update(true);
            Assert.True(_filter.Output);
            _filter.Update(false);
            Assert.False(_filter.Output);
        }

        [Fact]
        public void RisingHysteresis()
        {
            _filter.RisingDelayTime = TimeSpan.FromMilliseconds(5);
            Assert.False(_filter.Output);
            _filter.Update(true);
            Assert.False(_filter.Output);
            for (int i = 0; i < 10; i++)
            {
                _filter.Update(true);
                Thread.Sleep(2);
                if (_filter.Output)
                {
                    break;
                }
            }

            Assert.True(_filter.Output);
            _filter.Update(false);
            Assert.False(_filter.Output);
        }

        [Fact]
        public void FallingHysteresis()
        {
            _filter.FallingDelayTime = TimeSpan.FromMilliseconds(5);
            Assert.False(_filter.Output);
            _filter.Update(true);
            Assert.True(_filter.Output);
            for (int i = 0; i < 10; i++)
            {
                _filter.Update(false);
                Thread.Sleep(2);
                if (!_filter.Output)
                {
                    break;
                }
            }

            Assert.False(_filter.Output);
        }

        [Fact]
        public void BothHysteresis()
        {
            _filter.RisingDelayTime = TimeSpan.FromMilliseconds(5);
            _filter.FallingDelayTime = TimeSpan.FromMilliseconds(5);
            Assert.False(_filter.Output);
            _filter.Update(true);
            Assert.False(_filter.Output);
            for (int i = 0; i < 10; i++)
            {
                _filter.Update(true);
                Thread.Sleep(2);
                if (_filter.Output)
                {
                    break;
                }
            }

            Assert.True(_filter.Output);
            _filter.Update(false);
            Assert.True(_filter.Output);
            Thread.Sleep(10);
            _filter.Update(false);
            Assert.False(_filter.Output);
        }
    }
}
