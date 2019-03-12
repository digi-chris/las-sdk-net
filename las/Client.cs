using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using RestSharp;
using Newtonsoft.Json;

using Lucidtech.Las.Cred;
using Lucidtech.Las.AWSSignatureV4;

namespace Lucidtech.Las
{
	public class Client
	{

		/* URL information */
		private string Endpoint { get; }
		private string Stage { get; }

		private RestClient ApiClient { get; }

		private Auth Access { get; }

		public Client()
		{
			var cred = new Credentials();
			Access = new Auth(cred);
			Endpoint = "https://demo.api.lucidtech.ai";
			Stage = "v1";
			ApiClient = new RestClient(Endpoint);
		}

		public RestRequest ClientRestRequest(Method method, string path, object dictBody)
		{
			Uri endpoint = new Uri(string.Concat(Endpoint, "/", Stage, path));

			/* Create a request */
			var request = new RestRequest(endpoint, method);

			request.AddJsonBody(dictBody);

			/* Add the signing headers to the request */
			byte[] body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(dictBody));
			var headers = CreateSigningHeaders(method.ToString(), path, body);
			foreach (var entry in headers) { request.AddHeader(entry.Key, entry.Value); }

			return request;
		}
		public IRestResponse PostDocuments(string contentType, string consentId)
		{
			var dictBody = new Dictionary<string, string>() { {"contentType", contentType}, {"consentId", consentId} };
			
			RestRequest request = ClientRestRequest(Method.POST, "/documents", dictBody);

			IRestResponse response = ApiClient.Execute(request);

			return response;
		}

		public IRestResponse PutDocument(string documentPath, string contentType, string presignedUrl)
		{
			var body = File.ReadAllBytes(documentPath);
			var request = new RestRequest(presignedUrl);
			request.AddBody(body);
			request.AddHeader("Content-Type", contentType);
			IRestResponse response = ApiClient.Put(request);
			return response;

		}
		public IRestResponse PostPredictions(string documentId, string modelName)
		{
			var dictBody = new Dictionary<string, string>() { {"documentId", documentId}, {"modelName", modelName} };
			
			RestRequest request = ClientRestRequest(Method.POST, "/documents", dictBody);

			IRestResponse response = ApiClient.Execute(request);

			return response;
		}
		public IRestResponse PostDocumentId(string documentId, List<Dictionary<string, string>> feedback)
		{
			var dictBody = new Dictionary<string, List<Dictionary<string,string>>>() { {"feedback", feedback}};
			
			RestRequest request = ClientRestRequest(Method.POST, $"/documents/{documentId}", dictBody);

			IRestResponse response = ApiClient.Execute(request);

			return response;
		}
		
		public IRestResponse DeleteConsentId(string consentId)
		{
			var dictBody = new Dictionary<string, string>() {} ;
			
			RestRequest request = ClientRestRequest(Method.DELETE, $"/consents/{consentId}", dictBody);

			IRestResponse response = ApiClient.Execute(request);

			return response;
		}
		
        private Dictionary<string, string> CreateSigningHeaders(string method, string path, byte[] body)
        {
	        var uri = new Uri(string.Concat(Endpoint,"/",Stage, path));
            Dictionary<string, string> headers = Access.SignHeaders(
            uri: uri,
            method: method,
            body: body);
            headers.Add("Content-Type", "application/json");
            
            return headers;
        }
    } // Class Client
} // Namespace Lucidtech.Las
