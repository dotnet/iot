// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArduinoCsCompiler
{
    /// <summary>
    /// Similar to <see cref="Progress{T}"/>, but uses a synchronous callback
    /// </summary>
    /// <typeparam name="T">The type of progress (typically double)</typeparam>
    internal class SynchronousProgress<T> : IProgress<T>
    {
        private readonly T _endValue;
        public event Action<object, T>? ProgressChanged;

        public SynchronousProgress(T endValue)
        {
            _endValue = endValue;
        }

        public void Report(T value)
        {
            ProgressChanged?.Invoke(this, value);
        }

        public void Done()
        {
            Report(_endValue);
        }
    }
}
