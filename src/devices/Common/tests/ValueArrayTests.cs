// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Iot.Device.Common.Tests
{
    public class ValueArrayTests
    {
        private ValueArray<byte> _myArray;
        private byte[] _realArray;

        public ValueArrayTests()
        {
            _myArray = default;
            _realArray = new byte[ValueArray<Byte>.MaximumSize];
        }

        [Fact]
        public void InitialCondition()
        {
#pragma warning disable xUnit2013 // Do not use equality check to check for collection size.
            Assert.Equal(0, _myArray.Count);
#pragma warning restore xUnit2013 // Do not use equality check to check for collection size.
            Assert.Empty(_myArray);
        }

        [Fact]
        public void AssigningDefaultClearsArray()
        {
            _myArray.Count = 7;
            _myArray[2] = 33;
            _myArray = default;
            Assert.Empty(_myArray);
            Assert.Throws<IndexOutOfRangeException>(() => _myArray[2]);
            _myArray.Count = 7;
            Assert.Equal(0, _myArray[2]);
        }

        [Fact]
        public void CanBeConvertedToSpan()
        {
            _realArray[0] = 1;
            _realArray[1] = 2;
            var span = _realArray.AsSpan();
            Assert.Equal(1, span[0]);

            _myArray.Count = 2;
            _myArray[1] = 2;
            span = _myArray.AsSpan();
            Assert.Equal(2, span[1]);
        }

        [Fact]
        public void CanBeIteratedOver()
        {
            _myArray.Count = 3;
            _myArray[0] = 1;
            _myArray[1] = 2;
            _myArray[2] = 3;
            int sum = 0;
            int c = 0;
            foreach (byte b in _myArray)
            {
                sum += b;
                c++;
            }

            Assert.Equal(6, sum);
            Assert.Equal(3, c);
        }

        [Fact]
        public void InsertWorks()
        {
            var arr = new ValueArray<int>(5);
            for (int i = 0; i < 5; i++)
            {
                arr[i] = i + 1;
            }

            Assert.Equal(1, arr[0]);
            arr.Insert(0, 99);
            Assert.Equal(99, arr[0]);
            Assert.Equal(1, arr[1]);
            Assert.Equal(2, arr[2]);
            Assert.Equal(6, arr.Count);

            arr.Insert(1, 28);
            Assert.Equal(99, arr[0]);
            Assert.Equal(28, arr[1]);
            Assert.Equal(1, arr[2]);
            Assert.Equal(7, arr.Count);

            arr.Insert(7, 100);
            Assert.Equal(8, arr.Count);
            Assert.Equal(100, arr[7]);
        }

        [Fact]
        public void InsertFailsWhenFull()
        {
            _myArray.Count = 7;
            _myArray.Insert(1, 2);
            Assert.Throws<InvalidOperationException>(() => _myArray.Insert(2, 3));
            Assert.Throws<InvalidOperationException>(() => _myArray.Insert(8, 10));
            _myArray.Count = 2;
            Assert.Throws<IndexOutOfRangeException>(() => _myArray.Insert(7, 33));
        }

        [Fact]
        public void AddWorks()
        {
            _myArray = default;
            _myArray.Add(77);
            _myArray.Add(78);
            Assert.Equal(2, _myArray.Count);
            Assert.Equal(77, _myArray[0]);
            Assert.Equal(78, _myArray[1]);
        }

        [Fact]
        public void RemoveWorks()
        {
            _myArray = default;
            _myArray.Add(1);
            Assert.Single(_myArray);

            _myArray.Remove(1);
            Assert.Empty(_myArray);
        }

        [Fact]
        public void RemoveAtWorks()
        {
            _myArray.Count = 2;
            _myArray[1] = 1;
            _myArray.RemoveAt(0);
            Assert.Equal(1, _myArray[0]);
            Assert.Single(_myArray);
        }

        [Fact]
        public void RemoveWorksWhenFull()
        {
            _myArray = default;
            _myArray.Count = 8;
            _myArray[7] = 77;
            _myArray[2] = 22;
            _myArray.RemoveAt(2);
            Assert.Equal(7, _myArray.Count);
            Assert.Equal(77, _myArray[6]);
            Assert.NotEqual(22, _myArray[2]);
        }

        [Fact]
        public void RemoveFailsWhenEmpty()
        {
            _myArray.Count = 0;
            Assert.Throws<IndexOutOfRangeException>(() => _myArray.RemoveAt(0));
        }

        [Fact]
        public void RemoveFailsWhenOutOfBounds()
        {
            _myArray.Count = 2;
            Assert.Throws<IndexOutOfRangeException>(() => _myArray.RemoveAt(6));
        }

        [Fact]
        public unsafe void LargerTypeWorks()
        {
            var arr = new ValueArray<Int64>();
            Assert.True(sizeof(ValueArray<Int64>) > sizeof(Int64) * ValueArray<Int64>.MaximumSize);
            arr.Add(8589934591);
            arr.Add(0);
            arr.Insert(0, 1234567890);
            Assert.Equal(8589934591, arr[1]);
        }

        [Fact]
        public void ContainsWorks()
        {
            _myArray = default;
#pragma warning disable xUnit2017 // Do not use Contains() to check if a value exists in a collection
            Assert.False(_myArray.Contains(11));
            _myArray.Add(12);
            _myArray.Add(13);
            Assert.True(_myArray.Contains(13));
#pragma warning restore xUnit2017 // Do not use Contains() to check if a value exists in a collection
        }

        [Fact]
        public void CanCopyToArray()
        {
            _myArray = default;
            _myArray.Add(10);
            _myArray.Add(20);

            var bytearr = new byte[2];
            _myArray.CopyTo(bytearr, 0);
            Assert.Equal(10, bytearr[0]);
            Assert.Equal(20, bytearr[1]);
        }
    }
}
