using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AccessLogAnalyzerTests
{
    [TestClass]
    public class AccessLogCreatorTests
    {
        [TestMethod]
        [Owner("SDU")]
        public void TestDateTimeParsing()
        {
            var d = DateTime.Parse("2014-12-23 01:29:21");

            Assert.AreEqual(d.Year, 2014); 
            Assert.AreEqual(d.Month, 12);
            Assert.AreEqual(d.Day, 23);

            Assert.AreEqual(d.Hour, 1);
            Assert.AreEqual(d.Minute, 29);
            Assert.AreEqual(d.Second, 21);
        }
    }
}

