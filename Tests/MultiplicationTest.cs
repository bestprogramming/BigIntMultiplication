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
            output.WriteLine($"{"expected(max)",-20}{"actual",-20}");

            for (var n = 0; n < 5; n++)
            {
                output.WriteLine($"{expectedTicks.ElementAt(n),-20}{actualTicks.ElementAt(n),-20}");
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

            var table = $"\t\t\t{"leftLength",-20}{"rightLength",-20}{"expectedTick(max)",-20}{"actualTick",-20}{"Percent"}\r\n";
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
                    table += $"\t\t\t{leftLength,-20}{rightLength,-20}{expectedTick,-20}{actualTick,-20}{Percent(expectedTick, actualTick)}\r\n";
                }
            }

            output.WriteLine(table);
            File.WriteAllText($"{appDir}/MultiplicationTest.txt", table);

            /* 

			leftLength          rightLength         expectedTick(max)   actualTick          Percent
			16                  16                  9                   6                   66%
			16                  32                  18                  12                  66%
			16                  64                  35                  21                  60%
			16                  128                 70                  39                  55%
			16                  256                 139                 74                  53%
			16                  512                 279                 139                 49%
			16                  1024                560                 283                 50%
			16                  2048                1119                531                 47%
			32                  16                  24                  12                  50%
			32                  32                  43                  22                  51%
			32                  64                  85                  44                  51%
			32                  128                 170                 85                  50%
			32                  256                 337                 170                 50%
			32                  512                 675                 325                 48%
			32                  1024                814                 654                 80%
			32                  2048                1621                1284                79%
			64                  16                  31                  22                  70%
			64                  32                  50                  45                  90%
			64                  64                  83                  82                  98%
			64                  128                 167                 167                 100%
			64                  256                 335                 348                 103%
			64                  512                 652                 678                 103%
			64                  1024                1307                1326                101%
			64                  2048                2603                2774                106%
			128                 16                  62                  34                  54%
			128                 32                  100                 84                  84%
			128                 64                  162                 174                 107%
			128                 128                 251                 348                 138%
			128                 256                 502                 729                 145%
			128                 512                 1005                1418                141%
			128                 1024                2020                2853                141%
			128                 2048                3967                5703                143%
			256                 16                  123                 73                  59%
			256                 32                  200                 168                 84%
			256                 64                  324                 348                 107%
			256                 128                 501                 726                 144%
			256                 256                 774                 1451                187%
			256                 512                 1547                2939                189%
			256                 1024                3080                5869                190%
			256                 2048                5998                11977               199%
			512                 16                  243                 133                 54%
			512                 32                  400                 333                 83%
			512                 64                  627                 688                 109%
			512                 128                 1007                1423                141%
			512                 256                 1539                2949                191%
			512                 512                 2340                5909                252%
			512                 1024                4557                12082               265%
			512                 2048                8726                23725               271%
			1024                16                  493                 269                 54%
			1024                32                  803                 631                 78%
			1024                64                  1302                1358                104%
			1024                128                 2021                2826                139%
			1024                256                 3078                5870                190%
			1024                512                 4466                12076               270%
			1024                1024                6578                22858               347%
			1024                2048                13160               48398               367%
			2048                16                  992                 481                 48%
			2048                32                  1605                1276                79%
			2048                64                  2606                2717                104%
			2048                128                 4032                5761                142%
			2048                256                 6016                11991               199%
			2048                512                 8902                23926               268%
			2048                1024                13154               48146               366%
			2048                2048                20361               95910               471%

             */


        }
    }
}
