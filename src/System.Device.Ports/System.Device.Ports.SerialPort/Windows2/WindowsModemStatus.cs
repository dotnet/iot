// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.Ports.SerialPort.Windows;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using Windows.Win32;
using Windows.Win32.Devices.Communication;

#pragma warning disable CA1416 // Validate platform compatibility

namespace System.Device.Ports.SerialPort.Windows2
{
    internal class WindowsModemStatus
    {
        private readonly SafeHandle _portHandle;
        private MODEM_STATUS_FLAGS _status;

        public WindowsModemStatus(SafeHandle portHandle)
        {
            _portHandle = portHandle;
        }

        private MODEM_STATUS_FLAGS GetPinStatus()
        {
            if (_portHandle == null || _portHandle.IsInvalid)
            {
                throw new InvalidOperationException(Strings.Port_not_open);
            }

            if (!PInvoke.GetCommModemStatus(_portHandle, out MODEM_STATUS_FLAGS pinStatus))
            {
                WindowsHelpers.GetExceptionForLastWin32Error();
            }

            return pinStatus;
        }

        public void Read() => _status = GetPinStatus();
        /* public void Write() => */

        public bool SignalCD
        {
            get => _status.HasFlag(MODEM_STATUS_FLAGS.MS_RLSD_ON);
        }

        public bool SignalCTS
        {
            get => _status.HasFlag(MODEM_STATUS_FLAGS.MS_CTS_ON);
        }

        public bool SignalDSR
        {
            get => _status.HasFlag(MODEM_STATUS_FLAGS.MS_DSR_ON);
        }
    }
}

#pragma warning restore CA1416 // Validate platform compatibility
