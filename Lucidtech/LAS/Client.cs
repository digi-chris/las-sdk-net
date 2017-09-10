using System;
using System.Collections.Generic;
using System.IO;
using RestSharp;

namespace Lucidtech.LAS
{
    public class Detection
    {
        public string Label { get; set; }     
        public float Confidence { get; set; } 
        public string Value { get; set; }
    }
    
    public class Client
    {
        public string ApiKey { get; }
        public string Endpoint { get; }
        public string Stage { get; }
        public RestClient ApiClient { get; }
        
        public Client(string apiKey)
        {
            ApiKey = apiKey;
            Endpoint = "https://api.lucidtech.ai";
            Stage = "v0";
            ApiClient = new RestClient(Endpoint);
        }

        public List<Detection> ScanReceipt(string receiptUrl)
        {
            var endpoint = new Uri(string.Concat(Endpoint, "/", Stage, "/receipts"));
            
            var request = new RestRequest(endpoint, Method.POST);
            request.AddHeader("X-Api-Key", ApiKey);
            request.AddHeader("Content-Type", "image/jpeg");
            request.AddQueryParameter("url", receiptUrl);
            
            IRestResponse<List<Detection>> response = ApiClient.Execute<List<Detection>>(request);
            return response.Data;
        }
        
        public List<Detection> ScanReceipt(Stream receiptStream)
        {
            var endpoint = new Uri(string.Concat(Endpoint, "/", Stage, "/receipts"));
            
            var request = new RestRequest(endpoint, Method.POST);
            request.AddHeader("X-Api-Key", ApiKey);
            request.AddHeader("Content-Type", "image/jpeg");
            
            MemoryStream ms = new MemoryStream();
            receiptStream.CopyTo(ms);
            byte[] data = ms.ToArray();
            ms.Dispose();
            request.AddParameter("image/jpeg", data, ParameterType.RequestBody);
            
            IRestResponse<List<Detection>> response = ApiClient.Execute<List<Detection>>(request);
            return response.Data;
        }
    }
}
