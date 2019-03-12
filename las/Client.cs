using System;
using System.Collections.Generic;
using System.Text;

using RestSharp;
using Newtonsoft.Json;

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
	        Access = new Auth();
	        Endpoint = "https://demo.api.lucidtech.ai";
	        Stage = "v1";
	        ApiClient = new RestClient(Endpoint);
        }
        public IRestResponse PostDocuments(string contentType, string consentId)
        {
	        /* Define essential info for the request */
			Method method = Method.POST;
	        string path = "/documents";
	        Uri endpoint = new Uri(string.Concat(Endpoint, "/", Stage, path));
	        
	        /* Create a request */
			var request = new RestRequest(endpoint,method);
			request.RequestFormat = DataFormat.Json;
	        
	        
			/* Define the content of the request */
	        var dictBody = new Dictionary<string, string>(){
		        {"contentType",contentType},
		        {"consentId",consentId}}; 
	        string jsonBody = JsonConvert.SerializeObject(dictBody);
	        byte[] body = Encoding.UTF8.GetBytes(jsonBody);
			request.AddJsonBody(dictBody);
			
			/* Add the signing headers to the request */
	        var headers = CreateSigningHeaders(method.ToString(), path, body);
			foreach (var entry in headers) { request.AddHeader(entry.Key, entry.Value); }
			
			/* Perform request */
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
