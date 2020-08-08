// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;

namespace Iot.Device.Blinkt.Samples
{
    internal static class EnumerableExtensions
    {
        internal static IEnumerable<(int, T)> WithIndex<T>(this IEnumerable<T> enumerable)
        {
            var i = 0;
            foreach (T element in enumerable)
            {
                yield return (i, element);
                i++;
            }
        }
    }
}
