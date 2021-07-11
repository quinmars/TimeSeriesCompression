using System;
using System.Collections.Generic;
using System.Text;

namespace TimeSeriesCompression
{
    public class TimestampEncoder
    {
        private enum EncoderState
        {
            Timestamp,
            Delta,
            DeltaOfDelta
        }

        readonly BitWriter _writer;

        long _lastTicks = 0;
        long _lastDelta = 0;
        long _currentOffset = 0;

        EncoderState _state = EncoderState.Timestamp;

        public IEnumerable<ulong> Dwords => _writer.Dwords;
        public int ByteCount => _writer.ByteCount;

        public TimestampEncoder(int capacity)
        {
            _writer = new BitWriter(capacity);
        }

        public void Add(DateTimeOffset dt)
        {
            if (_state != EncoderState.Timestamp && dt.Offset.Ticks != _currentOffset)
            {
                _writer.WriteUInt64(0b1_1111_1110, 9);
                _state = EncoderState.Timestamp;
            }

            switch (_state)
            {
                case EncoderState.Timestamp:
                    _writer.WriteCompressedTimestamp(dt);
                    _currentOffset = dt.Offset.Ticks;
                    _state = EncoderState.Delta;
                    break;

                case EncoderState.Delta:
                    _lastDelta = dt.Ticks - _lastTicks;
                    _writer.WriteCompressedInt64(_lastDelta);
                    _state = EncoderState.DeltaOfDelta;
                    break;

                case EncoderState.DeltaOfDelta:
                    var delta = dt.Ticks - _lastTicks;
                    _writer.WriteCompressedInt64(delta - _lastDelta);
                    _lastDelta = delta;
                    break;
            }

            _lastTicks = dt.Ticks;
        }

        public void Encode(Span<byte> buffer)
        {
            _writer.Encode(buffer);
        }
    }
}
