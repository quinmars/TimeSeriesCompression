using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Xunit;

namespace TimeSeriesCompression.Tests
{
    public class ValueTests
    {
        public static double[] SingleValue { get; } = new[]
        {
            13.2
        };

        public static double[] TwoValues { get; } = new[]
        {
            13.2,
            13.0
        };

        public static double[] ThreeValues { get; } = new[]
        {
            13.2,
            13.0,
            13.2
        };
        
        public static double[] ZeroDeltaValues { get; } = new[]
        {
            13.2,
            13.2,
            13.2,
            13.2,
            13.2,
            13.2,
            13.2,
            13.2,
            13.2,
        };
        
        public static double[] IntegerValues { get; } = new[]
        {
            13.0,
            14,
            15,
            16,
            20,
            100,
            2,
            1234,
            14,
        };

        public static IEnumerable<object[]> Values { get; } = new[]
        {
            SingleValue,
            TwoValues,
            ThreeValues,
            ZeroDeltaValues,
            IntegerValues
        }
        .Select(s => new[] { s });

        [MemberData(nameof(Values))]
        [Theory]
        public void CompressionIdenity(double[] value)
        {
            var encoder = new ValueEncoder(value.Length);
            var count = value.Length;

            foreach (var dt in value)
            {
                encoder.Add(dt);
            }

            var buffer = new byte[encoder.ByteCount];
            encoder.Encode(buffer);

            var decoder = new ValueDecoder(buffer, count);

            decoder.Should()
                .Equal(value);
        }

        [InlineData(0xffff_ffff_ffff_ffffUL)]
        [InlineData(0xffff_ffff_ffff_fff3UL)]
        [InlineData(0xffff_ffff_ffff_ff33UL)]
        [InlineData(0xffff_ffff_ffff_ff00UL)]
        [InlineData(0xffff_fff0_0000_0000UL)]
        [InlineData(0x3000_0000_0000_0000UL)]
        [InlineData(0x0002_0000_0000_0000UL)]
        [InlineData(0UL)]
        [Theory]
        public void TrailingZeroCount(ulong value)
        {
            var expect = BitOperations.TrailingZeroCount(value);

            BitOps.TrailingZeroCount(value)
                .Should().Be(expect);
        }
        
        [InlineData(0x0fff_ffff_ffff_ffffUL)]
        [InlineData(0xf0ff_ffff_ffff_fff3UL)]
        [InlineData(0x00ff_ffff_ffff_ff33UL)]
        [InlineData(0x000f_ffff_ffff_ff00UL)]
        [InlineData(0x0000_fff0_0000_0000UL)]
        [InlineData(0x0002_0000_0000_0000UL)]
        [InlineData(0UL)]
        [Theory]
        public void LeadingZeroCount(ulong value)
        {
            var expect = BitOperations.LeadingZeroCount(value);

            BitOps.LeadingZeroCount(value)
                .Should().Be(expect);
        }
    }
}
