using System;

namespace Iot.Device.Arduino.Runtime
{
    [ArduinoReplacement(typeof(System.Array), false)]
    internal static class MiniArray
    {
        [ArduinoImplementation]
        public static void Copy(Array sourceArray, Array destinationArray, long length)
        {
            int ilength = (int)length;
            if (length != ilength)
            {
                throw new ArgumentOutOfRangeException();
            }

            Copy(sourceArray, destinationArray, ilength);
        }

        [ArduinoImplementation]
        public static void Copy(Array sourceArray, Array destinationArray, int length)
        {
            if (sourceArray == null)
            {
                throw new ArgumentNullException();
            }

            if (destinationArray == null)
            {
                throw new ArgumentNullException();
            }

            Copy(sourceArray, sourceArray.GetLowerBound(0), destinationArray, destinationArray.GetLowerBound(0), length, reliable: false);
        }

        // Copies length elements from sourceArray, starting at sourceIndex, to
        // destinationArray, starting at destinationIndex.
        [ArduinoImplementation]
        public static void Copy(Array sourceArray, int sourceIndex, Array destinationArray, int destinationIndex, int length)
        {
            // Less common
            Copy(sourceArray!, sourceIndex, destinationArray!, destinationIndex, length, reliable: false);
        }

        [ArduinoImplementation]
        private static void Copy(Array sourceArray, int sourceIndex, Array destinationArray, int destinationIndex, int length, bool reliable)
        {
            if (sourceArray == null || destinationArray == null)
            {
                throw new ArgumentNullException();
            }

            if (sourceArray.GetType() != destinationArray.GetType() && sourceArray.Rank != destinationArray.Rank)
            {
                throw new RankException();
            }

            if (length < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length), "Argument out of Range");
            }

            int srcLB = sourceArray.GetLowerBound(0);
            if (sourceIndex < srcLB || sourceIndex - srcLB < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(sourceIndex), "Argument out of Range");
            }

            sourceIndex -= srcLB;

            int dstLB = destinationArray.GetLowerBound(0);
            if (destinationIndex < dstLB || destinationIndex - dstLB < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(destinationIndex), "Argument out of Range");
            }

            destinationIndex -= dstLB;

            if ((uint)(sourceIndex + length) > (nuint)sourceArray.LongLength)
            {
                throw new ArgumentException();
            }

            if ((uint)(destinationIndex + length) > (nuint)destinationArray.LongLength)
            {
                throw new ArgumentException();
            }

            // The standard runtime does allow this under certain conditions. For simplicity, we don't.
            if (sourceArray.GetType() != destinationArray.GetType())
            {
                throw new ArrayTypeMismatchException();
            }

            CopyCore(sourceArray, sourceIndex, destinationArray, destinationIndex, length);
        }

        [ArduinoImplementation]
        public static void Copy(Array sourceArray, long sourceIndex, Array destinationArray, long destinationIndex, long length)
        {
            int isourceIndex = (int)sourceIndex;
            int idestinationIndex = (int)destinationIndex;
            int ilength = (int)length;

            Copy(sourceArray, isourceIndex, destinationArray, idestinationIndex, ilength);
        }

        [ArduinoImplementation(NativeMethod.ArrayCopyCore)]
        private static void CopyCore(Array sourceArray, int sourceIndex, Array destinationArray, int destinationIndex, int length)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation(NativeMethod.ArrayClear)]
        public static void Clear(Array array, int index, int length)
        {
            throw new NotImplementedException();
        }
    }
}
