// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

using Microsoft.Extensions.Time.Testing;

namespace Iot.Device.Button.Tests
{
    public class TestButton : ButtonBase
    {
        public TestButton()
            : this(TimeSpan.FromSeconds(1.5), TimeSpan.FromSeconds(2), TimeSpan.Zero, new FakeTimeProvider())
        {
        }

        public TestButton(TimeSpan debounceTime, TimeSpan holdingTime)
            : this(TimeSpan.FromSeconds(5), holdingTime, debounceTime, new FakeTimeProvider())
        {
        }

        private TestButton(TimeSpan doublePress, TimeSpan holdingTime, TimeSpan debounceTime, FakeTimeProvider timeProvider)
            : base(doublePress, holdingTime, debounceTime, timeProvider)
        {
            TimeProvider = timeProvider;
        }

        public FakeTimeProvider TimeProvider { get; }

        public void PressButton()
        {
            HandleButtonPressed();
        }

        public void ReleaseButton()
        {
            HandleButtonReleased();
        }
    }
}
