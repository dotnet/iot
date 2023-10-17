// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Button.Tests
{
    public class TestButton : ButtonBase
    {
        public TestButton()
            : base()
        {
        }

        public TestButton(TimeSpan debounceTime, TimeSpan holdingTime)
            : base(TimeSpan.FromSeconds(5), holdingTime, debounceTime)
        {
        }

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
