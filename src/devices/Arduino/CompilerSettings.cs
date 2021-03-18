using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable CS1591
namespace Iot.Device.Arduino
{
    [Serializable]
    public class CompilerSettings : IEquatable<CompilerSettings>, ICloneable
    {
        public CompilerSettings()
        {
            UseFlashForKernel = true;
            CreateKernelForFlashing = true;
            UseFlashForProgram = false;
            AdditionalSuppressions = new List<string>();
            LaunchProgramFromFlash = false;
        }

        /// <summary>
        /// True (the default) to copy code, if possible, to flash
        /// </summary>
        public bool UseFlashForKernel
        {
            get;
            set;
        }

        /// <summary>
        /// True to copy the entire program to flash (default: true). Cannot be used together with <see cref="UseFlashForKernel"/>
        /// </summary>
        public bool UseFlashForProgram
        {
            get;
            set;
        }

        /// <summary>
        /// True (the default) to copy a defined set of core functions to flash, but keep the user code in Ram.
        /// Setting this to false while keeping <see cref="UseFlashForProgram"/> true will cause the flash to be rewritten with each program load, but
        /// reduce the overall program size (because the core functions required will depend on the individual program, not a pre-defined set)
        /// </summary>
        public bool CreateKernelForFlashing
        {
            get;
            set;
        }

        public IList<string> AdditionalSuppressions
        {
            get;
            set;
        }

        /// <summary>
        /// Directly launch the program from flash after power is applied to the Arduino.
        /// Only works if <see cref="CreateKernelForFlashing"/> is false and <see cref="UseFlashForProgram"/> is true.
        /// </summary>
        public bool LaunchProgramFromFlash
        {
            get;
            set;
        }

        /// <summary>
        /// Automatically restarts the program when it crashes (only valid if <see cref="LaunchProgramFromFlash"/> is true)
        /// </summary>
        public bool AutoRestartProgram
        {
            get;
            set;
        }

        object ICloneable.Clone()
        {
            return MemberwiseClone();
        }

        public CompilerSettings Clone()
        {
            return (CompilerSettings)MemberwiseClone();
        }

        public bool DoCopyToFlash(bool forKernel)
        {
            if (forKernel)
            {
                return UseFlashForKernel && CreateKernelForFlashing;
            }

            return UseFlashForProgram && !CreateKernelForFlashing;
        }

        public bool Equals(CompilerSettings? other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return UseFlashForKernel == other.UseFlashForKernel && CreateKernelForFlashing == other.CreateKernelForFlashing && UseFlashForProgram == other.UseFlashForProgram &&
                   AdditionalSuppressions.SequenceEqual(other.AdditionalSuppressions);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((CompilerSettings)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(UseFlashForKernel, CreateKernelForFlashing, AdditionalSuppressions.Count);
        }

        public static bool operator ==(CompilerSettings? left, CompilerSettings? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(CompilerSettings? left, CompilerSettings? right)
        {
            return !Equals(left, right);
        }
    }
}
