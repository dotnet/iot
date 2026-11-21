// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Iot.Device.Common;
using Xunit;

namespace Common.Tests
{
    public class NetworkServiceSearcherTest
    {
        /// <summary>
        /// Check the primary network interface is not the loopback interface.
        /// </summary>
        [Fact]
        public void GetPrimaryNetworkInterface()
        {
            var result = NetworkServiceSearcher.GetPrimaryNetworkInterface();
            var available = NetworkInterface.GetIsNetworkAvailable();

            if (available)
            {
                Assert.NotEqual(result.Address, IPAddress.Loopback);
            }
            else
            {
                Assert.Equal(result.Address, IPAddress.Loopback);
            }
        }

        [Fact]
        public void GetAllValidAddressesInSubnet()
        {
            var list = NetworkServiceSearcher.GetAllValidAddressesInSubnet(IPAddress.Parse("192.168.1.10"),
                IPAddress.Parse("255.255.255.0"));
            Assert.NotNull(list);
            Assert.Equal(253, list.Count);
            Assert.Equal(IPAddress.Parse("192.168.1.1"), list[0]);
        }
    }
}
