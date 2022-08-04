// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using System.Diagnostics;
using System.IO.PortsTests;
using Legacy.Support;
using Xunit;

namespace System.Device.Ports.SerialPort.Tests
{
    public class Ctor_IContainer : PortsTest
    {
        [Fact]
        public void Verify()
        {
            SerialPortProperties serPortProp = new SerialPortProperties();
            Container container = new Container();
            using (SerialPort com = new SerialPort(container))
            {
                Assert.Single(container.Components);
                Assert.Equal(com, container.Components[0]);

                serPortProp.SetAllPropertiesToDefaults();

                Debug.WriteLine("Verifying properties is called");
                serPortProp.VerifyPropertiesAndPrint(com);
            }
        }
    }
}
