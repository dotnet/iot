// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;

namespace Units.Tests
{
    public sealed class SetCultureForTest : IDisposable
    {
        private CultureInfo _previousCulture;
        private CultureInfo _previousUiCulture;

        public SetCultureForTest(string cultureName)
        {
            _previousCulture = CultureInfo.CurrentCulture;
            _previousUiCulture = CultureInfo.CurrentUICulture;
            var culture = new CultureInfo(cultureName, false);
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
        }

        public void Dispose()
        {
            Thread.CurrentThread.CurrentCulture = _previousCulture;
            Thread.CurrentThread.CurrentUICulture = _previousUiCulture;
        }
    }
}
