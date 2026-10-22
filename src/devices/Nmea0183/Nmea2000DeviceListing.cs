// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Iot.Device.Nmea0183.Sentences;

namespace Iot.Device.Nmea0183
{
    /// <summary>
    /// Helps generate a list of devices on the NMEA 2000 bus.
    /// It will send out an address claim request and then a product information request to all devices on the bus, and then collect the responses.
    /// </summary>
    public class Nmea2000DeviceListing : NmeaSinkAndSource
    {
        private enum ComponentState
        {
            WaitingForConnection,
            AddressClaimSent,
            ProductInformationRequestSent,
            WaitingForData,
        }

        private DateTimeOffset _lastUpdate;
        private ConcurrentDictionary<uint, IsoAddressClaim> _devices;
        private ConcurrentDictionary<uint, ProductInformation> _deviceProductInformation;
        private ComponentState _state;

        /// <summary>
        /// Initializes a new instance of the Nmea2000DeviceListing class.
        /// </summary>
        /// <param name="interfaceName">The name of the interface for filtering.</param>
        public Nmea2000DeviceListing(string interfaceName)
            : base(interfaceName)
        {
            _state = ComponentState.WaitingForConnection;
            _lastUpdate = DateTimeOffset.UnixEpoch;
            _devices = new ConcurrentDictionary<uint, IsoAddressClaim>();
            _deviceProductInformation = new ConcurrentDictionary<uint, ProductInformation>();
            UpdateInterval = TimeSpan.FromMinutes(10);
        }

        /// <summary>
        /// Gets or sets the interval at which the device list is updated.
        /// The default is 10 minutes. Set to TimeSpan.Zero to disable automatic updates.
        /// </summary>
        public TimeSpan UpdateInterval
        {
            get;
            set;
        }

        /// <inheritdoc/>
        public override void StartDecode()
        {
        }

        /// <inheritdoc/>
        public override void SendSentence(NmeaSinkAndSource source, NmeaSentence sentence)
        {
            // Received a sentence (from the component's point of view)
            DateTimeOffset now = DateTimeOffset.UtcNow;
            if (now - _lastUpdate > UpdateInterval)
            {
                DispatchSentenceEvents(new IsoRequest(IsoAddressClaim.HexId));
                _lastUpdate = now;
                _state = ComponentState.AddressClaimSent;
            }

            if (sentence is IsoAddressClaim claim)
            {
                _devices[claim.MessageSource] = claim;
                if (_state != ComponentState.ProductInformationRequestSent)
                {
                    DispatchSentenceEvents(new IsoRequest(ProductInformation.HexId));
                    _state = ComponentState.ProductInformationRequestSent;
                }
            }

            if (sentence is ProductInformation productInformation)
            {
                _deviceProductInformation[productInformation.MessageSource] = productInformation;
                _state = ComponentState.WaitingForData;
            }
        }

        /// <summary>
        /// Receives the device list, if available
        /// </summary>
        /// <returns>A list of devices</returns>
        public List<Nmea2000DeviceInformation> GetDeviceList()
        {
            var ret = _devices.Select(x =>
                {
                    _deviceProductInformation.TryGetValue(x.Key, out ProductInformation? productInformation);
                    return new Nmea2000DeviceInformation(x.Value, productInformation);
                })
                .OrderBy(y => y.BusAddress)
                .ToList();
            return ret;
        }

        /// <inheritdoc/>
        public override void StopDecode()
        {
        }
    }
}
