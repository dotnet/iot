// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Disposables
{
    /// <summary>
    /// Utility to create disposable objects
    /// </summary>
    public static class Disposable
    {
        private class AnonymousDisposable : IDisposable
        {
            private readonly Action _onDispose;

            public AnonymousDisposable(Action onDispose)
            {
                _onDispose = onDispose ?? throw new ArgumentNullException(nameof(onDispose));
            }

            public void Dispose()
            {
                _onDispose();
            }
        }

        /// <summary>
        /// Creates a disposable object.
        /// </summary>
        /// <param name="onDispose">The action to perform on disposal</param>
        /// <returns>The disposable object</returns>
        public static IDisposable Create(Action onDispose)
        {
            return new AnonymousDisposable(onDispose);
        }
    }
}
