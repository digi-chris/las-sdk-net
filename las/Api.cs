using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using RestSharp;
using Newtonsoft.Json;

using Lucidtech.Las.Cred;
using Lucidtech.Las.Filetype;
using Lucidtech.Las.AWSSignatureV4;

namespace Lucidtech.Las
{
	public class Prediction
	{
		
        private Dictionary<string, string> Essentials { get; }
        private Dictionary<string, string> Fields { get; }
        public Prediction(string documentId, string consentId, string modelName,
            Dictionary<string, string> predictionResponse)
        {
	        Essentials = new Dictionary<string, string>()
	        {
		        {"documentId", documentId},
		        {"consentId", consentId},
		        {"modelName", modelName}
	        };
	        Fields = predictionResponse;
        }

        public object this[string s]
        {
	        get
	        {
		        if(string.Equals("fields",s))
		        {
			        return Fields;
		        }
		        foreach (var entry in Essentials)
		        {
			        if (string.Equals(entry.Key, s))
			        {
				        return entry.Value;
				        
			        } 
		        }
		        throw new KeyNotFoundException($"{s} is not present in the Prediction class");
	        }
        }
	}

	public class Api : Client
	{
		public object Predict(string documentPath, string modelName, string consentId)
		{
			string contentType = GetContentType(documentPath);
			consentId = string.IsNullOrEmpty(consentId) ? Guid.NewGuid().ToString() : consentId;
			string documentId = UploadDocument(documentPath, contentType, consentId);
			var predictionResponse = PostPredictions(documentId, modelName);
			return predictionResponse;
		}

		private string UploadDocument(string documentPath, string contentType, string consentId)
		{
			var postDocumentsResponse = ObjectToDict<Dictionary<string, string>>(PostDocuments(contentType, consentId));
			string documentId = postDocumentsResponse["documentId"];
			string presignedUrl = postDocumentsResponse["uploadUrl"];
			PutDocument(documentPath, contentType, presignedUrl);
			return documentId;
		}
		private string GetContentType(string documentPath)
		{
			var supportedFormats = new Dictionary<string, string>()
				{
					{"jpeg", "image/jpeg"},
					{"pdf", "application/pdf"}
					
				};
			string fmt = FileTests.WhatFile(documentPath);
			if (supportedFormats.ContainsKey(fmt))
			{
				return supportedFormats[fmt];
			}
			else
			{
				throw new FormatException($"The format of {documentPath} is not supported, use jpeg or pdf");	
			}

		}

	} // Class Api
} // Namespace Lucidtech.Las
