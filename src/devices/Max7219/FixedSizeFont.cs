// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;

namespace Iot.Device.Max7219
{
    /// <summary>
    /// Implementation of a <see cref="IFont"/> that uses a common array for all characters. 
    /// The number of bytes per character is constant and zero values between the characters are trimmed.
    /// </summary>
    public class FixedSizeFont : IFont
    {

        private readonly byte[] _data;
        private readonly int _bytesPerCharacter;
        private readonly byte[] _space;
 
        public FixedSizeFont(int bytesPerCharacter, byte[] data, int spaceWidth = 3)
        {
            _data = data;
            _bytesPerCharacter = bytesPerCharacter;
            _space = new byte[spaceWidth];
        }

        public IReadOnlyList<byte> this[char chr]
        {
            get
            {
                int start = chr * _bytesPerCharacter;
                int end = start + _bytesPerCharacter;
                if (end > _data.Length)
                    return _space; //character is not defined

                if (chr == ' ')
                    return _space;

                // trim the font
                while (start < end && _data[start] == 0)
                    start++;
                while (end > start && _data[end - 1] == 0)
                    end--;

                return new ArraySegment<byte>(_data, start, end - start);
            }
        }
    }
}