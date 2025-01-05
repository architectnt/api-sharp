/*
    Copyright (C) 2025 Architect Enterprises
    This file is apart of the API and are MIT licensed
*/

// Alternative to System.Random
namespace ArchitectAPI.Internal
{
    public static class RandomProviders
    {
        /// <summary>
        /// The IHaveNoIdea of all time
        /// </summary>
        /// <returns>A seed</returns>
        public static ulong GenerateRandomSeed()
        {
            ulong iHaveNoIdea = (ulong)System.Diagnostics.Stopwatch.GetTimestamp();
            iHaveNoIdea ^= (iHaveNoIdea << 13) | (iHaveNoIdea >> 51);
            iHaveNoIdea ^= (iHaveNoIdea >> 7) | (iHaveNoIdea << 57);
            iHaveNoIdea ^= (iHaveNoIdea << 17) | (iHaveNoIdea >> 47);
            iHaveNoIdea ^= (iHaveNoIdea >> 11) | (iHaveNoIdea << 53);
            iHaveNoIdea ^= (iHaveNoIdea << 31);
            iHaveNoIdea ^= (iHaveNoIdea >> 29);
            iHaveNoIdea ^= 0xA5A5A5A5A5A5A5A5;
            iHaveNoIdea ^= (iHaveNoIdea << 43) | (iHaveNoIdea >> 21);
            return iHaveNoIdea;
        }

        public static ulong[] SplitMix64(ulong seed, uint states)
        {
            ulong[] r = new ulong[states];
            ulong s = seed;

            for (uint i = 0; i < states; i++)
            {
                s += 0x9E3779B97f4A7C15UL;
                ulong tmp = (s ^ (s >> 30)) * 0xBF58476D1CE4E5B9UL;
                tmp = (tmp ^ (tmp >> 27)) * 0x94D049BB133111EBUL;
                r[i] = tmp ^ (tmp >> 31);
            }
            return r;
        }

        public static void InitializeAll()
        {
            RandomPCG.Initialize();
            Random256.Initialize();
            Random128.Initialize();
            RandomLFSR.Initialize();
        }
    }

    /// <summary>
    /// Architect Enterprises - Xoshiro256** Random Number Generater
    /// </summary>
    public static class Random256
    {
        static ulong x, y, z, w;
        public static ulong Value
        {
            get
            {
                ulong rst = Rotl(y * 5, 7) * 9;
                ulong t = y << 17;
                z ^= x; w ^= y; y ^= z; x ^= w; z ^= t;
                w = Rotl(w, 45);

                return rst;
            }
        }

        /// <summary>
        /// Creates a new RNG instance
        /// </summary>
        /// <param name="seed">Specifies a custom seed</param>
        public static void Initialize(ulong seed = 0)
        {
            if (seed == 0) seed = RandomProviders.GenerateRandomSeed();
            ulong[] s = RandomProviders.SplitMix64(seed, 4);
            x = s[0]; y = s[1]; z = s[2]; w = s[3];
        }

        static ulong Rotl(ulong x, int k)
            => (x << k) | (x >> (64 - k));

        public static uint Next32()
            => (uint)(Value >> 32);

        public static int Range(int min, int max)
            => (int)(Value % ((ulong)(max - min))) + min;

        public static float Range(float min, float max)
            => (float)((Value / (double)ulong.MaxValue) * (max - min) + min);

        public static int Range(int max)
            => (int)(Value % ((ulong)max));

        public static float Range(float max)
            => (float)(Value / (double)ulong.MaxValue) * max;

        public static bool InChance(float c)
            => (float)((Value / (double)ulong.MaxValue) * 100f) < c;

        public static float RandomFloat()
            => (float)(Value / (double)ulong.MaxValue);

        public static float Normalized()
            => (float)(Value / (double)ulong.MaxValue) * 2.0f - 1.0f;

        public static double RandomDouble()
            => Value / (double)ulong.MaxValue;
    }

    /// <summary>
    /// Architect Enterprises - Xorshift128 32-bit Random Number Generater
    /// </summary>
    public static class Random128
    {
        static uint x, y, z, w;
        public static uint Value
        {
            get
            {
                uint t = x ^ (x << 11);
                x = y; y = z; z = w;
                return w = w ^ (w >> 19) ^ (t ^ (t >> 8));
            }
        }

        /// <summary>
        /// Creates a new RNG instance
        /// </summary>
        /// <param name="seed">Specifies a custom seed</param>
        public static void Initialize(uint seed = 0)
        {
            ulong[] s = RandomProviders.SplitMix64(seed != 0 ? seed : RandomProviders.GenerateRandomSeed(), 4);
            x = (uint)s[0]; y = (uint)(s[1] >> 32); z = (uint)s[2]; w = (uint)(s[3] >> 32);
        }

        public static int Range(int min, int max)
            => (int)(Value % ((uint)(max - min))) + min;

        public static float Range(float min, float max)
            => (Value / (float)uint.MaxValue) * (max - min) + min;

        public static int Range(int max)
            => (int)(Value % ((uint)max));

        public static float Range(float max)
            => (Value / (float)uint.MaxValue) * max;

        public static bool InChance(float c)
            => ((Value / (float)uint.MaxValue) * 100f) < c;

        public static float RandomFloat()
            => Value / (float)uint.MaxValue;

        public static float Normalized()
            => (Value / (float)uint.MaxValue) * 2.0f - 1.0f;

        public static double RandomDouble()
            => Value / (double)uint.MaxValue;
    }

    /// <summary>
    /// Architect Enterprises - Basic 64 bit LFSR Random Number Generater
    /// </summary>

    public static class RandomLFSR
    {
        static ulong rngn;
        public static ulong Value
        {
            get
            {
                ulong b = (rngn >> 63) ^ (rngn >> 62) ^ (rngn >> 60) ^ (rngn >> 59);
                rngn <<= 1;
                rngn |= (~b) & 1;
                return rngn;
            }
        }

        public static void Initialize(ulong seed = 0)
        {
            rngn = seed == 0 ? RandomProviders.GenerateRandomSeed() : seed;
            if (rngn == 0) rngn = 1;
        }

        public static int Range(int min, int max)
            => (int)(Value % ((ulong)(max - min))) + min;

        public static float Range(float min, float max)
            => (float)((Value / (double)ulong.MaxValue) * (max - min) + min);

        public static int Range(int max)
            => (int)(Value % ((ulong)max));

        public static float Range(float max)
            => (float)(Value / (double)ulong.MaxValue) * max;

        public static bool InChance(float c)
            => (float)((Value / (double)ulong.MaxValue) * 100f) < c;

        public static float RandomFloat()
            => (float)(Value / (double)ulong.MaxValue);

        public static float Normalized()
            => (float)(Value / (double)ulong.MaxValue) * 2.0f - 1.0f;

        public static double RandomDouble()
            => Value / (double)ulong.MaxValue;
    }



    public static class RandomPCG
    {
        static ulong s;
        static ulong i;
        public static ulong Value
        {
            get
            {
                ulong odst = s;
                s = odst * 6364136223846793005UL + (i | 1);
                ulong xshft = ((odst >> 18) ^ odst) >> 27;
                ulong r = odst >> 59;
                return (xshft >> (int)r) | (xshft << ((-(int)r) & 31));
            }
        }

        public static void Initialize(ulong seed = 0, ulong ic = 0)
        {
            if (seed == 0) seed = RandomProviders.GenerateRandomSeed();
            if (ic == 0) ic = RandomProviders.GenerateRandomSeed() | 1;
            s = 0U;
            i = (ic << 1) | 1;
            Next();
            s += seed;
            Next();
        }

        /* 
            because we cant basically do what it does in c we're limited by pointless casts to make roslyn happy
            beware casting overhead
        */
        static ulong Next() => Value;

        public static int Range(int min, int max)
            => (int)(Value % ((ulong)(max - min))) + min;

        public static float Range(float min, float max)
            => (float)((Value / (double)ulong.MaxValue) * (max - min)) + min;

        public static int Range(int max)
            => (int)(Value % ((ulong)max));

        public static float Range(float max)
            => (float)(Value / (double)ulong.MaxValue) * max;

        public static bool InChance(float c)
            => (float)((Value / (double)ulong.MaxValue) * 100f) < c;

        public static float RandomFloat()
            => (float)(Value / (double)ulong.MaxValue);

        public static float Normalized()
            => (float)(Value / (double)ulong.MaxValue) * 2.0f - 1.0f;

        public static double RandomDouble()
            => Value / (double)ulong.MaxValue;
    }
}
