// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Disable these StyleCop rules for this file, as we are using native names here.
#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable SA1307 // Field should begin with upper-case letter
#pragma warning disable CS1591

using System;
using System.Runtime.InteropServices;

partial class Interop
{
    private const int _IOC_NRBITS = 8;
    private const int _IOC_TYPEBITS = 8;
    private const int _IOC_SIZEBITS = 14;
    private const int _IOC_DIRBITS = 2;

    private const int _IOC_NRMASK = (1 << _IOC_NRBITS) - 1;
    private const int _IOC_TYPEMASK = (1 << _IOC_TYPEBITS) - 1;
    private const int _IOC_SIZEMASK = (1 << _IOC_SIZEBITS) - 1;
    private const int _IOC_DIRMASK = (1 << _IOC_DIRBITS) - 1;

    private const int _IOC_NRSHIFT = 0;
    private const int _IOC_TYPESHIFT = _IOC_NRSHIFT + _IOC_NRBITS;
    private const int _IOC_SIZESHIFT = _IOC_TYPESHIFT + _IOC_TYPEBITS;
    private const int _IOC_DIRSHIFT = _IOC_SIZESHIFT + _IOC_SIZEBITS;

    private const int _IOC_NONE = 0;
    private const int _IOC_WRITE = 1;
    private const int _IOC_READ = 2;

    /*
* Used to create numbers.
*
* NOTE: _IOW means userland is writing and kernel is reading. _IOR
* means userland is reading and kernel is writing.
#define _IO(type,nr) _IOC(_IOC_NONE,(type),(nr),0)
#define _IOR(type,nr,size) _IOC(_IOC_READ,(type),(nr),(_IOC_TYPECHECK(size)))
#define _IOW(type,nr,size) _IOC(_IOC_WRITE,(type),(nr),(_IOC_TYPECHECK(size)))
#define _IOWR(type,nr,size) _IOC(_IOC_READ|_IOC_WRITE,(type),(nr),(_IOC_TYPECHECK(size)))
#define _IOR_BAD(type,nr,size) _IOC(_IOC_READ,(type),(nr),sizeof(size))
#define _IOW_BAD(type,nr,size) _IOC(_IOC_WRITE,(type),(nr),sizeof(size))
#define _IOWR_BAD(type,nr,size) _IOC(_IOC_READ|_IOC_WRITE,(type),(nr),sizeof(size))
*/

    public static int _IOC(int dir, int type, int nr, int size)
            => ((dir) << _IOC_DIRSHIFT) | ((type) << _IOC_TYPESHIFT) | ((nr) << _IOC_NRSHIFT) | ((size) << _IOC_SIZESHIFT);

    public static int _IO(int type, int nr) => _IOC(_IOC_NONE, type, nr, 0);
    public static int _IOR(int type, int nr, Type size) => _IOC(_IOC_READ, type, nr, _IOC_TYPECHECK(size));
    public static int _IOW(int type, int nr, Type size) => _IOC(_IOC_WRITE, type, nr, _IOC_TYPECHECK(size));
    public static int _IOWR(int type, int nr, Type size) => _IOC(_IOC_READ | _IOC_WRITE, type, nr, _IOC_TYPECHECK(size));
    public static int _IOC_TYPECHECK(Type t) => Marshal.SizeOf(t);
}
