/*
    Copyright (C) 2025 Architect Enterprises
    This file is apart of the API and are MIT licensed
*/

using System.Runtime.InteropServices;
namespace ArchitectAPI.Subsystems.Audio.Modules
{
    public unsafe class Echo : IAudioEffect
    {
        float* echoBuffer;
        int echoIndex = 0;
        float echoVolume = 0.5f;
        int delaySamples;

        public float DelaySeconds
        {
            get => delaySamples / (float)AudioProcessor.frequency;
            set
            {
                delaySamples = (int)(AudioProcessor.frequency * value);
                if (echoBuffer != null)
                {
                    Marshal.FreeHGlobal((nint)echoBuffer);
                }
                echoBuffer = (float*)Marshal.AllocHGlobal(delaySamples * sizeof(float));
                for (int i = 0; i < delaySamples; i++)
                {
                    echoBuffer[i] = 0.0f;
                }
                echoIndex = 0;
            }
        }

        public float EchoVolume
        {
            get => echoVolume;
            set => echoVolume = value;
        }

        public Echo()
        {
            DelaySeconds = 0.4f;
            EchoVolume = 0.6f;
        }

        ~Echo()
        {
            if (echoBuffer != null)
            {
                Marshal.FreeHGlobal((nint)echoBuffer);
                echoBuffer = null;
            }
        }

        public void OnAudioFilterRead(float* samples, byte channels, uint length)
        {
            int ch;
            for (uint i = 0; i < length; i++)
            {
                for (ch = 0; ch < channels; ch++)
                {
                    *(samples + i * channels + ch) += *(echoBuffer + echoIndex) * echoVolume;
                    *(echoBuffer + echoIndex) = *(samples + i * channels + ch) * echoVolume;
                    echoIndex = (echoIndex + 1) % delaySamples;
                }
            }
        }
    }

    public unsafe class LowpassFilter : IAudioEffect
    {
        float cutoffFrequency;
        public float CutoffFrequency
        {
            get => cutoffFrequency;
            set
            {
                cutoffFrequency = value;
                CalculateCoefficients();
            }
        }

        float a0, b1;
        float z1 = 0.0f;

        public LowpassFilter()
        {
            CutoffFrequency = 1200f;
        }

        void CalculateCoefficients()
        {
            float frac = cutoffFrequency / AudioProcessor.frequency / 2;
            float x = (float)Math.Exp(-2.0 * Math.PI * frac);
            a0 = 1.0f - x;
            b1 = x;
        }

        public void OnAudioFilterRead(float* samples, byte channels, uint length)
        {
            for (uint i = 0; i < length; i++)
            {
                z1 = *(samples + i) * a0 + z1 * b1;
                *(samples + i) = z1;
            }
        }
    }

}
