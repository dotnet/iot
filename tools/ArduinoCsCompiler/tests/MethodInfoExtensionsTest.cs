// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ArduinoCsCompiler;
using Iot.Device.Arduino;
using Xunit;

namespace Iot.Device.Arduino.Tests
{
    public class MethodInfoExtensionsTest
    {
        [Fact]
        public void ClassSignature()
        {
            Assert.Equal("System.String", typeof(string).ClassSignature());
            Assert.Equal("System.Int32", typeof(Int32).ClassSignature());
            Assert.Equal("System.Collections.Generic.List<T>", typeof(List<>).ClassSignature());
            Assert.Equal("System.Collections.Generic.List<System.Int32>", typeof(List<int>).ClassSignature());
            Assert.Equal("System.Collections.Generic.List<System.Collections.Generic.List<System.Int32>>", typeof(List<List<int>>).ClassSignature());
            Assert.Equal("System.Collections.Generic.Dictionary<TKey, TValue>", typeof(Dictionary<,>).ClassSignature());
            Assert.Equal("System.Collections.Generic.Dictionary<System.Int32, System.String>", typeof(Dictionary<int, string>).ClassSignature());
            Assert.Equal("Dictionary<Int32, String>", typeof(Dictionary<int, string>).ClassSignature(false));
            Assert.Equal("String", typeof(string).ClassSignature(false));
            Assert.Equal("WithGenericArg<T>", typeof(WithGenericArg<>).ClassSignature(false));
            Assert.Equal("WithGenericArg<Int32>+Internal", typeof(WithGenericArg<Int32>.Internal).ClassSignature(false));
            Assert.Equal("WithGenericArg<Int32>+Internal2<Boolean>", typeof(WithGenericArg<Int32>.Internal2<bool>).ClassSignature(false));
        }

        [Theory]
        [InlineData(typeof(ClassDeclaration), "AddClassMember", false, "public void ClassDeclaration.AddClassMember(ClassMember member)")]
        [InlineData(typeof(ClassDeclaration), "ToString", false, "public virtual String ClassDeclaration.ToString()")]
        [InlineData(typeof(Dictionary<int, string>), "Add", true, "public virtual void System.Collections.Generic.Dictionary<System.Int32, System.String>.Add(System.Int32 key, System.String value)")]
        [InlineData(typeof(WithGenericArg<Int32>.Internal), "Foo", true, "public System.Int32 Iot.Device.Arduino.Tests.WithGenericArg<System.Int32>+Internal.Foo()")]
        [InlineData(typeof(WithGenericArg<Int32>.Internal2<bool>), "Foo2", true, "public System.Boolean Iot.Device.Arduino.Tests.WithGenericArg<System.Int32>+Internal2<System.Boolean>.Foo2()")]
        public void MethodSignature(Type cls, string methodName, bool useFullNamespaces, string expectedString)
        {
            var me = cls.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (me == null)
            {
                throw new InvalidOperationException("Method not found");
            }

            Assert.Equal(expectedString, me.MethodSignature(useFullNamespaces));
        }

        [Fact]
        public void MethodSignature2()
        {
            var me = typeof(WithGenericArg<int>).GetMethod("TestGenericArg", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (me == null)
            {
                throw new InvalidOperationException("Method not found");
            }

            me = me.MakeGenericMethod(typeof(string));

            Assert.Equal("public String WithGenericArg<Int32>.TestGenericArg<String>(String input)", me.MethodSignature(false));
        }
    }
}
