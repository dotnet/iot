using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Iot.Device.Arduino;
using Xunit;

namespace Arduino.Tests
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
        }

        [Theory]
        [InlineData(typeof(ClassDeclaration), "AddClassMember", false, "public void ClassDeclaration.AddClassMember(ClassMember member)")]
        [InlineData(typeof(ClassDeclaration), "ToString", false, "public virtual String ClassDeclaration.ToString()")]
        [InlineData(typeof(System.Collections.Generic.Dictionary<int, string>), "Add", true, "public virtual void System.Collections.Generic.Dictionary<System.Int32, System.String>.Add(System.Int32 key, System.String value)")]
        public void MethodSignature(Type cls, string methodName, bool useFullNamespaces, string expectedString)
        {
            var me = cls.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (me == null)
            {
                throw new InvalidOperationException("Method not found");
            }

            Assert.Equal(expectedString, me.MethodSignature(useFullNamespaces));
        }
    }
}
