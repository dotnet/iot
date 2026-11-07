// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

#pragma warning disable CS1591
namespace Iot.Device.Nmea0183.Sentences
{
    /// <summary>
    /// Provides information about a connected NMEA2000 device and helper methods for NMEA2000 device information lookup
    /// </summary>
    public class Nmea2000DeviceInformation
    {
        private readonly IsoAddressClaim _addressClaim;
        private readonly ProductInformation _productInformation;

        public Nmea2000DeviceInformation(IsoAddressClaim addressClaim, ProductInformation? productInformation)
        {
            _addressClaim = addressClaim;
            _productInformation = productInformation ?? new ProductInformation(0, 0, string.Empty,
                string.Empty, string.Empty, string.Empty, 0, 1);
        }

        public byte BusAddress => _addressClaim.MessageSource;

        public string Name => _productInformation.ModelId;

        public string SoftwareVersion => _productInformation.SoftwareVersionCode;

        public string SerialNumber => _productInformation.SerialCode;

        public int LoadEquivalentNumber => _productInformation.LoadEquivalency;

        public ushort NmeaVersion => _productInformation.Nmea2000Version;

        public UInt32 UniqueIdentifier => _addressClaim.UniqueNumber;

        public UInt32 DeviceInstance => _addressClaim.DeviceInstance;

        public DeviceClass DeviceClass => _addressClaim.DeviceClass;

        public ManufacturerCode Manufacturer => _addressClaim.ManufacturerCode;

        public IndustryCode Industry => _addressClaim.IndustryCode;

        public bool ArbitraryAddressCapable => _addressClaim.ArbitraryAddressCapable;

        public string DeviceDescription => _addressClaim.DeviceDescription;

        public string ClassDescription => _addressClaim.ClassDescription;

        /// <summary>
        /// Lookup table for device function and class combinations to specific device descriptions
        /// Key: (DeviceFunction, DeviceClass), Value: Description
        /// Based on NMEA2000 specification
        /// </summary>
        private static readonly Dictionary<(byte Function, byte Class), string> DeviceDescriptions = new()
        {
            // System Tools (Class 10)
            { (130, 10), "Diagnostic" },
            { (140, 10), "Bus/Network Analyzer" },

            // Safety Systems (Class 20)
            { (150, 20), "Alarm Enunciator" },
            { (155, 20), "Man Overboard" },
            { (160, 20), "EPIRB" },

            // Inter/Intranetwork Devices (Class 25)
            { (130, 25), "NMEA 0183 Gateway" },
            { (135, 25), "NMEA 2000 Repeater/Bridge" },
            { (140, 25), "PC Gateway" },
            { (145, 25), "Router" },
            { (150, 25), "Network Analyzer" },

            // Electrical Distribution (Class 30)
            { (140, 30), "Battery" },
            { (150, 30), "Charger" },
            { (160, 30), "Inverter" },
            { (170, 30), "Converter" },
            { (180, 30), "AC Bus" },
            { (190, 30), "DC Bus" },
            { (195, 30), "Switch Bank" },

            // Electrical Generation (Class 35)
            { (140, 35), "Alternator/Generator" },
            { (160, 35), "Solar Panel" },
            { (170, 35), "Wind Generator" },

            // Steering and Control (Class 40)
            { (150, 40), "Autopilot" },
            { (155, 40), "Heading Sensor" },
            { (160, 40), "Rudder" },
            { (170, 40), "Trim Tabs" },

            // Propulsion (Class 50)
            { (140, 50), "Main Engine" },
            { (145, 50), "Auxiliary Engine" },
            { (150, 50), "Engine Controller" },
            { (160, 50), "Transmission" },
            { (170, 50), "Throttle" },
            { (180, 50), "Fuel Flow" },
            { (190, 50), "Engine Gateway" },

            // Navigation (Class 60)
            { (130, 60), "LORAN C" },
            { (135, 60), "Speed Log" },
            { (140, 60), "Turn Rate Indicator" },
            { (145, 60), "Integrated Navigation" },
            { (150, 60), "GPS" },
            { (155, 60), "Chart Plotter" },
            { (160, 60), "DECCA" },
            { (165, 60), "Sounder, depth" },
            { (170, 60), "Integrated Instrumentation" },
            { (175, 60), "Autopilot, Route Controller" },
            { (180, 60), "Radar" },
            { (185, 60), "Echo Sounder" },
            { (190, 60), "AIS" },
            { (195, 60), "TAS (Track and Trace)" },
            { (200, 60), "Voyage Data Recorder" },

            // Communication (Class 70)
            { (160, 70), "Radio: MF/HF" },
            { (170, 70), "Radio: VHF" },
            { (180, 70), "Radio: SSB" },

            // Instrumentation (Class 75)
            { (130, 75), "Time/Date systems" },
            { (140, 75), "VDR" },
            { (150, 75), "Integrated Instrumentation" },
            { (160, 75), "General Purpose Displays" },
            { (170, 75), "General Sensor Box" },
            { (180, 75), "Weather Instruments" },
            { (190, 75), "Transducer/general" },
            { (200, 75), "NMEA 0183 Converter" },

            // External Environment (Class 80)
            { (130, 80), "Atmospheric" },
            { (140, 80), "Water" },

            // Internal Environment (Class 85)
            { (130, 85), "Heating" },
            { (140, 85), "Air Conditioning" },
            { (150, 85), "Refrigeration" },
            { (160, 85), "Ventilation" },

            // Deck, Cargo, Fishing Equipment (Class 90)
            { (130, 90), "Anchor, Windlass" },
            { (140, 90), "Hatch, Door" },
            { (150, 90), "Sail Control" },
        };

        /// <summary>
        /// Gets the description of a device class
        /// </summary>
        /// <param name="deviceClass">The device class code</param>
        /// <returns>Description of the device class</returns>
        public static string GetClassDescription(DeviceClass deviceClass)
        {
            if (Enum.IsDefined(typeof(DeviceClass), deviceClass))
            {
                return GetEnumDescription((DeviceClass)deviceClass);
            }

            return $"Unknown Class ({deviceClass})";
        }

        /// <summary>
        /// Gets the specific device description based on function and class combination
        /// </summary>
        /// <param name="function">The device function code</param>
        /// <param name="deviceClass">The device class code</param>
        /// <returns>Specific device description, or generic description if not found</returns>
        public static string GetDeviceDescription(byte function, DeviceClass deviceClass)
        {
            // Try to find specific device description
            if (DeviceDescriptions.TryGetValue((function, (byte)deviceClass), out string? description))
            {
                return description;
            }

            // Fallback to generic function + class description
            string functionDesc = $"Unknown function {function} for this class";
            string classDesc = GetClassDescription(deviceClass);

            return $"{functionDesc} - {classDesc}";
        }

        /// <summary>
        /// Gets all known device descriptions
        /// </summary>
        /// <returns>Dictionary of (Function, Class) to Description mappings</returns>
        public static IReadOnlyDictionary<(byte Function, byte Class), string> GetAllDeviceDescriptions()
        {
            return DeviceDescriptions;
        }

        /// <summary>
        /// Checks if a specific function/class combination is known
        /// </summary>
        /// <param name="function">The device function code</param>
        /// <param name="deviceClass">The device class code</param>
        /// <returns>True if the combination is known, false otherwise</returns>
        public static bool IsKnownDevice(byte function, byte deviceClass)
        {
            return DeviceDescriptions.ContainsKey((function, deviceClass));
        }

        /// <summary>
        /// Helper method to get description attribute from enum
        /// </summary>
        private static string GetEnumDescription<T>(T enumerationValue)
            where T : struct, Enum
        {
            string? enumVal = enumerationValue.ToString();
            if (string.IsNullOrEmpty(enumVal))
            {
                return string.Empty;
            }

            MemberInfo[]? memberInfo = typeof(T).GetMember(enumVal);
            if (memberInfo != null && memberInfo.Length > 0)
            {
                object[] attributes = memberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);

                if (attributes != null && attributes.Length > 0)
                {
                    return ((DescriptionAttribute)attributes[0]).Description;
                }
            }

            return enumVal;
        }

        public string GetDeviceDescription()
        {
            return GetDeviceDescription(_addressClaim.DeviceFunction, DeviceClass);
        }

        public override string ToString()
        {
            return $"Device {Name} type {GetDeviceDescription()} bus address 0x{BusAddress:X2}";
        }
    }
}
