// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace ArduinoCsCompiler.Runtime
{
    /// <summary>
    /// This class provides simple assertion functions. Unlike the other classes of the Arduino runtime, this one is public
    /// and intended to be called by user code
    /// </summary>
    public static class MiniAssert
    {
        public static void That(bool condition)
        {
            if (!condition)
            {
                throw new MiniAssertionException();
            }
        }

        public static void That(bool condition, string message)
        {
            if (!condition)
            {
                throw new MiniAssertionException(message);
            }
        }

        public static void False(bool condition)
        {
            if (condition)
            {
                throw new MiniAssertionException();
            }
        }

        public static void False(bool condition, string message)
        {
            if (condition)
            {
                throw new MiniAssertionException(message);
            }
        }

        public static void NotNull(object? obj)
        {
            if (obj == null)
            {
                throw new MiniAssertionException();
            }
        }

        public static void IsNull(object? obj)
        {
            if (obj != null)
            {
                throw new MiniAssertionException();
            }
        }

        public static void AreEqual(string expected, string actual)
        {
            That(actual == expected, $"Expected '{expected}', actual '{actual}'");
        }

        public static void AreEqual(int expected, int actual)
        {
            That(expected == actual, $"Expected {expected}, actual {actual}");
        }

        public static void AreNotEqual(int expected, int actual)
        {
            That(expected != actual, $"Expected {expected}, actual {actual}");
        }

        public static void AreEqual(double expected, double actual)
        {
            AreEqual(expected, actual, 0);
        }

        public static void AreEqual(double expected, double actual, double delta)
        {
            double effectiveDelta = Math.Abs(expected - actual);
            That(effectiveDelta <= delta, $"Expected {expected}, actual {actual} +/- {delta}");
        }

        public static void IsNotNull(object obj)
        {
            if (obj == null)
            {
                throw new MiniAssertionException("Object was null");
            }
        }
    }
}
