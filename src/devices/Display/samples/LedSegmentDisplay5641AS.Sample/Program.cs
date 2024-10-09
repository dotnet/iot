// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

/* This sample displays the output of a stopwatch on the 5641AS segment display.
 *   It includes a button for starting and stopping the stopwatch, but this is optional
 *   and is only included to show how to respond to events within the refresh loop.
 * Refer to the Fritzing diagram for pinout.
*/
using LedSEgmentDisplay5641AS.Sample;

var segmentStopwatch = new SegmentStopwatch();
segmentStopwatch.Run();
