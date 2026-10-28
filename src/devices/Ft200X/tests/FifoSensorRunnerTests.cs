// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Iot.Device.Ft200X.Samples;
using Iot.Device.FtCommon;
using Xunit;

namespace Iot.Device.Ft200X.Tests
{
    public class FifoSensorRunnerTests
    {
        [Theory]
        [InlineData(1)]
        [InlineData(3)]
        [InlineData(256)]
        public void ForwardsFragmentedAndConsecutiveTemperatureRequests(int readLimit)
        {
            var transport = new FakeTransport { ReadLimit = readLimit };
            foreach (byte value in Convert.FromHexString("00000000000100000102000000010000"))
            {
                transport.Incoming.Enqueue(value);
            }

            using var adapter = new Ft200XDevice(transport);
            var replies = new List<string>();
            var runner = new FifoSensorRunner(adapter, new TemperatureSensor(), reply => replies.Add(Convert.ToHexString(reply.Span)));
            while (runner.ProcessAvailable())
            {
            }

            Assert.Equal(new[] { "00002001", "0001092E", "00020000", "05010000" }, replies);
            Assert.Equal(replies.Count, transport.Written.Count);
            for (int i = 0; i < replies.Count; i++)
            {
                Assert.Equal(replies[i], Convert.ToHexString(transport.Written[i]));
            }
        }

        [Fact]
        public void QueuesCommandDependentResponsesIncludingLargerThanInputBuffer()
        {
            var transport = new FakeTransport();
            transport.Incoming.Enqueue(1);
            transport.Incoming.Enqueue(2);
            transport.Incoming.Enqueue(3);
            using var adapter = new Ft200XDevice(transport);
            var replies = new List<byte[]>();
            var runner = new FifoSensorRunner(adapter, new VariableLengthSensor(), reply => replies.Add(reply.ToArray()));

            Assert.True(runner.ProcessAvailable());
            Assert.False(runner.ProcessAvailable());
            Assert.Equal(3, transport.Written.Count);
            for (int i = 0; i < 3; i++)
            {
                Assert.Equal((i + 1) * 100, transport.Written[i].Length);
                Assert.All(transport.Written[i], value => Assert.Equal((byte)(i + 1), value));
                Assert.Equal(transport.Written[i], replies[i]);
            }
        }

        [Fact]
        public void IdleAndCancelledRunnerDoesNotReadWriteOrDisposeAdapter()
        {
            var transport = new FakeTransport();
            using var adapter = new Ft200XDevice(transport);
            var runner = new FifoSensorRunner(adapter, new TemperatureSensor(), _ => Assert.Fail("Unexpected reply."));
            Assert.False(runner.ProcessAvailable());
            transport.Incoming.Enqueue(0);
            runner.Run(new CancellationToken(true));
            Assert.Equal(0, transport.ReadCalls);
            Assert.Empty(transport.Written);
            Assert.False(transport.Disposed);
        }

        [Fact]
        public void FailedWritePropagatesWithoutRetryOrSuccessNotification()
        {
            var failure = new IOException("Simulated write failure.");
            var transport = new FakeTransport { WriteError = failure };
            transport.Incoming.Enqueue(1);
            using var adapter = new Ft200XDevice(transport);
            var runner = new FifoSensorRunner(adapter, new VariableLengthSensor(), _ => Assert.Fail("Unexpected reply."));

            Assert.Same(failure, Assert.Throws<IOException>(() => runner.ProcessAvailable()));
            Assert.Single(transport.Written);
            Assert.False(transport.Disposed);
        }

        private sealed class VariableLengthSensor : IFifoSensor
        {
            public ReadOnlyMemory<byte> ProcessByte(byte value)
            {
                byte[] response = new byte[value * 100];
                Array.Fill(response, value);
                return response;
            }
        }

        private sealed class FakeTransport : IFtXSeriesI2cSlaveTransport
        {
            public Queue<byte> Incoming { get; } = new Queue<byte>();

            public List<byte[]> Written { get; } = new List<byte[]>();

            public int ReadLimit { get; set; } = int.MaxValue;

            public int ReadCalls { get; private set; }

            public bool Disposed { get; private set; }

            public IOException? WriteError { get; set; }

            public uint BytesAvailable => (uint)Incoming.Count;

            public int Read(Span<byte> buffer)
            {
                ReadCalls++;
                int count = Math.Min(buffer.Length, Math.Min(ReadLimit, Incoming.Count));
                for (int i = 0; i < count; i++)
                {
                    buffer[i] = Incoming.Dequeue();
                }

                return count;
            }

            public int Write(ReadOnlySpan<byte> buffer)
            {
                Written.Add(buffer.ToArray());
                if (WriteError is not null)
                {
                    throw WriteError;
                }

                return buffer.Length;
            }

            public FtXSeriesConfiguration ReadConfiguration() => throw new NotSupportedException();

            public void ProgramConfiguration(FtXSeriesConfiguration configuration) => throw new NotSupportedException();

            public void Dispose() => Disposed = true;
        }
    }
}
