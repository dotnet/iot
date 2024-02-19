// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Iot.Device.Common;
using UnitsNet;
using UnitsNet.Units;

namespace Iot.Device.Nmea0183.Ais
{
    /// <summary>
    /// Extension methods on AIS targets
    /// </summary>
    public static class AisTargetExtensions
    {
        /// <summary>
        /// Calculates the distance to another target
        /// </summary>
        /// <param name="self">First target</param>
        /// <param name="other">Other target</param>
        /// <returns>The distance between the two targets, based on their last known position</returns>
        public static Length DistanceTo(this AisTarget self, AisTarget other)
        {
            GreatCircle.DistAndDir(self.Position, other.Position, out Length distance, out _);
            return distance.ToUnit(LengthUnit.NauticalMile);
        }

        /// <summary>
        /// Returns the age of this target, relative to the indicated time.
        /// </summary>
        /// <param name="self">The target under investigation</param>
        /// <param name="toTime">The time to compare to (often <see cref="DateTimeOffset.UtcNow"/>)</param>
        /// <returns></returns>
        public static TimeSpan Age(this AisTarget self, DateTimeOffset toTime)
        {
            return toTime - self.LastSeen;
        }

        /// <summary>
        /// Calculates the relative position data set to another vessel.
        /// </summary>
        /// <param name="self">Our own vessel</param>
        /// <param name="other">The other ship or target (can also be a stationary target, such as an <see cref="AidToNavigation"/> instance)</param>
        /// <param name="now">The time at which the comparison occurs. Typically now, but it is also possible to estimate the dangers at another time. When playing
        /// back old data, this must correspond to the playback time</param>
        /// <param name="parameters">The parameters used for the calculation</param>
        /// <returns>An instance of <see cref="ShipRelativePosition"/> with all possible fields filled out.</returns>
        public static ShipRelativePosition? RelativePositionTo(this Ship self, AisTarget other, DateTimeOffset now, TrackEstimationParameters parameters)
        {
            return RelativePositionsTo(self, new List<AisTarget>()
            {
                other
            }, now, parameters).FirstOrDefault();
        }

        /// <summary>
        /// Calculates the relative positions and the collision vectors from one ship to a group of targets.
        /// The algorithm used is inspired by https://core.ac.uk/download/74237799.pdf
        /// </summary>
        /// <param name="self">The own ship</param>
        /// <param name="others">The list of visible AIS targets</param>
        /// <param name="now">The current time (or the time the data is valid for)</param>
        /// <param name="parameters">Parameters controlling the accuracy and speed of the calculation</param>
        /// <returns>A list of relative positions between our ship and the targets. Targets without a valid position are skipped.</returns>
        /// <exception cref="ArgumentException">Our ship has no valid position</exception>
        /// <exception cref="InvalidDataException">An internal error occurred</exception>
        public static List<ShipRelativePosition> RelativePositionsTo(this Ship self, IEnumerable<AisTarget> others, DateTimeOffset now, TrackEstimationParameters parameters)
        {
            List<ShipRelativePosition> retList = new List<ShipRelativePosition>();

            if (self.Position.ContainsValidPosition() == false)
            {
                throw new ArgumentException("The own ship has no valid position", nameof(self));
            }

            var self1 = EstimatePosition(self, now, parameters.NormalStepSize);

            List<MovingTarget> thisTrack = GetEstimatedTrack(self1, now - parameters.StartTimeOffset, now + parameters.EndTimeOffset, parameters.NormalStepSize);

            foreach (var other in others)
            {
                if (other.Position.ContainsValidPosition() == false)
                {
                    continue;
                }

                MovingTarget? otherAsMovingTarget = other as MovingTarget;
                Length distance;
                Angle direction;

                AisSafetyState state = AisSafetyState.Safe;

                if (other.LastSeen + parameters.TargetLostTimeout < now)
                {
                    // For a lost target, don't do a full computation
                    state = AisSafetyState.Lost;
                    otherAsMovingTarget = null;
                }

                if (otherAsMovingTarget == null)
                {
                    GreatCircle.DistAndDir(self.Position, other.Position, out distance, out direction);

                    Angle? relativeDirection = null;

                    if (self1.TrueHeading.HasValue)
                    {
                        relativeDirection = (direction - self1.TrueHeading.Value).Normalize(false);
                    }

                    // The other is not a ship - Assume static position (but make sure a lost target doesn't become a
                    // dangerous target - we warn about lost targets separately)
                    if (distance < parameters.WarningDistance && state != AisSafetyState.Lost)
                    {
                        state = AisSafetyState.Dangerous;
                    }

                    retList.Add(new ShipRelativePosition(self, other, distance, direction, state, now)
                    {
                        RelativeDirection = relativeDirection,
                    });
                }
                else
                {
                    var otherPos = other.Position;
                    GreatCircle.DistAndDir(self1.Position, otherPos, out distance, out direction);
                    List<MovingTarget> otherTrack = GetEstimatedTrack(otherAsMovingTarget, now - parameters.StartTimeOffset, now + parameters.EndTimeOffset, parameters.NormalStepSize);

                    if (thisTrack.Count != otherTrack.Count || thisTrack.Count < 1)
                    {
                        // The two lists must have equal length and contain at least one element
                        throw new InvalidDataException("Internal error: Data structures inconsistent");
                    }

                    Angle? relativeDirection = null;

                    if (self1.TrueHeading.HasValue)
                    {
                        relativeDirection = (direction - self1.TrueHeading.Value).Normalize(false);
                    }

                    // Some really large distance
                    Length minimumDistance = Length.FromAstronomicalUnits(1);
                    DateTimeOffset timeOfMinimumDistance = default;
                    int usedIndex = 0;
                    for (int i = 0; i < thisTrack.Count; i++)
                    {
                        GreatCircle.DistAndDir(thisTrack[i].Position, otherTrack[i].Position, out Length distance1, out _, out _);
                        if (distance1 < minimumDistance)
                        {
                            minimumDistance = distance1;
                            timeOfMinimumDistance = thisTrack[i].LastSeen;
                            usedIndex = i;
                        }
                    }

                    // if the closest point is the first or the last element, we assume it's more than that, and leave the fields empty
                    if (usedIndex == 0 || usedIndex == thisTrack.Count - 1)
                    {
                        retList.Add(new ShipRelativePosition(self, other, distance, direction, AisSafetyState.FarAway, now)
                        {
                            RelativeDirection = relativeDirection,
                            ClosestPointOfApproach = null,
                            TimeOfClosestPointOfApproach = null,
                        });
                    }
                    else
                    {
                        var pos = new ShipRelativePosition(self, other, distance, direction, state, now)
                        {
                            RelativeDirection = relativeDirection,
                            // Todo: Should subtract the size of both ships here (ideally considering the direction of the ships hulls)
                            ClosestPointOfApproach = minimumDistance,
                            TimeOfClosestPointOfApproach = timeOfMinimumDistance,
                        };

                        var timeToClosest = pos.TimeToClosestPointOfApproach(now);
                        if (pos.ClosestPointOfApproach < parameters.WarningDistance &&
                            timeToClosest > -TimeSpan.FromMinutes(1) && timeToClosest < parameters.WarningTime)
                        {
                            pos.SafetyState = AisSafetyState.Dangerous;
                        }

                        retList.Add(pos);
                    }
                }
            }

            return retList;
        }

        /// <summary>
        /// Estimates where a ship will be after some time.
        /// </summary>
        /// <param name="ship">The ship to extrapolate</param>
        /// <param name="extrapolationTime">How much time shall pass. Very large values are probably useless, because the ship might start a turn.</param>
        /// <param name="stepSize">The extrapolation step size. Smaller values will lead to better estimation, but are computationally expensive</param>
        /// <returns>A <see cref="Ship"/> instance with the estimated position and course</returns>
        /// <exception cref="ArgumentOutOfRangeException">Stepsize is not positive</exception>
        /// <remarks>The reference time is the position/time the last report was received from this ship. To be able to compare two ships, the
        /// times still need to be aligned. Use the overload <see cref="EstimatePosition(Iot.Device.Nmea0183.Ais.MovingTarget,System.DateTimeOffset,System.TimeSpan)"/> if you
        /// want to estimate the ship position at a certain position in time.</remarks>
        public static MovingTarget EstimatePosition(this MovingTarget ship, TimeSpan extrapolationTime, TimeSpan stepSize)
        {
            if (stepSize <= TimeSpan.FromMilliseconds(1))
            {
                throw new ArgumentOutOfRangeException(nameof(stepSize),
                    "Step size must be positive and greater than 1ms");
            }

            if (extrapolationTime.Duration() < stepSize)
            {
                stepSize = extrapolationTime.Duration(); // one step only
            }

            DateTimeOffset currentTime = ship.LastSeen;
            MovingTarget newShip = ship with { Position = new GeographicPosition(ship.Position), IsEstimate = true };
            Angle cogChange;
            if (ship.RateOfTurn.HasValue && Math.Abs(ship.RateOfTurn.Value.DegreesPerMinute) > 1)
            {
                var rot = ship.RateOfTurn.Value;
                cogChange = rot * stepSize;
            }
            else
            {
                // No turn indication -> Calculate directly
                Length distance = extrapolationTime * newShip.SpeedOverGround;
                newShip.Position = GreatCircle.CalcCoords(newShip.Position, newShip.CourseOverGround, distance);
                newShip.LastSeen = currentTime + extrapolationTime;
                return newShip;
            }

            // Differentiate between moving forward and backward in time. Note that stepSize is expected
            // to be positive in either case
            if (extrapolationTime > TimeSpan.Zero)
            {
                while (currentTime < ship.LastSeen + extrapolationTime)
                {
                    currentTime += stepSize;
                    newShip.CourseOverGround = (newShip.CourseOverGround + cogChange).Normalize(true);
                    Length distanceDuringStep = stepSize * newShip.SpeedOverGround;
                    newShip.Position =
                        GreatCircle.CalcCoords(newShip.Position, newShip.CourseOverGround, distanceDuringStep);
                    newShip.LastSeen = currentTime;
                }
            }
            else
            {
                while (currentTime > ship.LastSeen + extrapolationTime) // extrapolationTime is negative here
                {
                    currentTime -= stepSize;
                    Length distanceDuringStep = -(stepSize * newShip.SpeedOverGround);
                    newShip.Position =
                        GreatCircle.CalcCoords(newShip.Position, newShip.CourseOverGround, distanceDuringStep);

                    // To get the closes possible inverse of the above, we correct the cog afterwards here
                    newShip.CourseOverGround = (newShip.CourseOverGround - cogChange).Normalize(true);
                    newShip.LastSeen = currentTime;
                }
            }

            return newShip;
        }

        /// <summary>
        /// Estimates where a ship will at a certain time.
        /// </summary>
        /// <param name="ship">The ship to extrapolate</param>
        /// <param name="time">The time at which the position shall be estimated. The estimate is better the closer this time is to the last position
        /// of the ship.</param>
        /// <param name="stepSize">The extrapolation step size. Smaller values will lead to better estimation, but are computationally expensive</param>
        /// <returns>A <see cref="Ship"/> instance with the estimated position and course</returns>
        /// <exception cref="ArgumentOutOfRangeException">Stepsize is not positive</exception>
        public static MovingTarget EstimatePosition(this MovingTarget ship, DateTimeOffset time, TimeSpan stepSize)
        {
            TimeSpan delta = ship.Age(time);
            return EstimatePosition(ship, delta, stepSize);
        }

        /// <summary>
        /// Calculate a track estimation for a ship
        /// </summary>
        /// <param name="ship">The ship to move</param>
        /// <param name="startTime">The time at which the track should start (may be in the past)</param>
        /// <param name="endTime">The time at which the track should end</param>
        /// <param name="stepSize">The step size of the returned track</param>
        /// <returns>A list of targets (for each estimated position from <paramref name="startTime"/> to <paramref name="endTime"/>).</returns>
        /// <exception cref="ArgumentOutOfRangeException">Stepsize is to small or negative</exception>
        /// <exception cref="ArgumentException">Start time is after end time</exception>
        /// <remarks>The calculation of this track may be expensive, when the timespan between start and end is large or stepSize is small. Or when the
        /// timespan is far from the time the ship was last seen.</remarks>
        public static List<MovingTarget> GetEstimatedTrack(this MovingTarget ship, DateTimeOffset startTime, DateTimeOffset endTime, TimeSpan stepSize)
        {
            if (stepSize <= TimeSpan.FromMilliseconds(1))
            {
                throw new ArgumentOutOfRangeException(nameof(stepSize),
                    "Step size must be positive and greater than 1ms");
            }

            if (startTime >= endTime)
            {
                throw new ArgumentException("startTime must be before endTime");
            }

            List<MovingTarget> track = new List<MovingTarget>();

            DateTimeOffset currentTime = startTime;
            var ship1 = EstimatePosition(ship, currentTime, stepSize);
            track.Add(ship1);
            while (currentTime < endTime)
            {
                currentTime += stepSize;
                ship1 = ship1.EstimatePosition(stepSize, stepSize); // One step ahead
                track.Add(ship1);
            }

            return track;
        }
    }
}
