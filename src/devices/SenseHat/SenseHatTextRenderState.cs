// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Drawing;

namespace Iot.Device.SenseHat
{
    /// <summary>
    /// Render state containing parameters that should not change mid-frame when rendering
    /// text. Any change in state requires construction of a new state object, either
    /// by using the constructor or by calling one of the "Apply..." methods.
    /// </summary>
    public class SenseHatTextRenderState
    {
        /// <summary>
        /// Construct the render state.
        /// </summary>
        /// <param name="textRenderMatrix">Matrix containing the rendered text</param>
        /// <param name="textColor">Color of the text</param>
        /// <param name="textBackgroundColor">Color of the text background</param>
        /// <param name="textRotation">Text rotation</param>
        public SenseHatTextRenderState(
            SenseHatTextRenderMatrix textRenderMatrix,
            Color textColor,
            Color textBackgroundColor,
            SenseHatTextRotation textRotation)
        {
            TextRenderMatrix = textRenderMatrix;
            TextColor = textColor;
            TextBackgroundColor = textBackgroundColor;
            TextRotation = textRotation;
        }

        /// <summary>
        /// Clone the render state and apply the new text color.
        /// </summary>
        /// <param name="textColor">The new text color to apply</param>
        public SenseHatTextRenderState ApplyTextColor(Color textColor)
        {
            return new SenseHatTextRenderState(
                TextRenderMatrix,
                textColor,
                TextBackgroundColor,
                TextRotation);
        }

        /// <summary>
        /// Clone the render state and apply the new text background color.
        /// </summary>
        /// <param name="textBackgroundColor">The new text background color to apply</param>
        public SenseHatTextRenderState ApplyTextBackgroundColor(Color textBackgroundColor)
        {
            return new SenseHatTextRenderState(
                TextRenderMatrix,
                TextColor,
                textBackgroundColor,
                TextRotation);
        }

        /// <summary>
        /// Clone the render state and apply the new text rotation.
        /// </summary>
        /// <param name="textRotation">The new text rotation to apply</param>
        public SenseHatTextRenderState ApplyTextRotation(SenseHatTextRotation textRotation)
        {
            return new SenseHatTextRenderState(
                TextRenderMatrix,
                TextColor,
                TextBackgroundColor,
                textRotation);
        }

        /// <summary>
        /// Matrix containing the rendered text.
        /// </summary>
        public readonly SenseHatTextRenderMatrix TextRenderMatrix;

        /// <summary>
        /// Color of the text.
        /// </summary>
        public readonly Color TextColor;

        /// <summary>
        /// Color of the text background.
        /// </summary>
        public readonly Color TextBackgroundColor;

        /// <summary>
        /// Text rotation.
        /// </summary>
        public readonly SenseHatTextRotation TextRotation;
    }
}