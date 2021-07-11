using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace TimeSeriesCompression
{
    public static class ValueCompression
    {
        private enum EncoderState
        {
            Value,
            Delta,
        }

        public static double[] Decode(byte[] buffer, int count)
        {
            var array = new double[count];
            var reader = new BitReader(buffer);

            ulong value = 0;
            ulong lastValue = 0;
            ulong leadingBits = 0;
            ulong trailingBits = 0;

            var state = EncoderState.Value;

            for (int i = 0; i < count; i++)
            {
                switch (state)
                {
                    case EncoderState.Value:
                        value = reader.ReadUInt64();
                        state = EncoderState.Delta;
                        break;

                    case EncoderState.Delta:
                        if (reader.TryGetUInt64(0, 1))
                        {
                            value = lastValue;
                            break;
                        }

                        var a = reader.ReadUInt64(2);
                        if (a == 0b11)
                        {
                            leadingBits = reader.ReadUInt64(5);
                            trailingBits = reader.ReadUInt64(6);
                        }
                        
                        var w = (int)(64 - trailingBits - leadingBits);
                        var delta = reader.ReadUInt64(w) << (int)trailingBits;

                        value = lastValue ^ delta;

                        break;
                }

                array[i] = value.ToDouble();
                lastValue = value;
            }

            return array;
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
