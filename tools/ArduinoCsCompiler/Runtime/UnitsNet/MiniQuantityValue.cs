// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using UnitsNet;

namespace ArduinoCsCompiler.Runtime.UnitsNet
{
    [ArduinoReplacement(typeof(QuantityValue), true)]
    internal struct MiniQuantityValue
    {
        private readonly double _value;

        private MiniQuantityValue(double val)
        {
            _value = val;
        }

        #region To QuantityValue

        // Prefer double for integer types, since most quantities use that type as of now and
        // that avoids unnecessary casts back and forth.
        // If we later change to use decimal more, we should revisit this.

        /// <summary>Implicit cast from <see cref="byte"/> to <see cref="QuantityValue"/>.</summary>
        public static implicit operator MiniQuantityValue(byte val) => new MiniQuantityValue((double)val);

        /// <summary>Implicit cast from <see cref="short"/> to <see cref="QuantityValue"/>.</summary>
        public static implicit operator MiniQuantityValue(short val) => new MiniQuantityValue((double)val);

        /// <summary>Implicit cast from <see cref="int"/> to <see cref="QuantityValue"/>.</summary>
        public static implicit operator MiniQuantityValue(int val) => new MiniQuantityValue((double)val);

        /// <summary>Implicit cast from <see cref="long"/> to <see cref="QuantityValue"/>.</summary>
        public static implicit operator MiniQuantityValue(long val) => new MiniQuantityValue((double)val);

        /// <summary>Implicit cast from <see cref="float"/> to <see cref="QuantityValue"/>.</summary>
        public static implicit operator MiniQuantityValue(float val) => new MiniQuantityValue(val); // double

        /// <summary>Implicit cast from <see cref="double"/> to <see cref="QuantityValue"/>.</summary>
        public static implicit operator MiniQuantityValue(double val) => new MiniQuantityValue(val); // double
        #endregion

        public static explicit operator double(MiniQuantityValue number)
        {
            return number._value;
        }
    }
}
