using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using grisu.net;

namespace Tests
{
    [TestClass]
    public class TestGrisu
    {
        [TestMethod]
        public void TestMethod1()
        {
            Random r = new Random(1);
            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < 1000000; ++i)
            {
                double value = (r.NextDouble() - 0.5) * Double.MaxValue;
                Grisu.DoubleToString(value, builder);
                string asString = builder.ToString();
                builder.Clear();
                double roundTrip = double.Parse(asString);
                Assert.AreEqual(value, roundTrip, "{0}: {1} != {2}", i, BitConverter.DoubleToInt64Bits(roundTrip), BitConverter.DoubleToInt64Bits(value));
            }
        }
    }
}
