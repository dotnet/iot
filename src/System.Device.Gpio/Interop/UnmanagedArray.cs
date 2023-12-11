// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.InteropServices;

namespace System.Device.Gpio;

internal sealed class UnmanagedArray<T> : SafeHandle
{
    private readonly int _length;
    private readonly int _size;

    public static readonly UnmanagedArray<T> Empty = new(0);

    public UnmanagedArray(int length)
        : base(IntPtr.Zero, true)
    {
        if (length < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(length), "Length must be 0 or greater");
        }

        _length = length;
        _size = Marshal.SizeOf(typeof(T));
        SetHandle(Marshal.AllocHGlobal(_size * length));
    }

    protected override bool ReleaseHandle()
    {
        Marshal.FreeHGlobal(handle);
        return true;
    }

    public override bool IsInvalid => handle == IntPtr.Zero;

    public T[] ReadToManagedArray()
    {
        var managedArray = new T[_length];
        var unmanagedArrayPtrAsLong = Environment.Is64BitOperatingSystem ? handle.ToInt64() : handle.ToInt32();

        for (int i = 0; i < _length; i++)
        {
            IntPtr elementPtr = new(unmanagedArrayPtrAsLong + i * _size);
            var structure = Marshal.PtrToStructure<T>(elementPtr);
            if (structure == null)
            {
                continue;
            }

            managedArray[i] = structure;
        }

        return managedArray;
    }

    public static implicit operator IntPtr(UnmanagedArray<T> unmanagedArray)
    {
        return !unmanagedArray.IsInvalid ? unmanagedArray.handle : throw new InvalidOperationException("Invalid handle");
    }
}

