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
