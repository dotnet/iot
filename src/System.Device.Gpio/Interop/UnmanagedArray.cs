// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.InteropServices;

namespace System.Device.Gpio;

internal sealed class UnmanagedArray<T> : SafeHandle
{
    private readonly int _arrayLength;
    private readonly int _typeSize;

    public UnmanagedArray(int arrayLength)
        : base(IntPtr.Zero, true)
    {
        if (arrayLength < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(arrayLength), "Length must be 0 or greater");
        }

        _arrayLength = arrayLength;
        _typeSize = Marshal.SizeOf<T>();
        SetHandle(Marshal.AllocHGlobal(_typeSize * arrayLength));
    }

    protected override bool ReleaseHandle()
    {
        Marshal.FreeHGlobal(handle);
        return true;
    }

    public override bool IsInvalid => handle == IntPtr.Zero;

    public T[] ReadToManagedArray()
    {
        var managedArray = new T[_arrayLength];
        var unmanagedArrayPtrAsLong = Environment.Is64BitOperatingSystem ? handle.ToInt64() : handle.ToInt32();

        for (int i = 0; i < _arrayLength; i++)
        {
            IntPtr elementPtr = new(unmanagedArrayPtrAsLong + i * _typeSize);
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
