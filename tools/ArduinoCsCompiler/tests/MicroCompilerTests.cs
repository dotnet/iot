// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArduinoCsCompiler;
using UnitsNet.Units;
using Xunit;

namespace Iot.Device.Arduino.Tests
{
    /// <summary>
    /// Unit tests for the compiler (integration tests for the compiler are separate)
    /// </summary>
    public class MicroCompilerTests
    {
        [Fact]
        public void TestClassComparator()
        {
            Type a = typeof(ElectricChargeUnit[]);
            Type b = typeof(ElectricAdmittanceUnit[]);

            ClassDeclaration ca = new ClassDeclaration(a, 4, 4, 1, new List<ClassMember>(), new List<Type>());
            ClassDeclaration cb = new ClassDeclaration(b, 4, 4, 2, new List<ClassMember>(), new List<Type>());

            Assert.False(ca.Equals(cb));

            var cp = new MicroCompiler.ClassDeclarationByInheritanceSorter();

            int result1 = cp.Compare(ca, cb);
            int result2 = cp.Compare(cb, ca);
            Assert.NotEqual(result1, result2);
            Assert.NotEqual(0, result1);
        }

        [Fact]
        public void AssignArrays()
        {
            Type a = typeof(S1[]);
            Type b = typeof(S2[]);

            Assert.True(a.IsAssignableFrom(b));
            Assert.True(b.IsAssignableFrom(a));
        }

        [Fact]
        public void AssignEnums()
        {
            Type a = typeof(S1);
            Type b = typeof(S2);

            Assert.False(a.IsAssignableFrom(b));
            Assert.False(b.IsAssignableFrom(a));
        }

        [Fact]
        public void NullableEqualityComparer()
        {
            var t = new NullableEqualityComparer1<int>();
            Assert.False(t.Equals(null));
        }

        [Fact]
        public void Nullability1()
        {
            var t = typeof(Nullable<int>);
            var args = t.GetGenericArguments();
            Assert.True(args.Length == 1);
            Assert.Equal(typeof(int), args[0]);
            var targs = t.GenericTypeArguments;
            Assert.Equal(args, targs);
        }

        internal enum S1
        {
            None,
            One,
            Two
        }

        internal enum S2
        {
            Keins,
            Eins,
            Zwei,
        }

        public sealed class NullableEqualityComparer1<T> : EqualityComparer<T?>
            where T : struct, IEquatable<T>
        {
            public override bool Equals(T? x, T? y)
            {
                throw new NotImplementedException();
            }

            public override int GetHashCode(T? obj)
            {
                throw new NotImplementedException();
            }
        }
    }
}
