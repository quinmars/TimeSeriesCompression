using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace TimeSeriesCompression.Tests
{
    public class TimestampTests
    {
        public static DateTimeOffset[] SingleTimestamp { get; } = new[]
        {
            DateTimeOffset.Parse("2011-06-07T11:00:00.5233212+01:00")
        };

        public static DateTimeOffset[] TwoTimestamps { get; } = new[]
        {
            DateTimeOffset.Parse("2011-06-07T11:00:00.5233212+01:00"),
            DateTimeOffset.Parse("2011-06-07T11:01:00.5231212+01:00"),
        };

        public static DateTimeOffset[] ThreeTimestamps { get; } = new[]
        {
            DateTimeOffset.Parse("2011-06-07T11:00:00.5233212+01:00"),
            DateTimeOffset.Parse("2011-06-07T11:01:00.5231212+01:00"),
            DateTimeOffset.Parse("2011-06-07T11:02:00.5234212+01:00"),
        };

        public static DateTimeOffset[] ZeroDeltaOfDeltaTimestamps { get; } = new[]
        {
            DateTimeOffset.Parse("2011-06-07T11:00:00+00:00"),
            DateTimeOffset.Parse("2011-06-07T11:00:01+00:00"),
            DateTimeOffset.Parse("2011-06-07T11:00:02+00:00"),
            DateTimeOffset.Parse("2011-06-07T11:00:03+00:00"),
            DateTimeOffset.Parse("2011-06-07T11:00:04+00:00"),
        };
        
        public static DateTimeOffset[] ZeroDeltaTimestamps { get; } = new[]
        {
            DateTimeOffset.Parse("2011-06-07T11:00:00+00:00"),
            DateTimeOffset.Parse("2011-06-07T11:00:00+00:00"),
            DateTimeOffset.Parse("2011-06-07T11:00:00+00:00"),
            DateTimeOffset.Parse("2011-06-07T11:00:00+00:00"),
            DateTimeOffset.Parse("2011-06-07T11:00:00+00:00"),
            DateTimeOffset.Parse("2011-06-07T11:00:00+00:00"),
            DateTimeOffset.Parse("2011-06-07T11:00:00+00:00"),
            DateTimeOffset.Parse("2011-06-07T11:00:00+00:00"),
        };

        public static DateTimeOffset[] JumpingOffsetTimestamps { get; } = new[]
        {
            DateTimeOffset.Parse("2011-06-07T11:00:00+00:00"),
            DateTimeOffset.Parse("2011-06-07T11:00:01+01:00"),
            DateTimeOffset.Parse("2011-06-07T11:00:02+00:00"),
            DateTimeOffset.Parse("2011-06-07T11:00:03+00:00"),
            DateTimeOffset.Parse("2011-06-07T11:00:04+00:00"),
            DateTimeOffset.Parse("2011-06-07T11:00:05+01:00"),
            DateTimeOffset.Parse("2011-06-07T11:00:06+00:00"),
            DateTimeOffset.Parse("2011-06-07T11:00:07+00:00"),
        };
        
        public static DateTimeOffset[] CETTimestamps { get; } = new[]
        {
            DateTimeOffset.Parse("2011-06-07T11:00:00+01:00"),
            DateTimeOffset.Parse("2011-06-07T11:00:01+01:00"),
            DateTimeOffset.Parse("2011-06-07T11:00:02+01:00"),
            DateTimeOffset.Parse("2011-06-07T11:00:03+01:00"),
            DateTimeOffset.Parse("2011-06-07T11:00:04+01:00"),
            DateTimeOffset.Parse("2011-06-07T11:00:05+01:00"),
            DateTimeOffset.Parse("2011-06-07T11:00:06+01:00"),
            DateTimeOffset.Parse("2011-06-07T11:00:07+01:00"),
        };

        public static DateTimeOffset[] NegativeDeltaOfDeltaTimestamps { get; } = new[]
        {
            DateTimeOffset.Parse("2011-06-07T11:00:00.95+00:00"),
            DateTimeOffset.Parse("2011-06-07T11:00:01.9+00:00"),
            DateTimeOffset.Parse("2011-06-07T11:00:02.7+00:00"),
            DateTimeOffset.Parse("2011-06-07T11:00:03.4+00:00"),
            DateTimeOffset.Parse("2011-06-07T11:00:04.0+00:00"),
        };
        
        public static DateTimeOffset[] IrregularTimestamps { get; } = new[]
        {
            DateTimeOffset.Parse("2011-06-07T11:00:00.5233212+01:00"),
            DateTimeOffset.Parse("2011-06-07T11:00:00.5233213+01:00"),
            DateTimeOffset.Parse("2011-06-07T11:00:00.5233214+01:00"),
            DateTimeOffset.Parse("2011-06-07T11:00:00.5233214+01:00"),
            DateTimeOffset.Parse("2011-06-07T11:00:00.5243214+01:00"),
            DateTimeOffset.Parse("2011-06-07T11:00:00.6243214+01:00"),
            DateTimeOffset.Parse("2011-06-07T11:00:01.1243214+01:00"),
            DateTimeOffset.Parse("2011-06-07T11:00:01.1243554+01:00"),
            DateTimeOffset.Parse("2011-06-07T11:00:02.1253554+01:00"),
            DateTimeOffset.Parse("2011-06-07T11:00:06.1254554+01:00"),
            DateTimeOffset.Parse("2011-06-07T11:01:01.1277554+01:00"),
            DateTimeOffset.Parse("2011-06-07T11:02:01.5277554+01:00"),
            DateTimeOffset.Parse("2011-06-07T11:02:12.1377554+01:00"),
            DateTimeOffset.Parse("2011-06-07T11:06:01.1277554+01:00"),
        };

        public static DateTimeOffset[] RandomDeltaTimestamps { get; } = new[]
        {
            DateTimeOffset.Parse("1900-06-07T11:00:00.5233212+00:00"),
            DateTimeOffset.Parse("1920-01-21T18:10:23.212+00:00"),
            DateTimeOffset.Parse("2000-12-12T12:12:12.1212122+00:00"),
            DateTimeOffset.Parse("2000-12-12T12:12:12.1212124+00:00"),
            DateTimeOffset.Parse("2000-12-12T12:12:12.1212128+00:00"),
            DateTimeOffset.Parse("2000-12-12T12:12:12.1212140+00:00"),
            DateTimeOffset.Parse("2000-12-12T12:12:12.1212400+00:00"),
            DateTimeOffset.Parse("2000-12-12T12:12:12.1214000+00:00"),
            DateTimeOffset.Parse("2000-12-12T12:12:12.1240000+00:00"),
            DateTimeOffset.Parse("2000-12-12T12:12:12.1400000+00:00"),
            DateTimeOffset.Parse("2000-12-12T12:12:12.4000000+00:00"),
            DateTimeOffset.Parse("2000-12-12T12:12:14.0000000+00:00"),
            DateTimeOffset.Parse("2000-12-12T12:12:40.0000000+00:00"),
            DateTimeOffset.Parse("2000-12-12T12:14:00.0000000+00:00"),
            DateTimeOffset.Parse("2000-12-12T12:40:00.0000000+00:00"),
            DateTimeOffset.Parse("2000-12-12T14:00:00.0000000+00:00"),
            DateTimeOffset.Parse("2000-12-12T20:00:00.0000000+00:00"),
            DateTimeOffset.Parse("2000-12-14T20:00:00.0000000+00:00"),
            DateTimeOffset.Parse("2000-12-22T20:00:00.0000000+00:00"),
            DateTimeOffset.Parse("2001-01-22T20:00:00.0000000+00:00"),
            DateTimeOffset.Parse("2002-01-22T20:00:00.0000000+00:00"),
            DateTimeOffset.Parse("2020-01-22T20:00:00.0000000+00:00"),
            DateTimeOffset.Parse("2040-01-22T20:00:00.0000000+00:00"),
            DateTimeOffset.Parse("2400-01-22T20:00:00.0000000+00:00"),
            DateTimeOffset.Parse("4000-01-22T20:00:00.0000000+00:00"),
        };


        public static IEnumerable<object[]> Timestamps { get; } = new[]
        {
            SingleTimestamp,
            TwoTimestamps,
            ThreeTimestamps,
            ZeroDeltaTimestamps,
            ZeroDeltaOfDeltaTimestamps,
            JumpingOffsetTimestamps,
            NegativeDeltaOfDeltaTimestamps,
            IrregularTimestamps,
            RandomDeltaTimestamps,
        }
        .SelectMany(s => s.Multiplex())
        .Select(s => new[] { s });

        [MemberData(nameof(Timestamps))]
        [Theory]
        public void CompressionIdenity(DateTimeOffset[] timestamps)
        {
            var encoder = new TimestampEncoder(timestamps.Length);
            var count = timestamps.Length;

            foreach (var dt in timestamps)
            {
                encoder.Add(dt);
            }

            var buffer = new byte[encoder.ByteCount];
            encoder.Encode(buffer);

            var result = TimestampCompression.Decode(buffer, count);

            result.Should()
                .Equal(timestamps);
        }
    }
}
