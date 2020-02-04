using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Policy;
using System.Text;
using System.Web;
using Polly;

using RestSharp;
using Newtonsoft.Json;

using Lucidtech.Las.Core;
using Lucidtech.Las.Utils;

namespace Lucidtech.Las
{
    /// <summary>
    /// A low level client to invoke api methods from Lucidtech AI Services.
    /// </summary>
    public class Client 
    {
        private string Endpoint { get; }
        private RestClient RestSharpClient { get; }
        private AmazonCredentials Credentials { get; }

        /// <summary>
        /// Client constructor.
        /// </summary>
        /// <param name="endpoint"> Url to the host </param>
        /// <param name="credentials"> Keys and credentials needed for authorization </param>
        public Client(string endpoint, AmazonCredentials credentials)
        {
            Credentials = credentials;
            Endpoint = endpoint;
            var uri = new Uri(endpoint);
            RestSharpClient = new RestClient(uri.GetLeftPart(UriPartial.Authority));
        }
        
        /// <summary>
        /// Client constructor with credentials read from local file.
        /// </summary>
        /// <param name="endpoint"> Url to the host </param>
        public Client(string endpoint) : this(endpoint, new AmazonCredentials()) {}

        /// <summary>
        /// Creates a document handle, calls the POST /documents endpoint
        /// </summary>
        /// <example>
        /// Create a document handle for a jpeg image
        /// <code>
        /// Client client = new Client('&lt;endpoint&gt;');
        /// var response = client.PostDocuments("image/jpeg", "bar");
        /// </code>
        /// </example>
        /// <param name="contentType"> A mime type for the document handle </param>
        /// <param name="consentId"> An identifier to mark the owner of the document handle </param>
        /// <returns>
        /// A deserialized object that can be interpreted as a Dictionary with the fields
        /// with documentId, uploadUrl, contentType and consentId
        /// </returns>
        ///         
        public object PostDocuments(byte[] content, string contentType, string consentId, 
            string batchId = null, List<Dictionary<string, string>> feedback = null)
        {
            string base64Content = System.Convert.ToBase64String(content);
            //string base64String = System.Text.Encoding.UTF8.GetString(base64Content)
            var dictBody = new Dictionary<string, string>()
            {
                {"content", base64Content},
                {"contentType", contentType}, 
                {"consentId", consentId},
            };
            if (!string.IsNullOrEmpty(batchId)) { dictBody.Add("batchId", batchId); }
            if (feedback != null) { 
                string fb = JsonConvert.SerializeObject(feedback);
                dictBody.Add("feedback", fb);
            }
            
            RestRequest request = ClientRestRequest(Method.PATCH , "/documents", dictBody);
            return ExecuteRequestResilient(RestSharpClient, request);
        }

        /// <summary>
        /// Convenience method for putting a document to presigned url.
        /// </summary>
        /// <example>
        /// Put an example file to the location specified by a presigned url
        /// <code>
        /// Client client = new Client('&lt;endpoint&gt;'); 
        /// client.PutDocument("/full/path/to/example.jpeg","image/jpeg",'&lt;presignedUrl&gt;'); 
        /// </code>
        /// </example>
        /// <param name="documentPath"> Path to document to upload </param>
        /// <param name="contentType"> Mime type of document to upload.
        /// Same as provided to <see cref="PostDocuments"/></param>
        /// <param name="presignedUrl"> Presigned upload url from <see cref="PostDocuments"/> </param>
        /// <returns>
        /// An empty object 
        /// </returns>
        ///         
        public object PutDocument(string documentPath, string contentType, string presignedUrl, 
            Dictionary<string, string> additionalHeaders = null)
        {
            byte[] body = File.ReadAllBytes(documentPath);
            var request = new RestRequest(Method.PUT);
            request.AddHeader("Content-Type", contentType);
            if (additionalHeaders != null)
            {
                foreach (var pair in additionalHeaders)
                {
                    request.AddHeader(pair.Key, pair.Value);
                } 
            }
            request.AddParameter(contentType, body, ParameterType.RequestBody);
            RestClient client = new RestClient(presignedUrl);
            return ExecuteRequestResilient(client, request);
        }
        
        /// <summary>
        /// Run inference and create a prediction, calls the POST /predictions endpoint.
        /// </summary>
        /// <example>
        /// Run inference and create a prediction using the invoice model
        /// on the document specified by '&lt;documentId&gt;'
        /// <code>
        /// Client client = new Client('&lt;endpoint&gt;'); 
        /// var response = client.PostPredictions('&lt;documentId&gt;',"invoice"); 
        /// </code>
        /// </example>
        /// <param name="documentId"> Path to document to
        /// upload Same as provided to <see cref="PostDocuments"/></param>
        /// <param name="modelName"> Mime type of document to upload.
        /// Same as provided to <see cref="PostDocuments"/></param>
        /// <returns>
        /// A deserialized object that can be interpreted as a Dictionary with the fields documentId and predictions.
        /// the value of predictions is the output from the model
        /// </returns>
        public object PostPredictions(string documentId, string modelName, bool autoRotate = false, int maxPages = 0)
        {
            var dictBody = new Dictionary<string, object>() { {"documentId", documentId}, {"modelName", modelName}, {"autoRotate", autoRotate} };
			if (maxPages != 0) { dictBody.Add("maxPages", maxPages);}
            RestRequest request = ClientRestRequest(Method.POST, "/predictions", dictBody);
            return ExecuteRequestResilient(RestSharpClient, request);
        }
        
        /// <summary>
        /// Create a new batch for your documents, calls the POST /batches endpoint.
        /// </summary>
        /// <example>
        /// Create a new batch with the provided description.
        /// on the document specified by '&lt;documentId&gt;'
        /// <code>
        /// Client client = new Client(); 
        /// var response = client.PostBatches("training data gathered from the Mars Rover"); 
        /// </code>
        /// </example>
        /// <param name="description"> A brief description of the purpose of the batch
        /// <returns>
        /// A deserialized object that can be interpreted as a Dictionary with the fields batchId and description.
        /// batchId can be used as an input when posting documents to make them a part of this batch.
        /// </returns>
        public object PostBatches(string description)
        {
            var dictBody = new Dictionary<string, string>() { {"description", description} };
            RestRequest request = ClientRestRequest(Method.POST, "/batches", dictBody);
            return ExecuteRequestResilient(RestSharpClient, request);
        }
        
        /// <summary>
        /// Post feedback to the REST API, calls the POST /documents/{documentId} endpoint.
        /// Posting feedback means posting the ground truth data for the particular document.
        /// This enables the API to learn from past mistakes. /// </summary> /// <example>
        /// <code>
        /// Client client = new Client('&lt;endpoint&gt;'); 
        /// var feedback = new List&lt;Dictionary&lt;string, string&gt;&gt;() 
        /// { 
        ///     new Dictionary&lt;string, string&gt;(){{"label", "total_amount"},{"value", "54.50"}}, 
        ///     new Dictionary&lt;string, string&gt;(){{"label", "purchase_date"},{"value", "2007-07-30"}} 
        /// }; 
        /// var response = client.PostDocumentId('&lt;documentId&gt;', feedback); 
        /// </code>
        /// </example>
        /// <param name="documentId"> Path to document to upload,
        /// Same as provided to <see cref="PostDocuments"/></param>
        /// <param name="feedback"> A list of feedback items </param>
        /// <returns>
        /// A deserialized object that can be interpreted as a Dictionary with the fields
        /// documentId, consentId, uploadUrl, contentType and feedback.
        /// </returns>
        ///         
        public object PostDocumentId(string documentId, List<Dictionary<string, string>> feedback)
        {
            var dictBody = new Dictionary<string, List<Dictionary<string,string>>>() {{"feedback", feedback}};
            
            // Doing a manual cast from Dictionary to object to help out the serialization process
            string bodyString = JsonConvert.SerializeObject(dictBody);
            object body = JsonConvert.DeserializeObject(bodyString);
            
            RestRequest request = ClientRestRequest(Method.POST, $"/documents/{documentId}", body);
            return ExecuteRequestResilient(RestSharpClient, request);
        }
        
        /// <summary>
        /// Delete documents with this consentId, calls DELETE /consent/{consentId} endpoint.
        /// </summary>
        /// <example><code>
        /// Client client = new Client('&lt;endpoint&gt;'); 
        /// var response = client.DeleteConsentId('&lt;consentId&gt;'); 
        /// </code></example>
        /// <param name="consentId"> Delete documents with provided consentId </param>
        /// <returns>
        /// A deserialized object that can be interpreted as a Dictionary with the fields
        /// consentId and documentIds 
        /// </returns>
        public object DeleteConsentId(string consentId)
        {
            var dictBody = new Dictionary<string, string> {};
            RestRequest request = ClientRestRequest(Method.DELETE, $"/consents/{consentId}", dictBody);
            return ExecuteRequestResilient(RestSharpClient, request);
        }
        
        /// <summary>
        /// Create a HTTP web request for the REST API. 
        /// </summary>
        /// <param name="method"> The request method, e.g. POST, PUT, GET, DELETE </param>
        /// <param name="path"> The path to the domain upon which to apply the request,
        /// the total path will be <see cref="Endpoint"/>path</param>
        /// <param name="dictBody"> The content of the request </param>
        /// <returns>
        /// An object of type <see cref="RestRequest"/> defined by the input
        /// </returns>
        private RestRequest ClientRestRequest(Method method, string path, object dictBody)
        {
            Uri endpoint = new Uri(string.Concat(Endpoint, path));

            var request = new RestRequest(endpoint, method, DataFormat.Json);
            request.JsonSerializer = JsonSerialPublisher.Default;
            request.AddJsonBody(dictBody);

            byte[] body = Encoding.UTF8.GetBytes(request.JsonSerializer.Serialize(dictBody));
            var headers = CreateSigningHeaders(path);
            foreach (var entry in headers) { request.AddHeader(entry.Key, entry.Value); }

            return request;
        }
        

        private Dictionary<string, string> CreateSigningHeaders(string path)
        {
            var headers = new Dictionary<string, string>(){
                {"Authorization", $"Bearer {Credentials.AccessToken}"},
                {"X-Api-Key", Credentials.ApiKey}
                };
            headers.Add("Content-Type", "application/json");
            
            return headers;
        }

        private object ExecuteRequestResilient(RestClient client, RestRequest request)
        {
            var clogged = Policy
                .Handle<TooManyRequestsException>()
                .WaitAndRetry(new[]
                {
                    TimeSpan.FromSeconds(0.5),
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(2),
                    TimeSpan.FromSeconds(4)
                });
            var bad = Policy
                .Handle<RequestException>(e => !FatalCode(e.Response.StatusCode))
                .Retry();

            var policy = Policy.Wrap(clogged, bad);
            var result = policy.Execute(() => ExecuteRequest(client, request));
            return result;
        }
        
        private object ExecuteRequest(RestClient client, RestRequest request)
        {
            IRestResponse response = client.Execute(request);
            return JsonDecode(response);
        }

        private static bool FatalCode(HttpStatusCode code)
        {
            return 400 <= (int) code && (int) code < 500;
        }
        
        private object JsonDecode(IRestResponse response)
        {
            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                throw new InvalidCredentialsException("Credentials provided is not valid.");
            }
            if ( (int)response.StatusCode == 429 && response.Content.Contains("Too Many Requests"))
            {
                throw new TooManyRequestsException("You have reached the limit of requests per second.");
            }
            if ( (int)response.StatusCode == 429 && response.Content.Contains("Limit Exceeded"))
            {
                throw new LimitExceededException("You have reached the limit of total requests per month.");
            }
            if (response.ResponseStatus == ResponseStatus.Error || response.StatusCode != HttpStatusCode.OK)
            {
                throw new RequestException(response);
            }
            try
            {
                var jsonResponse = JsonSerialPublisher.DeserializeObject(response.Content);
                return jsonResponse;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error in response. Returned {e}");
                throw new Exception(response.ToString());
            }
        }
    } 
}

