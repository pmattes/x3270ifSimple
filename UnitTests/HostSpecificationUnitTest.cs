// <copyright file="HostSpecificationUnitTest.cs" company="Paul Mattes">
//     Copyright (c) Paul Mattes. All rights reserved.
// </copyright>

namespace UnitTests
{
    using System;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using X3270is;

    /// <summary>
    /// Test the <see cref="HostSpecification"/> class.
    /// </summary>
    [TestClass]
    public class HostSpecificationUnitTest
    {
        /// <summary>
        /// Trivial success test for a <see cref="HostSpecification"/>.
        /// </summary>
        [TestMethod]
        public void HostSpecificationTrivial()
        {
            var h = new HostSpecification("x");
            Assert.AreEqual("x", h.ToString());
        }

        /// <summary>
        /// Elaborate success test for a <see cref="HostSpecification"/>.
        /// </summary>
        [TestMethod]
        public void HostSpecificationElaborate()
        {
            const string HostName = "1::2";
            const int Port = 921;
            const string AcceptName = "blimey.farfle.net";
            const string Lu1 = "FRED";
            const string Lu2 = "BOB";

            var h = new HostSpecification()
            {
                HostName = HostName,
                Port = Port,
                TlsTunnel = true,
                ValidateHostCertificate = false,
                LogicalUnits = new[] { Lu1, Lu2 },
                AcceptName = AcceptName
            };

            Assert.AreEqual(
                "L:Y:" + Lu1 + "," + Lu2 + "@[" + HostName + "]:" + Port + "=" + AcceptName,
                h.ToString());
        }

        /// <summary>
        /// Trivial failure test for a <see cref="HostSpecification"/>.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void HostSpecificationBadHostName()
        {
            var h = new HostSpecification("x=");
        }

        /// <summary>
        /// Trivial failure test for a <see cref="HostSpecification"/>.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void HostSpecificationEmptyHostName()
        {
            var h = new HostSpecification(" ");
        }

        /// <summary>
        /// Bad port test for a <see cref="HostSpecification"/>.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void HostSpecificationBadPort()
        {
            var h = new HostSpecification("x")
            {
                Port = -3
            };
        }

        /// <summary>
        /// Bad port test for a <see cref="HostSpecification"/>.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void HostSpecificationBadPort2()
        {
            var h = new HostSpecification("x")
            {
                Port = 0xfffff
            };
        }

        /// <summary>
        /// Bad LU name test for a <see cref="HostSpecification"/>.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void HostSpecificationBadLuName()
        {
            var h = new HostSpecification("x");
            h.AddLogicalUnitName("@$");
        }

        /// <summary>
        /// Bad LU name test for a <see cref="HostSpecification"/>.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void HostSpecificationBadLuName2()
        {
            var h = new HostSpecification("x");
            h.AddLogicalUnitName(null);
        }

        /// <summary>
        /// Bad accept name test for a <see cref="HostSpecification"/>.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void HostSpecificationBadAcceptName()
        {
            var h = new HostSpecification("x")
            {
                AcceptName = "=fred"
            };
        }

        /// <summary>
        /// Bad accept name test for a <see cref="HostSpecification"/>.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void HostSpecificationBadAcceptName2()
        {
            var h = new HostSpecification("x")
            {
                AcceptName = null
            };
        }
    }
}
