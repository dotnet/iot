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
    internal static class MathUtils
    {
        public const float pi = (float)Math.PI;

        // Some of these names are lowercase to match names used in shading languages.
        // Most of these are used in code similar to fragment shaders
        // and it makes it easier to convert shading language to C# graphics
        public static Vector3 clamp(Vector3 c, float a, float b)
        {
            return new Vector3(Math.Clamp(c.X, a, b), Math.Clamp(c.Y, a, b), Math.Clamp(c.Z, a, b));
        }

        public static float mod(float a, float b)
        {
            float ret = a % b;
            if (ret < 0)
            {
                ret += b;
            }

            return ret;
        }

        public static Vector3 mod(Vector3 a, float b)
        {
            return new Vector3(mod(a.X, b), mod(a.Y, b), mod(a.Z, b));
        }

        public static Vector3 Add(Vector3 v, float s)
        {
            return new Vector3(v.X + s, v.Y + s, v.Z + s);
        }

        public static Vector3 abs(Vector3 vector)
        {
            return new Vector3(Math.Abs(vector.X), Math.Abs(vector.Y), Math.Abs(vector.Z));
        }

        public static Vector3 mix(Vector3 a, Vector3 b, float f)
        {
            return a * (1 - f) + b * f;
        }

        public static Vector3 hsv2rgb_smooth(Vector3 c)
        {
            float c1 = c.X + 6.0f;

            Vector3 v1 = Add(new Vector3(0.0f, 4.0f, 2.0f), c.X * 6.0f);
            Vector3 rgb = clamp(Add(abs(Add(mod(v1, 6.0f), -3.0f)), -1.0f), 0.0f, 1.0f);

            rgb = rgb*rgb*(Add(-2.0f * rgb, 3.0f)); // cubic smoothing

            return c.Z * mix(new Vector3(1.0f, 1.0f, 1.0f), rgb, c.Y);
        }

        public static float smoothstep(float edge0, float edge1, float x)
        {
            // Scale, bias and saturate x to 0..1 range
            x = Math.Clamp((x - edge0) / (edge1 - edge0), 0.0f, 1.0f);
            // Evaluate polynomial
            return x * x * (3 - 2 * x);
        }

        public static Vector3 HSV(Vector2 uv, float time)
        {
            Vector2 p = uv - new Vector2(0.5f, 0.5f);
            float a = (float)(Math.Atan2(p.Y, p.X) / 2f / Math.PI);

            a = mod(a + time / 10.0f, 1.0f);

            float r = p.Length();

            float ha = a;
            float h = mod(ha, 1.0f);

            float s = 1.0f;
            float v = r * 2.0f;

            return hsv2rgb_smooth(new Vector3(a, s, v));
        }

        public static byte Col(float x)
        {
            x *= 255f;
            x = Math.Clamp(x, 0f, 255f);
            return (byte)x;
        }

        public static byte Col(double x, double d, double e)
        {
            x *= e;
            x = Math.Pow(x, d);
            x = Math.Clamp(x, 0.0f, 1.0f);
            return (byte)(x * 255);
        }

        public static byte ColR(double x)
        {
            return Col(x, 1.9, 0.95);
        }

        public static byte ColG(double x)
        {
            return Col(x, 1.9, 0.95);
        }

        public static byte ColB(double x)
        {
            return Col(x, 1.9, 0.95);
        }

        public static Color ToSRGB(double x, double y, double z)
        {
            return Color.FromArgb(
                ColR(x),
                ColG(y),
                ColB(z));
        }

        public static Color ColorFromVec3(Vector3 v)
        {
            return Color.FromArgb(Col(v.X), Col(v.Y), Col(v.Z));
        }

        public static Vector2 Rot(Vector2 uv, float angle)
        {
            float s = (float)Math.Sin(angle);
            float c = (float)Math.Cos(angle);
            return new Vector2(
                uv.X * c + uv.Y * s,
                uv.X * s - uv.Y * c
            );
        }
    }
}
