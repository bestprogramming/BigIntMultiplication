using System.Buffers;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using Xunit.Abstractions;

namespace Tests
{
    public class MultiplicationTest(ITestOutputHelper output) : BaseTest(output)
    {
        [DllImport(dllPath)]
        private static unsafe extern int MultiplyByEqualLength(int length, ulong* left, ulong* right, ulong* result);

        [DllImport(dllPath)]
        private static unsafe extern int MultiplyByGreaterLength(int leftLength, int rightLength, ulong* left, ulong* right, ulong* result);

        private unsafe static ulong[] Multiply(ulong[] left, ulong[] right)
        {
            int leftLength = left.Length;
            int rightLength = right.Length;

            ulong[] result;
            ulong[] ret;

            if (leftLength == rightLength)
            {
                int size = 2 * leftLength;
                result = ArrayPool<ulong>.Shared.Rent(size);

                fixed (ulong* p1 = left, p2 = right, p = result)
                {
                    size = MultiplyByEqualLength(leftLength, p1, p2, p);
                    ret = result[..size];
                }
            }
            else if (leftLength > rightLength)
            {
                int size = leftLength + rightLength;
                result = ArrayPool<ulong>.Shared.Rent(size);

                fixed (ulong* p1 = left, p2 = right, p = result)
                {
                    size = MultiplyByGreaterLength(leftLength, rightLength, p1, p2, p);
                    ret = result[..size];
                }
            }
            else
            {
                int size = rightLength + leftLength;
                result = ArrayPool<ulong>.Shared.Rent(size);

                fixed (ulong* p1 = left, p2 = right, p = result)
                {
                    size = MultiplyByGreaterLength(rightLength, leftLength, p2, p1, p);
                    ret = result[..size];
                }
            }

            ArrayPool<ulong>.Shared.Return(result);

            return ret;
        }



        [Fact]
        public void EqualLengthT1()
        {
            ulong[] left;
            ulong[] right;
            BigInteger expected;
            ulong[] actual;


            left = [0xFFFFFFFFFFFFFFFFul];
            right = [0xFFFFFFFFFFFFFFFFul];
            expected = ToBigInteger(left) * ToBigInteger(right);
            actual = Multiply(left, right);
            Assert.True(Eq(expected, actual));

            left = [0xFFFFFFFFFFFFFFFFul, 0xFFFFFFFFFFFFFFFFul, 0xFFFFFFFFFFFFFFFFul];
            right = [0xFFFFFFFFFFFFFFFFul, 0xFFFFFFFFFFFFFFFFul, 0xFFFFFFFFFFFFFFFFul];
            expected = ToBigInteger(left) * ToBigInteger(right);
            actual = Multiply(left, right);
            Assert.True(Eq(expected, actual));
        }

        [Fact]
        public void GreaterLengthT1()
        {
            ulong[] left;
            ulong[] right;
            BigInteger expected;
            ulong[] actual;


            left = [0xFFFFFFFFFFFFFFFFul, 0xFFFFFFFFFFFFFFFFul, 0xFFFFFFFFFFFFFFFFul];
            right = [0xFFFFFFFFFFFFFFFFul, 0xFFFFFFFFFFFFFFFFul];
            expected = ToBigInteger(left) * ToBigInteger(right);
            actual = Multiply(left, right);
            Assert.True(Eq(expected, actual));

            left = [0xFFFFFFFFFFFFFFFFul, 0xFFFFFFFFFFFFFFFFul, 0xFFFFFFFFFFFFFFFFul, 0xFFFFFFFFFFFFFFFFul, 0xFFFFFFFFFFFFFFFFul];
            right = [0xFFFFFFFFFFFFFFFFul, 0xFFFFFFFFFFFFFFFFul, 0xFFFFFFFFFFFFFFFFul];
            expected = ToBigInteger(left) * ToBigInteger(right);
            actual = Multiply(left, right);
            Assert.True(Eq(expected, actual));
        }

        [Fact]
        public void LessLengthT1()
        {
            ulong[] left;
            ulong[] right;
            BigInteger expected;
            ulong[] actual;


            left = [0xFFFFFFFFFFFFFFFFul, 0xFFFFFFFFFFFFFFFFul];
            right = [0xFFFFFFFFFFFFFFFFul, 0xFFFFFFFFFFFFFFFFul, 0xFFFFFFFFFFFFFFFFul];
            expected = ToBigInteger(left) * ToBigInteger(right);
            actual = Multiply(left, right);
            Assert.True(Eq(expected, actual));

            left = [0xFFFFFFFFFFFFFFFFul, 0xFFFFFFFFFFFFFFFFul, 0xFFFFFFFFFFFFFFFFul];
            right = [0xFFFFFFFFFFFFFFFFul, 0xFFFFFFFFFFFFFFFFul, 0xFFFFFFFFFFFFFFFFul, 0xFFFFFFFFFFFFFFFFul, 0xFFFFFFFFFFFFFFFFul];
            expected = ToBigInteger(left) * ToBigInteger(right);
            actual = Multiply(left, right);
            Assert.True(Eq(expected, actual));
        }

        [Fact]
        public void VsBigIntegerT1()
        {
            var actualTicks = new List<long>();
            var expectedTicks = new List<long>();

            var testCount = 100;
            var leftLength = 8192;
            var rightLength = 8192;

            for (var test = 0; test < testCount; test++)
            {
                var left = new ulong[leftLength];
                var right = new ulong[rightLength];
                for (var n = 0; n < leftLength; n++)
                {
                    left[n] = RandomUlong();
                }

                for (var n = 0; n < rightLength; n++)
                {
                    right[n] = RandomUlong();
                }

                var b1 = ToBigInteger(left);
                var b2 = ToBigInteger(right);

                var swExpected = Stopwatch.StartNew();
                var expected = b1 * b2;
                swExpected.Stop();
                expectedTicks.Add(swExpected.ElapsedTicks);

                var swActual = Stopwatch.StartNew();
                var actual = Multiply(left, right);
                swActual.Stop();
                actualTicks.Add(swActual.ElapsedTicks);

                Assert.True(Eq(expected, actual));
            }

            expectedTicks.Sort();
            actualTicks.Sort();

            output.WriteLine($"leftLength:{leftLength}, rightLength:{rightLength}");
            const int pad = 20;
            output.WriteLine($"{"expected(max)",-pad}{"actual",-pad}");

            for (var n = 0; n < 5; n++)
            {
                output.WriteLine($"{expectedTicks.ElementAt(n),-pad}{actualTicks.ElementAt(n),-pad}");
                //Assert.True(actualTicks.ElementAt(n) <= expectedTicks.ElementAt(n));
            }
        }

        [Fact]
        public void VsBigIntegerT2()
        {
            var actualTicks = new List<long>();
            var expectedTicks = new List<long>();

            var testCount = 300;

            var leftLengths = new[] { 16, 32, 64, 128, 256, 512, 1024, 2048 };
            var rightLengths = new[] { 16, 32, 64, 128, 256, 512, 1024, 2048 };

            const int pad = 20;
            var table = $"\t\t\t|{"leftLength",-pad}|{"rightLength",-pad}|{"expectedTick(max)",-pad}|{"actualTick",-pad}|{"Percent |"}\r\n";
            table += $"\t\t\t|{" :".PadRight(pad, '-')}|{" :".PadRight(pad, '-')}|{" :".PadRight(pad, '-')}|{" :".PadRight(pad, '-')}|{" :----- |"}\r\n";

            foreach (var leftLength in leftLengths)
            {
                foreach (var rightLength in rightLengths)
                {
                    actualTicks.Clear();
                    expectedTicks.Clear();

                    for (var test = 0; test < testCount; test++)
                    {
                        var left = new ulong[leftLength];
                        var right = new ulong[rightLength];
                        for (var n = 0; n < leftLength; n++)
                        {
                            left[n] = RandomUlong();
                        }

                        for (var n = 0; n < rightLength; n++)
                        {
                            right[n] = RandomUlong();
                        }

                        var b1 = ToBigInteger(left);
                        var b2 = ToBigInteger(right);

                        var swExpected = Stopwatch.StartNew();
                        var expected = b1 * b2;
                        swExpected.Stop();
                        expectedTicks.Add(swExpected.ElapsedTicks);

                        var swActual = Stopwatch.StartNew();
                        var actual = Multiply(left, right);
                        swActual.Stop();
                        actualTicks.Add(swActual.ElapsedTicks);

                        Assert.True(Eq(expected, actual));
                    }

                    expectedTicks.Sort();
                    actualTicks.Sort();

                    var expectedTick = expectedTicks.First();
                    var actualTick = actualTicks.First();
                    table += $"\t\t\t|{leftLength,-pad}|{rightLength,-pad}|{expectedTick,-pad}|{actualTick,-pad}|{Percent(expectedTick, actualTick),-8}|\r\n";
                }
            }

            output.WriteLine(table);
            File.WriteAllText($"{appDir}/MultiplicationTest.txt", table);

            /* 

			|leftLength          |rightLength         |expectedTick(max)   |actualTick          |Percent |
			| :------------------| :------------------| :------------------| :------------------| :----- |
			|16                  |16                  |9                   |6                   |66%     |
			|16                  |32                  |18                  |12                  |66%     |
			|16                  |64                  |36                  |21                  |58%     |
			|16                  |128                 |72                  |40                  |55%     |
			|16                  |256                 |141                 |78                  |55%     |
			|16                  |512                 |278                 |144                 |51%     |
			|16                  |1024                |561                 |265                 |47%     |
			|16                  |2048                |1128                |528                 |46%     |
			|32                  |16                  |18                  |12                  |66%     |
			|32                  |32                  |30                  |23                  |76%     |
			|32                  |64                  |58                  |45                  |77%     |
			|32                  |128                 |116                 |82                  |70%     |
			|32                  |256                 |233                 |170                 |72%     |
			|32                  |512                 |467                 |334                 |71%     |
			|32                  |1024                |931                 |640                 |68%     |
			|32                  |2048                |1871                |1323                |70%     |
			|64                  |16                  |48                  |23                  |47%     |
			|64                  |32                  |85                  |46                  |54%     |
			|64                  |64                  |144                 |85                  |59%     |
			|64                  |128                 |289                 |183                 |63%     |
			|64                  |256                 |330                 |356                 |107%    |
			|64                  |512                 |654                 |696                 |106%    |
			|64                  |1024                |1308                |1390                |106%    |
			|64                  |2048                |2612                |2709                |103%    |
			|128                 |16                  |61                  |37                  |60%     |
			|128                 |32                  |100                 |84                  |84%     |
			|128                 |64                  |162                 |176                 |108%    |
			|128                 |128                 |253                 |355                 |140%    |
			|128                 |256                 |503                 |723                 |143%    |
			|128                 |512                 |1009                |1405                |139%    |
			|128                 |1024                |1996                |2834                |141%    |
			|128                 |2048                |4039                |5794                |143%    |
			|256                 |16                  |123                 |72                  |58%     |
			|256                 |32                  |200                 |169                 |84%     |
			|256                 |64                  |325                 |351                 |108%    |
			|256                 |128                 |504                 |707                 |140%    |
			|256                 |256                 |774                 |1467                |189%    |
			|256                 |512                 |1543                |2963                |192%    |
			|256                 |1024                |3046                |5860                |192%    |
			|256                 |2048                |5796                |11690               |201%    |
			|512                 |16                  |243                 |143                 |58%     |
			|512                 |32                  |400                 |317                 |79%     |
			|512                 |64                  |650                 |686                 |105%    |
			|512                 |128                 |1010                |1451                |143%    |
			|512                 |256                 |1545                |2954                |191%    |
			|512                 |512                 |2341                |5972                |255%    |
			|512                 |1024                |4377                |12077               |275%    |
			|512                 |2048                |8760                |23290               |265%    |
			|1024                |16                  |488                 |266                 |54%     |
			|1024                |32                  |803                 |671                 |83%     |
			|1024                |64                  |1278                |1398                |109%    |
			|1024                |128                 |2024                |2849                |140%    |
			|1024                |256                 |3083                |5869                |190%    |
			|1024                |512                 |4467                |11852               |265%    |
			|1024                |1024                |6603                |23285               |352%    |
			|1024                |2048                |12974               |47030               |362%    |
			|2048                |16                  |930                 |493                 |53%     |
			|2048                |32                  |1602                |1293                |80%     |
			|2048                |64                  |2607                |2777                |106%    |
			|2048                |128                 |4032                |5703                |141%    |
			|2048                |256                 |5770                |11730               |203%    |
			|2048                |512                 |8731                |22610               |258%    |
			|2048                |1024                |13186               |46620               |353%    |
			|2048                |2048                |20239               |94075               |464%    |


             */


        }
    }
}
