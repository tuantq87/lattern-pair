namespace LanternPair.Core.Rng
{
    /// <summary>
    /// mulberry32 PRNG, bit-for-bit identical to the prototype's `mulberry32(a)`,
    /// so a seed typed on the web build produces the same run here.
    /// </summary>
    public sealed class Mulberry32
    {
        private int state;

        public Mulberry32(uint seed)
        {
            state = unchecked((int)seed);
        }

        /// <summary>
        /// Next value in [0, 1), the same as one call of the JavaScript closure.
        /// </summary>
        public double Next()
        {
            unchecked
            {
                state += 0x6D2B79F5;
                int t = (state ^ (int)((uint)state >> 15)) * (1 | state);
                t = (t + ((t ^ (int)((uint)t >> 7)) * (61 | t))) ^ t;
                return (uint)(t ^ (int)((uint)t >> 14)) / 4294967296.0;
            }
        }

        /// <summary>
        /// `(r() * count) | 0`: an index in [0, count).
        /// </summary>
        public int NextIndex(int count)
        {
            return JsMath.Index(Next(), count);
        }
    }
}
