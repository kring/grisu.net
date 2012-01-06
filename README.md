Grisu is a fast, new algorithm for converting a floating-point number to a decimal string.  It was introduced in a paper by Florian Loitsch in 2010 (http://dl.acm.org/citation.cfm?doid=1809028.1806623) and it as much as four times faster than previous techniques.

The code here is a port of the C++ code in the "double-conversion" project on Google Code (http://code.google.com/p/double-conversion/) to C#.  My focus is on very fast JSON serialization of floating point numbers, however, so I eliminated all the options and configurability.

Instead of offering options, my port aims to:

 - Guarantee that the generated strings can be parsed back to an identical double.  It uses the fewest digits possible while still achieving this.
 - Produce the most compact JSON-compatible strings possible by choosing regular decimal or scientific notation, whichever is shorter.
 - Be really, really fast.

 Using this library is simple, because it really has only one method.

    StringBuilder builder = new StringBuilder();
    GrisuDotNet.Grisu.DoubleToString(1.23, builder);
    Assert.AreEqual("1.23", builder.ToString());

In my tests on a 64-bit version of .NET 4, this library writes doubles to a StringBuilder over two times faster than StringBuilder's default serialization of doubles, while guaranteeing that the strings round-trip back to the same doubles.  When the "R" format specifier is used with Double.ToString to guarantee that a double round-trips, this library produces output over four times faster and also generates more compact output.

If you make any improvements to this code - especially performance improvements - please send me a pull request.