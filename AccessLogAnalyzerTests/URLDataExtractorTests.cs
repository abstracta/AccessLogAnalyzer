using System;
using Abstracta.AccessLogAnalyzer;
using Abstracta.AccessLogAnalyzer.DataExtractors;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AccessLogAnalyzerTests
{
    [TestClass]
    public class URLDataExtractorTests
    {
        // todo add lot of more test cases here

        [TestMethod]
        [Owner("SDU")]
        public void TomcatURL_01()
        {
            const string format = "%A %b %B %H %m %p %q %r %s %t %U %v %T %I";
            const string input1 = "10.7.1.27 1384 1384 HTTP/1.1 GET 80 ?k1_wJacaF0ChzT3XyPtBDw== GET /seguridad/servlet/msjinhabilitado?" +
                "k1_wJacaF0ChzT3XyPtBDw== HTTP/1.1 200 [01/Aug/2014:00:01:33 -0300] /seguridad/servlet/msjinhabilitado as1.fucacnet 0.003 TP-Processor47";

            var tf = new TomcatDataExtractor(format);
            tf.SetLine(input1);

            Assert.AreEqual("as1.fucacnet", tf.RemoteHost, "HOST");
            Assert.AreEqual(200, tf.ResponseCode, "RCODE");
            Assert.AreEqual(1384, tf.ResponseSize, "RSIZE");
            Assert.AreEqual(0.003, tf.ResponseTime, "RTIME");
            Assert.AreEqual(DateTime.Parse("01/08/2014 00:01:33"), tf.Time, "TIME");
            Assert.AreEqual(TimeUnitType.Seconds, tf.TimeUnit, "TimeUnit");
            Assert.AreEqual("GET /seguridad/servlet/msjinhabilitado?k1_wJacaF0ChzT3XyPtBDw==", tf.Url, "URL");

            const string input2 = "10.7.1.27 65 65 HTTP/1.1 GET 80  GET /microcoop8/ HTTP/1.1 200 [01/Aug/2014:00:01:28 -0300] /microcoop8/ as1.fucacnet 0.000 TP-Processor20";
            tf.SetLine(input2);

            Assert.AreEqual("as1.fucacnet", tf.RemoteHost, "HOST");
            Assert.AreEqual(200, tf.ResponseCode, "RCODE");
            Assert.AreEqual(65, tf.ResponseSize, "RSIZE");
            Assert.AreEqual(0.000, tf.ResponseTime, "RTIME");
            Assert.AreEqual(DateTime.Parse("01/08/2014 00:01:28"), tf.Time, "TIME");
            Assert.AreEqual(TimeUnitType.Seconds, tf.TimeUnit, "TimeUnit");
            Assert.AreEqual("GET /microcoop8/", tf.Url, "URL");
        }

        [TestMethod]
        [Owner("SDU")]
        public void TomcatURL_02()
        {
            const string format = "%a %u %S %t \"%r\" %s %b %D";
            const string input1 =
                "192.168.240.152 - - [08/Aug/2014:21:02:08 -0300] \"POST /wscanales/servlet/uy.com.grupobbva.awscmconsultamovimientos HTTP/1.1\" 200 1322 31";

            var tf = new TomcatDataExtractor(format);
            tf.SetLine(input1);

            Assert.AreEqual("192.168.240.152", tf.RemoteHost, "HOST");
            Assert.AreEqual(200, tf.ResponseCode, "RCODE");
            Assert.AreEqual(1322, tf.ResponseSize, "RSIZE");
            Assert.AreEqual(31, tf.ResponseTime, "RTIME");
            Assert.AreEqual(DateTime.Parse("08/08/2014 21:02:08"), tf.Time, "TIME");
            Assert.AreEqual(TimeUnitType.Milliseconds, tf.TimeUnit, "TimeUnit");
            Assert.AreEqual("POST /wscanales/servlet/uy.com.grupobbva.awscmconsultamovimientos", tf.Url, "URL");
        }

        [TestMethod]
        [Owner("SDU")]
        public void TomcatURL_03()
        {
            const string format = " %D-%a %t \"%r\" %s %b";
            const string input1 =
                " 31-192.168.240.152 [08/Aug/2014:21:02:08 -0300] \"POST /wscanales/servlet/uy.com.grupobbva.awscmconsultamovimientos HTTP/1.1\" 200 1322";

            var tf = new TomcatDataExtractor(format);
            tf.SetLine(input1);

            Assert.AreEqual("192.168.240.152", tf.RemoteHost, "HOST");
            Assert.AreEqual(200, tf.ResponseCode, "RCODE");
            Assert.AreEqual(1322, tf.ResponseSize, "RSIZE");
            Assert.AreEqual(31, tf.ResponseTime, "RTIME");
            Assert.AreEqual(DateTime.Parse("08/08/2014 21:02:08"), tf.Time, "TIME");
            Assert.AreEqual(TimeUnitType.Milliseconds, tf.TimeUnit, "TimeUnit");
            Assert.AreEqual("POST /wscanales/servlet/uy.com.grupobbva.awscmconsultamovimientos", tf.Url, "URL");
        }

        [TestMethod]
        [Owner("SDU")]
        public void ApacheURL_01()
        {
            const string format = "%a %u %H %t %T \"%r\" %>s %b";
            const string input = "10.7.5.126 - - [01/Aug/2014:03:47:07 -0300] 0 \"GET /lafoto13.jpg HTTP/1.1\" 404 210";

            var tf = new ApacheDataExtractor(format);
            tf.SetLine(input);

            Assert.AreEqual("10.7.5.126", tf.RemoteHost, "HOST");
            Assert.AreEqual(404, tf.ResponseCode, "RCODE");
            Assert.AreEqual(210, tf.ResponseSize, "RSIZE");
            Assert.AreEqual(0, tf.ResponseTime, "RTIME");
            Assert.AreEqual(DateTime.Parse("01/08/2014 03:47:07"), tf.Time, "TIME");
            Assert.AreEqual(TimeUnitType.Seconds, tf.TimeUnit, "TimeUnit");
            Assert.AreEqual("GET /lafoto13.jpg", tf.Url, "URL");
        }

        [TestMethod]
        [Owner("SDU")]
        public void ApacheURL_02()
        {
            const string format = "%D-%a_%l %u %t \"%r\" %>s %b \"%{Referer}i\" \"%{User-Agent}i\"";
            const string input = "12-172.16.0.3_- - [25/Sep/2002:14:04:19 +0200] \"GET / HTTP/1.1\" 401 - \"\" \"Mozilla/5.0 (X11; U; Linux i686; en-US; rv:1.1) Gecko/20020827\"";

            var tf = new ApacheDataExtractor(format);
            tf.SetLine(input);

            Assert.AreEqual("172.16.0.3", tf.RemoteHost, "HOST");
            Assert.AreEqual(401, tf.ResponseCode, "RCODE");
            Assert.AreEqual(0, tf.ResponseSize, "RSIZE");
            Assert.AreEqual(12, tf.ResponseTime, "RTIME");
            Assert.AreEqual(DateTime.Parse("25/09/2002 14:04:19"), tf.Time, "TIME");
            Assert.AreEqual(TimeUnitType.Microseconds, tf.TimeUnit, "TimeUnit");
            Assert.AreEqual("GET /", tf.Url, "URL");
        }

        [TestMethod]
        [Owner("SDU")]
        public void AccessLogURL_01()
        {
            const string format = "HOST TIME URL RCODE RTIME";
            const string input = "172.19.2.75	18/12/2013 00:00:14	/manager/html	200	16";

            var tf = new AccessLogExtractor(format);
            tf.SetLine(input);

            Assert.AreEqual("172.19.2.75", tf.RemoteHost, "HOST");
            Assert.AreEqual(200, tf.ResponseCode, "RCODE");
            Assert.AreEqual(0, tf.ResponseSize, "RSIZE");
            Assert.AreEqual(16, tf.ResponseTime, "RTIME");
            Assert.AreEqual(DateTime.Parse("18/12/2013 00:00:14"), tf.Time, "TIME");
            Assert.AreEqual(TimeUnitType.Milliseconds, tf.TimeUnit, "TimeUnit");
            Assert.AreEqual("/manager/html", tf.Url, "URL");
        }

        [TestMethod]
        [Owner("SDU")]
        public void AccessLogURL_02()
        {
            const string format = "HOST TIME URL RCODE RTIME RSIZE MICROSECOND";
            const string input = "10.34.140.79	07/03/2014 07:20:37	GET /jkmanager/?cmd=update&from=list&w=TEINdmzAN-loadbalancer&sw=TEINdmzANn2-serverajp13&vwa=1	200	2191	6178";

            var tf = new AccessLogExtractor(format);
            tf.SetLine(input);

            Assert.AreEqual("10.34.140.79", tf.RemoteHost, "HOST");
            Assert.AreEqual(200, tf.ResponseCode, "RCODE");
            Assert.AreEqual(6178, tf.ResponseSize, "RSIZE");
            Assert.AreEqual(2191, tf.ResponseTime, "RTIME");
            Assert.AreEqual(DateTime.Parse("07/03/2014 07:20:37"), tf.Time, "TIME");
            Assert.AreEqual(TimeUnitType.Microseconds, tf.TimeUnit, "TimeUnit");
            Assert.AreEqual("GET /jkmanager/?cmd=update&from=list&w=TEINdmzAN-loadbalancer&sw=TEINdmzANn2-serverajp13&vwa=1", tf.Url, "URL");
        }

        [TestMethod]
        [Owner("SDU")]
        public void AccessLogURL_03()
        {
            const string format = "HOST TIME URL RCODE RTIME RSIZE SECOND";
            const string input = "10.7.5.126\t01/08/2014 03:47:07\t\"GET /lafoto13.jpg\"\t404\t2\t15241";

            var tf = new AccessLogExtractor(format);
            tf.SetLine(input);

            Assert.AreEqual("10.7.5.126", tf.RemoteHost, "HOST");
            Assert.AreEqual(404, tf.ResponseCode, "RCODE");
            Assert.AreEqual(15241, tf.ResponseSize, "RSIZE");
            Assert.AreEqual(2, tf.ResponseTime, "RTIME");
            Assert.AreEqual(DateTime.Parse("01/08/2014 03:47:07"), tf.Time, "TIME");
            Assert.AreEqual(TimeUnitType.Seconds, tf.TimeUnit, "TimeUnit");
            Assert.AreEqual("\"GET /lafoto13.jpg\"", tf.Url, "URL");
        }
    }
}
