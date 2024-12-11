// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ArduinoCsCompiler
{
    public class DebuggerVariable : ClassMember
    {
        public DebuggerVariable(string name, VariableKind variableType, int token, int sizeOfField)
            : base(name, variableType, token, sizeOfField)
        {
        }

        public DebuggerVariable(FieldInfo field, VariableKind variableType, int token, int sizeOfField)
            : base(field, variableType, token, sizeOfField, 0)
        {
        }

        public object? Value
        {
            get;
            set;
        }

        public Type? Type
        {
            get;
            set;
        }

        public override string ToString()
        {
            string typeName = string.Empty;
            if (Type != null)
            {
                typeName = Type.Name;
            }
            else
            {
                typeName = "Type dynamically assigned";
            }

            string pre = $"{Name} (Runtime type {VariableType}; declared type {typeName}): ";
            if (VariableType == VariableKind.Object)
            {
                return $"{pre}Reference to object at 0x{Value:X}";
            }
            else if (VariableType == VariableKind.AddressOfVariable)
            {
                return $"{pre}Indirect reference to variable at 0x{Value:X}";
            }
            else if (VariableType == VariableKind.FunctionPointer)
            {
                return $"{pre}Function pointer address 0x{Value:X}";
            }
            else if (Type == typeof(DateTime))
            {
                try
                {
                    Int64 v = 0;
                    if (Value is Int64)
                    {
                        v = (Int64)Value;
                    }
                    else if (Value is UInt64)
                    {
                        v = (Int64)((UInt64)Value);
                    }

                    DateTime dt = new DateTime(v);
                    return $"{pre}{dt.ToLongTimeString()}";
                }
                catch (ArgumentOutOfRangeException)
                {
                    return $"{pre}{Value} (invalid DateTime value)";
                }
            }
            else if (Type == typeof(TimeSpan))
            {
                try
                {
                    Int64 v = 0;
                    if (Value is Int64)
                    {
                        v = (Int64)Value;
                    }
                    else if (Value is UInt64)
                    {
                        v = (Int64)((UInt64)Value);
                    }

                    TimeSpan dt = new TimeSpan(v);
                    return $"{pre}{dt.ToString()}";
                }
                catch (ArgumentOutOfRangeException)
                {
                    return $"{pre}{Value} (invalid TimeSpan value)";
                }
            }
            else if (Value != null)
            {
                return $"{pre}{Value}";
            }
            else
            {
                return $"{pre}(The debugger cannot show this value type)";
            }
        }
    }
}
