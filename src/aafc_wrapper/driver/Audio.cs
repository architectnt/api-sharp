using System.Runtime.InteropServices;
namespace ArchitectAPI.Subsystems.Audio
{
    public static class AudioDriver
    {
        public static AudioVoice[] Voices;
        public static uint MaxVoices { get; private set; }

        public static void CreateVoices(ushort n = 64)
        {
            Voices = new AudioVoice[n];
            for (int i = 0; i < n; i++)
            {
                Voices[i] = new AudioVoice();
            }
            MaxVoices = n;
        }

        public static void ResetAllVoices()
        {
            for (int i = 0; i < MaxVoices; i++)
            {
                Voices[i].ResetVoice();
            }
        }

        public static void AssignClip(AudioClip clip, ushort index)
        {
            Voices[index].clip = clip;
        }

        public static AudioVoice GetVoice(ushort index)
        {
            return Voices[index]; 
        }

        public static void PlayVoice(ulong index)
        {
            AudioVoice v = Voices[index];
            if (v.clip != null)
            {
                if (v.pitch > 0)
                {
                    v.position = 0;
                }
                else if (v.pitch < 0)
                {
                    v.position = v.clip.SampleLength - 1;
                }
                v.isPlaying = true;
            }
        }

        public static void StopVoice(ulong index)
        {
            AudioVoice v = Voices[index];
            v.isPlaying = false;
            v.position = 0;
        }

        public static void PauseVoice(ulong index)
        {
            AudioVoice v = Voices[index];
            v.isPlaying = false;
        }

        public static void ResumeVoice(ulong index)
        {
            AudioVoice v = Voices[index];
            v.isPlaying = true;
        }

        public unsafe static void MixPlaybackAudio(float* output)
        {
            int frames = (int)AudioProcessor.frames;

            float* mixBuffer = stackalloc float[frames];

            for (int i = 0; i < MaxVoices; i++)
            {
                AudioProcessor.ProcessAudio(mixBuffer, Voices[i]);
            }

            AudioProcessor.buffer.Write(mixBuffer, frames);
            AudioProcessor.buffer.Read(output, frames);
        }
    }

    /// <summary>
    /// Represents an audio channel.
    /// </summary>
    public class AudioVoice()
    {
        public AudioClip clip;
        public List<IAudioEffect> effects = [];

        public double position;
        public float pitch = 1f;
        public float volume = 1;
        public float pan = 0;
        public bool isPlaying;
        public bool loop;

        public T GetEffect<T>() where T : class, IAudioEffect
        {
            for (int i = 0; i < effects.Count; i++)
            {
                if (effects[i].GetType() == typeof(T))
                {
                    return (T)effects[i];
                }
            }
            return null;
        }

        public T AddEffect<T>() where T : IAudioEffect, new()
        {
            T effect = new();
            effects.Add(effect);
            return effect;
        }

        public void ResetVoice()
        {
            isPlaying = false;
            position = 0;
            pitch = 1f;
            volume = 1;
            pan = 0;
            loop = false;
            clip?.Dispose();
        }
    }

    public unsafe class AudioBuffer : IDisposable
    {
        float* buffer;
        readonly int size;
        int wrind;

        bool disposed = false;

        public AudioBuffer(int s)
        {
            size = s;
            wrind = 0;
            buffer = (float*)Marshal.AllocHGlobal(s * sizeof(float));
            for(int i = 0; i < size; i++)
            {
                buffer[i] = 0;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if ((IntPtr)buffer != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal((IntPtr)buffer);
                    buffer = null;
                }
                disposed = true;
            }
        }

        ~AudioBuffer()
        {
            Dispose(false);
        }

        public void Clear()
        {
            for (int i = 0; i < size; i++)
            {
                *(buffer + i) = 0f;
            }
        }

        public void Write(float* data, int len)
        {
            for (int i = 0; i < len; i++)
            {
                *(buffer + i) = *(data + i);
                wrind = (wrind + 1) % size;
            }
        }

        public void Read(float* outp, int len)
        {
            int ri = wrind - len;
            if (ri < 0) ri += size;

            for (int i = 0; i < len; i++)
            {
                *(outp + i) = *(buffer + ri);
                ri = (ri + 1) % size;
            }
        }
    }

    public unsafe interface IAudioEffect
    {
        void OnAudioFilterRead(float* samples, byte channels, uint lenth);
    }

    public unsafe interface IInterpolation
    {
        float Interpolate(float* samples, uint len, byte channels, double position, byte channel);
    }

    public unsafe static class AudioProcessor
    {
        public static AudioBuffer buffer;
        public static IInterpolation interp;

        public static uint frequency;
        public static byte channels;
        public static uint len;
        public static uint frames;

        /// <summary>
        /// Current FrameTime of the Audio
        /// </summary>
        public static double DSPTime { get; private set; }

        public static void Initialize(uint freq, byte chn, uint size, IInterpolation itrp = null)
        {
            frequency = freq;
            channels = chn;
            len = size;
            frames = len / sizeof(float);
            buffer = new((int)frames);
            interp = itrp;
        }

        public static void ProcessAudio(float* output, AudioVoice aud)
        {
            if (aud == null || !aud.isPlaying || aud.clip == null)
                return;

            AudioClip clip = aud.clip;
            uint pframes = frames / channels;
            DSPTime += (double)pframes / frequency;

            double pitchfactor = aud.pitch * ((double)clip.Frequency / frequency);

            float pan = Math.Clamp(aud.pan, -1, 1);
            float vol = Math.Clamp(aud.volume, 0f, 1.0f);
            float avl = Math.Clamp(vol * (1.0f - pan), 0f, 1.0f);
            float avr = Math.Clamp(vol * (1.0f + pan), 0f, 1.0f);

            float* smpbuffr = stackalloc float[(int)frames];

            byte ch;
            uint i;
            for (i = 0; i < pframes; i++)
            {
                for (ch = 0; ch < channels; ch++)
                {
                    float smpl = 0;

                    if (aud.isPlaying)
                    {
                        double spos = aud.position + i * pitchfactor;

                        if (spos >= clip.SampleLength || spos < 0)
                        {
                            if (aud.loop)
                            {
                                aud.position %= clip.SampleLength;
                                spos %= clip.SampleLength;
                            }
                            else
                            {
                                aud.isPlaying = false;
                                aud.position = 0;
                            }
                        }

                        smpl = interp != null 
                            ? interp.Interpolate(clip.Samples, clip.ActualSampleLength, clip.Channels, spos, ch) 
                            : *(clip.Samples + ((uint)spos * clip.Channels + (clip.Channels > 1 ? ch : 0)));
                    }

                    smpl *= (ch == 0) ? avl : avr;

                    *(smpbuffr + i * channels + ch) = smpl;
                }
            }

            for (int e = 0; e < aud.effects.Count; e++)
            {
                aud.effects[e].OnAudioFilterRead(smpbuffr, channels, pframes);
            }

            for (i = 0; i < pframes; i++)
            {
                for (ch = 0; ch < channels; ch++)
                {
                    *(output + i * channels + ch) += *(smpbuffr + i * channels + ch);
                }
            }

            if (aud.isPlaying) aud.position += pframes * pitchfactor;
        }
    }
}
