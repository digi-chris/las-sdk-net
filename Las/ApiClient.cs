using System;
using System.Collections.Generic;

using Newtonsoft.Json.Linq;

using Lucidtech.Las.Utils;
using Lucidtech.Las.Core;

namespace Lucidtech.Las
{
    /// <summary>
    /// A high level client to invoke api methods from Lucidtech AI Services.
    /// </summary
	public class ApiClient : Client 
	{
		/// <summary>
        ///	Run inference and create prediction on document, this method takes care of creating and uploaded document
        /// as well as running inference to create prediction on document.
        /// </summary>
        /// <param name="documentPath">Path to document to run inference on </param>
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
			var postDocumentsResponse = JsonSerialPublisher.ObjectToDict<Dictionary<string, string>>(PostDocuments(contentType, consentId));
			string documentId = postDocumentsResponse["documentId"];
			string presignedUrl = postDocumentsResponse["uploadUrl"];
			PutDocument(documentPath, contentType, presignedUrl);
			return documentId;
		}
		private static string GetContentType(string documentPath)
		{
			var supportedFormats = new Dictionary<string, string>()
				{
					{"jpeg", "image/jpeg"},
					{"pdf", "application/pdf"}
				};
			string fmt = FileType.WhatFile(documentPath);
			if (supportedFormats.ContainsKey(fmt))
			{
				return supportedFormats[fmt];
			}
			else
			{
				throw new FormatException($"The format of {documentPath} is not supported, use jpeg or pdf");	
			}
		}
	} 
} 
