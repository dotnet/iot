// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Reflection;
using Iot.Device.BuildHat.Models;
using Xunit;

namespace Iot.Device.BuildHat.Tests
{
    public class BrickTests
    {
        [Theory]
        [InlineData(0, PositionWay.Clockwise, 0, 0, 1)]
        [InlineData(90, PositionWay.Clockwise, 0, 0, 0.25)]
        [InlineData(-90, PositionWay.Clockwise, 0, 0, 0.75)]
        [InlineData(0, PositionWay.AntiClockwise, 0, 0, -1)]
        [InlineData(90, PositionWay.AntiClockwise, 0, 0, -0.75)]
        [InlineData(-90, PositionWay.AntiClockwise, 0, 0, -0.25)]
        [InlineData(0, PositionWay.Shortest, 0, 0, 0)]
        [InlineData(90, PositionWay.Shortest, 0, 0, 0.25)]
        [InlineData(-90, PositionWay.Shortest, 0, 0, -0.25)]
        public void ToAbsolutePositionTest(int targetPosition, PositionWay way, int actualPosition, int actualAbsolutePosition, double expectedNewPosition)
        {
            Brick brick = GetBrick();
            var brickType = typeof(Brick);
            var toAbsolutePosition = brickType.GetMethod("ToAbsolutePosition", BindingFlags.NonPublic | BindingFlags.Instance);
            var newPosition = toAbsolutePosition?.Invoke(brick, new object[] { targetPosition, way, actualPosition, actualAbsolutePosition });
            Assert.Equal(expectedNewPosition, newPosition);
        }

        private Brick GetBrick()
        {
            var brickType = typeof(Brick);
            var brick = brickType.Assembly.CreateInstance(brickType.FullName!, false, BindingFlags.Instance | BindingFlags.NonPublic, null, null, null, null);
            return (Brick)brick!;
        }
    }
}
