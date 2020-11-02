// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading;
using Windows.Foundation;

internal static partial class Interop
{
    public static TResult? WaitForCompletion<TResult>(this IAsyncOperation<TResult> operation)
    {
        using (var waitEvent = new ManualResetEvent(false))
        {
            operation.Completed += (i, s) => waitEvent.Set();

            if (operation.Status == AsyncStatus.Started)
            {
                waitEvent.WaitOne();
            }

            return operation.Status switch
            {
                AsyncStatus.Canceled => default,
                AsyncStatus.Completed => operation.GetResults(),
                AsyncStatus.Error => throw operation.ErrorCode,
                _ => throw new InvalidOperationException($"Unexpected asynchronous operation state: {operation.Status}")
            };
        }
    }
}
