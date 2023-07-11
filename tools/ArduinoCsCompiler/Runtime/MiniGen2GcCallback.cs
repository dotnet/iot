// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArduinoCsCompiler.Runtime
{
    /// <summary>
    /// This is a very strange class. It internally creates a reference that is immediately garbage, expecting a callback
    /// on it by the finalizer thread.
    /// Since we do not currently run any finalizers, we can as well also just forget about registration
    /// </summary>
    [ArduinoReplacement("System.Gen2GcCallback", null, true, typeof(System.GC), IncludingPrivates = true)]
    internal class MiniGen2GcCallback
    {
        /// <summary>
        /// Schedule 'callback' to be called in the next GC.  If the callback returns true it is
        /// rescheduled for the next Gen 2 GC.  Otherwise the callbacks stop.
        /// </summary>
        public static void Register(Func<bool> callback)
        {
            // Create a unreachable object that remembers the callback function and target object.
            // new Gen2GcCallback(callback);
        }

        /// <summary>
        /// Schedule 'callback' to be called in the next GC.  If the callback returns true it is
        /// rescheduled for the next Gen 2 GC.  Otherwise the callbacks stop.
        ///
        /// NOTE: This callback will be kept alive until either the callback function returns false,
        /// or the target object dies.
        /// </summary>
        public static void Register(Func<object, bool> callback, object targetObj)
        {
            // Create a unreachable object that remembers the callback function and target object.
            // new Gen2GcCallback(callback, targetObj);
        }
    }
}
