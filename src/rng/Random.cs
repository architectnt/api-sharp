using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

// Alternative to System.Random
namespace ArchitectAPI.Internal
{
    /// <summary>
    /// Architect Enterprises - Xoshiro256** 64-bit Random Number Generater (Slower)
    /// </summary>
    public unsafe static class Random256
    {
        static ulong* s;

        /// <summary>
        /// Creates a new RNG instance
        /// </summary>
        /// <param name="seed">Specifies a custom seed</param>
        public static void Intialize(ulong seed = 0)
        {
            s = (ulong*)Marshal.AllocHGlobal(4 * sizeof(ulong));
            byte* seedBytes = stackalloc byte[32];

            if (seed == 0)
            {
                using RandomNumberGenerator rng = RandomNumberGenerator.Create();
                rng.GetNonZeroBytes(new Span<byte>(seedBytes, 32));
            }
            else
            {
                for (int i = 0; i < 32; i += 8)
                {
                    ulong* p = (ulong*)(seedBytes + i);
                    *p = seed;
                }
            }

            for (int i = 0; i < 4; i++)
            {
                *(s + i) = *((ulong*)(seedBytes + i * 8));
            }

            if (s[0] == 0 && s[1] == 0 && s[2] == 0 && s[3] == 0)
            {
                throw new InvalidOperationException("RNG states all at zero.");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static ulong Rotl(ulong x, int k)
        {
            return (x << k) | (x >> (64 - k));
        }

        public static ulong NextUInt64()
        {
            ulong rst = Rotl(*(s + 1) * 5, 7) * 9;

            ulong t = *(s + 1) << 17;

            *(s + 2) ^= *s;
            *(s + 3) ^= *(s + 1);
            *(s + 1) ^= *(s + 2);
            *s ^= *(s + 3);

            *(s + 2) ^= t;

            *(s + 3) = Rotl(*(s + 3), 45);

            return rst;
        }

        public static uint NextUInt32()
        {
            return (uint)(NextUInt64() >> 32);
        }

        public static int Range(int min, int max)
        {
            return min + (int)(NextUInt64() % ((ulong)(max - min)));
        }

        public static float Range(float min, float max)
        {
            return min + ((NextUInt64() / (float)ulong.MaxValue) * (max - min));
        }

        public static int Range(int max)
        {
            return 0 + (int)(NextUInt64() % ((ulong)(max - 0)));
        }

        public static float Range(float max)
        {
            return 0.0f + ((NextUInt64() / (float)ulong.MaxValue) * (max - 0));
        }

        public static bool InChance(float c)
        {
            return (0.0f + ((NextUInt64() / (float)ulong.MaxValue) * (100f - 0f))) < c;
        }

        public static float RandomFloat()
        {
            return NextUInt64() / (float)ulong.MaxValue;
        }

        public static double RandomDouble()
        {
            return NextUInt64() / (double)ulong.MaxValue;
        }
    }

    /// <summary>
    /// Architect Enterprises - Xorshift128 32-bit Random Number Generater (Fast)
    /// </summary>
    public unsafe static class Random128
    {
        static uint x, y, z, w;

        /// <summary>
        /// Creates a new RNG instance
        /// </summary>
        /// <param name="seed">Specifies a custom seed</param>
        public static void Intialize(uint seed = 0)
        {
            byte* seedBytes = stackalloc byte[16];

            if (seed == 0)
            {
                using RandomNumberGenerator rng = RandomNumberGenerator.Create();
                rng.GetNonZeroBytes(new Span<byte>(seedBytes, 16));
            }
            else
            {
                for (int i = 0; i < 16; i += 8)
                {
                    uint* p = (uint*)(seedBytes + i);
                    *p = seed;
                }
            }

            x = *((uint*)seedBytes);
            y = *((uint*)(seedBytes + 4));
            z = *((uint*)(seedBytes + 8));
            w = *((uint*)(seedBytes + 12));
        }

        public static uint NextUInt32()
        {
            uint t = x ^ (x << 11);
            x = y; y = z; z = w;
            return w = w ^ (w >> 19) ^ (t ^ (t >> 8));
        }

        public static int Range(int min, int max)
        {
            return min + (int)(NextUInt32() % ((uint)(max - min)));
        }

        public static float Range(float min, float max)
        {
            return min + ((NextUInt32() / (float)uint.MaxValue) * (max - min));
        }

        public static int Range(int max)
        {
            return 0 + (int)(NextUInt32() % ((uint)(max - 0)));
        }

        public static float Range(float max)
        {
            return 0.0f + ((NextUInt32() / (float)uint.MaxValue) * (max - 0));
        }

        public static bool InChance(float c)
        {
            return (0.0f + ((NextUInt32() / (float)uint.MaxValue) * (100f - 0f))) < c;
        }

        public static float RandomFloat()
        {
            return NextUInt32() / (float)uint.MaxValue;
        }

        public static double RandomDouble()
        {
            return NextUInt32() / (double)uint.MaxValue;
        }
    }
}
