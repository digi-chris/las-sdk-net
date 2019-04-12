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
        private ApiClient Sara { get; set; }
        
        [OneTimeSetUp]
        public void Init()
        {
            Luke = new ApiClient(Example.Endpoint());
            Sara = new ApiClient(ExampleDocSplit.Endpoint());
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

        private Dictionary<string, string>PostAndPutDoc()
        {
            var postDocResponse = JsonSerialPublisher.ObjectToDict<Dictionary<string, string>>(
                Luke.PostDocuments(Example.ContentType(), Example.ConsentId()));
            Luke.PutDocument(Example.DocPath(),Example.ContentType(), postDocResponse["uploadUrl"]);
            return postDocResponse;
        }
        
        [Test]
        public void TestSendFeedback()
        {
            var postDocResponse = PostAndPutDoc();
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
            var postDocResponse = PostAndPutDoc();
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
        public void TestDocSpiltPrediction()
        {
            if (ExampleDocSplit.Endpoint() == Example.Endpoint())
            {
                Console.WriteLine($"The Demo API does currently not support Document split, use another endpoint"); 
                return;
            }
            var response = Sara.Predict(
                documentPath: ExampleDocSplit.DocPath(),
                modelName: ExampleDocSplit.ModelType(),
                consentId: ExampleDocSplit.ConsentId());

            Console.WriteLine($"\n$ Predict response = apiClient.Predict(...);");
            Console.WriteLine(response.ToJsonString(Formatting.Indented));
            
            Assert.IsTrue(response.ConsentId.Equals(ExampleDocSplit.ConsentId()));
            Assert.IsTrue(response.ModelName.Equals(ExampleDocSplit.ModelType()));
            var expected = new Dictionary<string, Type>()
            {
                {"type", typeof(string)},
                {"start", typeof(Int64)},
                {"end", typeof(Int64)},
                {"confidence", typeof(double)}
            };
            CheckFields(response.Fields, expected);
        }
        
        [Test]
        public void TestPrediction()
        {
            var response = Luke.Predict(
                documentPath: Example.DocPath(),modelName: Example.ModelType(),consentId: Example.ConsentId());

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

        private object PutDocument()
        {
            return Toby.PutDocument(Example.DocPath(), Example.ContentType(), (string) PostDocResponse["uploadUrl"]);
        }
        
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
            Toby = new Client(Example.Endpoint());
        }
        
        [SetUp]
        public void PostDocs()
        {
            var response = Toby.PostDocuments(Example.ContentType(), Example.ConsentId());
            PostDocResponse = JsonSerialPublisher.ObjectToDict<Dictionary<string, object>>(response);
        }
        
        [Test]
        public void TestPostDocuments()
        {
            var expected = new List<string>(){"documentId", "uploadUrl", "contentType", "consentId"};
            CheckKeys(expected, PostDocResponse);
        }
        
        [Test]
        public void TestPutDocument()
        {
            var response = PutDocument();
            Assert.IsNull(response);
        }

        [Test]
        public void TestPostPredictions()
        {
            PutDocument();     
            var response = Toby.PostPredictions((string)PostDocResponse["documentId"],Example.ModelType());
            var expected = new List<string>(){"documentId", "predictions"};
            CheckKeys(expected, response);
        }

        [Test]
        public void TestPostDocumentId()
        {
            PutDocument();
            var feedback = new List<Dictionary<string, string>>()
            {
                new Dictionary<string, string>(){{"label", "total_amount"},{"value", "54.50"}},
                new Dictionary<string, string>(){{"label", "purchase_date"},{"value", "2007-07-30"}}
            };
            var response = Toby.PostDocumentId((string)PostDocResponse["documentId"], feedback);
            var expected = new List<string>(){"documentId", "consentId", "uploadUrl", "contentType", "feedback"};
            CheckKeys(expected, response);
        }
        
        [Test]
        public void TestDeleteConsentId()
        {
            PutDocument();
            var expected = new List<string>(){"consentId", "documentIds"};
            var response = Toby.DeleteConsentId((string)PostDocResponse["consentId"]);
            CheckKeys(expected, response);
        }
    }

    public static class Example
    {
        public static string ConsentId() { return "bar"; }
        public static string ContentType() { return "image/jpeg"; }
        public static string ModelType() { return "invoice"; }
        public static string Endpoint() { return "https://demo.api.lucidtech.ai/v1"; }
        public static string DocPath() { return Environment.ExpandEnvironmentVariables("Test/Files/example.jpeg"); }
    }

    public static class ExampleDocSplit
    {
        public static string ConsentId() { return "bar"; }
        public static string ContentType() { return "application/pdf"; }
        public static string ModelType() { return "documentSplit"; }
        public static string Endpoint() { return "https://demo.api.lucidtech.ai/v1"; }
        public static string DocPath() { return Environment.ExpandEnvironmentVariables("Test/Files/example.pdf"); }
    }

}
