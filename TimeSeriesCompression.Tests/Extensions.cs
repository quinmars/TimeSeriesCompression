using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeSeriesCompression.Tests
{
    public static class Extensions
    {
        public static IEnumerable<DateTimeOffset[]> Multiplex(this IEnumerable<DateTimeOffset> dts)
        {
            return new[]
            {
                TimeSpan.FromHours(-12),
                TimeSpan.FromHours(-11),
                TimeSpan.FromHours(-10),
                TimeSpan.FromHours(-9),
                TimeSpan.FromHours(-8),
                TimeSpan.FromHours(-7),
                TimeSpan.FromHours(-6),
                TimeSpan.FromHours(-5),
                TimeSpan.FromHours(-4),
                TimeSpan.FromHours(-3),
                TimeSpan.FromHours(-2),
                TimeSpan.FromHours(-1),
                TimeSpan.Zero,
                TimeSpan.FromHours(1),
                TimeSpan.FromHours(2),
                TimeSpan.FromHours(3),
                TimeSpan.FromHours(4),
                TimeSpan.FromHours(5),
                TimeSpan.FromHours(6),
                TimeSpan.FromHours(7),
                TimeSpan.FromHours(8),
                TimeSpan.FromHours(9),
                TimeSpan.FromHours(10),
                TimeSpan.FromHours(11),
                TimeSpan.FromHours(12),
                TimeSpan.FromHours(6.5),
                TimeSpan.FromMinutes(10),
            }
            .Select(o => dts.Select(d => new DateTimeOffset(d.Ticks, d.Offset + o)).ToArray());
        }
    }
}
