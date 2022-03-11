// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Iot.Device.Arduino;

namespace ArduinoCsCompiler
{
    internal class EquatableField : IEquatable<EquatableField>
    {
        public EquatableField(FieldInfo field)
        {
            Field = field;
            Name = field.Name;
            DeclaringType = field.DeclaringType;
        }

        public string Name
        {
            get;
        }

        public Type? DeclaringType
        {
            get;
        }

        public FieldInfo Field { get; }

        public static implicit operator EquatableField(FieldInfo field)
        {
            return new EquatableField(field);
        }

        public bool Equals(EquatableField? other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (DeclaringType == null)
            {
                return Name == other.Name;
            }

            return Name == other.Name && DeclaringType == other.DeclaringType;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((EquatableField)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = ArduinoImplementationAttribute.GetStaticHashCode(Name);
                hashCode = (hashCode ^ ArduinoImplementationAttribute.GetStaticHashCode(DeclaringType != null ? DeclaringType.Name : "ROOT"));
                return hashCode;
            }
        }

        public override string ToString()
        {
            return $"Field: {DeclaringType?.Name}.{Name}";
        }
    }
}
