// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.BrickPi3
{

    /// <summary>
    /// Exceptions classes
    /// </summary>
    public class SpiExceptions
    {
        public class FirmwareVersionError : Exception
        {
            public FirmwareVersionError() : base() { }
            public FirmwareVersionError(string msg) : base(msg) { }
            public FirmwareVersionError(string msg, Exception ex) : base(msg, ex) { }
        }

        public class SensorError : Exception
        {
            public SensorError() : base() { }
            public SensorError(string msg) : base(msg) { }
            public SensorError(string msg, Exception ex) : base(msg, ex) { }
        }

        public class I2cError : Exception
        {
            public I2cError() : base() { }
            public I2cError(string msg) : base(msg) { }
            public I2cError(string msg, Exception ex) : base(msg, ex) { }
        }

        public class ValueError : Exception
        {
            public ValueError() : base() { }
            public ValueError(string msg) : base(msg) { }
            public ValueError(string msg, Exception ex) : base(msg, ex) { }
        }

        public class IOError : Exception
        {
            public IOError() : base() { }
            public IOError(string msg) : base(msg) { }
            public IOError(string msg, Exception ex) : base(msg, ex) { }
        }

        public class RuntimeError : Exception
        {
            public RuntimeError() : base() { }
            public RuntimeError(string msg) : base(msg) { }
            public RuntimeError(string msg, Exception ex) : base(msg, ex) { }
        }
    }
}

