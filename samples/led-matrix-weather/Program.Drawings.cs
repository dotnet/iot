// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Iot.Device.LEDMatrix;
using Iot.Device.Graphics;
using System.Drawing;
using System.Numerics;

namespace LedMatrixWeather
{
    using static MathUtils;
    
    partial class Program
    {
        static float Line(Vector2 uv, float len)
        {
            if (uv.Y < 0)
            {
                return Math.Max(-uv.Y, Math.Abs(uv.X));
            }
            else if (uv.Y > len)
            {
                return Math.Max(uv.Y - len, Math.Abs(uv.X));
            }
            else
            {
                return Math.Abs(uv.X);
            }
        }

        static Vector3 Clock(Vector2 uv, DateTimeOffset time)
        {
            uv -= new Vector2(0.5f, 0.5f);

            float len = uv.Length();
            float outerRadius = 0.47f;
            float outerRadiusDist = Math.Abs(len - outerRadius);
            float outerCircle = 1.0f - smoothstep(0f, 0.025f, outerRadiusDist);

            float innerRadius = 0.04f;
            float innerRadiusDist = len - innerRadius;

            float secondsAngle = 2f * pi * ((float)time.Second + time.Millisecond / 1000f) / 60f;
            float minutesAngle = 2f * pi * (float)time.Minute / 60f + secondsAngle / 60f;
            float hoursAngle = 2f * pi * (float)(time.Hour % 12) / 12f + minutesAngle / 60f;

            float secondsLine = 1.0f - smoothstep(0f, 0.025f, Line(Rot(uv, secondsAngle), 0.4f));
            float minutesLine = 1.0f - smoothstep(0f, 0.025f, Line(Rot(uv, minutesAngle), 0.35f));
            float hoursLine = 1.0f - smoothstep(0f, 2f * 0.025f, Line(Rot(uv, hoursAngle), 0.2f));

            int ticks = 12;
            float tickSize = 1.0f / ticks;
            float halfTickSize = tickSize / 2;
            float tickDist = Math.Abs(mod(0.5f + ticks * (float)Math.Atan2(uv.Y, uv.X) / 2 / pi, 1.0f) - 0.5f) * 2 * pi / ticks * len;
            float tickCircleDist = Math.Abs(len - 0.4f);
            float dots = 1.0f - smoothstep(0f, 0.02f, Math.Max(tickDist, tickCircleDist));

            return mix(
                new Vector3(1, 0, 0),
                new Vector3(Math.Max(dots, Math.Max(outerCircle, secondsLine)), minutesLine, hoursLine),
                smoothstep(0f, 0.01f, innerRadiusDist));
        }

        static Vector3 OpenWeatherIcon(Vector2 uv, string icon, float time)
        {
            uv -= new Vector2(0.5f, 0.5f);

            switch (icon)
            {
                case "01d":
                case "01d.org":
                    return IconSunny(uv, time);
                case "01n":
                    return IconMoon(uv, time);
                case "02d":
                    return IconPartiallySunny(uv, time);
                case "02n": // partially cloudy at night
                    return IconMoon(uv, time);
                case "03d": // clouds day
                case "03n": // clouds night
                case "04d": // overcast clouds
                case "04n":
                case "11d": // with thunder
                case "11n":
                case "13d": // with snow
                case "13n":
                case "50d": // no clue what the picture represents but kinda looks like a cloud
                case "50n":
                    return IconClouds(uv, time);
                case "09d":
                case "09n":
                case "10d": // with a bit of the sun
                case "10n":
                    return IconRain(uv, time);
                default:
                {
                    // Don't know the icon
                    // We're in Seattle so let's assume rain
                    return IconRain(uv, time);
                }
            }
        }

        static Vector3 IconSunny(Vector2 uv, float time)
        {
            const int NumberOfRays = 10;
            const float OuterRadius = 0.45f;
            const float InnerRadius = 0.25f;

            const float RayLength = OuterRadius - InnerRadius;

            // spiral and rotate everything a bit
            uv = Rot(uv, uv.Length() * 2 * 2 * pi / 10f * (float)Math.Sin(2 * pi * time / 9.0f) - time / 2.0f);
            float angle = (float)Math.Atan2(uv.Y, uv.X) / 2 / pi * NumberOfRays;

            // for soft rays: (float)Math.Sin(2 * pi * angle);
            float rayShape = 2 * Math.Abs(mod(angle, 1.0f) - 0.5f);
            float rayLen = RayLength * rayShape;
            float radius = InnerRadius + rayLen;
            float radiusDist = uv.Length() - radius;
            return mix(
                new Vector3(1, 1, 0),
                new Vector3(0, 0, 0),
                smoothstep(0f, 0.02f, radiusDist));
        }

        static float Clouds(Vector2 uv, float time)
        {
            float r0 = 0.15f;
            Vector2 p0 = new Vector2(-0.2f, 0.1f + 0.05f * (float)Math.Sin(2 * pi * time / 7f + 0.1f));
            float c0 = (uv - p0).Length() - r0;

            float r1 = 0.15f + 0.05f * (float)Math.Sin(2 * pi * time / 13f);
            Vector2 p1 = new Vector2(0f, 0.08f * (float)Math.Sin(2 * pi * time / 5f + 0.7f));
            float c1 = (uv - p1).Length() - r1;

            float r2 = 0.15f;
            Vector2 p2 = new Vector2(0.2f, 0.05f - 0.05f * (float)Math.Sin(2 * pi * time / 4f + 0.9f));
            float c2 = (uv - p2).Length() - r2;

            float clouds = Math.Min(Math.Min(c0, c1), c2);
            return smoothstep(0f, 0.2f, clouds + 0.05f);
        }

        static readonly Vector3 s_cloudsColor = new Vector3(0.3f, 0.3f, 0.3f);
        static Vector3 IconClouds(Vector2 uv, float time)
        {
            float clouds = Clouds(uv, time);

            return mix(
                s_cloudsColor,
                new Vector3(0, 0, 0),
                clouds);
        }

        static Vector3 Rain(Vector2 uv, float time)
        {
            if (uv.Y < -0.1 || uv.X <= -0.4 || uv.X >= 0.4)
            {
                return new Vector3(0, 0, 0);
            }

            uv.Y -= time / 3f;

            float dropSpeed = 2 + 0.5f * (float)Math.Sin(2 * pi * uv.X * 77f);
            float dropNoise = 11 * uv.X + (float)Math.Sin(2 * pi * uv.X * 100f);
            float rain = smoothstep(0f, 0.05f, 
                (float)Math.Sin(2 * pi * (dropNoise + dropSpeed * uv.Y)) - 0.9f);

            return mix(
                new Vector3(0, 0, 0),
                new Vector3(0, 0, 0.3f),
                rain);
        }

        static Vector3 IconRain(Vector2 uv, float time)
        {
            float clouds = Clouds(uv - new Vector2(0f, -0.3f), time);
            return mix(
                s_cloudsColor,
                Rain(uv, time),
                clouds);
        }

        static Vector3 IconMoon(Vector2 uv, float time)
        {
            uv -= new Vector2(0.3f, 0.0f);
            uv = Rot(uv, 2 * pi / 128 * (float)Math.Sin(2 * pi * time / 3f));
            uv += new Vector2(0.35f, 0.0f);

            float r0 = 0.3f;
            Vector2 p0 = new Vector2(0.0f, 0.0f);
            float c0 = (uv - p0).Length() - r0;

            float r1 = 0.35f;
            Vector2 p1 = new Vector2(0.20f, 0.0f);
            float c1 = (uv - p1).Length() - r1;

            float moon = Math.Max(c0, -c1);
            return mix(
                new Vector3(0.5f, 0.5f, 0.5f),
                new Vector3(0, 0, 0),
                smoothstep(0f, 0.07f, moon));
            //return IconRain(uv, time);//new Vector3(0, 0, 0);
        }

        static Vector3 IconPartiallySunny(Vector2 uv, float time)
        {
            float clouds = Clouds(uv * 0.85f - new Vector2(0f, 0.15f), time);
            return mix(
                s_cloudsColor,
                IconSunny(uv, time),
                clouds) * 0.6f;
        }
    }
}
