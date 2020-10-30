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

    public sealed class ArduinoTask<T> : IArduinoTask, IDisposable
        where T : Delegate
    {
        private ConcurrentQueue<(MethodState, object[])> _collectedValues;
        private AutoResetEvent _dataAdded;
        internal ArduinoTask(T function, ArduinoCsCompiler compiler, ArduinoMethodDeclaration methodInfo)
        {
            Function = function;
            Compiler = compiler;
            MethodInfo = methodInfo;
            State = MethodState.Stopped;
            _collectedValues = new ConcurrentQueue<(MethodState, object[])>();
            _dataAdded = new AutoResetEvent(false);
        }

        public T Function { get; }
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

        public void InvokeAsync(params int[] arguments)
        {
            if (State == MethodState.Running)
            {
                throw new InvalidOperationException("Task is already running");
            }

            State = MethodState.Running;
            Compiler.Invoke(MethodInfo.MethodInfo, arguments);
        }

        public bool Invoke(CancellationToken cancellationToken, params int[] arguments)
        {
            InvokeAsync(arguments);
            Task<bool> task = WaitForResult(cancellationToken);
            task.Wait(cancellationToken);
            return task.Result;
        }

        public void Terminate()
        {
            Compiler.KillTask(MethodInfo.MethodInfo);
        }

        /// <summary>
        /// Returns a data set obtained from the realtime method.
        /// If this returns false, no data is available.
        /// If this returns true, the next data set is returned, together with the state of the task at that point.
        /// If the returned state is <see cref="MethodState.Stopped"/>, the data returned is the return value of the method.
        /// </summary>
        /// <param name="data">A set of values sent or returned by the task method</param>
        /// <param name="state">The state of the method matching the task at that time</param>
        /// <returns>True if data was available, false otherwise</returns>
        public bool GetMethodResults(out object[] data, out MethodState state)
        {
            if (_collectedValues.TryDequeue(out var d))
            {
                data = d.Item2;
                state = d.Item1;
                return true;
            }

            data = null;
            state = State;
            return false;
        }

        public void Dispose()
        {
            Compiler.TaskDone(this);
            _collectedValues.Clear();
            _dataAdded?.Dispose();
            _dataAdded = null;
        }

        /// <summary>
        /// Await at least one result data set (or the end of the method)
        /// </summary>
        /// <returns>True if there is at least one data set, false otherwise</returns>
        public async Task<bool> WaitForResult(CancellationToken token)
        {
            if (_dataAdded == null)
            {
                throw new ObjectDisposedException(nameof(ArduinoTask<T>));
            }

            if (State != MethodState.Running)
            {
                return false;
            }

            var task = Task.Factory.StartNew(() =>
            {
                while (_collectedValues.IsEmpty && State == MethodState.Running)
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
