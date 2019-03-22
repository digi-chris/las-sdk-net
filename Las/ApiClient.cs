using System;
using System.Collections.Generic;

using Newtonsoft.Json.Linq;

using Lucidtech.Las.Utils;
using Lucidtech.Las.Core;

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
        /// <param name="endpoint"> Url to the host </param>
        public ApiClient(string endpoint) : base(endpoint) { }
        
        /// <summary>
        /// ApiClient constructor with credentials read from local file.
        /// </summary>
        /// <param name="endpoint"> Url to the host </param>
        /// <param name="credentials"> Keys and credentials needed for authorization, see <see cref="Credentials"/> </param>
        public ApiClient(string endpoint, Credentials credentials) : base(endpoint, credentials) { }

        /// <summary>
        ///	Run inference and create prediction on document, this method takes care of creating and uploaded document
        /// as well as running inference to create prediction on document.
        /// <example> <code>
        /// using namespace Lucidtech.Las;
        /// ApiClient apiClient = new ApiClient('&lt;endpoint&gt;');
        /// Prediction response = apiClient.Predict(documentPath: "document.jpeg", modelName: "invoice", consentId: "bar");
        /// Console.WriteLine(response.ToJsonString(Formatting.Indented));
        /// </code></example>
        /// </summary>
        /// <param name="documentPath"> Path to document to run inference on </param>
        /// <param name="modelName"> The name of the model to use for inference </param>
        /// <param name="consentId"> An identifier to mark the owner of the document handle </param>
        ///            
        /// <returns>
        /// Prediction on document
        /// </returns>
        public Prediction Predict(string documentPath, string modelName, string consentId)
        {
            string contentType = GetContentType(documentPath);
            string documentId = UploadDocument(documentPath, contentType, consentId);
            var predictionResponse = PostPredictions(documentId, modelName);
            
            JObject jsonResponse = JObject.Parse(predictionResponse.ToString());
            var predictionString = jsonResponse["predictions"].ToString();
            var predictions = Serializer.DeserializeObject<List<Dictionary<string, object>>>(predictionString);
            Prediction prediction = new Prediction(documentId, consentId, modelName, predictions); 
            return prediction;
        }

        /// <summary>
        ///	Run inference and create prediction on document without specifying consent Id, this method takes care of creating and uploaded document
        /// as well as running inference to create prediction on document.
        /// <example> <code>
        /// using namespace Lucidtech.Las; \n
        /// ApiClient apiClient = new ApiClient('&lt;endpoint&gt;'); \n
        /// Prediction response = apiClient.Predict(documentPath: "document.jpeg", modelName: "invoice"); \n
        /// Console.WriteLine(response.ToJsonString(Formatting.Indented));
        /// </code></example>
        /// </summary>
        /// <param name="documentPath"> Path to document to run inference on </param>
        /// <param name="modelName"> The name of the model to use for inference </param>
        ///            
        /// <returns>
        /// Prediction on document
        /// </returns>
        public Prediction Predict(string documentPath, string modelName)
        {
            string consentId = Guid.NewGuid().ToString();
            return Predict(documentPath, modelName, consentId);
        }

        /// <summary>
        ///	Send feedback to the model.
        /// This method takes care of sending feedback related to document specified by documentId.
        /// Feedback consists of ground truth values for the document specified as a List of Dictionaries.
        /// <example> <code>
        /// using namespace Lucidtech.Las; \n
        /// ApiClient apiClient = new ApiClient('&lt;endpoint&gt;'); \n
        /// var feedback = new List&lt;Dictionary&lt;string, string&gt;&gt;() \n
        /// { \n
        ///     new Dictionary&lt;string, string&gt;(){{"label", "total_amount"},{"value", "54.50"}}, \n
        ///     new Dictionary&lt;string, string&gt;(){{"label", "purchase_date"},{"value", "2007-07-30"}} \n
        /// }; \n
        /// FeedbackResponse response = apiClient.SendFeedback(documentId: "&lt;documentId&gt;", feedback: feedback); \n
        /// Console.WriteLine(response.ToJsonString(Formatting.Indented));
        /// </code></example>
        /// </summary>
        /// <param name="documentId"> Document id </param>
        /// <param name="feedback"> Ground truth values </param>
        /// <returns> Data that can be used to confirm that the feedback uploaded was successful </returns>
        public FeedbackResponse SendFeedback(string documentId, List<Dictionary<string, string>> feedback)
        {
            return new FeedbackResponse(PostDocumentId(documentId, feedback));
        }

        /// <summary>
        /// Revoke consent and deleting all documents associated with consentId.
        /// Consent id is a parameter that is provided by the user upon making a prediction on a document.
        /// <example>
        /// <code>
        /// using namespace Lucidtech.Las; \n
        /// ApiClient apiClient = new ApiClient('&lt;endpoint&gt;'); \n
        /// RevokeResponse response = apiClient.RevokeConsent(consentId: '&lt;consentId&gt;');
        /// Console.WriteLine(response.ToJsonString(Formatting.Indented));
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="consentId"> Delete documents associated with this consent id </param>
        /// <returns> The document ids of the deleted documents, and their consent id </returns>
        public RevokeResponse RevokeConsent(string consentId)
        {
            return new RevokeResponse(DeleteConsentId(consentId));
        }

        /// <summary>
        /// Upload a document of type contentType currently located at documentPath to the cloud location
        /// that corresponds to consentId.
        /// </summary>
        /// <param name="documentPath"> The local path to the document that is going to be uploaded </param>
        /// <param name="contentType"> The type of the file located at documentPath </param>
        /// <param name="consentId"> The consent id </param>
        /// <returns></returns>
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
