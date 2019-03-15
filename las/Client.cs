using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using RestSharp;
using Newtonsoft.Json;

using Lucidtech.Las.Cred;
using Lucidtech.Las.AWSSignatureV4;
using Lucidtech.Las.Serializer;

namespace Lucidtech.Las
{
	public class Client
	{

		/* URL information */
		private string Endpoint { get; }
		private string Stage { get; }

		private RestClient ApiClient { get; }

		public LasSerializer Serializer { get; }
		private Auth Access { get; }

		public Client()
		{
			var cred = new Credentials();
			Access = new Auth(cred);
			Endpoint = "https://demo.api.lucidtech.ai";
			Stage = "v1";
			ApiClient = new RestClient(Endpoint);
			Serializer = new LasSerializer(new Newtonsoft.Json.JsonSerializer());
		}

		private object JsonDecode(IRestResponse response)
		{
			if (response.StatusCode != System.Net.HttpStatusCode.OK)
			{
				throw new ApplicationException(response.ErrorMessage);
			}
			try
			{
				var jsonResponse = Serializer.DeserializeObject(response.Content);
				return jsonResponse;
			}
			catch (Exception e)
			{
				Console.WriteLine($"Error in response. Returned {e}");
				throw;	
			}	
		}

		public object PostDocuments(string contentType, string consentId)
		{
			var dictBody = new Dictionary<string, string>() { {"contentType", contentType}, {"consentId", consentId} };
			
			RestRequest request = ClientRestRequest(Method.POST, "/documents", dictBody);

			IRestResponse response = ApiClient.Execute(request);

			return JsonDecode(response);
		}

		public object PutDocument(string documentPath, string contentType, string presignedUrl)
		{
			byte[] body = File.ReadAllBytes(documentPath);
			
			var request = new RestRequest(Method.PUT);
			request.AddHeader("Content-Type", contentType);
			request.AddParameter(contentType, body, ParameterType.RequestBody);
			
			RestClient client = new RestClient(presignedUrl);
			
			IRestResponse response = client.Execute(request);
			
			return JsonDecode(response);

		}
		public object PostPredictions(string documentId, string modelName)
		{
			var dictBody = new Dictionary<string, string>() { {"documentId", documentId}, {"modelName", modelName} };
			
			RestRequest request = ClientRestRequest(Method.POST, "/predictions", dictBody);

			IRestResponse response = ApiClient.Execute(request);

			return JsonDecode(response);
		}
		public object PostDocumentId(string documentId, List<Dictionary<string, string>> feedback)
		{
			var dictBody = new Dictionary<string, List<Dictionary<string,string>>>() {{"feedback", feedback}};
			
			// Doing a manual cast from Dictionary to object to help out the serialization process
			string bodyString = JsonConvert.SerializeObject(dictBody);
			object body = JsonConvert.DeserializeObject(bodyString);
			
			RestRequest request = ClientRestRequest(Method.POST, $"/documents/{documentId}", body);

			IRestResponse response = ApiClient.Execute(request);

			return JsonDecode(response);
		}
		
		public object DeleteConsentId(string consentId)
		{
			var dictBody = new Dictionary<string, string>() {} ;
			
			RestRequest request = ClientRestRequest(Method.DELETE, $"/consents/{consentId}", dictBody);

			IRestResponse response = ApiClient.Execute(request);

			return JsonDecode(response);
		}
		
		public RestRequest ClientRestRequest(Method method, string path, object dictBody)
		{
			Uri endpoint = new Uri(string.Concat(Endpoint, "/", Stage, path));

			/* Create a request */
			var request = new RestRequest(endpoint, method);
			request.JsonSerializer = LasSerializer.Default;
			request.RequestFormat = DataFormat.Json;
			request.AddJsonBody(dictBody);

			/* Add the signing headers to the request */
			byte[] body = Encoding.UTF8.GetBytes(request.JsonSerializer.Serialize(dictBody));
			var headers = CreateSigningHeaders(method.ToString(), path, body);
			foreach (var entry in headers) { request.AddHeader(entry.Key, entry.Value); }

			return request;
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
        public static T ObjectToDict<T>(object obj)
        {
            var serial = JsonConvert.SerializeObject(obj); 
			return JsonConvert.DeserializeObject<T>(serial);
        }
    } // Class Client
} // Namespace Lucidtech.Las
