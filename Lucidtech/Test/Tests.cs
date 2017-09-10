using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using Lucidtech.LAS;
using NUnit.Framework;

namespace Test
{
    [TestFixture]
    public class Tests
    {
        [Test]
        public void TestScanReceiptWithUrl()
        {
            string apiKey = ConfigurationManager.AppSettings["apiKey"];
            string receiptUrl = ConfigurationManager.AppSettings["receiptUrl"];
            
            var client = new Client(apiKey);
            List<Detection> detections = client.ScanReceipt(receiptUrl);
            
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
            string apiKey = ConfigurationManager.AppSettings["apiKey"];
            var client = new Client(apiKey);

            var bin = AppDomain.CurrentDomain.BaseDirectory;
            var receiptFile = string.Concat(bin, "/../../Files/img.jpeg");
            FileStream stream = new FileStream(receiptFile, FileMode.Open); 
            List<Detection> detections = client.ScanReceipt(stream);
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