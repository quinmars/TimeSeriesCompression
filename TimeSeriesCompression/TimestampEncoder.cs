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
        long _lastOffset = 0;

        EncoderState _state = EncoderState.Timestamp;

        public IEnumerable<ulong> Dwords => _writer.Dwords;
        public int ByteCount => _writer.ByteCount;

        public TimestampEncoder(int capacity)
        {
            _writer = new BitWriter(capacity);
        }

        public void Add(DateTimeOffset d)
        {
            var ticks = d.Ticks;
            var offset = d.Offset.Ticks;

            if (_state != EncoderState.Timestamp && offset != _lastOffset)
            {
                _writer.WriteUInt64(0b1_1111_1110, 9);
                _state = EncoderState.Timestamp;
            }

            switch (_state)
            {
                case EncoderState.Timestamp:
                    _writer.WriteUInt64((ulong)ticks);
                    _writer.WriteOffset(offset);

                    _lastOffset = offset;
                    _state = EncoderState.Delta;
                    break;

                case EncoderState.Delta:
                    _lastDelta = ticks - _lastTicks;
                    _writer.WriteCompressedInt64(_lastDelta);
                    _state = EncoderState.DeltaOfDelta;
                    break;

                case EncoderState.DeltaOfDelta:
                    var delta = ticks - _lastTicks;
                    _writer.WriteCompressedInt64(delta - _lastDelta);
                    _lastDelta = delta;
                    break;
            }

            _lastTicks = ticks;
        }

        public void Encode(Span<byte> buffer)
        {
            _writer.Encode(buffer);
        }
    }
}
