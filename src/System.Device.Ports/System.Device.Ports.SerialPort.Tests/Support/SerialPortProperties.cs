// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.IO.Ports;
using System.Reflection;
using System.Text;
using System.Device.Ports.SerialPort;
using Xunit;
using SerialPort = System.Device.Ports.SerialPort.Tests.SerialPort;

namespace Legacy.Support
{
    public class SerialPortProperties
    {
        // All of the following properties are the defualts of SerialPort when the
        // just the default constructor has been called. The names of the data members here must
        // begin with default followed by the EXACT(case sensitive) name of
        // the property in SerialPort class. If a default for a property is not set
        // here then the property can never be set in this class and will never be
        // used for verification.
        private const int DefaultBaudRate = 9600;
        public static readonly string DefaultPortName = "COM1";
        private const int DefaultDataBits = 8;
        public static readonly StopBits DefaultStopBits = StopBits.One;
        public static readonly Parity DefaultParity = Parity.None;
        public static readonly Handshake DefaultHandshake = Handshake.None;
        public static readonly Encoding DefaultEncoding = new ASCIIEncoding();
        public static readonly bool DefaultDtrEnable = false;
        public static readonly bool DefaultRtsEnable = false;
        private const int DefaultReadTimeout = -1;
        private const int DefaultWriteTimeout = -1;
        public static readonly Type DefaultBytesToRead = typeof(InvalidOperationException);
        public static readonly Type DefaultBytesToWrite = typeof(InvalidOperationException);
        public static readonly bool DefaultIsOpen = false;
        public static readonly Type DefaultBaseStream = typeof(InvalidOperationException);
        private const int DefaultReceivedBytesThreshold = 1;
        public static readonly bool DefaultDiscardNull = false;
        public static readonly byte DefaultParityReplace = (byte)'?';
        public static readonly Type DefaultCDHolding = typeof(InvalidOperationException);
        public static readonly Type DefaultCtsHolding = typeof(InvalidOperationException);
        public static readonly Type DefaultDsrHolding = typeof(InvalidOperationException);
        public static readonly string DefaultNewLine = "\n";
        public static readonly Type DefaultBreakState = typeof(InvalidOperationException);
        private const int DefaultReadBufferSize = 4 * 1024;
        private const int DefaultWriteBufferSize = 2 * 1024;

        // All of the following properties are the defualts of SerialPort when the
        // serial port connection is open. The names of the data members here must
        // begin with openDefault followed by the EXACT(case sensitive) name of
        // the property in SerialPort class.
        private const int OpenDefaultBaudRate = 9600;
        public static readonly string OpenDefaultPortName = "COM1";
        private const int OpenDefaultDataBits = 8;
        public static readonly StopBits OpenDefaultStopBits = StopBits.One;
        public static readonly Parity OpenDefaultParity = Parity.None;
        public static readonly Handshake OpenDefaultHandshake = Handshake.None;
        public static readonly Encoding OpenDefaultEncoding = new ASCIIEncoding();
        public static readonly bool OpenDefaultDtrEnable = false;
        public static readonly bool OpenDefaultRtsEnable = false;
        private const int OpenDefaultReadTimeout = -1;
        private const int OpenDefaultWriteTimeout = -1;
        private const int OpenDefaultBytesToRead = 0;
        private const int OpenDefaultBytesToWrite = 0;
        public static readonly bool OpenDefaultIsOpen = true;
        private const int OpenDefaultReceivedBytesThreshold = 1;
        public static readonly bool OpenDefaultDiscardNull = false;
        public static readonly byte OpenDefaultParityReplace = (byte)'?';

        // Removing the following properties from default checks.  Sometimes these can be true (as read from the GetCommModemStatus win32 API)
        // which causes test failures.  Since these are read-only properties and are simply obtained from a bitfield, there is a very low probability
        // of regression involving these properties.
        //
        // public static readonly bool openDefaultCDHolding = false;
        // public static readonly bool openDefaultCtsHolding = false;
        // public static readonly bool openDefaultDsrHolding = false;
        public static readonly string OpenDefaultNewLine = "\n";
        public static readonly bool OpenDefaultBreakState = false;
        // private const int openReadBufferSize = 4 * 1024;
        // private const int openWriteBufferSize = 2 * 1024;
        private Hashtable _properties;
        private Hashtable _openDefaultProperties;
        private Hashtable _defaultProperties;

        public SerialPortProperties()
        {
            _properties = new Hashtable();
            _openDefaultProperties = new Hashtable();
            _defaultProperties = new Hashtable();
            LoadDefaults();
        }

        public object? GetDefualtOpenProperty(string name)
        {
            return _openDefaultProperties[name];
        }

        public object? GetDefualtProperty(string name)
        {
            return _defaultProperties[name];
        }

        public object? GetProperty(string name)
        {
            return _properties[name];
        }

        public void SetAllPropertiesToOpenDefaults()
        {
            SetAllPropertiesToDefaults(_openDefaultProperties);
        }

        public void SetAllPropertiesToDefaults()
        {
            SetAllPropertiesToDefaults(_defaultProperties);
        }

        public object? SetopenDefaultProperty(string name)
        {
            return SetDefaultProperty(name, _openDefaultProperties);
        }

        public object? SetdefaultProperty(string name)
        {
            return SetDefaultProperty(name, _defaultProperties);
        }

        public object? SetProperty(string name, object? value)
        {
            object? retVal = null;

            if (null != _openDefaultProperties[name])
            {
                retVal = _properties[name];
                _properties[name] = value;
            }

            return retVal;
        }

        public override string ToString()
        {
            StringBuilder strBuf = new StringBuilder();

            foreach (object key in _properties.Keys)
            {
                strBuf.Append(key + "=" + _properties[key] + "\n");
            }

            return strBuf.ToString();
        }

        internal Hashtable? VerifyProperties(SerialPort? port)
        {
            Type serialPortType = typeof(SerialPort);
            Hashtable badProps = new Hashtable();

            // If we could not get the type of SerialPort then verification can not
            // occur and we should throw an exception
            if (null == serialPortType)
            {
                throw new Exception("Could not create a Type object of SerialPort");
            }

            // For each key in the properties Hashtable verify that the property in the
            // SerialPort Object port with the same name as the key has the same value as
            // the value corresponding to the key in the HashTable. If the port is in a
            // state where accessing a property is suppose to throw an exception this can be
            // verified by setting the value in the hashtable to the System.Type of the
            // expected exception
            foreach (object key in _properties.Keys)
            {
                object? value = _properties[key];

                try
                {
                    PropertyInfo serialPortProperty = serialPortType.GetProperty((string)key)!;
                    object serialPortValue;

                    if ((string)key == "RtsEnable" &&
                        ((port?.Handshake == Handshake.RequestToSend &&
                            (Handshake?)_properties["Handshake"] == Handshake.RequestToSend) ||
                        (port?.Handshake == Handshake.RequestToSendXOnXOff &&
                            (Handshake?)_properties["Handshake"] == Handshake.RequestToSendXOnXOff)))
                    {
                        continue;
                    }

                    serialPortValue = serialPortProperty.GetValue(port, null)!;

                    if (value == null)
                    {
                        if (value != serialPortValue)
                        {
                            badProps[key] = serialPortValue;
                        }
                    }
                    else if (!value.Equals(serialPortValue))
                    {
                        badProps[key] = serialPortValue;
                    }
                }
                catch (Exception e)
                {
                    // An exception was thrown while retrieving the value of the property in
                    // the SerialPort object. However this may be the exepected operation of
                    // SerialPort so the type of the exception must be verified. Reflection
                    // throws it's own exception ontop of SerialPorts so we must use the
                    // InnerException of the own that Reflection throws
                    if (null != e.InnerException)
                    {
                        if (null == value || !value.Equals(e.InnerException.GetType()))
                        {
                            badProps[key] = e.GetType();
                        }
                    }
                    else
                    {
                        badProps[key] = e.GetType();
                    }
                }
            }

            if (0 == badProps.Count)
            {
                return null;
            }
            else
            {
                return badProps;
            }
        }

        internal void VerifyPropertiesAndPrint(SerialPort? port)
        {
            Hashtable? badProps;

            if (null == (badProps = VerifyProperties(port)))
            {
                // SerialPort values correctly set
                return;
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                foreach (object key in badProps.Keys)
                {
                    sb.AppendLine($"{key}={badProps[key]} expected {GetProperty((string)key)}");
                }

                Assert.True(false, sb.ToString());
            }
        }

        private void LoadDefaults()
        {
            LoadFields("openDefault", _openDefaultProperties);
            LoadFields("default", _defaultProperties);
        }

        private void LoadFields(string startsWith, Hashtable fields)
        {
            Type serialPropertiesType = typeof(SerialPortProperties);

            // For each field in this class that starts with the string startsWith create
            // a key value pair in the hashtable fields with key being the rest of the
            // field name after startsWith and the value is whatever the value is of the field
            // in this class
            foreach (FieldInfo defaultField in serialPropertiesType.GetFields())
            {
                string defaultFieldName = defaultField.Name;

                if (0 == defaultFieldName.IndexOf(startsWith))
                {
                    string fieldName = defaultFieldName.Replace(startsWith, string.Empty);
                    object? defaultValue = defaultField.GetValue(this);

                    fields[fieldName] = defaultValue;
                }
            }
        }

        private void SetAllPropertiesToDefaults(Hashtable defaultProperties)
        {
            _properties.Clear();

            // For each key in the defaultProperties Hashtable set poperties[key]
            // with the corresponding value in defaultProperties
            foreach (object key in defaultProperties.Keys)
            {
                _properties[key] = defaultProperties[key];
            }
        }

        private object? SetDefaultProperty(string name, Hashtable defaultProperties)
        {
            object? retVal = null;

            // Only Set the default value if it exists in the defaultProperties Hashtable
            // This will prevent the ability to create arbitrary keys(Property names)
            if (null != defaultProperties[name])
            {
                retVal = _properties[name];
                _properties[name] = defaultProperties[name];
            }

            return retVal;
        }
    }
}
