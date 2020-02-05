using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.XPath;
using NUnit.Framework;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Lucidtech.Las;
using Lucidtech.Las.Core;
using Lucidtech.Las.Utils;
using Newtonsoft.Json.Bson;
using RestSharp;

namespace Test
{
    [TestFixture]
    public class TestApi
    {

        private ApiClient Luke { get; set; }
        
        [OneTimeSetUp]
        public void Init()
        {
            //Luke = new ApiClient(Example.Endpoint());
            Luke = new ApiClient();
        }
        
        private static void CheckFields<T>(List<Dictionary<string, T>> fields, Dictionary<string, Type> expected) 
        {
            foreach (var field in fields)
            {
                foreach (var pair in expected)
                {
                    Assert.IsTrue(field.ContainsKey(pair.Key));
                    Assert.IsTrue(field[pair.Key].GetType() == pair.Value);
                }
            }
        }

        private Dictionary<string, string>PostDoc()
        {
            byte[] body = File.ReadAllBytes(Example.DocPath());
            var response = Luke.PostDocuments(body, Example.ContentType(), Example.ConsentId());
            return JsonSerialPublisher.ObjectToDict<Dictionary<string, string>>(response);
        }
        
        [Test]
        public void TestSendFeedback()
        {
            var postDocResponse = PostDoc();
            var feedback = new List<Dictionary<string, string>>()
            {
                new Dictionary<string, string>(){{"label", "total_amount"},{"value", "54.50"}},
                new Dictionary<string, string>(){{"label", "purchase_date"},{"value", "2007-07-30"}}
            };
            var response = Luke.SendFeedback(postDocResponse["documentId"], feedback);
            
            Console.WriteLine($"\n$ FeedbackResponse response = apiClient.SendFeedback(...);");
            Console.WriteLine(response.ToJsonString(Formatting.Indented));
            var expected = new Dictionary<string, Type>()
            {
                {"label", typeof(string)},
                {"value", typeof(string)},
            };
            CheckFields(response.Feedback, expected);
        }
        
        [Test]
        public void TestRevokeConsent()
        {
            var postDocResponse = PostDoc();
            RevokeResponse response = Luke.RevokeConsent(postDocResponse["consentId"]);
            
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
            var response = Luke.Predict(
                documentPath: Example.DocPath(), modelName: Example.ModelType(), consentId: Example.ConsentId());

            Console.WriteLine($"\n$ Predict response = apiClient.Predict(...);");
            Console.WriteLine(response.ToJsonString(Formatting.Indented));
            
            Assert.IsTrue(response.ConsentId.Equals(Example.ConsentId()));
            Assert.IsTrue(response.ModelName.Equals(Example.ModelType()));
            var expected = new Dictionary<string, Type>()
            {
                {"label", typeof(string)},
                {"value", typeof(string)},
                {"confidence", typeof(double)}
            };
            CheckFields(response.Fields, expected);
        }
    }

    [TestFixture]
    public class TestClient 
    {
        private Client Toby { get; set; }
        private Dictionary<string, object> PostDocResponse { get; set; }

        private static void CheckKeys(List<string> expected, object response)
        {
            var res = JsonSerialPublisher.ObjectToDict<Dictionary<string, object>>(response);
            Console.WriteLine(JsonConvert.SerializeObject(response, Formatting.Indented));
            foreach (var key in expected)
            {
                Assert.IsTrue(res.ContainsKey(key));
                Console.WriteLine($"{key}: {res[key]}");
            }
        }

        [OneTimeSetUp]
        public void InitClient()
        {
            //Toby = new Client(Example.Creds());
            Toby = new Client();
        }
        
        [SetUp]
        public void PostDocs()
        {
            byte[] body = File.ReadAllBytes(Example.DocPath());
            var response = Toby.PostDocuments(body, Example.ContentType(), Example.ConsentId());
            PostDocResponse = JsonSerialPublisher.ObjectToDict<Dictionary<string, object>>(response);
        }
        
        [Test]
        public void TestPostDocuments()
        {
            var expected = new List<string>(){"documentId", "contentType", "consentId"};
            CheckKeys(expected, PostDocResponse);
        }
        
        [Test]
        public void TestGetDocuments()
        {
			var response = Toby.GetDocuments();
            var expected = new List<string>(){"documentId", "contentType", "consentId"};
            CheckKeys(expected, response);
        }
        
        [Test]
        public void TestPostPredictions()
        {
            var response = Toby.PostPredictions((string)PostDocResponse["documentId"], Example.ModelName(), true, 1);
            //var response = Toby.PostPredictions(Example.DocumentId(), Example.ModelName(), true, 1);
            Console.WriteLine($"PostPredictions. {response}");
            var expected = new List<string>(){"documentId", "predictions"};
            CheckKeys(expected, response);
        }

        [Test]
        public void TestGetDocumentId()
        {
			//var response = Toby.GetDocumentId(Example.DocumentId());
			var response = Toby.GetDocumentId((string)PostDocResponse["documentId"]);
            var expected = new List<string>(){"documentId", "contentType", "consentId"};
            CheckKeys(expected, response);
        }
        
        [Test]
        public void TestPostDocumentId()
        {
            var feedback = new List<Dictionary<string, string>>()
            {
                new Dictionary<string, string>(){{"label", "total_amount"},{"value", "54.50"}},
                new Dictionary<string, string>(){{"label", "purchase_date"},{"value", "2007-07-30"}}
            };
            var response = Toby.PostDocumentId((string)PostDocResponse["documentId"], feedback);
            var expected = new List<string>(){"documentId", "consentId", "contentType", "feedback"};
            CheckKeys(expected, response);
        }
        
        [Test]
        public void TestDeleteConsentId()
        {
            var expected = new List<string>(){"consentId", "documentIds"};
            var response = Toby.DeleteConsentId((string)PostDocResponse["consentId"]);
            CheckKeys(expected, response);
        }

        [Test]
        public void TestPostBatches()
        {
			var response = Toby.PostBatches(Example.Description());
            var expected = new List<string>(){"batchId", "description"};
            CheckKeys(expected, PostDocResponse);
        }
        
    }

/*
    [TestFixture]
    public class TestClientKms
    {
        private Dictionary<string, object> PostDocResponse { get; set; }

        [Test]
        public void TestPrediction()
        {
            if (ExampleExtraFlags.Endpoint() == Example.Endpoint())
            {
                Console.WriteLine($"The Demo API does currently not support extra flags, use another endpoint"); 
                return;
            }
            
            var flags = new Dictionary<string, string>() {{"x-amz-server-side-encryption", "aws:kms"}};
            
            ApiClient apiClient = new ApiClient(ExampleExtraFlags.Endpoint(), new Credentials(
                ExampleExtraFlags.accessKey(), 
                ExampleExtraFlags.secretKey(), 
                ExampleExtraFlags.apiKey()));
            
            var postResponse = apiClient.PostDocuments(ExampleExtraFlags.ContentType(), ExampleExtraFlags.ConsentId());
            PostDocResponse = JsonSerialPublisher.ObjectToDict<Dictionary<string, object>>(postResponse);
            
            var putResponse = apiClient.PutDocument(ExampleExtraFlags.DocPath(), ExampleExtraFlags.ContentType(),
                (string) PostDocResponse["uploadUrl"], flags);
            
            var response = apiClient.PostPredictions((string) PostDocResponse["documentId"], ExampleExtraFlags.ModelType());
            
            JObject jsonResponse = JObject.Parse(response.ToString());
            var predictionString = jsonResponse["predictions"].ToString();
            var predictions = JsonSerialPublisher.DeserializeObject<List<Dictionary<string, Dictionary<string, object>>>>(predictionString);
            foreach (var line in predictions)
            {
                Console.WriteLine("\n New Line Found");
                foreach (var field in line)
                {
                    Console.WriteLine($"{field.Key}: {field.Value["val"]}, confidence: {field.Value["confidence"]}");
                }
            }
        }
    }

*/
    public static class Example 
    {

        public static byte[] Content() { return  Encoding.ASCII.GetBytes("%PDF-1.4foobarbaz");; }
        public static string ConsentId() { return "bar"; }
        public static string ContentType() { return "image/jpeg"; }
        public static string DocumentId() { return "abcdefghijklabcdefghijklabcdefghijkl"; }
        public static string Description() { return "This is my new batch for receipts july 2020"; }
        public static string ModelType() { return "invoice"; }
        public static string ModelName() { return "invoice"; }
        public static string Endpoint() { return "http://127.0.0.1:4010"; }
        public static string DocPath() { return Environment.ExpandEnvironmentVariables("Test/Files/example.jpeg"); }
		public static AmazonCredentials Creds() {return new AmazonCredentials("foo", "bar", "baz", "baaz", "http://127.0.0.1:4010"); }
    }

    public static class ExampleDocSplit
    {
        public static string ConsentId() { return "bar"; }
        public static string ContentType() { return "application/pdf"; }
        public static string ModelType() { return "documentSplit"; }
        public static string Endpoint() { return "https://demo.api.lucidtech.ai/v1"; }
        public static string DocPath() { return Environment.ExpandEnvironmentVariables("Test/Files/example.pdf"); }
    }

    public static class ExampleExtraFlags
    {
        public static string ConsentId() { return "bar"; }
        public static string ContentType() { return "application/pdf"; }
        public static string ModelType() { return "invoice"; }
        public static string Endpoint() { return "http://127.0.0.1:4010"; }
        public static string DocPath() { return Environment.ExpandEnvironmentVariables("Test/Files/example.pdf"); }

        public static string apiKey() { return ""; }
        public static string secretKey() { return ""; }
        public static string accessKey() { return ""; }
            
    }

}
