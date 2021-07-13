using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace TimeSeriesCompression
{
    public class ValueDecoder : IEnumerable<double>
    {
        private readonly byte[] _buffer;
        private readonly int _count;

        private enum EncoderState
        {
            Value,
            Delta,
        }

        public ValueDecoder(byte[] buffer, int count)
        {
            _buffer = buffer;
            _count = count;
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(_buffer, _count);
        }

        IEnumerator<double> IEnumerable<double>.GetEnumerator()
            => GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        public struct Enumerator : IEnumerator<double>
        {
            private readonly BitReader _reader;
            private int _count;
            
            private ulong _value;
            private ulong _leadingBits;
            private ulong _trailingBits;
            private EncoderState _state;
            private int _index;

            public double Current => _value.ToDouble();

            object IEnumerator.Current => Current;

            public Enumerator(byte[] buffer, int count)
            {
                _reader = new BitReader(buffer);
                _count = count;
                
                _value = 0;
                _leadingBits = 0;
                _trailingBits = 0;
                _state = EncoderState.Value;
                _index = 0;
            }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                if (_index >= _count)
                {
                    return false;
                }

                switch (_state)
                {
                    case EncoderState.Value:
                        _value = _reader.ReadUInt64();
                        _state = EncoderState.Delta;
                        break;

                    case EncoderState.Delta:
                        if (_reader.TryReadUInt64(0, 1))
                        {
                            // _value is unchanged
                            break;
                        }

                        var a = _reader.ReadUInt64(2);
                        if (a == 0b11)
                        {
                            _leadingBits = _reader.ReadUInt64(5);
                            _trailingBits = _reader.ReadUInt64(6);
                        }
                        
                        var w = (int)(64 - _trailingBits - _leadingBits);
                        var delta = _reader.ReadUInt64(w) << (int)_trailingBits;

                        _value = _value ^ delta;

                        break;
                }

                _index++;
                return true;
            }

            public void Reset()
            {
            }
        }
    }
}
