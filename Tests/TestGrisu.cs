using System;
using System.Diagnostics;
using System.Text;
using grisu.net;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class TestGrisu
    {
        public static void Main(string[] args)
        {
            new TestGrisu().TestPerformance();
        }

        const ulong kSignMask = 0x8000000000000000;

        [Test]
        [Explicit]
        public void TestPerformance()
        {
            Random r = new Random(1);
            double[] values = new double[10000000];
            ulong[] valuesi = new ulong[values.Length];
            for (int i = 0; i < values.Length; ++i)
            {
                values[i] = (r.NextDouble() - 0.5) * Math.Pow(10, r.NextDouble() * 308);
                valuesi[i] = (ulong)BitConverter.DoubleToInt64Bits(values[i]);
            }

            StringBuilder builder = new StringBuilder();

            Stopwatch sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < values.Length; ++i)
            {
                //builder.AppendFormat("{0:R}", values[i]);
            }
            sw.Stop();
            //Console.WriteLine("builtin length: " + builder.ToString().Length);
            Console.WriteLine("builtin time: " + sw.ElapsedMilliseconds);
            if (values.Length < 100)
                Console.WriteLine(builder.ToString());

            builder = new StringBuilder(300000000);
            //for (int j = 0; j < 10; ++j)
            {
                builder.Clear();
                sw = new Stopwatch();
                sw.Start();
                for (int i = 0; i < values.Length; ++i)
                {
                    Grisu.DoubleToString(values[i], builder);
                }
                sw.Stop();
                //Console.WriteLine("grisu length: " + builder.ToString().Length);
                Console.WriteLine("grisu time: " + sw.ElapsedMilliseconds);
            }
            if (values.Length < 100)
                Console.WriteLine(builder.ToString());
        }

        [Test]
        public void TestDoubleToString()
        {
            CheckDoubleToStringEquals("0.0", 0.0);
            CheckDoubleToStringEquals("12345.0", 12345.0);
            CheckDoubleToStringEquals("1.2345e27", 12345e23);
            CheckDoubleToStringEquals("1e21", 1e21);
            CheckDoubleToStringEquals("1e20", 1e20);
            CheckDoubleToStringEquals("1.1111111111111111e20", 111111111111111111111.0);
            CheckDoubleToStringEquals("1.1111111111111111e21", 1111111111111111111111.0);
            CheckDoubleToStringEquals("1.1111111111111111e22", 11111111111111111111111.0);
            CheckDoubleToStringEquals("-1e-5", -0.00001);
            CheckDoubleToStringEquals("-1e-6", -0.000001);
            CheckDoubleToStringEquals("-1e-7", -0.0000001);
            CheckDoubleToStringEquals("0.0", -0.0);
            CheckDoubleToStringEquals("0.1", 0.1);
            CheckDoubleToStringEquals("0.01", 0.01);
            CheckDoubleToStringEquals("1.0", 1.0);
            CheckDoubleToStringEquals("10.0", 10.0);
            CheckDoubleToStringEquals("1e4", 10000.0);
            CheckDoubleToStringEquals("1e5", 100000.0);
            CheckDoubleToStringEquals("1e-6", 0.000001);
            CheckDoubleToStringEquals("1e-7", 0.0000001);
            CheckDoubleToStringEquals("1e20", 100000000000000000000.0);
            CheckDoubleToStringEquals("Infinity", double.PositiveInfinity);
            CheckDoubleToStringEquals("-Infinity", double.NegativeInfinity);
            CheckDoubleToStringEquals("NaN", double.NaN);
            CheckDoubleToStringEquals("NaN", -double.NaN);
        }

        private void CheckDoubleToStringEquals(string expected, double value)
        {
            StringBuilder builder = new StringBuilder();
            Grisu.DoubleToString(value, builder);
            Assert.AreEqual(expected, builder.ToString());
        }
    }
}
