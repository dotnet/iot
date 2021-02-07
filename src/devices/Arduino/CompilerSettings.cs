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
            UseFlash = true;
            CreateKernelForFlashing = true;
            AdditionalSuppressions = new List<string>();
        }

        /// <summary>
        /// True (the default) to copy code, if possible, to flash
        /// </summary>
        public bool UseFlash
        {
            get;
            set;
        }

        /// <summary>
        /// True (the default) to copy a defined set of core functions to flash, but keep the user code in Ram.
        /// Setting this to false while keeping <see cref="UseFlash"/> true will cause the flash to be rewritten with each program load, but
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

        object ICloneable.Clone()
        {
            return MemberwiseClone();
        }

        public CompilerSettings Clone()
        {
            return (CompilerSettings)MemberwiseClone();
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

            return UseFlash == other.UseFlash && CreateKernelForFlashing == other.CreateKernelForFlashing && AdditionalSuppressions.SequenceEqual(other.AdditionalSuppressions);
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
            return HashCode.Combine(UseFlash, CreateKernelForFlashing, AdditionalSuppressions.Count);
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
