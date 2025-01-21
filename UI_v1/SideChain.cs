using NAudio.Wave;
using System.Diagnostics;

namespace WpfApp1.plugin
{
    public class SideChain : ISampleProvider
    {
        private readonly ISampleProvider sourceProvider;
        private readonly int sampleRate;
        private readonly int channels;
        private readonly float maxVolume;
        private readonly float bpm;
        private int samplesPerBeat;
        private int samplePosition;
        private readonly float ThreeOrFour;
        public bool isActive { get; set; } = false;

        public SideChain(ISampleProvider sourceProvider, float bpm, bool isActive, float maxVolume = 2.0f, float ThreeOrFour = 0.25f)
        {
            this.sourceProvider = sourceProvider;
            this.bpm = bpm;
            this.maxVolume = maxVolume;
            this.ThreeOrFour = ThreeOrFour;
            this.isActive = isActive;
            sampleRate = sourceProvider.WaveFormat.SampleRate;
            channels = sourceProvider.WaveFormat.Channels;

            // caulaete sample rate 每拍分60個
            samplesPerBeat = (int)(sampleRate * 60f / bpm);
            samplePosition = 0;
        }

        public WaveFormat WaveFormat => sourceProvider.WaveFormat;

        public int Read(float[] buffer, int offset, int count)
        {
            int samplesRead = sourceProvider.Read(buffer, offset, count);
            //Debug.WriteLine($"Read called, samples read: {samplesRead}");

            if (isActive)
            {
                Debug.WriteLine("Is active.");
                for (int n = 0; n < samplesRead; n++)
                {
                    int positionInBeat = samplePosition % samplesPerBeat;

                    // 四分之一拍内音量從0到最大
                    float volume = 1.0f;
                    if (positionInBeat < samplesPerBeat / ThreeOrFour)
                    {
                        volume = (float)positionInBeat / (samplesPerBeat / ThreeOrFour) * maxVolume * 4;
                    }

                    buffer[offset + n] *= volume;

                    if ((n + 1) % channels == 0)
                    {
                        samplePosition++;
                    }
                }
            }
            return samplesRead;
        }
    }
}
