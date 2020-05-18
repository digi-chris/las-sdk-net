using System;
using System.IO;
using System.Collections.Generic;

using Newtonsoft.Json.Linq;

using Lucidtech.Las.Core;
using Lucidtech.Las.Utils;

namespace Lucidtech.Las
{
    
    /// <summary>
    /// A high level client to invoke API methods from Lucidtech AI Services.
    /// </summary>
    public class ApiClient : Client 
    {
        
        /// <summary>
        /// ApiClient constructor with credentials read from local file.
        /// </summary>
        public ApiClient() : base() {}
        
        /// <summary>
        /// ApiClient constructor.
        /// </summary>
        /// <param name="credentials"> Keys and credentials needed for authorization </param>
        public ApiClient(AmazonCredentials credentials) : base(credentials) {}

        /// <summary>
        /// Run inference and create prediction on document, this method takes care of creating and uploaded document
        /// as well as running inference to create prediction on document.
        /// </summary>
        /// <example> <code>
        /// using namespace Lucidtech.Las; 
        /// ApiClient apiClient = new ApiClient(); 
        /// Prediction response =
        /// apiClient.Predict(documentPath: "document.jpeg", modelName: "invoice", consentId: "bar"); 
        /// Console.WriteLine(response.ToJsonString(Formatting.Indented)); 
        /// </code></example>
        /// <param name="documentPath"> Path to document to run inference on </param>
        /// <param name="modelName"> The name of the model to use for inference </param>
        /// <param name="consentId"> An identifier to mark the owner of the document handle </param>
        /// <returns>
        /// Prediction on document
        /// </returns>
        public Prediction Predict(string documentPath, string modelName, string consentId = "default")
        {
            string contentType = GetContentType(documentPath);
            byte[] body = File.ReadAllBytes(documentPath);
            var createDocumentsResponse = JsonSerialPublisher.ObjectToDict<Dictionary<string, object>>(
                CreateDocument(body, contentType, consentId));
            string documentId = (string)createDocumentsResponse["documentId"];
            var predictionResponse = CreatePrediction(documentId, modelName);

            JObject jsonResponse = JObject.Parse(predictionResponse.ToString());
            var predictionString = jsonResponse["predictions"].ToString();
            var predictions = JsonSerialPublisher.DeserializeObject<List<Dictionary<string, object>>>(predictionString);
            Prediction prediction = new Prediction(documentId, consentId, modelName, predictions);
            return prediction;
        }

        /// <summary>
        /// Send feedback to the model.
        /// This method takes care of sending feedback related to a document specified by documentId.
        /// Feedback consists of ground truth values for the document specified as a List of Dictionaries.
        /// </summary>
        /// <example><code>
        /// using namespace Lucidtech.Las; 
        /// ApiClient apiClient = new ApiClient(); 
        /// var feedback = new List&lt;Dictionary&lt;string, string&gt;&gt;() 
        /// { 
        ///     new Dictionary&lt;string, string&gt;(){{"label", "total_amount"},{"value", "54.50"}}, 
        ///     new Dictionary&lt;string, string&gt;(){{"label", "purchase_date"},{"value", "2007-07-30"}} 
        /// }; 
        /// FeedbackResponse response = apiClient.SendFeedback(documentId: '&lt;documentId&gt;', feedback: feedback); 
        /// Console.WriteLine(response.ToJsonString(Formatting.Indented)); 
        /// </code></example>
        /// <param name="documentId"> Document id </param>
        /// <param name="feedback"> Ground truth values </param>
        /// <returns> Data that can be used to confirm that the feedback uploaded was successful </returns>
        public FeedbackResponse SendFeedback(string documentId, List<Dictionary<string, string>> feedback)
        {
            return new FeedbackResponse(UpdateDocument(documentId, feedback));
        }

        /// <summary>
        /// Revoke consent and delete all documents associated with consentId.
        /// Consent id is a parameter that is provided by the user upon making a prediction on a document.
        /// </summary>
        /// <example><code>
        /// using namespace Lucidtech.Las; 
        /// ApiClient apiClient = new ApiClient(); 
        /// RevokeResponse response = apiClient.RevokeConsent(consentId: '&lt;consentId&gt;'); 
        /// Console.WriteLine(response.ToJsonString(Formatting.Indented)); 
        /// </code></example>
        /// <param name="consentId"> Delete documents associated with this consent id </param>
        /// <returns> The document ids of the deleted documents, and their consent id </returns>
        public RevokeResponse RevokeConsent(string consentId)
        {
            return new RevokeResponse(DeleteConsent(consentId));
        }

        private static string GetContentType(string documentPath)
        {
            var supportedFormats = new Dictionary<string, string>()
                {
                    {"jpeg", "image/jpeg"},
                    {"pdf", "application/pdf"}
                };
            string fmt = FileType.WhatFile(documentPath);
            if (!supportedFormats.ContainsKey(fmt))
            {
                throw new FormatException($"The format of {documentPath} is not supported, use jpeg or pdf");
            }
            return supportedFormats[fmt];
        }
    } 
} 
