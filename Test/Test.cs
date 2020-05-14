using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;

using Newtonsoft.Json;
using Moq;
using Moq.Protected;

using Lucidtech.Las;
using Lucidtech.Las.Core;
using Lucidtech.Las.Utils;

namespace Test
{
    [TestFixture]
    public class TestApi
    {

        private ApiClient Luke { get; set; }

        [OneTimeSetUp]
        public void Init()
        {
            var mockCreds = new Mock<AmazonCredentials>("test", "test", "test", "test", "http://localhost:4010");
            mockCreds
              .Protected()
              .Setup<(string, DateTime)>("GetClientCredentials")
              .Returns(("foobar", DateTime.Now));
            mockCreds
              .Protected()
              .Setup("CommonConstructor");

            Luke = new ApiClient(mockCreds.Object);
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
        
        private Dictionary<string, object>CreateDoc()
        {
            byte[] body = File.ReadAllBytes(Example.DocPath());
            var response = Luke.CreateDocuments(body, Example.ContentType(), Example.ConsentId());
            var postDocResponse = JsonSerialPublisher.ObjectToDict<Dictionary<string, object>>(response);
            return postDocResponse;
        }
        
        [Test]
        public void TestSendFeedback()
        {
            var postDocResponse = CreateDoc();
            var feedback = new List<Dictionary<string, string>>()
            {
                new Dictionary<string, string>(){{"label", "total_amount"},{"value", "54.50"}},
                new Dictionary<string, string>(){{"label", "purchase_date"},{"value", "2007-07-30"}}
            };
            var response = Luke.SendFeedback((string)postDocResponse["documentId"], feedback);
            
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
            var postDocResponse = CreateDoc();
            RevokeResponse response = Luke.RevokeConsent((string)postDocResponse["consentId"]);
            
            Console.WriteLine($"\n$ RevokeResponse response = apiClient.RevokeConsent(...);");
            Console.WriteLine(response.ToJsonString(Formatting.Indented));
            
            //Assert.IsTrue(response.ConsentId.Equals(Example.ConsentId()));
            Assert.IsNotEmpty(response.ConsentId);
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
        private Dictionary<string, object> CreateDocResponse { get; set; }

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
          var mockCreds = new Mock<AmazonCredentials>("test", "test", "test", "test", "http://localhost:4010");

          mockCreds
            .Protected()
            .Setup<(string, DateTime)>("GetClientCredentials")
            .Returns(("foobar", DateTime.Now));
          mockCreds
            .Protected()
            .Setup("CommonConstructor");

          Toby = new Client(mockCreds.Object);
        }
        
        [SetUp]
        public void CreateDocs()
        {
            byte[] body = File.ReadAllBytes(Example.DocPath());
            var response = Toby.CreateDocuments(body, Example.ContentType(), Example.ConsentId());
            CreateDocResponse = JsonSerialPublisher.ObjectToDict<Dictionary<string, object>>(response);
        }
        
        [Test]
        public void TestCreateDocuments()
        {
            var expected = new List<string>(){"documentId", "contentType", "consentId", "batchId"};
            CheckKeys(expected, CreateDocResponse);
        }
        
        [Test]
        public void TestGetDocuments()
        {
            var response = Toby.GetDocuments();
            var expected = new List<string>(){"documents"};
            CheckKeys(expected, response);
        }
        
        [Test]
        public void TestCreatePredictionsBareMinimum()
        {
            var response = Toby.CreatePredictions((string)CreateDocResponse["documentId"], Example.ModelName());
            Console.WriteLine($"CreatePredictions. {response}");
            var expected = new List<string>(){"documentId", "predictions"};
            CheckKeys(expected, response);
        }

        [Test]
        public void TestCreatePredictionsMaxPages()
        {
            var response = Toby.CreatePredictions((string)CreateDocResponse["documentId"], Example.ModelName(),
                                                maxPages: 2);
            Console.WriteLine($"CreatePredictions. {response}");
            var expected = new List<string>(){"documentId", "predictions"};
            CheckKeys(expected, response);
        }

        [Test]
        public void TestCreatePredictionsAutoRotate()
        {
            var response = Toby.CreatePredictions((string)CreateDocResponse["documentId"], Example.ModelName(), 
                                                autoRotate: true);
            Console.WriteLine($"CreatePredictions. {response}");
            var expected = new List<string>(){"documentId", "predictions"};
            CheckKeys(expected, response);
        }

        [Test]
        public void TestCreatePredictionsExtras()
        {
            var extras = new Dictionary<string, object>() {{"maxPages", 1}};
            var response = Toby.CreatePredictions((string)CreateDocResponse["documentId"], Example.ModelName(), 
                                                extras: extras);
            Console.WriteLine($"CreatePredictions. {response}");
            var expected = new List<string>(){"documentId", "predictions"};
            CheckKeys(expected, response);
        }

        [Test]
        public void TestGetDocumentId()
        {
            var response = Toby.GetDocumentId((string)CreateDocResponse["documentId"]);
            var expected = new List<string>(){"documentId", "contentType", "consentId"};
            CheckKeys(expected, response);
        }
        
        [Test]
        public void TestCreateDocumentId()
        {
            var feedback = new List<Dictionary<string, string>>()
            {
                new Dictionary<string, string>(){{"label", "total_amount"},{"value", "54.50"}},
                new Dictionary<string, string>(){{"label", "purchase_date"},{"value", "2007-07-30"}}
            };
            var response = Toby.CreateDocumentId((string)CreateDocResponse["documentId"], feedback);
            var expected = new List<string>(){"documentId", "consentId", "contentType", "feedback"};
            CheckKeys(expected, response);
        }
        
        [Test]
        public void TestDeleteConsentId()
        {
            var expected = new List<string>(){"consentId", "documentIds"};
            var response = Toby.DeleteConsentId((string)CreateDocResponse["consentId"]);
            CheckKeys(expected, response);
        }

        [Test]
        public void TestCreateBatches()
        {
            var response = Toby.CreateBatches(Example.Description());
            var expected = new List<string>(){"batchId", "description"};
            CheckKeys(expected, response);
        }
        
    }

/*
    [TestFixture]
    public class TestClientKms
    {
        private Dictionary<string, object> CreateDocResponse { get; set; }

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
            
            var postResponse = apiClient.CreateDocuments(ExampleExtraFlags.ContentType(), ExampleExtraFlags.ConsentId());
            CreateDocResponse = JsonSerialPublisher.ObjectToDict<Dictionary<string, object>>(postResponse);
            
            var putResponse = apiClient.PutDocument(ExampleExtraFlags.DocPath(), ExampleExtraFlags.ContentType(),
                (string) CreateDocResponse["uploadUrl"], flags);
            
            var response = apiClient.CreatePredictions((string) CreateDocResponse["documentId"], ExampleExtraFlags.ModelType());
            
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
        public static AmazonCredentials Creds() 
        {
            return new AmazonCredentials("foo", "bar", "baz", "baaz", "http://127.0.0.1:4010"); 
        }
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
        public static string DocPath() 
        { 
            return Environment.ExpandEnvironmentVariables("Test/Files/example.pdf"); 
        }
        public static string apiKey() { return ""; }
        public static string secretKey() { return ""; }
        public static string accessKey() { return ""; }
            
    }

}
