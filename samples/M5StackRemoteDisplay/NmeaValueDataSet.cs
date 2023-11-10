﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Iot.Device.Nmea0183;
using UnitsNet;

namespace Iot.Device.Ili934x.Samples
{
    internal class NmeaValueDataSet : NmeaDataSet
    {
        private readonly Func<SentenceCache, IQuantity?> _valueFunc;
        private readonly string _format;
        private IQuantity? _lastValue;

        public NmeaValueDataSet(String name, Func<SentenceCache, IQuantity?> valueFunc, string format = "F2")
        : base(name)
        {
            _valueFunc = valueFunc;
            _format = format;
            _lastValue = null;
        }

        public override string Value
        {
            get
            {
                if (_lastValue == null)
                {
                    return "N/A";
                }

                return _lastValue.Value.ToString(_format, CultureInfo.CurrentCulture);
            }
        }

        public override string Unit
        {
            get
            {
                if (_lastValue == null)
                {
                    return string.Empty;
                }

                var unitName = _lastValue.Unit;
                return unitName.ToString();
            }
        }

        public override bool Update(SentenceCache cache, double tolerance)
        {
            var newValue = _valueFunc.Invoke(cache);
            if (_lastValue != null)
            {
                if (newValue == null)
                {
                    _lastValue = null;
                    return true;
                }

                bool ret = Math.Abs((double)newValue.Value - (double)_lastValue.Value) > tolerance;
                _lastValue = newValue;

                return ret;
            }

            _lastValue = newValue;
            return newValue != null;
        }
    }
}
