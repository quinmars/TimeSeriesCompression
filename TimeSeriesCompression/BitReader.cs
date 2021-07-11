using System;
using System.Collections.Generic;
using System.Text;

namespace TimeSeriesCompression
{
    internal class BitReader
    {
        const int DwordBitCount = 64;
        readonly private List<ulong> _dwords = new List<ulong>();
 
        int _currentBitIndex;

        public List<ulong> Dwords => _dwords;
        public int ByteCount => (_currentBitIndex + 7) / 8;

        public BitReader(byte[] source)
        {
            for (int i = 0; i < source.Length; i += 8)
            {
                ulong v = 0;
                var min = Math.Min(source.Length, i + 8);
                
                for (int j = i; j < min; j++)
                {
                    var k = 56 - 8 * (j % 8);

                    var b = (ulong)(source[j]) << k;
                    v |= b;
                }

                _dwords.Add(v);
            }
        }

        internal ulong ReadUInt64()
        {
            return ReadUInt64(DwordBitCount);
        }

        internal bool TryGetUInt64(ulong val, int width)
        {
            if (PeakUInt64(width) == val)
            {
                _currentBitIndex += width;
                return true;
            }
            else
            {
                return false;
            }

        }

        internal long ReadInt64(int width)
        {
            long val;
            var invWidth = DwordBitCount - width;
            
            var offset = _currentBitIndex % DwordBitCount;
            var index = _currentBitIndex / DwordBitCount;

            if (offset == 0)
            {
                val = (long)_dwords[index];
            }
            else
            {
                val = (long)(_dwords[index] << offset);

                if (offset + width >= DwordBitCount && index + 1 < _dwords.Count)
                {
                    val |= (long)(_dwords[index + 1] >> (DwordBitCount - offset));
                }
            }
            _currentBitIndex += width;
            return val >> invWidth;
        }

        internal long ReadInt64()
        {
            long val;
            
            var offset = _currentBitIndex % DwordBitCount;
            var index = _currentBitIndex / DwordBitCount;

            if (offset == 0)
            {
                val = (long)_dwords[index];
            }
            else
            {
                val = (long)(_dwords[index] << offset);

                if (index + 1 < _dwords.Count)
                {
                    val |= (long)(_dwords[index + 1] >> (DwordBitCount - offset));
                }
            }
            _currentBitIndex += DwordBitCount;
            return val;
        }

        internal ulong ReadUInt64(int width)
        {
            var v = PeakUInt64(width);
            _currentBitIndex += width;
            return v;
        }

        internal ulong PeakUInt64(int width)
        {
            ulong val;
            var invWidth = DwordBitCount - width;
            
            var offset = _currentBitIndex % DwordBitCount;
            var index = _currentBitIndex / DwordBitCount;

            if (offset == 0)
            {
                val = _dwords[index];
            }
            else
            {
                val = _dwords[index] << offset;

                if (offset + width >= DwordBitCount && index + 1 < _dwords.Count)
                {
                    val |= _dwords[index + 1] >> (DwordBitCount - offset);
                }
            }
            return val >> invWidth;
        }
    }
}
