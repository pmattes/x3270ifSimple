// <copyright file="QuoteUnitTest.cs" company="Paul Mattes">
//     Copyright (c) Paul Mattes. All rights reserved.
// </copyright>

namespace UnitTests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using X3270if;

    /// <summary>
    /// Test the <see cref="X3270if.Quote(string)"/> method.
    /// </summary>
    [TestClass]
    public class QuoteUnitTest
    {
        /// <summary>
        /// Test for quoting not needed.
        /// </summary>
        [TestMethod]
        public void QuoteNotNeeded()
        {
            const string arg = "abc124-=+*&%$^\"\\#@!";
            Assert.AreEqual(arg, X3270if.Quote(arg));
        }

        /// <summary>
        /// Test for quoting not needed.
        /// </summary>
        [TestMethod]
        public void QuoteEmpty()
        {
            Assert.AreEqual("\"\"", X3270if.Quote(string.Empty));
        }

        /// <summary>
        /// Tests for simple cases where quoting is necessary.
        /// </summary>
        [TestMethod]
        public void QuoteSimple()
        {
            var easy = new[] { "abc 123", "abc,123", "abc)123", "abc(123" };
            foreach (var arg in easy)
            {
                Assert.AreEqual("\"" + arg + "\"", X3270if.Quote(arg));
            }
        }

        /// <summary>
        /// Tests for needed backslashes.
        /// </summary>
        [TestMethod]
        public void QuoteBackslash()
        {
            // Starts with a double quote (but doesn't contain any other trigger characters).
            Assert.AreEqual("\"\\\"abc\"", X3270if.Quote("\"abc"));

            // Double quote in the middle.
            Assert.AreEqual("\"a bc\\\"def\"", X3270if.Quote("a bc\"def"));
            Assert.AreEqual("\"a bc\\\"d\\\"ef\"", X3270if.Quote("a bc\"d\"ef"));

            // Double quote after backslash (backslash should make no difference).
            Assert.AreEqual("\"a bc\\\\\"def\"", X3270if.Quote("a bc\\\"def"));

            // Double quote at the end.
            Assert.AreEqual("\"a bc\\\"def\\\\\"", X3270if.Quote("a bc\"def\\"));
        }
    }
}
