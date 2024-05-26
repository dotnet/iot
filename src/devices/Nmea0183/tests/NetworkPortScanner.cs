// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;
using System.Text;
using Xunit;

namespace Iot.Device.Nmea0183.Tests
{
    /// <summary>
    /// This class helps finding a free network port for a test.
    /// Could be moved to the real library, but with what name?
    /// </summary>
    internal class NetworkPortScanner
    {
        /// <summary>
        /// Returns the next free port to a given start port number.
        /// If in a unit test the same port is opened quickly in sequence it might fail since Windows TCP / IP stack
        /// keeps the port some time in TIME_WAIT for an amount of time and it would fail if tried to open during that time.
        /// So choose another port for each test run.
        /// NOTE: Active ports may change while enumerating that but there's no chance to handle that.
        /// </summary>
        public static int GetNextAvailableTcpPort(int startPortNumber)
        {
            var portNumber = startPortNumber;
            try
            {
                while (portNumber < UInt16.MaxValue - 1)
                {
                    if (IsPortAvailable(portNumber))
                    {
                        return portNumber;
                    }

                    portNumber++;
                }
            }
            catch (SocketException)
            {
                // The above approach didn't work, try alternate
                while (portNumber < UInt16.MaxValue - 1)
                {
                    if (IsPortAvailableViaOpeningSocket(portNumber))
                    {
                        return portNumber;
                    }

                    portNumber++;
                }
            }

            throw new InvalidOperationException($"No free port found above {startPortNumber}");
        }

        private static bool IsPortAvailableViaOpeningSocket(int portNumber)
        {
            TcpListener? listener = null;
            try
            {
                listener = TcpListener.Create(portNumber);
                listener.Start();
                listener.Stop();
            }
            catch (SocketException)
            {
                return false;
            }
            finally
            {
                listener?.Stop();
            }

            return true;
        }

        /// <summary>
        /// Checks whether the given port is available.
        /// Note that the operation is not atomic - the result could already be obsolete when the method returns
        /// </summary>
        public static bool IsPortAvailable(int portNumber)
        {
            if (ListLocalActiveTcpEndpoints().Any(x => x.Port == portNumber))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Returns a list of all active endpoints on the system.
        /// </summary>
        private static IEnumerable<IPEndPoint> ListLocalActiveTcpEndpoints()
        {
            var ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            var activeTcpConnections = ipGlobalProperties.GetActiveTcpConnections();
            List<IPAddress> localIpAddresses;

            string ownHostName = Dns.GetHostName();

            try
            {
                Assert.False(string.IsNullOrWhiteSpace(ownHostName));
                localIpAddresses = Dns.GetHostEntry(ownHostName).AddressList
                    .Where(x => x.AddressFamily == AddressFamily.InterNetwork).ToList();
            }
            catch (SocketException)
            {
                // Documentation says that an empty string returns the local IPs (null is not valid, though)
                localIpAddresses = Dns.GetHostAddresses(string.Empty).Where(x => x.AddressFamily == AddressFamily.InterNetwork).ToList();
            }

            localIpAddresses.Add(IPAddress.Loopback);
            // All TCP clients with remote side local machine (not the same than listeners)
            var remoteEp = activeTcpConnections.Select(ac => ac.RemoteEndPoint).Where(rp => localIpAddresses.Contains(rp.Address)).ToList();
            // All TCP clients local side
            var localEp = activeTcpConnections.Select(ac => ac.LocalEndPoint).ToList();
            // All TCP servers local side
            var listenerEp = ipGlobalProperties.GetActiveTcpListeners();
            var activeEndPoints = new List<IPEndPoint>();
            activeEndPoints.AddRange(remoteEp);
            activeEndPoints.AddRange(localEp);
            activeEndPoints.AddRange(listenerEp);
            return activeEndPoints;
        }
    }
}
