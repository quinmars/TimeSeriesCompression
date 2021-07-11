using System;
using System.Collections.Generic;
using System.Text;

namespace TimeSeriesCompression
{
    internal class BitWriter
    {
        const int DwordBitCount = 64;
        readonly private List<ulong> _dwords;

        int _currentBitIndex;

        public List<ulong> Dwords => _dwords;
        public int ByteCount => (_currentBitIndex + 7) / 8;

        public BitWriter(int capacity)
        {
            _currentBitIndex = 0;
            _dwords = new List<ulong>(capacity);
        }

        internal void WriteUInt64(ulong val)
        {

            var offset = _currentBitIndex % DwordBitCount;

            if (offset == 0)
            {
                _dwords.Add(val);
            }
            else
            {
                var upper = val >> offset;
                var i = _dwords.Count - 1;
                _dwords[i] = _dwords[i] | upper;
                
                var lower = val << (DwordBitCount - offset);
                _dwords.Add(lower);
            }

            _currentBitIndex += DwordBitCount;
        }
        
        internal void WriteUInt64(ulong val, int width)
        {
            var invWidth = DwordBitCount - width;
            val <<= invWidth;

            var offset = _currentBitIndex % DwordBitCount;

            if (offset == 0)
            {
                _dwords.Add(val);
            }
            else
            {
                var upper = val >> offset;
                var i = _dwords.Count - 1;
                _dwords[i] = _dwords[i] | upper;
               
                if (offset + width > DwordBitCount)
                {
                    var lower = val << (DwordBitCount - offset);
                    _dwords.Add(lower);
                }
            }

            _currentBitIndex += width;
        }

        internal void Encode(Span<byte> buffer)
        {
            var count = Math.Min(ByteCount, buffer.Length);
            var dwords = Dwords;

            for (int i = 0; i < count; i++)
            {
                var j = i / 8;
                var k = 56 - 8*(i % 8);

                buffer[i] = (byte)(dwords[j] >> k);
            }
        }
    }
}
