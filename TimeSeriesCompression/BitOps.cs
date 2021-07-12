using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

[assembly: InternalsVisibleTo("TimeSeriesCompression.Tests")]

namespace TimeSeriesCompression
{
    static internal class BitOps
    {
        public static int TrailingZeroCount(ulong value)
        {
            var b = value & (~value + 1);   // this gives a 1 to the left of the trailing 0's
            b--;              // this gets us just the trailing 1's that need counting
            b = (b & 0x5555_5555_5555_5555) + ((b >> 1) & 0x5555_5555_5555_5555);  // 2 bit sums of 1 bit numbers
            b = (b & 0x3333_3333_3333_3333) + ((b >> 2) & 0x3333_3333_3333_3333);  // 4 bit sums of 2 bit numbers
            b = (b & 0x0f0f_0f0f_0f0f_0f0f) + ((b >> 4) & 0x0f0f_0f0f_0f0f_0f0f);  // 8 bit sums of 4 bit numbers
            b = (b & 0x00ff_00ff_00ff_00ff) + ((b >> 8) & 0x00ff_00ff_00ff_00ff);  // 16 bit sums of 8 bit numbers
            b = (b & 0x0000_ffff_0000_ffff) + ((b >> 16) & 0x0000_ffff_0000_ffff); // sum of 16 bit numbers
            b = (b & 0x0000_0000_ffff_ffff) + ((b >> 32) & 0x0000_0000_ffff_ffff); // sum of 32 bit numbers
            return (int)b;
        }

        public static int LeadingZeroCount(ulong value)
        {
            if (value == 0)
            {
                return 64;
            }

            // Round up to the next highest power of 2
            var v = value - 1;
            v |= v >> 1;
            v |= v >> 2;
            v |= v >> 4;
            v |= v >> 8;
            v |= v >> 16;
            v |= v >> 32;
            v++;
            if ((v & value) == 0)
            {
                return 64 - TrailingZeroCount(v);
            }

            return 63 - TrailingZeroCount(v);
        }

        internal static double ToDouble(this ulong value)
        {
            return Unsafe.As<ulong, double>(ref value);
        }

        internal static ulong ToUInt64(this double value)
        {
            return Unsafe.As<double, ulong>(ref value);
        }
    }
}
