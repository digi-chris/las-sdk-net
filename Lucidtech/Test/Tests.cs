using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Lucidtech;
using NUnit.Framework;

namespace Test
{
    [TestFixture]
    public class Tests
    {
        [Test]
        public void TestScanReceiptWithUrl()
        {
            const string apiKey = "";
            var client = new Client(apiKey);

            const string receiptUrl = "http://www.salesreceiptstore.com/gasoline-receipts/narrow-gasoline-receipt.JPG";
            List<Detection> detections = client.ScanReceiptWithUrl(receiptUrl);
            foreach (var detection in detections)
            {
                Console.WriteLine(detection.Label);
                Assert.IsNotNull(detection.Label);
                Console.WriteLine(detection.Confidence);
                Assert.IsNotNull(detection.Confidence);
                Console.WriteLine(detection.Value);
                Assert.IsNotNull(detection.Value);
            }
        }

        [Test]
        public void TestScanReceiptWithFile()
        {
            const string apiKey = "";
            var client = new Client(apiKey);

            var bin = AppDomain.CurrentDomain.BaseDirectory;
            var receiptFile = string.Concat(bin, "/../../Files/img.jpeg");
            List<Detection> detections = client.ScanReceiptWithFile(receiptFile);
            foreach (var detection in detections)
            {
                Console.WriteLine(detection.Label);
                Assert.IsNotNull(detection.Label);
                Console.WriteLine(detection.Confidence);
                Assert.IsNotNull(detection.Confidence);
                Console.WriteLine(detection.Value);
                Assert.IsNotNull(detection.Value);
            }
        }
    }
}