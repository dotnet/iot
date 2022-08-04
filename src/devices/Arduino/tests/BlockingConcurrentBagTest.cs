// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Text;
using Iot.Device.Arduino;
using Xunit;

namespace Iot.Device.Arduino.Tests
{
    public class BlockingConcurrentBagTest
    {
        private BlockingConcurrentBag<int> _blockingConcurrentBag;

        public BlockingConcurrentBagTest()
        {
            _blockingConcurrentBag = new BlockingConcurrentBag<int>();
        }

        [Fact]
        public void CanAddAndRemove()
        {
            _blockingConcurrentBag.Add(1);
            Assert.True(_blockingConcurrentBag.TryRemoveElement(x => true, TimeSpan.Zero, out int a));
            Assert.True(a == 1);
        }

        [Fact]
        public void ReturnsFalseWhenElementDoesNotMatch()
        {
            _blockingConcurrentBag.Add(1);
            Assert.False(_blockingConcurrentBag.TryRemoveElement(x => x == 3, TimeSpan.FromSeconds(1), out int a));
        }
    }
}
