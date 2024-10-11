// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArduinoCsCompiler
{
    internal class AnalysisStack : ICloneable
    {
        private Stack<EquatableMethod> _stack;
        public AnalysisStack()
        {
            _stack = new Stack<EquatableMethod>();
        }

        public AnalysisStack(EquatableMethod method)
            : this()
        {
            Push(method);
        }

        public void Push(EquatableMethod method)
        {
            _stack.Push(method);
        }

        public void Pop()
        {
            _stack.Pop();
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        public AnalysisStack Clone()
        {
            var ret = new AnalysisStack();
            ret._stack = new Stack<EquatableMethod>(_stack);
            return ret;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var e in _stack)
            {
                sb.AppendLine(e.MethodSignature(false));
            }

            return sb.ToString();
        }
    }
}
