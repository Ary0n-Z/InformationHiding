namespace DIH_core.Src.WAV
{
    public struct WaveFormat
    {
        private uint dataSize;
        public uint SectionSize {
            get {
                return dataSize;
            } set
            {
                dataSize = (value % 2 == 0) ? value : value + 1; 
            }
        }
        public ushort CompressionCode { get; set; }
        public ushort NumberOfChannels { get; set; }
        public uint SampleRate { get; set; }
        public ushort SignificantBitsPerSample { get; set; }
        public uint AvarageBytesPerSecond { get; set; }
        public ushort BlockAlign { get; set; }

    }
}
