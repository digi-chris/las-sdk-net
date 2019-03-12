using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

using NUnit.Framework;
using RestSharp;

using Lucidtech.Las;
using Lucidtech.Las.AWSSignatureV4;
using Lucidtech.Las.Cred;

namespace Test
{
    [TestFixture]
    public class Tests
    {
        [Test]
        public void TestPutDocument()
        {
            Client client = new Client();
            string presignedUrl = "FILLMEIN";
			IRestResponse response = client.PutDocument("image/jpeg", "application/json", presignedUrl);
            Console.WriteLine($"response status: {response.ResponseStatus}, and status code {response.StatusCode}");
            Assert.IsTrue(response.ResponseStatus==ResponseStatus.Completed);
            Assert.IsTrue(response.StatusCode==HttpStatusCode.OK);
        }
        [Test]
        public void TestPostDocuments()
        {
            Client client = new Client();
			IRestResponse response = client.PostDocuments("image/jpeg","bar");
            Console.WriteLine($"response status: {response.ResponseStatus}, and status code {response.StatusCode}");
            Assert.IsTrue(response.ResponseStatus==ResponseStatus.Completed);
            Assert.IsTrue(response.StatusCode==HttpStatusCode.OK);
        }

        [Test]
        public void TestPostPredictions()
        {
            Client client = new Client();
			IRestResponse response = client.PostPredictions("foo","bar");
            Console.WriteLine($"response status: {response.ResponseStatus}, and status code {response.StatusCode}");
            Assert.IsTrue(response.ResponseStatus==ResponseStatus.Completed);
            Assert.IsTrue(response.StatusCode==HttpStatusCode.OK);
        }

        [Test]
        public void TestPostDocumentId()
        {
            Client client = new Client();
            var feedback = new List<Dictionary<string, string>>();
            IRestResponse response = client.PostDocumentId("foo", feedback);
            Console.WriteLine($"response status: {response.ResponseStatus}, and status code {response.StatusCode}");
            Assert.IsTrue(response.ResponseStatus==ResponseStatus.Completed);
            Assert.IsTrue(response.StatusCode==HttpStatusCode.OK);
        }
        
        [Test]
        public void TestDeleteConsentId()
        {
            Client client = new Client();
            IRestResponse response = client.DeleteConsentId("Delete me");
            Console.WriteLine($"response status: {response.ResponseStatus}, and status code {response.StatusCode}");
            Assert.IsTrue(response.ResponseStatus==ResponseStatus.Completed);
            Assert.IsTrue(response.StatusCode==HttpStatusCode.OK);
        }

        [Test]
        public void TestHashSigning()
        {
            var testDict = new Dictionary<string, string>()
            {
                {"hello", "goodbye"},
                {"aws-signing-key", "testString"},
                {"12307875849320", "123456472890"},
                {"56789$%&)*(}|}", "$%^&&*()__&$#$%^**("}
            };
                
            var answers = new List<string>()
            {
                "8148a089d169a89a3ef0b22a6eb9abc1d57e7073a737c90a0378cf2c4e3994de",
                "744946f64d8580b720d51c35cfefbd349cf79668d2e8689a0dc4f2fd1273e153",
                "301f69db9dd8b78f9b25a6650fb766745c927907185a884628b7bdc565e823e7",
                "3d3bb37211f46ba09ff61a923b7dc21db17c6f4b2f03324c32d70fd6900243f3"
            };

            var cred = new Credentials("just", "some", "random", "stuff");
            var auth = new Auth(cred);
            for (int i = 0; i < answers.Count; i++)
            {
                /* Test input */
                string key = testDict.Keys.ElementAt(i);
		        byte[] bytes = Encoding.UTF8.GetBytes(key);
		        
		        /* Result */
                byte[] res = Auth.SignHash(bytes, Encoding.UTF8.GetBytes(testDict[key]));
                
                Assert.Zero(String.CompareOrdinal(Auth.StringFromByteArray(res), answers[i]));
            }
            
        }
        /*
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
        */
    }
}