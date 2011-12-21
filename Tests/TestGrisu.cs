using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
using grisu.net;
using System.Diagnostics;

namespace Tests
{
    //[TestClass]
    public class TestGrisu
    {
        public static void Main(string[] args)
        {
            new TestGrisu().TestMethod1();
        }

        //[TestMethod]
        public void TestMethod1()
        {
            Random r = new Random(1);
            double[] values = new double[10000000];
            double smallest = Double.MaxValue;
            double largest = 0.0;
            for (int i = 0; i < values.Length; ++i)
            {
                values[i] = (r.NextDouble() - 0.5) * Math.Pow(10, r.NextDouble() * 308);
                smallest = Math.Min(smallest, Math.Abs(values[i]));
                largest = Math.Max(largest, Math.Abs(values[i]));
                //values[i] = 1.23e5 / Math.Pow(10, i);
            }

            Console.WriteLine("smallest: " + smallest);
            Console.WriteLine("largest: " + largest);

            StringBuilder builder = new StringBuilder();

            Stopwatch sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < values.Length; ++i)
            {
                builder.AppendFormat("{0:R}", values[i]);
            }
            sw.Stop();
            Console.WriteLine("builtin length: " + builder.ToString().Length);
            Console.WriteLine("builtin time: " + sw.ElapsedMilliseconds);
            if (values.Length < 100)
                Console.WriteLine(builder.ToString());

            builder = new StringBuilder();
            sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < values.Length; ++i)
            {
                Grisu.DoubleToString(values[i], builder);
            }
            sw.Stop();
            Console.WriteLine("grisu length: " + builder.ToString().Length);
            Console.WriteLine("grisu time: " + sw.ElapsedMilliseconds);
            if (values.Length < 100)
                Console.WriteLine(builder.ToString());
        }
    }
}
