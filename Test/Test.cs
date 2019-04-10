using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using NUnit.Framework;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Lucidtech.Las;
using Lucidtech.Las.Core;
using Lucidtech.Las.Utils;

namespace Test
{
    [TestFixture]
    public class TestApi
    {
        [Test]
        public void TestSendFeedback()
        {
            ApiClient apiClient = new ApiClient(Example.Endpoint());
            var postDocResponse = JsonSerialPublisher.ObjectToDict<Dictionary<string, string>>(
                apiClient.PostDocuments(Example.ContentType(), Example.ConsentId()));
            apiClient.PutDocument(Example.DocPath(),Example.ContentType(),postDocResponse["uploadUrl"]);
            var feedback = new List<Dictionary<string, string>>()
            {
                new Dictionary<string, string>(){{"label", "total_amount"},{"value", "54.50"}},
                new Dictionary<string, string>(){{"label", "purchase_date"},{"value", "2007-07-30"}}
            };
            var response = apiClient.SendFeedback(postDocResponse["documentId"], feedback);
            
            Console.WriteLine($"\n$ FeedbackResponse response = apiClient.SendFeedback(...);");
            Console.WriteLine(response.ToJsonString(Formatting.Indented));
            
            foreach (var field in response.Feedback)
            {
                Assert.IsTrue(field.ContainsKey("label"));
                Assert.IsTrue(field.ContainsKey("value"));
                Assert.IsTrue(field["label"] is string);
                Assert.IsTrue(field["value"] is string);
            }
            
        }
        [Test]
        public void TestRevokeConsent()
        {
            ApiClient apiClient = new ApiClient(Example.Endpoint());
            var postDocResponse = JsonSerialPublisher.ObjectToDict<Dictionary<string, string>>(
                apiClient.PostDocuments(Example.ContentType(), Example.ConsentId()));
            apiClient.PutDocument(
                Example.DocPath(),Example.ContentType(),(string)postDocResponse["uploadUrl"]);
            RevokeResponse response = apiClient.RevokeConsent(postDocResponse["consentId"]);
            
            Console.WriteLine($"\n$ RevokeResponse response = apiClient.RevokeConsent(...);");
            Console.WriteLine(response.ToJsonString(Formatting.Indented));
            
            Assert.IsTrue(response.ConsentId.Equals(Example.ConsentId()));
            foreach (var documentId in response.DocumentIds)
            {
                Assert.IsNotEmpty(documentId);
            }
            
        }
        [Test]
        public void TestPrediction()
        {
            ApiClient apiClient = new ApiClient(Example.Endpoint());
            Prediction response = apiClient.Predict(
                documentPath: Example.DocPath(),modelName: Example.ModelType(),consentId: Example.ConsentId());

            Console.WriteLine($"\n$ Predict response = apiClient.Predict(...);");
            Console.WriteLine(response.ToJsonString(Formatting.Indented));
            
            Assert.IsTrue(response.ConsentId.Equals(Example.ConsentId()));
            Assert.IsTrue(response.ModelName.Equals(Example.ModelType()));
            foreach (var field in response.Fields)
            {
                Assert.IsTrue(field.ContainsKey("label"));
                Assert.IsTrue(field.ContainsKey("value"));
                Assert.IsTrue(field.ContainsKey("confidence"));
                
                Assert.IsTrue(field["label"] is string);
                Assert.IsTrue(field["value"] is string);
                Assert.IsTrue(field["confidence"] is double);
            }
        }
    }
    public class TestClient
    {
        [Test]
        public void TestPutDocument()
        {
            Console.WriteLine("TestPutDocument");
            Client client = new Client(Example.Endpoint());
            var postDocResponse = JsonSerialPublisher.ObjectToDict<Dictionary<string, string>>(
                client.PostDocuments(Example.ContentType(), Example.ConsentId()));
            var response = client.PutDocument(
                Example.DocPath(),Example.ContentType(),(string)postDocResponse["uploadUrl"]);
            Assert.IsNull(response);
        }

        [Test]
        public void TestPostDocuments()
        {
            Console.WriteLine("TestPostDocuments");
            Client client = new Client(Example.Endpoint());
            var response = client.PostDocuments(Example.ContentType(), Example.ConsentId());
            var expected = new List<string>(){"documentId", "uploadUrl", "contentType", "consentId"};
            var dictResponse = JsonSerialPublisher.ObjectToDict<Dictionary<string, string>>(response);
            foreach (var key in expected)
            {
                Assert.IsTrue(dictResponse.ContainsKey(key));
                //Console.WriteLine($"{key}: {dictResponse[key]}");
            }
        }

        [Test]
        public void TestPostPredictions()
        {
            Console.WriteLine("TestPostPredictions");
            Client client = new Client(Example.Endpoint());
            var postDocResponse = JsonSerialPublisher.ObjectToDict<Dictionary<string, string>>(
                client.PostDocuments(Example.ContentType(), Example.ConsentId()));
            
            client.PutDocument(Example.DocPath(),Example.ContentType(),(string)postDocResponse["uploadUrl"]);
            
            string documentId = (string)postDocResponse["documentId"];
            var response = client.PostPredictions(documentId,Example.ModelType());
            
            var expected = new List<string>(){"documentId", "predictions"};
            JObject jsonResponse = JObject.Parse(response.ToString());
            foreach (var field in jsonResponse)
            {
                Assert.IsTrue(expected.Contains(field.Key));
                //Console.WriteLine($"Key: {field.Key}, Val: {field.Value.ToString()}" );
            }
        }

        [Test]
        public void TestPostDocumentId()
        {
            Console.WriteLine("TestPostDocumentId");
            Client client = new Client(Example.Endpoint());
            var postDocResponse = JsonSerialPublisher.ObjectToDict<Dictionary<string, string>>(
                client.PostDocuments(Example.ContentType(), Example.ConsentId()));
            
            client.PutDocument(Example.DocPath(),Example.ContentType(),postDocResponse["uploadUrl"]);
            
            var feedback = new List<Dictionary<string, string>>()
            {
                new Dictionary<string, string>(){{"label", "total_amount"},{"value", "54.50"}},
                new Dictionary<string, string>(){{"label", "purchase_date"},{"value", "2007-07-30"}}
            };
            
            var response = client.PostDocumentId(postDocResponse["documentId"], feedback);
            var expected = new List<string>(){"documentId", "consentId", "uploadUrl", "contentType", "feedback"};
            JObject jsonResponse = JObject.Parse(response.ToString());
            foreach (var field in jsonResponse)
            {
                Assert.IsTrue(expected.Contains(field.Key));
                //Console.WriteLine($"Key: {field.Key}, Val: {field.Value.ToString()}" );
            }
        }
        
        [Test]
        public void TestDeleteConsentId()
        {
            Console.WriteLine("TestDeleteConsentId");
            Client client = new Client(Example.Endpoint());
            var postDocResponse = JsonSerialPublisher.ObjectToDict<Dictionary<string, string>>(
                client.PostDocuments(Example.ContentType(), Example.ConsentId()));
            
            client.PutDocument(Example.DocPath(),Example.ContentType(),postDocResponse["uploadUrl"]);
            
            var expected = new List<string>(){"consentId", "documentIds"};
            var response = client.DeleteConsentId(postDocResponse["consentId"]);
            JObject jsonResponse = JObject.Parse(response.ToString());
            
            foreach (var field in jsonResponse)
            {
                Assert.IsTrue(expected.Contains(field.Key));
                //Console.WriteLine($"Key: {field.Key}, Val: {field.Value.ToString()}" );
            }
        }

        [Test]
        public void TestHashSigning()
        {
            Console.WriteLine("TestHashSigning");
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

            for (int i = 0; i < answers.Count; i++)
            {
                string key = testDict.Keys.ElementAt(i);
                byte[] bytes = Encoding.UTF8.GetBytes(key);
                byte[] res = AmazonAuthorization.SignHash(bytes, Encoding.UTF8.GetBytes(testDict[key]));
                Assert.Zero(string.CompareOrdinal(AmazonAuthorization.StringFromByteArray(res), answers[i]));
            }
        }
    }
    
    public static class Example
    {
        public static string ConsentId() { return "bar"; }
        public static string ContentType() { return "image/jpeg"; }
        public static string ModelType() { return "invoice"; }
        public static string Endpoint() { return "https://demo.api.lucidtech.ai/v1"; }
        public static string DocPath() { 
            return Environment.ExpandEnvironmentVariables(
                "%HOME%/dev/src/las-sdk-net-base/Test/Files/example.jpeg");
        }
    }
}