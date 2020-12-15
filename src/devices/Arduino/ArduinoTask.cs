using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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
                    // The token (if any) that caused the problem. May also be the error location
                    int targetToken = (int)d.Item2[1];
                    SystemException sysEx = (SystemException)exceptionCode;

                    List<int> stackTrace = new List<int>();
                    stackTrace.AddRange(d.Item2.Cast<int>());

                    string textualStackTrace = String.Empty;
                    // The first 0 indicates the start of the stack trace, so search it
                    int idx = stackTrace.IndexOf(0);
                    if (idx > 0)
                    {
                        stackTrace.RemoveRange(0, idx + 1);

                        foreach (var m in stackTrace)
                        {
                            // this can be the same as above (if the error is within the given method), but not
                            // necessarily, since the outer token can point to whatever did not work
                            var resolved2 = set.InverseResolveToken(m);
                            if (resolved2 != null)
                            {
                                textualStackTrace += $" at {resolved2.DeclaringType} - {resolved2}\r\n";
                            }
                        }
                    }

                    if (sysEx == SystemException.InvalidOpCode)
                    {
                        string instrName = OpCodeDefinitions.OpcodeDef[targetToken].Name;
                        throw new InvalidOperationException($"Invalid Opcode: 0x{targetToken:X4}: {instrName}" + textualStackTrace);
                    }

                    var resolved = set.InverseResolveToken(targetToken);
                    if (resolved == null)
                    {
                        // We're probably missing a method - let's at least show the main context
                        resolved = set.MainEntryPointInternal!;
                    }

                    Exception ex;
                    if (exceptionCode < 0xFF)
                    {
                        switch (sysEx)
                        {
                            case SystemException.MissingMethod:
                                ex = new MissingMethodException(resolved.DeclaringType?.Name, resolved.Name + textualStackTrace);
                                break;
                            case SystemException.NullReference:
                                ex = new NullReferenceException($"NullReferenceException in {resolved.DeclaringType} - {resolved} " + textualStackTrace);
                                break;
                            case SystemException.StackOverflow:
                                ex = new StackOverflowException($"StackOverflow in {resolved.DeclaringType} - {resolved} " + textualStackTrace);
                                break;
                            case SystemException.DivideByZero:
                                ex = new DivideByZeroException($"Integer Division by zero in {resolved.DeclaringType} - {resolved} " + textualStackTrace);
                                break;
                            case SystemException.IndexOutOfRange:
                                ex = new IndexOutOfRangeException($"Index out of range in {resolved.DeclaringType} - {resolved} " + textualStackTrace);
                                break;
                            case SystemException.OutOfMemory:
                                ex = new OutOfMemoryException($"Out of memory allocating an instance of {resolved.DeclaringType} - {resolved} " + textualStackTrace);
                                break;
                            case SystemException.ArrayTypeMismatch:
                                ex = new ArrayTypeMismatchException($"Array type did not match in STELM or LDELEM instruction in {resolved.DeclaringType} - {resolved} " + textualStackTrace);
                                break;
                            case SystemException.InvalidOperation:
                                ex = new InvalidOperationException($"An invalid operation was attempted in {resolved.DeclaringType} - {resolved}.");
                                break;
                            case SystemException.ClassNotFound:
                                ex = new TypeInitializationException($"{resolved.DeclaringType} - {resolved}", new MissingMethodException());
                                break;
                            case SystemException.InvalidCast:
                                ex = new InvalidCastException($"Cast to {resolved.DeclaringType} - {resolved} is not possible. " + textualStackTrace);
                                break;
                            case SystemException.NotSupported:
                                ex = new NotSupportedException($"An unsupported operation was attempted in {resolved.DeclaringType} - {resolved} at " + textualStackTrace);
                                break;
                            default:
                                ex = new InvalidOperationException("Unknown exception " + textualStackTrace);
                                break;
                        }
                    }
                    else
                    {
                        // TypeInfo inherits from Type, so a TypeInfo is always also a Type
                        var resolvedException = set.InverseResolveToken(exceptionCode) as Type;

                        if (resolvedException == null)
                        {
                            throw new InvalidOperationException("Internal error: Unknown exception type");
                        }

                        ex = (Exception)Activator.CreateInstance(resolvedException, BindingFlags.Public, null, $"Location: {resolved.DeclaringType} - {resolved} {textualStackTrace}")!;
                    }

                    throw ex;
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
