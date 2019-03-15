using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using RestSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Lucidtech.Las.Cred;
using Lucidtech.Las.Filetype;
using Lucidtech.Las.AWSSignatureV4;

namespace Lucidtech.Las
{
	public class Prediction
	{
		
        private Dictionary<string, string> Essentials { get; }
        public List<Dictionary<string, object>> Fields{ get; }
        public Prediction(string documentId, string consentId, string modelName,
            List<Dictionary<string, object>> predictionResponse)
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

    /// <summary>
    /// A high level client to invoke api methods from Lucidtech AI Services.
    /// </summary
    /// <param name="endpoint">Domain endpoint of the api, e.g. https://&lt;prefix&gt;.api.lucidtech.ai/&lt;version&gt;.</param>
    /// <param name="credentials"> Credentials to use, instance of Class `~Las.Credentials`</param>
    ///            
    /// <returns>
    /// Prediction on document
    /// </returns>
    ///         
	public class Api : Client
	{
		/// <summary>
        ///	Run inference and create prediction on document, this method takes care of creating and uploaded document
        /// as well as running inference to create prediction on document.
        /// </summary>
        /// <param name="documentPath">Path to document to run inference on.</param>
        /// <param name="modelName"> The name of the model to use for inference </param>
        /// <param name="consentId"> An identifier to mark the owner of the document handle </param>
        ///            
        /// <returns>
        /// Prediction on document
        /// </returns>
        ///         
		public Prediction Predict(string documentPath, string modelName, string consentId)
		{
			string contentType = GetContentType(documentPath);
			consentId = string.IsNullOrEmpty(consentId) ? Guid.NewGuid().ToString() : consentId;
			string documentId = UploadDocument(documentPath, contentType, consentId);
			var predictionResponse = PostPredictions(documentId, modelName);
			
			JObject jsonResponse = JObject.Parse(predictionResponse.ToString());
			var predictionString = jsonResponse["predictions"].ToString();
			var predictions = Serializer.DeserializeObject<List<Dictionary<string, object>>>(predictionString);
			Prediction prediction = new Prediction(documentId, consentId, modelName, predictions); 
			return prediction;
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
