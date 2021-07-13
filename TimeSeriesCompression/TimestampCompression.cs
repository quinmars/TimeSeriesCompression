using System;
using System.Collections.Generic;

namespace TimeSeriesCompression
{
    public static class TimestampCompression
    {
        internal static void WriteCompressedInt64(this BitWriter writer, long delta)
        {
            var d = delta < 0 ? ~delta : delta;

            // A value of 0 is encoded a single 0 bit
            if (delta == 0)
            {
                writer.WriteUInt64(0b0, 1);
            }
            // ~ 10ns
            // 7 bit wide mask == 0x7F
            // Write 0b10 followed by:
            // 8 bits (1 bit for negative numbers)
            else if ((d & 0x7F) == d)
            {
                writer.WriteUInt64(0b10, 2);
                writer.WriteUInt64((ulong)delta, 8);
            }
            // ~ 1ms
            // 14 bit wide mask == 0x3FFF
            // Write 0b110 followed by:
            // 15 bits (1 bit for negative numbers)
            else if ((d & 0x3FFF) == d)
            {
                writer.WriteUInt64(0b110, 3);
                writer.WriteUInt64((ulong)delta, 15);
            }
            // ~ 100ms
            // 20 bit wide mask 0xFFFFF
            // Write 0b1110 followed by:
            // 21 bits (1 bit for negative numbers)
            else if ((d & 0xF_FFFF) == d)
            {
                writer.WriteUInt64(0b1110, 4);
                writer.WriteUInt64((ulong)delta, 21);
            }
            // ~ 1s
            // 24 bit wide mask 0xffffff
            // Write 0b11110 followed by:
            // 25 bits (1 bit for negative numbers)
            else if ((d & 0xFF_FFFF) == d)
            {
                writer.WriteUInt64(0b1_1110, 5);
                writer.WriteUInt64((ulong)delta, 25);
            }
            // ~ 1s
            // 27 bit wide mask 0x7ffffff
            // Write 0b11110 followed by:
            // 28 bits (1 bit for negative numbers)
            else if ((d & 0x7FF_FFFF) == d)
            {
                writer.WriteUInt64(0b11_1110, 6);
                writer.WriteUInt64((ulong)delta, 28);
            }
            // ~ 10s
            // 30 bit wide mask 0x3fffffff
            // Write 0b11110 followed by:
            // 31 bits (1 bit for negative numbers)
            else if ((d & 0x3FFF_FFFF) == d)
            {
                writer.WriteUInt64(0b111_1110, 7);
                writer.WriteUInt64((ulong)delta, 31);
            }
            // Write 0b11110 followed by:
            // The whole value
            else
            {
                writer.WriteUInt64(0b1111_1110, 8);
                writer.WriteUInt64((ulong)delta);
            }
        }
        
        internal static void WriteOffset(this BitWriter writer, long ticks)
        {
            switch (ticks)
            {
                case 0:             // UTC, special case
                    writer.WriteUInt64(0, 1);
                    break;
                case 432000000000:  // GMT+12
                    writer.WriteUInt64(0b100_0001, 7);
                    break;
                case 396000000000:  // GMT+11
                    writer.WriteUInt64(0b100_0010, 7);
                    break;
                case 360000000000:  // GMT+10
                    writer.WriteUInt64(0b100_0011, 7);
                    break;
                case 324000000000:  // GMT+9
                    writer.WriteUInt64(0b100_0100, 7);
                    break;
                case 288000000000:  // GMT+8
                    writer.WriteUInt64(0b100_0101, 7);
                    break;
                case 252000000000:  // GMT+7
                    writer.WriteUInt64(0b100_0110, 7);
                    break;
                case 216000000000:  // GMT+6
                    writer.WriteUInt64(0b100_0111, 7);
                    break;
                case 180000000000:  // GMT+5
                    writer.WriteUInt64(0b100_1000, 7);
                    break;
                case 144000000000:  // GMT+4
                    writer.WriteUInt64(0b100_1001, 7);
                    break;
                case 108000000000:  // GMT+3
                    writer.WriteUInt64(0b100_1010, 7);
                    break;
                case 72000000000:   // GMT+2
                    writer.WriteUInt64(0b100_1011, 7);
                    break;
                case 36000000000:   // GMT+1
                    writer.WriteUInt64(0b100_1100, 7);
                    break;
                case -36000000000:  // GMT-1
                    writer.WriteUInt64(0b100_1101, 7);
                    break;
                case -72000000000:  // GMT-2
                    writer.WriteUInt64(0b100_1110, 7);
                    break;
                case -108000000000: // GMT-3
                    writer.WriteUInt64(0b100_1111, 7);
                    break;
                case -144000000000: // GMT-4
                    writer.WriteUInt64(0b101_0000, 7);
                    break;
                case -180000000000: // GMT-5
                    writer.WriteUInt64(0b101_0001, 7);
                    break;
                case -216000000000: // GMT-6
                    writer.WriteUInt64(0b101_0010, 7);
                    break;
                case -252000000000: // GMT-7
                    writer.WriteUInt64(0b101_0011, 7);
                    break;
                case -288000000000: // GMT-8
                    writer.WriteUInt64(0b101_0100, 7);
                    break;
                case -324000000000: // GMT-9
                    writer.WriteUInt64(0b101_0101, 7);
                    break;
                case -360000000000: // GMT-10
                    writer.WriteUInt64(0b101_0110, 7);
                    break;
                case -396000000000: // GMT-11
                    writer.WriteUInt64(0b101_0111, 7);
                    break;
                case -432000000000: // GMT-12
                    writer.WriteUInt64(0b101_1000, 7);
                    break;
                default:
                    writer.WriteUInt64(0b110, 3);
                    writer.WriteUInt64((ulong)ticks);
                    break;

            }
        }
        
        internal static long ReadCompressedInt64(this BitReader reader)
        {
            // A value of 0 is encoded a single 0 bit
            if (reader.TryReadUInt64(0, 1))
            {
                return 0;
            }
            // ~ 10ns
            // 7 bit wide mask == 0x7F
            // Write 0b10 followed by:
            // 8 bits (1 bit for negative numbers)
            else if (reader.TryReadUInt64(0b10, 2))
            { 
                return reader.ReadInt64(8);
            }
            // ~ 1ms
            // 14 bit wide mask == 0x3FFF
            // Write 0b110 followed by:
            // 15 bits (1 bit for negative numbers)
            else if (reader.TryReadUInt64(0b110, 3))
            { 
                return reader.ReadInt64(15);
            }
            // ~ 100ms
            // 20 bit wide mask 0xFFFFF
            // Write 0b1110 followed by:
            // 21 bits (1 bit for negative numbers)
            else if (reader.TryReadUInt64(0b1110, 4))
            {
                return reader.ReadInt64(21);
            }
            // ~ 1s
            // 24 bit wide mask 0xffffff
            // Write 0b11110 followed by:
            // 25 bits (1 bit for negative numbers)
            else if (reader.TryReadUInt64(0b1_1110, 5))
            {
                return reader.ReadInt64(25);
            }
            // ~ 1s
            // 27 bit wide mask 0x7ffffff
            // Write 0b11110 followed by:
            // 28 bits (1 bit for negative numbers)
            else if (reader.TryReadUInt64(0b11_1110, 6))
            {
                return reader.ReadInt64(28);
            }
            // ~ 10s
            // 30 bit wide mask 0x3fffffff
            // Write 0b11110 followed by:
            // 31 bits (1 bit for negative numbers)
            else if (reader.TryReadUInt64(0b111_1110, 7))
            {
                return reader.ReadInt64(31);
            }
            // Write 0b11110 followed by:
            // The whole value
            else if (reader.TryReadUInt64(0b1111_1110, 8))
            {
                return reader.ReadInt64();
            }
            else
            {
                throw new InvalidOperationException();
            }
        }
        
        internal static long ReadOffset(this BitReader reader)
        {
            if (reader.TryReadUInt64(0, 1))
            {
                return 0;
            }
            else if (reader.TryReadUInt64(0b10, 2))
            {
                ulong tz = reader.ReadUInt64(5);

                switch (tz)
                {
                    case 0b0_0001:
                        return 432000000000;  // GMT+12
                    case 0b0_0010:
                        return 396000000000;  // GMT+11
                    case 0b0_0011:
                        return 360000000000;  // GMT+10
                    case 0b0_0100:
                        return 324000000000;  // GMT+9
                    case 0b0_0101:
                        return 288000000000;  // GMT+8
                    case 0b0_0110:
                        return 252000000000;  // GMT+7
                    case 0b0_0111:
                        return 216000000000;  // GMT+6
                    case 0b0_1000:
                        return 180000000000;  // GMT+5
                    case 0b0_1001:
                        return 144000000000;  // GMT+4
                    case 0b0_1010:
                        return 108000000000;  // GMT+3
                    case 0b0_1011:
                        return 72000000000;   // GMT+2
                    case 0b0_1100:
                        return 36000000000;   // GMT+1
                    case 0b0_1101:
                        return -36000000000;  // GMT-1
                    case 0b0_1110:
                        return -72000000000;  // GMT-2
                    case 0b0_1111:
                        return -108000000000; // GMT-3
                    case 0b1_0000:
                        return -144000000000; // GMT-4
                    case 0b1_0001:
                        return -180000000000; // GMT-5
                    case 0b1_0010:
                        return -216000000000; // GMT-6
                    case 0b1_0011:
                        return -252000000000; // GMT-7
                    case 0b1_0100:
                        return -288000000000; // GMT-8
                    case 0b1_0101:
                        return -324000000000; // GMT-9
                    case 0b1_0110:
                        return -360000000000; // GMT-10
                    case 0b1_0111:
                        return -396000000000; // GMT-11
                    case 0b1_1000:
                        return -432000000000; // GMT-12
                    default:
                        throw new InvalidOperationException();
                }
            }
            else if (reader.TryReadUInt64(0b110, 3))
            {
                return reader.ReadInt64();
            }
            else
            {
                throw new InvalidOperationException();
            }
        }
    }
}
