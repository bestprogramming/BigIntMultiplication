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
			16                  16                  9                   7                   77%
			16                  32                  18                  12                  66%
			16                  64                  35                  21                  60%
			16                  128                 70                  39                  55%
			16                  256                 140                 80                  57%
			16                  512                 278                 139                 50%
			16                  1024                549                 266                 48%
			16                  2048                1117                582                 52%
			32                  16                  18                  13                  72%
			32                  32                  30                  24                  80%
			32                  64                  58                  46                  79%
			32                  128                 117                 88                  75%
			32                  256                 231                 172                 74%
			32                  512                 461                 330                 71%
			32                  1024                927                 657                 70%
			32                  2048                1854                1255                67%
			64                  16                  49                  23                  46%
			64                  32                  87                  46                  52%
			64                  64                  144                 92                  63%
			64                  128                 288                 179                 62%
			64                  256                 552                 346                 62%
			64                  512                 652                 699                 107%
			64                  1024                1306                1397                106%
			64                  2048                2619                2789                106%
			128                 16                  62                  39                  62%
			128                 32                  100                 88                  88%
			128                 64                  162                 180                 111%
			128                 128                 251                 369                 147%
			128                 256                 504                 714                 141%
			128                 512                 1010                1413                139%
			128                 1024                2025                2892                142%
			128                 2048                4064                5772                142%
			256                 16                  124                 77                  62%
			256                 32                  201                 174                 86%
			256                 64                  326                 334                 102%
			256                 128                 503                 724                 143%
			256                 256                 774                 1492                192%
			256                 512                 1501                2939                195%
			256                 1024                3111                5962                191%
			256                 2048                5818                11954               205%
			512                 16                  245                 156                 63%
			512                 32                  403                 318                 78%
			512                 64                  650                 681                 104%
			512                 128                 994                 1414                142%
			512                 256                 1549                2903                187%
			512                 512                 2326                6062                260%
			512                 1024                4701                12078               256%
			512                 2048                8921                23813               266%
			1024                16                  496                 279                 56%
			1024                32                  806                 653                 81%
			1024                64                  1309                1420                108%
			1024                128                 2030                2867                141%
			1024                256                 3114                5932                190%
			1024                512                 4423                11760               265%
			1024                1024                6811                24268               356%
			1024                2048                13620               47401               348%
			2048                16                  987                 536                 54%
			2048                32                  1581                1326                83%
			2048                64                  2617                2828                108%
			2048                128                 3712                5335                143%
			2048                256                 5789                11281               194%
			2048                512                 9167                24276               264%
			2048                1024                13829               48808               352%
			2048                2048                19690               99399               504%

             */


        }
    }
}
