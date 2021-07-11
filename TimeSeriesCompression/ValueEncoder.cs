using System;

namespace TimeSeriesCompression
{
    public class ValueEncoder
    {
        private enum EncoderState
        {
            Value,
            FirstDelta,
            Delta,
        }

        readonly BitWriter _writer;

        public int ByteCount => _writer.ByteCount;

        EncoderState _state = EncoderState.Value;
        ulong _lastValue = 0;
        int _lastLeadingBits = 65;
        int _lastTrailingBits = 65;

        public ValueEncoder(int capacity)
        {
            _writer = new BitWriter(capacity);
        }

        public void Add(double val)
        {
            ulong value = val.ToUInt64();

            switch (_state)
            {
                case EncoderState.Value:
                    _writer.WriteUInt64(value);
                    _state = EncoderState.Delta;
                    break;

                case EncoderState.Delta:
                    var delta = _lastValue ^ value;

                    if (delta == 0)
                    {
                        _writer.WriteUInt64(0, 1);
                        break;
                    }

                    var leadingBits = Math.Min(BitOps.LeadingZeroCount(delta), 31);
                    var trailingBits = BitOps.TrailingZeroCount(delta);

                    if (leadingBits == _lastLeadingBits && trailingBits == _lastTrailingBits)
                    {
                        _writer.WriteUInt64(0b10, 2);
                    }
                    else
                    {
                        _writer.WriteUInt64(0b11, 2);
                        _writer.WriteUInt64((ulong)leadingBits, 5);
                        _writer.WriteUInt64((ulong)trailingBits, 6);
                    }

                    var w = 64 - trailingBits - leadingBits;
                    _writer.WriteUInt64(delta >> trailingBits, w);

                    break;
            }

            _lastValue = value;
        }

        public void Encode(Span<byte> buffer)
        {
            _writer.Encode(buffer);
        }
    }
}
