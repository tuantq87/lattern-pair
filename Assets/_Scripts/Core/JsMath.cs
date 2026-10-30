using System;

namespace LanternPair.Core
{
    /// <summary>
    /// The few JavaScript number semantics the HTML5 prototype relies on.
    /// Used wherever Core must reproduce the prototype exactly (same seed, same planet).
    /// </summary>
    public static class JsMath
    {
        private const double TwoPow32 = 4294967296.0;

        /// <summary>
        /// ECMAScript ToUint32, i.e. the `x >>> 0` idiom: truncate, then wrap modulo 2^32.
        /// </summary>
        public static uint ToUint32(double x)
        {
            if (double.IsNaN(x) || double.IsInfinity(x)) return 0;

            double wrapped = Math.Truncate(x) % TwoPow32;
            if (wrapped < 0) wrapped += TwoPow32;
            return (uint)wrapped;
        }

        /// <summary>
        /// JavaScript Math.round: halves round towards +infinity (C# Math.Round rounds them to even).
        /// </summary>
        public static double Round(double x)
        {
            return Math.Floor(x + 0.5);
        }

        /// <summary>
        /// `+x.toFixed(2)` for the prototype's non-negative tuning values.
        /// </summary>
        public static double ToFixed2(double x)
        {
            return Math.Round(x * 100.0, MidpointRounding.AwayFromZero) / 100.0;
        }

        /// <summary>
        /// `(value * count) | 0` for a value in [0, 1): an index in [0, count).
        /// </summary>
        public static int Index(double value, int count)
        {
            return (int)(value * count);
        }
    }
}
