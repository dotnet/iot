// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;

namespace System
{
    /// <summary>
    /// On netstandard2.0 RandomHelper provides the implementation for Random.NextBytes.
    /// On netstandard2.1 RandomHelper redirect calls to Random.NextBytes.
    /// </summary>
    public static class RandomHelper
    {
        /// <summary>
        /// Fills the elements of a specified array of bytes with random numbers.
        /// </summary>
        /// <param name="random">The random.</param>
        /// <param name="buffer">The buffer to be filled.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NextSpan(this Random random, Span<byte> buffer)
        {
#if NETSTANDARD2_0
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = (byte)random.Next();
            }
#else
            random.NextBytes(buffer);
#endif
        }

    }
}
