/*
    Copyright (C) 2025 Architect Enterprises
    This file is apart of the API and are MIT licensed
*/

using System;
using System.Runtime.InteropServices;

namespace ArchitectAPI.Wrappers.Audio
{
    public unsafe partial class LibAAFC {
        public const string AAFCPATH = ".core/Internal/aafc";

        [LibraryImport(AAFCPATH)]
        private static partial ushort aafc_getversion();
        public static ushort Version => aafc_getversion();

        /// <summary>
        /// AAFC Header structure
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct AAFC_HEADER {
            public ushort signature, version;
            public uint freq;
            public byte channels, bps, sampletype;
            public uint samplelength, loopst, loopend;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct AAFC_LCHEADER {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 5)]
            public string headr;

            public uint version;
            public uint freq;
            public byte channels;
            public uint samplelength;
            public byte bps;
            public byte sampletype;
        }

        /// <summary>
        /// Output struct
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct AAFCOUTPUT {
            public nuint size;
            public byte* data;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct AAFCDECOUTPUT {
            public AAFC_HEADER header;
            public float* data;
        }


        [LibraryImport(AAFCPATH)]
        private static partial AAFCDECOUTPUT aafc_import(byte* data);
        [LibraryImport(AAFCPATH)]
        private static partial AAFCOUTPUT aafc_export(float* samples, uint freq, uint channels, uint samplelength, byte bps = 16, byte sampletype = 1, [MarshalAs(UnmanagedType.Bool)] bool forcemono = false, uint samplerateoverride = 0, [MarshalAs(UnmanagedType.Bool)] bool nm = false, float pitch = 1);
        [LibraryImport(AAFCPATH)]
        private static partial IntPtr aafc_getheader(byte* data);
        [LibraryImport(AAFCPATH)]
        public static partial IntPtr aafc_float_to_int(float* arr, long length, byte type);
        [LibraryImport(AAFCPATH)]
        public static partial IntPtr aafc_int_to_float(IntPtr arr, long length, byte type);
        [LibraryImport(AAFCPATH)]
        public static partial IntPtr aafc_resample_data(float* input, uint samplerateoverride, AAFC_HEADER* h, float pitch = 1, [MarshalAs(UnmanagedType.Bool)] bool nointerp = false);
        [LibraryImport(AAFCPATH)]
        public static partial IntPtr aafc_normalize(float* arr, AAFC_HEADER* h);


        public static byte[] Export(float[] samples, uint channels, uint samplerate, bool mono = false, byte bps = 16, byte sampletype = 1, uint sproverride = 0, bool nm = false, float pitch = 1) {
            fixed (float* fptr = samples) {
                AAFCOUTPUT afo = aafc_export(fptr, samplerate, channels, (uint)samples.LongLength, bps, sampletype, mono, sproverride, nm, pitch);
                byte[] rst = new byte[afo.size];
                Marshal.Copy((IntPtr)afo.data, rst, 0, (int)afo.size);
                Marshal.FreeHGlobal((IntPtr)afo.data);
                return rst;
            }
        }

        public static byte[] Export(byte[] isamples, uint channels, uint samplerate, bool mono = false, byte bps = 16, byte sampletype = 1, uint sproverride = 0, bool nm = false, float pitch = 1) {
            float[] samples = new float[isamples.LongLength];
            fixed (byte* ptr = isamples)
                Marshal.Copy(aafc_int_to_float((nint)ptr, isamples.Length, 8), samples, 0, isamples.Length);

            fixed (float* fptr = samples) {
                AAFCOUTPUT afo = aafc_export(fptr, samplerate, channels, (uint)samples.LongLength, bps, sampletype, mono, sproverride, nm, pitch);
                byte[] rst = new byte[afo.size];
                Marshal.Copy((IntPtr)afo.data, rst, 0, (int)afo.size);
                Marshal.FreeHGlobal((IntPtr)afo.data);
                return rst;
            }
        }

        public static byte[] Export(short[] isamples, uint channels, uint samplerate, bool mono = false, byte bps = 16, byte sampletype = 1, uint sproverride = 0, bool nm = false, float pitch = 1) {
            float[] samples = new float[isamples.LongLength];
            fixed (short* ptr = isamples)
                Marshal.Copy(aafc_int_to_float((nint)ptr, isamples.LongLength, 16), samples, 0, isamples.Length);

            fixed (float* fptr = samples) {
                AAFCOUTPUT afo = aafc_export(fptr, samplerate, channels, (uint)samples.LongLength, bps, sampletype, mono, sproverride, nm, pitch);
                byte[] rst = new byte[afo.size];
                Marshal.Copy((IntPtr)afo.data, rst, 0, (int)afo.size);
                Marshal.FreeHGlobal((IntPtr)afo.data);
                return rst;
            }
        }

        public static byte[] Export(int[] isamples, uint channels, uint samplerate, bool mono = false, byte bps = 16, byte sampletype = 1, uint sproverride = 0, bool nm = false, float pitch = 1) {
            float[] samples = new float[isamples.LongLength];
            fixed (int* ptr = isamples)
                Marshal.Copy(aafc_int_to_float((nint)ptr, isamples.LongLength, 32), samples, 0, isamples.Length);

            fixed (float* fptr = samples) {
                AAFCOUTPUT afo = aafc_export(fptr, samplerate, channels, (uint)samples.LongLength, bps, sampletype, mono, sproverride, nm, pitch);
                byte[] rst = new byte[afo.size];
                Marshal.Copy((IntPtr)afo.data, rst, 0, (int)afo.size);
                Marshal.FreeHGlobal((IntPtr)afo.data);
                return rst;
            }
        }

        public static AudioClip Import(byte[] bytes, string n) {
            fixed (byte* bptr = bytes) {
                AAFCDECOUTPUT d = aafc_import(bptr);
                return ((nuint)d.data != nuint.Zero)?new(n, d.data, d.header):null;
            }
        }

        // read aafc from file system
        public static AudioClip LoadAAFC(string filename) 
            => Import(System.IO.File.ReadAllBytes(filename), System.IO.Path.GetFileNameWithoutExtension(filename));
    }

    public unsafe class AudioClip : IDisposable {
        LibAAFC.AAFC_HEADER h; // aafc referencing


        public string Name { get; private set; }
        public float* Samples { get; private set; }
        public uint Frequency => h.freq;
        public uint SampleLength => h.samplelength / h.channels;
        public uint ActualSampleLength => h.samplelength;
        public byte Channels => h.channels;

        bool disposed = false;

        public AudioClip(string name, float[] samples, LibAAFC.AAFC_HEADER header) {
            h = header;
            Name = name;
            Samples = (float*)Marshal.AllocHGlobal(samples.Length * sizeof(float));
            Marshal.Copy(samples, 0, (nint)Samples, samples.Length);
        }

        public AudioClip(string name, float* samples, LibAAFC.AAFC_HEADER header) {
            h = header;
            Name = name;
            Samples = samples;
        }

        public void Resample(uint newSampleRate, float pitch = 1) {
            if (newSampleRate == h.freq && pitch == 1) return;
            fixed (LibAAFC.AAFC_HEADER* ptr = &h){
                float* rsptr = Samples;
                IntPtr newSamplesPtr = LibAAFC.aafc_resample_data(rsptr, newSampleRate, ptr, pitch);
                if (newSamplesPtr != IntPtr.Zero) {
                    if (Samples != null)
                        Marshal.FreeHGlobal((IntPtr)Samples);
                    Samples = (float*)newSamplesPtr;
                }
            }
        }

        public void Normalize() {
            fixed (LibAAFC.AAFC_HEADER* ptr = &h){
                float* rsptr = Samples;
                IntPtr newSamplesPtr = LibAAFC.aafc_normalize(rsptr, ptr);
                if (newSamplesPtr != IntPtr.Zero) {
                    if (Samples != null)
                        Marshal.FreeHGlobal((IntPtr)Samples);
                    Samples = (float*)newSamplesPtr;
                }
            }
        }

        public byte[] ToByteSamples()
        {
            byte[] rst = new byte[h.samplelength];
            Marshal.Copy(LibAAFC.aafc_float_to_int(Samples, h.samplelength, 8), rst, 0, (int)h.samplelength);
            return rst;
        }

        public short[] ToShortSamples()
        {
            short[] rst = new short[h.samplelength];
            Marshal.Copy(LibAAFC.aafc_float_to_int(Samples, h.samplelength, 16), rst, 0, (int)h.samplelength);
            return rst;
        }

        public int[] ToIntSamples()
        {
            int[] rst = new int[h.samplelength];
            Marshal.Copy(LibAAFC.aafc_float_to_int(Samples, h.samplelength, 32), rst, 0, (int)h.samplelength);
            return rst;
        }

        public void Dispose() {
            if (!disposed) {
                if (Samples != null) {
                    Marshal.FreeHGlobal((nint)Samples);
                    Samples = null;
                }
                disposed = true;
            }
            GC.SuppressFinalize(this);
        }

        ~AudioClip() => Dispose();
    }
}
