// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Tm16xx
{
    /// <summary>
    /// Represents Titanmec led devices. This is an abstract class.
    /// </summary>
    /// <remarks><para>Not all derived classes support all methods declared in this abstract class. Check the document before use.</para>
    /// <para>Some derived class may need to be disposed after use.</para></remarks>
    public abstract class Tm16xxBase
    {
        /// <summary>
        /// Gets or sets whether exception should be thrown when IO error occurred.
        /// </summary>
        public virtual bool ThrowWhenIoError { get; set; }

        /// <summary>
        /// Gets or sets the order of characters.
        /// </summary>
        public abstract byte[] CharacterOrder { get; set; }

        /// <summary>
        /// Gets or sets the switch of the screen.
        /// </summary>
        public abstract bool IsScreenOn { get; set; }

        /// <summary>
        /// Gets or sets the brightness.
        /// </summary>
        /// <remarks>The value range, the mapping of value and brightness are not consistent in different devices. Check the document before use.</remarks>
        public abstract byte ScreenBrightness { get; set; }

        /// <summary>
        /// Gets or sets the segment mode.
        /// </summary>
        /// <remarks>Not all derived classes support supports all kinds of segment mode. Check the document before use.</remarks>
        public abstract LedSegment LedSegment { get; set; }

        #region One Byte Display

        /// <summary>
        /// Displays a series of raw data. Each byte represents a character.
        /// </summary>
        /// <param name="rawData">The raw data to display. Size of the array has to be equal or less than the count of the supported characters on the led board.</param>
        public abstract void Display(ReadOnlySpan<byte> rawData);

        /// <summary>
        /// Displays a series of pre-build characters.
        /// </summary>
        /// <param name="characters">The characters to display. Size of the array has to be equal or less than the count of the supported characters on the led board.</param>
        public virtual void Display(ReadOnlySpan<Character> characters)
        {
            Display(MemoryMarshal.AsBytes(characters));
        }

        /// <summary>
        /// Displays a series of raw data. Each byte represents a character.
        /// </summary>
        /// <param name="rawData">The raw data array to display. Size of the array has to be equal or less than the count of the supported characters on the led board.</param>
        public virtual void Display(params byte[] rawData)
        {
            Display(rawData.AsSpan());
        }

        /// <summary>
        /// Displays a series of pre-build characters.
        /// </summary>
        /// <param name="characters">The character array to display. Size of the array has to be equal or less than the count of the supported characters on the led board.</param>
        public virtual void Display(params Character[] characters)
        {
            Display(characters.AsSpan());
        }

        /// <summary>
        /// Displays a raw data byte on the specified position.
        /// </summary>
        /// <param name="characterPosition">The position to display.</param>
        /// <param name="rawData">The raw data to display.</param>
        public abstract void Display(byte characterPosition, byte rawData);

        /// <summary>
        /// Displays a pre-build character on the specified position.
        /// </summary>
        /// <param name="characterPosition">The position to display.</param>
        /// <param name="character">The character to display.</param>
        public virtual void Display(byte characterPosition, Character character)
        {
            Display(characterPosition, (byte)character);
        }

        #endregion

        /// <summary>
        /// Clears the display.
        /// </summary>
        public abstract void ClearDisplay();

    }
}
