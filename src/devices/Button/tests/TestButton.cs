// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.Extensions.Time.Testing;

namespace Iot.Device.Button.Tests
{
    public class TestButton : ButtonBase
    {
        public TestButton()
            : this(new FakeTimeProvider())
        {
        }

        public TestButton(TimeSpan debounceTime, TimeSpan holdingTime)
            : this(TimeSpan.FromSeconds(5), holdingTime, debounceTime, new FakeTimeProvider())
        {
        }

        private TestButton(FakeTimeProvider timeProvider)
            : base(timeProvider)
        {
            TimeProvider = timeProvider;
        }

        private TestButton(TimeSpan doublePressTime, TimeSpan holdingTime, TimeSpan debounceTime, FakeTimeProvider timeProvider)
            : base(doublePressTime, holdingTime, debounceTime, timeProvider)
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
