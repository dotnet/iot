// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Buffers.Binary;

namespace Iot.Device.HuskyLens
{
    /// <summary>
    /// Represents and arrow object
    /// </summary>
    public class Arrow : HuskyObject
    {
        /// <summary>
        /// Origin of the arrow
        /// </summary>
        public Point Origin { get; }

        /// <summary>
        /// Trget of the arrow
        /// </summary>
        public Point Target { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="origin">Origin of the arrow</param>
        /// <param name="target">Target of the arrow</param>
        /// <param name="id">Learned Id</param>
        public Arrow(Point origin, Point target, int id = 0)
        : base(id)
        {
            Origin = origin;
            Target = target;
        }

        /// <inheritdoc/>
        public override string ToString() => $"Arrow: o={Origin}, t={Target}, id={Id}";

        /// <summary>
        /// Creates a new object from the wire protocol data
        /// </summary>
        /// <param name="data">the bytes from the wire</param>
        /// <returns>initialized object</returns>
        public static Arrow FromData(ReadOnlySpan<byte> data)
        {
            var ox = BinaryPrimitives.ReadInt16LittleEndian(data.Slice(0, 2));
            var oy = BinaryPrimitives.ReadInt16LittleEndian(data.Slice(2, 2));
            var tx = BinaryPrimitives.ReadInt16LittleEndian(data.Slice(4, 2));
            var ty = BinaryPrimitives.ReadInt16LittleEndian(data.Slice(6, 2));
            var id = BinaryPrimitives.ReadInt16LittleEndian(data.Slice(8, 2));
            return new Arrow(new Point(ox, oy), new Point(tx, ty), id);
        }
    }
}
