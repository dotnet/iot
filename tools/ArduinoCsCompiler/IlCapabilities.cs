namespace ArduinoCsCompiler
{
    public record IlCapabilities
    {
        public int IntSize
        {
            get;
            init;
        }

        public int PointerSize
        {
            get;
            init;
        }

        public long FlashSize
        {
            get;
            init;
        }

        public int RamSize
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
