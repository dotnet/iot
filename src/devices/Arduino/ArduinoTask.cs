using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

#pragma warning disable CS1591
namespace Iot.Device.Arduino
{
    public interface IArduinoTask : IDisposable
    {
        MethodState State { get; }
        ArduinoMethodDeclaration MethodInfo { get; }
        void AddData(MethodState state, object[] args);
    }

    public sealed class ArduinoTask : IArduinoTask, IDisposable
    {
        private ConcurrentQueue<(MethodState, object[])> _collectedValues;
        private AutoResetEvent _dataAdded;
        internal ArduinoTask(ArduinoCsCompiler compiler, ArduinoMethodDeclaration methodInfo)
        {
            Compiler = compiler;
            MethodInfo = methodInfo;
            State = MethodState.Stopped;
            _collectedValues = new ConcurrentQueue<(MethodState, object[])>();
            _dataAdded = new AutoResetEvent(false);
        }

        public ArduinoCsCompiler Compiler { get; }
        public ArduinoMethodDeclaration MethodInfo { get; }

        /// <summary>
        /// Returns the current state of the task
        /// </summary>
        public MethodState State
        {
            get;
            private set;
        }

        public void AddData(MethodState state, object[] args)
        {
            _collectedValues.Enqueue((state, args));
            State = state;
            _dataAdded.Set();
        }

        public void InvokeAsync(params object[] arguments)
        {
            if (State == MethodState.Running)
            {
                throw new InvalidOperationException("Task is already running");
            }

            State = MethodState.Running;
            Compiler.Invoke(MethodInfo.MethodBase, arguments);
        }

        public bool Invoke(CancellationToken cancellationToken, params object[] arguments)
        {
            InvokeAsync(arguments);
            Task<bool> task = WaitForResult(cancellationToken);
            task.Wait(cancellationToken);
            return task.Result;
        }

        public void Terminate()
        {
            Compiler.KillTask(MethodInfo.MethodBase);
        }

        /// <summary>
        /// Returns a data set obtained from the realtime method.
        /// If this returns false, no data is available.
        /// If this returns true, the next data set is returned, together with the state of the task at that point.
        /// If the returned state is <see cref="MethodState.Stopped"/>, the data returned is the return value of the method.
        /// </summary>
        /// <param name="set">The execution set the result belongs to (required to look up correct return types)</param>
        /// <param name="data">A set of values sent or returned by the task method</param>
        /// <param name="state">The state of the method matching the task at that time</param>
        /// <returns>True if data was available, false otherwise</returns>
        public bool GetMethodResults(ExecutionSet set, out object[] data, out MethodState state)
        {
            if (_collectedValues.TryDequeue(out var d))
            {
                data = d.Item2;
                state = d.Item1;
                if (state == MethodState.Aborted && data.Length >= 2)
                {
                    int exceptionCode = (int)d.Item2[0];
                    int targetToken = (int)d.Item2[1];
                    SystemException ex = (SystemException)exceptionCode;

                    if (ex == SystemException.InvalidOpCode)
                    {
                        string instrName = OpCodeDefinitions.OpcodeDef[targetToken].Name;
                        throw new InvalidOperationException($"Invalid Opcode: 0x{targetToken:X4}: {instrName}");
                    }

                    var resolved = set.InverseResolveToken(targetToken);
                    if (resolved == null)
                    {
                        throw new InvalidOperationException("Internal error: Unknown exception arguments");
                    }

                    if (exceptionCode < 0xFF)
                    {
                        switch (ex)
                        {
                            case SystemException.MissingMethod:
                                throw new MissingMethodException(resolved.DeclaringType?.Name, resolved.Name);
                            case SystemException.NullReference:
                                throw new NullReferenceException($"NullReferenceException in {resolved.DeclaringType} - {resolved}");
                            case SystemException.StackOverflow:
                                throw new StackOverflowException($"StackOverflow in {resolved.DeclaringType} - {resolved}");

                            default:
                                throw new InvalidOperationException("Unknown exception");
                        }
                    }

                    // TypeInfo inherits from Type, so a TypeInfo is always also a Type
                    var resolvedException = set.InverseResolveToken(exceptionCode) as Type;

                    if (resolvedException == null)
                    {
                        throw new InvalidOperationException("Internal error: Unknown exception type");
                    }

                    Exception x = (Exception)Activator.CreateInstance(resolvedException, BindingFlags.Public, null, $"Location: {resolved.DeclaringType} - {resolved}")!;
                    throw x;
                }

                return true;
            }

            data = default!;
            state = State;
            return false;
        }

        public void Dispose()
        {
            Compiler.TaskDone(this);
            _collectedValues.Clear();
            _dataAdded?.Dispose();
            _dataAdded = null!;
        }

        /// <summary>
        /// Await at least one result data set (or the end of the method)
        /// </summary>
        /// <returns>True if the task has ended (whether successfully or with an error), false if a timeout occurred</returns>
        public async Task<bool> WaitForResult(CancellationToken token)
        {
            if (_dataAdded == null)
            {
                throw new ObjectDisposedException(nameof(ArduinoTask));
            }

            if (State != MethodState.Running)
            {
                return true;
            }

            var task = Task.Factory.StartNew(() =>
            {
                while (State == MethodState.Running)
                {
                    if (token.IsCancellationRequested)
                    {
                        return false;
                    }

                    WaitHandle.WaitAny(new WaitHandle[]
                    {
                        token.WaitHandle, _dataAdded
                    });
                }

                return !_collectedValues.IsEmpty;
            });

            await task;
            return task.Result;
        }

        public bool WaitForResult()
        {
            var task = WaitForResult(CancellationToken.None);
            task.Wait();
            return task.Result;
        }
    }
}
