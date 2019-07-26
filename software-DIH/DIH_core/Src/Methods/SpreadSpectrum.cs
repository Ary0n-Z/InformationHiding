
using DIH_core.Src.MethodsInterface;
using DIH_core.Src.WAV;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace DIH_core.Src.Methods
{
    class SpreadSpectrum : MethodBase, IHidingMethod
    {
        [HidingMethodParam(
            Type = HidingMethodParamAttribute.ParamType.Range,
            MaxRange = 1,
            MinRange = 0,
            Description = "Define how much of noise will be added.",
            NotInExtract = true
            )]
        public double NoiseRate { get; set; } = 0.1d;
        [HidingMethodParam(
            Type = HidingMethodParamAttribute.ParamType.IntValue,
            Description = "Random seed for pseudo random signal generation."
            )]
        public int RandomSeed { get; set; } = 0;
        [HidingMethodParam(
            Type = HidingMethodParamAttribute.ParamType.FilePath,
            Description = "Select audio cover that was used for embedding.",
            Required = true,
            NotInEmbed = true
        )]
        public string PrimeSignalFilePath { get; set; } = "";
        [HidingMethodParam(
           Type = HidingMethodParamAttribute.ParamType.IntValue,
           Description = "Length of the embedded message.",
           Required = true,
           NotInEmbed = true
       )]
        public int MessageLength { get; set; }
        public string Description()
        {
            return "Direct-sequence spread spectrum";
        }
        private void RndSignal(Random rnd, int[] buffer)
        {
            for (int i = 0; i < buffer.Length; i++)
                buffer[i] = rnd.Next(0, 2) * 2 - 1;
        }
        private int[] BytesToBits(byte[] bytes)
        {
            int[] bits = new int[bytes.Length * 8];
            int c = 0;
            for (int i = 0; i < bytes.Length; i++)
                for (int j = 0; j < 8; j++)
                    bits[c++] = (bytes[i] >> j) & 1;
            return bits;
        }
        public DigitalAudio Embed(DigitalAudio cover, byte[] message)
        {
            var primeSignal = (int[])cover.GetSignal().Clone();
            //var embedingData = CreateEmbeddedMessage(message);
            var embedingData = new BitArray(message);
            int n_bits = embedingData.Length;
            var signal = cover.GetSignal();
           
            //Segmentation
            int samplesPerSegment = (int)Math.Floor(signal.Length / (double)n_bits);
            var rnd = new Random(RandomSeed);
            int lseg, rseg;
            int[] pseudoRandomSignal = new int[samplesPerSegment];
            int max = 0, min = 0;
            for (int i = 0; i < n_bits; i++)
            {
                lseg = i * samplesPerSegment;
                rseg = samplesPerSegment * (i + 1);
                RndSignal(rnd, pseudoRandomSignal);
                int bit = (embedingData[i] ? 1 : -1);
                //int bit = 1;
                for (int j = lseg, k = 0; j < rseg; j++, k++)
                {
                    var mix = pseudoRandomSignal[k] * signal[j] * bit;
                    signal[j] += (int)(NoiseRate * mix);
                    if (signal[j] > max) max = signal[j];
                    if (signal[j] < min) min = signal[j];
                }
            }
            //if (max > short.MaxValue || min < short.MinValue)
            //{
            //    float scaleFactor = ((short.MaxValue - short.MinValue) / (float)(max - min));
            //    for (int i = 0; i < signal.Length; i++)
            //        signal[i] = (int)(scaleFactor * (signal[i] - min) + short.MinValue);
            //}
            // Extract --------------------------
            rnd = new Random(RandomSeed);
            var difference = new double[samplesPerSegment];
            List<bool> outBits = new List<bool>();
            double primeMean = 0, difMean = 0;
            lseg = 0; rseg = 0;
            int segCount = n_bits;
            for (int i = 0; i < segCount; i++)
            {
                lseg = i * samplesPerSegment;
                rseg = samplesPerSegment * (i + 1);
                RndSignal(rnd, pseudoRandomSignal);
                for (int j = lseg, k = 0; j < rseg; j++, k++)
                {
                    difference[k] = (signal[j] - primeSignal[j]) * pseudoRandomSignal[k];
                    primeMean += primeSignal[j];
                    difMean += signal[j];
                }
                outBits.Add(Math.Sign(primeMean) == Math.Sign(difMean));
                primeMean = 0; difMean = 0;
            }
            return cover;
        }

        private void WriteNoise(int[] noiseSignal,int ml)
        {
            DigitalAudio digitalAudio = new DigitalAudio();
            digitalAudio.AddChannel(noiseSignal);
            digitalAudio.SampleRate = 44100;
            digitalAudio.SignificantBitsPerSample = 16;
            var stream = File.Create("nr_"+ NoiseRate + "ml_" + ml+".wav");
            using(WavWriter writer = new WavWriter(stream))
                writer.WriteWavDefault(digitalAudio);
        }

        private static byte[] BitArrayToByteArray(BitArray bits)
        {
            byte[] ret = new byte[(bits.Length - 1) / 8 + 1];
            bits.CopyTo(ret, 0);
            return ret;
        }
        public byte[] Extract(DigitalAudio stegoAudio)
        {
            int[] primeSignal;
            using (WavReader reader = new WavReader(File.OpenRead(PrimeSignalFilePath)))
                primeSignal = reader.ReadAudio().GetSignal();
            var stegoSignal = stegoAudio.GetSignal();
            int samplesPerSegment = (int)Math.Floor((double)stegoSignal.Length / (MessageLength*8));

            var rnd = new Random(RandomSeed); int lseg, rseg;
            int[] pseudoRandomSignal = new int[samplesPerSegment];
            var difference = new double[samplesPerSegment];
            List<bool> message = new List<bool>();
            double primeMean = 0,difMean = 0;
            for (int i = 0; (i + 1) * samplesPerSegment <= stegoSignal.Length; i++)
            {
                lseg = i * samplesPerSegment;
                rseg = samplesPerSegment * (i + 1);
                RndSignal(rnd, pseudoRandomSignal);
                for (int j = lseg, k = 0; j < rseg; j++, k++)
                {
                    primeSignal[j] = (int)(primeSignal[j] * 0.95);

                    difference[k] = (stegoSignal[j] - primeSignal[j])* pseudoRandomSignal[k];
                    primeMean += primeSignal[j];
                    difMean += stegoSignal[j];
                }
                message.Add(Math.Sign(primeMean) == Math.Sign(difMean));
                primeMean = 0; difMean = 0;
            }
            return BitArrayToByteArray(new BitArray(message.ToArray()));
        }

        public long GetAvailableSpace(DigitalAudio stegoAudio)
        {
            return stegoAudio.SamplesInChannel/8;
        }
    }
}
