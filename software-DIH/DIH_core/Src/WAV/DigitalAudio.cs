using System;
using System.Collections;
using System.Collections.Generic;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Storage;

namespace DIH_core.Src.WAV
{
    public class DigitalAudio
    {
        private Dictionary<ushort, int[]> channels = new Dictionary<ushort, int[]>();
        public long SamplesInChannel { get {
                return channels.Count > 0 ? channels[0].LongLength : 0;
            }}
        public ushort NumOfChannels { get; private set; } = 0;
        public uint SampleRate { get; set; }
        public ushort SignificantBitsPerSample { get; set; }
        public long DataSize { get
            {
                return NumOfChannels * SamplesInChannel;
            }
        }
        public int[] GetSignal()
        {
            return channels[0];
        }
        public int[] RewriteSignal(int[] signal )
        {
            return channels[0] = signal;
        }
        public void AppendZeros(long v)
        {
            for(ushort i = 0; i < NumOfChannels; i++) {
                var signal = channels[i];
                Array.Resize(ref signal, (int)(signal.Length + v));
            }
        }
        public int[][] GetAllSignals()
        {
            var signals = new int[NumOfChannels][];
            for (ushort i = 0; i < NumOfChannels; i++)
                signals[i] = channels[i];
            return signals;
        }                                                                          
        public void AddChannel(int[] signal)
        {
            channels.Add(NumOfChannels++, signal);
        }

    }
}
