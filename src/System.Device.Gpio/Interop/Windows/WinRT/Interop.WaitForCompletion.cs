using System;
using System.Threading;
using Windows.Foundation;

internal partial class Interop
{
    public static TResult WaitForCompletion<TResult>(IAsyncOperation<TResult> operation)
    {
        using (var waitEvent = new ManualResetEvent(false))
        {
            void FromIdAsyncCompleted(IAsyncOperation<TResult> asyncInfo, AsyncStatus asyncStatus)
            {
                waitEvent.Set();
            }

            operation.Completed += FromIdAsyncCompleted;
            if (operation.Status == AsyncStatus.Started)
            {
                waitEvent.WaitOne();
            }
            if (operation.Completed != null)
            {
                operation.Completed -= FromIdAsyncCompleted;
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