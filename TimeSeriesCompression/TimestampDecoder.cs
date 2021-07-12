using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace TimeSeriesCompression
{
    public class TimestampDecoder : IEnumerable<DateTimeOffset>
    {
        private readonly byte[] _buffer;
        private readonly int _count;

        private enum EncoderState
        {
            Timestamp,
            Delta,
            DeltaOfDelta
        }

        public TimestampDecoder(byte[] buffer, int count)
        {
            _buffer = buffer;
            _count = count;
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(_buffer, _count);
        }

        IEnumerator<DateTimeOffset> IEnumerable<DateTimeOffset>.GetEnumerator()
            => GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        public struct Enumerator : IEnumerator<DateTimeOffset>
        {
            private readonly BitReader _reader;
            private int _count;
            
            private long _currentTicks;
            private long _currentOffset;
            private long _delta;

            private EncoderState _state;
            private int _index;

            public DateTimeOffset Current => new DateTimeOffset(_currentTicks, new TimeSpan(_currentOffset)); 

            object IEnumerator.Current => Current;

            public Enumerator(byte[] buffer, int count)
            {
                _reader = new BitReader(buffer);
                _count = count;
                
                _currentTicks = 0;
                _currentOffset = 0;
                _delta = 0;
                _state = EncoderState.Timestamp;
                _index = 0;
            }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                long ticks = 0;

                if (_index >= _count)
                {
                    return false;
                }

                if (_state != EncoderState.Timestamp && _reader.TryGetUInt64(0b1_1111_1110, 9))
                {
                    _state = EncoderState.Timestamp;
                }

                switch (_state)
                {
                    case EncoderState.Timestamp:
                        ticks = _reader.ReadInt64();
                        _currentOffset = _reader.ReadOffset();
                        _state = EncoderState.Delta;
                        break;

                    case EncoderState.Delta:
                        _delta = _reader.ReadCompressedInt64();
                        ticks = _currentTicks + _delta;
                        _state = EncoderState.DeltaOfDelta;
                        break;

                    case EncoderState.DeltaOfDelta:
                        var deltaOfDelta = _reader.ReadCompressedInt64();
                        _delta = _delta + deltaOfDelta;
                        ticks = _currentTicks + _delta;
                        break;
                }

                _currentTicks = ticks;
                _index++;
                return true;
            }

            public void Reset()
            {
            }
        }
    }
}
