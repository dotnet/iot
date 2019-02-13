// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading;
using Windows.Foundation;

internal static partial class Interop
{
    public static TResult WaitForCompletion<TResult>(this IAsyncOperation<TResult> operation)
    {
        using (var waitEvent = new ManualResetEvent(false))
        {
            operation.Completed += (i, s) => waitEvent.Set();

            if (operation.Status == AsyncStatus.Started)
            {
                waitEvent.WaitOne();
            }

            switch (operation.Status)
            {
                case AsyncStatus.Canceled:
                    return default;
                case AsyncStatus.Completed:
                    return operation.GetResults();
                case AsyncStatus.Error:
                    throw operation.ErrorCode;
                default:
                    throw new InvalidOperationException($"Unexpected asynchronous operation state: {operation.Status}");
            }
        }
    }
}
