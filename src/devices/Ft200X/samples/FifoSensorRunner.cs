// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading;

namespace Iot.Device.Ft200X.Samples
{
    /// <summary>
    /// Forwards adapter payload bytes to a sensor and queues its variable-length responses.
    /// </summary>
    /// <remarks>The caller owns the adapter. Transport and handler failures propagate without retrying.</remarks>
    internal sealed class FifoSensorRunner
    {
        private readonly Ft200XDevice _adapter;
        private readonly IFifoSensor _sensor;
        private readonly Action<ReadOnlyMemory<byte>> _responseQueued;
        private readonly byte[] _incoming = new byte[256];

        /// <summary>
        /// Creates a runner without resetting, purging, or taking ownership of the adapter.
        /// </summary>
        /// <param name="adapter">The open adapter.</param>
        /// <param name="sensor">The handler owning request framing and response lengths.</param>
        /// <param name="responseQueued">
        /// Called synchronously after a response is queued, not necessarily consumed by the master.
        /// Copy the response if it must be retained after this callback.
        /// </param>
        public FifoSensorRunner(Ft200XDevice adapter, IFifoSensor sensor, Action<ReadOnlyMemory<byte>> responseQueued)
        {
            ArgumentNullException.ThrowIfNull(adapter);
            ArgumentNullException.ThrowIfNull(sensor);
            ArgumentNullException.ThrowIfNull(responseQueued);
            _adapter = adapter;
            _sensor = sensor;
            _responseQueued = responseQueued;
        }

        /// <summary>
        /// Polls until cancellation, waiting briefly when no input is available.
        /// </summary>
        /// <param name="cancellationToken">Stops processing without disposing the adapter.</param>
        public void Run(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (!ProcessAvailable())
                {
                    cancellationToken.WaitHandle.WaitOne(5);
                }
            }
        }

        /// <summary>
        /// Reads at most one buffer of available input and immediately queues each completed response.
        /// </summary>
        /// <returns>Whether any input bytes were processed.</returns>
        public bool ProcessAvailable()
        {
            uint available = _adapter.BytesAvailable;
            if (available == 0)
            {
                return false;
            }

            int count = _adapter.Read(_incoming.AsSpan(0, (int)Math.Min(available, (uint)_incoming.Length)));
            for (int i = 0; i < count; i++)
            {
                ReadOnlyMemory<byte> response = _sensor.ProcessByte(_incoming[i]);
                if (!response.IsEmpty)
                {
                    _adapter.Write(response.Span);
                    _responseQueued(response);
                }
            }

            return count != 0;
        }
    }
}
