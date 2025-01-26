using System.Numerics;
using Xunit.Abstractions;

namespace Tests
{
    public class BaseTest(ITestOutputHelper output)
    {
        protected readonly ITestOutputHelper output = output;
        protected static readonly string appDir = AppDomain.CurrentDomain.BaseDirectory.Split("\\Tests\\")[0];
        protected const string dllPath = "BigIntMultiplication.dll";
        protected static readonly Random Random = new();

        protected static ulong RandomUlong()
        {
            uint u1 = unchecked((uint)Random.Next(int.MinValue, int.MaxValue));
            uint u2 = unchecked((uint)Random.Next(int.MinValue, int.MaxValue));
            ulong ret = (ulong)u1 << 32;
            ret |= u2;
            return ret;
        }

        protected static bool Eq(BigInteger expected, ulong[] actual) => expected == ToBigInteger(actual);

        protected static BigInteger ToBigInteger(ulong[] bits)
        {
            if (bits.Length == 0) return BigInteger.Zero;

            var bytes = new byte[bits.Length * 8 + 1];

            for (var n = 0; n < bits.Length; n++)
            {
                var index = n * 8;
                var b = bits[n];
                var bs = BitConverter.GetBytes(b);
                bytes[index] = bs[0];
                bytes[index + 1] = bs[1];
                bytes[index + 2] = bs[2];
                bytes[index + 3] = bs[3];
                bytes[index + 4] = bs[4];
                bytes[index + 5] = bs[5];
                bytes[index + 6] = bs[6];
                bytes[index + 7] = bs[7];
            }

            return new BigInteger(bytes);
        }

        protected static string Percent(long expectedTick, long actualTick) => expectedTick != 0 ? $"{actualTick * 100 / expectedTick}%" : $"{actualTick}/0?";

    }
}
