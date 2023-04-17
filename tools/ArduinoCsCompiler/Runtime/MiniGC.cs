// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Iot.Device.Arduino;

namespace ArduinoCsCompiler.Runtime
{
    [ArduinoReplacement(typeof(System.GC), true, IncludingPrivates = true)]
    internal static class MiniGC
    {
        public static int MaxGeneration
        {
            get
            {
                return 0;
            }
        }

        public static T[] AllocateUninitializedArray<T>(int length, bool pinned = false) // T[] rather than T?[] to match `new T[length]` behavior
        {
            return new T[length]; // Initializing is so much cheaper than emulating the implementation, so that we don't care.
        }

        public static void KeepAlive(object? obj)
        {
            // This in fact is a no-op
        }

        public static void SuppressFinalize(object obj)
        {
            // No op (we're not running any finalizers)
        }

        public static void ReRegisterForFinalize(object obj)
        {
            // No op, like above
        }

#if NET5_0_OR_GREATER
        public static GCMemoryInfo GetGCMemoryInfo()
        {
            return GetGCMemoryInfo(GCKind.Any);
        }

        public static GCMemoryInfo GetGCMemoryInfo(GCKind kind)
        {
            MiniGcMemoryInfoData data = new MiniGcMemoryInfoData();
            data._concurrent = false;
            data._compacted = false;
            data._finalizationPendingCount = 0;
            data._generation = 0;
            data._heapSizeBytes = GetTotalMemory(true);
            data._totalCommittedBytes = GetTotalAllocatedBytes(false);
            data._totalAvailableMemoryBytes = GetTotalAvailableMemoryBytes();
            MiniGcMemoryInfo instance = new MiniGcMemoryInfo(data);
            GCMemoryInfo ret = default;
            // The return value of As<T1, T2> is ref, but this should make a copy, so we can safely return it.
            ret = MiniUnsafe.As<MiniGcMemoryInfo, GCMemoryInfo>(ref instance);
            return ret;
        }

        [ArduinoImplementation("GcTotalAvailableMemoryBytes")]
        private static long GetTotalAvailableMemoryBytes()
        {
            throw new NotImplementedException();
        }
#endif

        [ArduinoImplementation("GcGetTotalAllocatedBytes")]
        public static long GetTotalAllocatedBytes(bool precise = false)
        {
            throw new NotImplementedException();
        }

        public static void Collect()
        {
            Collect(0, GCCollectionMode.Forced, true, false);
        }

        public static void Collect(int generation)
        {
            Collect(generation, GCCollectionMode.Forced, true, false);
        }

        public static void Collect(int generation, GCCollectionMode mode)
        {
            Collect(generation, mode, true, false);
        }

        public static void Collect(int generation, GCCollectionMode mode, bool blocking)
        {
            Collect(generation, mode, blocking, false);
        }

        [ArduinoImplementation("GcCollect")]
        public static void Collect(int generation, GCCollectionMode mode, bool blocking, bool compacting)
        {
            throw new NotImplementedException();
        }

        public static int GetGeneration(object obj)
        {
            // Out GC has only one generation (so far)
            return 0;
        }

        [ArduinoImplementation("GcGetTotalMemory")]
        public static long GetTotalMemory(bool forceFullCollection)
        {
            throw new NotImplementedException();
        }

#if NET5_0_OR_GREATER
        [ArduinoReplacement(typeof(System.GCMemoryInfo), true, IncludingPrivates = true)]
        internal struct MiniGcMemoryInfo
        {
            private readonly MiniGcMemoryInfoData _data; // place holder
            public MiniGcMemoryInfo(MiniGcMemoryInfoData obj)
            {
                _data = obj;
            }

            /// <summary>
            /// High memory load threshold when this GC occured
            /// </summary>
            public long HighMemoryLoadThresholdBytes => _data._highMemoryLoadThresholdBytes;

            /// <summary>
            /// Memory load when this GC ocurred
            /// </summary>
            public long MemoryLoadBytes => _data._memoryLoadBytes;

            /// <summary>
            /// Total available memory for the GC to use when this GC ocurred.
            ///
            /// If the environment variable COMPlus_GCHeapHardLimit is set,
            /// or "Server.GC.HeapHardLimit" is in runtimeconfig.json, this will come from that.
            /// If the program is run in a container, this will be an implementation-defined fraction of the container's size.
            /// Else, this is the physical memory on the machine that was available for the GC to use when this GC occurred.
            /// </summary>
            public long TotalAvailableMemoryBytes => _data._totalAvailableMemoryBytes;

            /// <summary>
            /// The total heap size when this GC ocurred
            /// </summary>
            public long HeapSizeBytes => _data._heapSizeBytes;

            /// <summary>
            /// The total fragmentation when this GC ocurred
            ///
            /// Let's take the example below:
            ///  | OBJ_A |     OBJ_B     | OBJ_C |   OBJ_D   | OBJ_E |
            ///
            /// Let's say OBJ_B, OBJ_C and and OBJ_E are garbage and get collected, but the heap does not get compacted, the resulting heap will look like the following:
            ///  | OBJ_A |           F           |   OBJ_D   |
            ///
            /// The memory between OBJ_A and OBJ_D marked `F` is considered part of the FragmentedBytes, and will be used to allocate new objects. The memory after OBJ_D will not be
            /// considered part of the FragmentedBytes, and will also be used to allocate new objects
            /// </summary>
            public long FragmentedBytes => _data._fragmentedBytes;

            /// <summary>
            /// The index of this GC. GC indices start with 1 and get increased at the beginning of a GC.
            /// Since the info is updated at the end of a GC, this means you can get the info for a BGC
            /// with a smaller index than a foreground GC finished earlier.
            /// </summary>
            public long Index => _data._index;

            /// <summary>
            /// The generation this GC collected. Collecting a generation means all its younger generation(s)
            /// are also collected.
            /// </summary>
            public int Generation => _data._generation;

            /// <summary>
            /// Is this a compacting GC or not.
            /// </summary>
            public bool Compacted => _data._compacted;

            /// <summary>
            /// Is this a concurrent GC (BGC) or not.
            /// </summary>
            public bool Concurrent => _data._concurrent;

            /// <summary>
            /// Total committed bytes of the managed heap.
            /// </summary>
            public long TotalCommittedBytes => _data._totalCommittedBytes;

            /// <summary>
            /// Promoted bytes for this GC.
            /// </summary>
            public long PromotedBytes => _data._promotedBytes;

            /// <summary>
            /// Number of pinned objects this GC observed.
            /// </summary>
            public long PinnedObjectsCount => _data._pinnedObjectsCount;

            /// <summary>
            /// Number of objects ready for finalization this GC observed.
            /// </summary>
            public long FinalizationPendingCount => _data._finalizationPendingCount;

            /// <summary>
            /// Pause durations. For blocking GCs there's only 1 pause; for BGC there are 2.
            /// </summary>
            public ReadOnlySpan<TimeSpan> PauseDurations
            {
                get
                {
                    return ReadOnlySpan<TimeSpan>.Empty;
                }
            }

            /// <summary>
            /// This is the % pause time in GC so far. If it's 1.2%, this number is 1.2.
            /// </summary>
            public double PauseTimePercentage => (double)_data._pauseTimePercentage / 100.0;

            /// <summary>
            /// Generation info for all generations.
            /// </summary>
            public ReadOnlySpan<GCGenerationInfo> GenerationInfo
            {
                get
                {
                    return ReadOnlySpan<GCGenerationInfo>.Empty;
                }
            }

        }

#pragma warning disable CS0649 // Member is unused
        internal class MiniGcMemoryInfoData
        {
            internal long _highMemoryLoadThresholdBytes;
            internal long _totalAvailableMemoryBytes;
            internal long _memoryLoadBytes;
            internal long _heapSizeBytes;
            internal long _fragmentedBytes;
            internal long _totalCommittedBytes;
            internal long _promotedBytes;
            internal long _pinnedObjectsCount;
            internal long _finalizationPendingCount;
            internal long _index;
            internal int _generation;
            internal int _pauseTimePercentage;
            internal bool _compacted;
            internal bool _concurrent;
        }
#endif
    }
}
