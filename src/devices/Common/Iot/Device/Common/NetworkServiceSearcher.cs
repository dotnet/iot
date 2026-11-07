// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Iot.Device.Common
{
    /// <summary>
    /// This is a helper class to search for network services on the local network.
    /// It can be used to find devices that respond to a specific HTTP request, for example.
    /// It is useful when a device does not properly implement mDNS or other discovery protocols and has a dynamic IP,
    /// but can be identified by a specific HTTP header or response.
    /// </summary>
    public class NetworkServiceSearcher
    {
        /// <summary>
        /// Get the default IP address to bind to
        /// </summary>
        /// <returns></returns>
        public static IPAddress GetLocalIPv4Address()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip;
                }
            }

            return IPAddress.Loopback;
        }

        /// <summary>
        /// Get all valid IP addresses in a subnet
        /// </summary>
        /// <param name="addressInSubnet">An IP address in the subnet (typically our own)</param>
        /// <param name="subnetMask">The subnet mask</param>
        /// <returns>A list of valid IP addresses in the subnet, excluding our own, the broadcast address, and the network address</returns>
        public static List<IPAddress> GetAllValidAddressesInSubnet(IPAddress addressInSubnet, IPAddress subnetMask)
        {
            byte[] ipAddressBytes = addressInSubnet.GetAddressBytes().Reverse().ToArray();
            if (ipAddressBytes.Length != 4)
            {
                throw new NotSupportedException("This method only supports IPv4 addresses");
            }

            byte[] subnetMaskBytes = subnetMask.GetAddressBytes().Reverse().ToArray();
            if (subnetMaskBytes.Length != 4)
            {
                throw new NotSupportedException("The subnet mask must be IPv4");
            }

            UInt32 rawaddress = BitConverter.ToUInt32(ipAddressBytes);
            UInt32 rawsubnetMask = BitConverter.ToUInt32(subnetMaskBytes);
            UInt32 rawsubnetMaskInversed = rawsubnetMask ^ 0xFFFFFFFF;
            // Now this is the starting address (not inclusive, since address x.x.x.0 is not available)
            rawaddress = rawaddress & rawsubnetMask;
            List<IPAddress> reply = new List<IPAddress>();
            UInt32 offset = 1;
            while (true)
            {
                UInt32 thisAddress = rawaddress + offset;
                byte[] addrBytes = BitConverter.GetBytes(thisAddress).Reverse().ToArray();
                IPAddress addressToUse = new IPAddress(addrBytes);
                if (!addressToUse.Equals(addressInSubnet))
                {
                    // Don't add our own
                    reply.Add(addressToUse);
                }

                offset++;
                // Abort when our offset is equal to the subnet mask (since address x.x.x.255 is also not available)
                if (offset >= rawsubnetMaskInversed)
                {
                    break;
                }
            }

            return reply;
        }

        /// <summary>
        /// Returns the first operational network interface that is not a loopback, along with its IP address and subnet mask.
        /// </summary>
        /// <returns>A tuple containing the IP address and subnet mask of the primary network interface</returns>
        public static (IPAddress Address, IPAddress Mask) GetPrimaryNetworkInterface()
        {
            var adapters = NetworkInterface.GetAllNetworkInterfaces();
            foreach (var adapter in adapters)
            {
                if (adapter.OperationalStatus != OperationalStatus.Up)
                {
                    continue;
                }

                if (adapter.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                {
                    continue;
                }

                var prop = adapter.GetIPProperties();
                foreach (var uni in prop.UnicastAddresses)
                {
                    if (uni.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        return (uni.Address, uni.IPv4Mask);
                    }
                }
            }

            return (IPAddress.Loopback, IPAddress.Parse("255.255.255.0"));
        }

        /// <summary>
        /// Returns true if the candidate IP address responds to an HTTP request with a specific header value, indicating it is a Yacht Devices interface.
        /// </summary>
        /// <param name="client">The HttpClient to use for the request</param>
        /// <param name="candidate">The candidate IP address to check</param>
        /// <param name="logger">An optional logger for logging information</param>
        /// <param name="expectedIdentifier">The expected identifier value in the HTTP header</param>
        /// <returns>True if the candidate IP address is a Yacht Devices interface, false otherwise</returns>
        public static async Task<bool> IsYachtDevicesInterface(HttpClient client, IPAddress candidate, ILogger? logger, string expectedIdentifier)
        {
            try
            {
                using CancellationTokenSource ts = new CancellationTokenSource(1000);
                var uri = new Uri($"http://{candidate.ToString()}/", UriKind.Absolute);
                var reply = await client.GetAsync(uri, ts.Token);
                // The header contains a single entry with the declaration "YDWG", which should
                // be enough to identify the device
                if (reply.IsSuccessStatusCode && reply.Headers.Any(x =>
                        x.Value.Any(y => y.Equals(expectedIdentifier, StringComparison.OrdinalIgnoreCase))))
                {
                    return true;
                }
            }
            catch (Exception x) when (x is UnauthorizedAccessException or SocketException or OperationCanceledException
                                          or AggregateException or HttpRequestException)
            {
                logger?.LogInformation($"{candidate} doesn't work: {x.Message}");
                return false;
            }
            catch (Exception y)
            {
                logger?.LogInformation($"Unexpected error trying: {y.Message}");
                return false;
            }

            return false;
        }
    }
}
