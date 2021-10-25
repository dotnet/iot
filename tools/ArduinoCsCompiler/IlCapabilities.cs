using UnitsNet;

namespace ArduinoCsCompiler
{
    public record IlCapabilities
    {
        public Information IntSize
        {
            get;
            init;
        }

        public Information PointerSize
        {
            get;
            init;
        }

        public Information FlashSize
        {
            get;
            init;
        }

        public Information RamSize
        {
            get;
            init;
        }

        public override string ToString()
        {
            return $"{nameof(IntSize)}: {IntSize}, {nameof(PointerSize)}: {PointerSize}, {nameof(FlashSize)}: {FlashSize}, {nameof(RamSize)}: {RamSize}";
        }
    }
}
