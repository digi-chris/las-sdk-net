using System;
using System.Collections.Generic;
using System.Net;
using Polly;

using RestSharp;
using Newtonsoft.Json;

using Lucidtech.Las.Core;
using Lucidtech.Las.Utils;

namespace Lucidtech.Las
{
    /// <summary>
    /// Client to invoke api methods from Lucidtech AI Services.
    /// </summary>
    public class Client 
    {
        private RestClient RestSharpClient { get; }
        private Credentials LasCredentials { get; }

        /// <summary>
        /// Client constructor.
        /// </summary>
        /// <param name="credentials"> Keys, endpoints and credentials needed for authorization </param>
        public Client(Credentials credentials)
        {
            LasCredentials = credentials;
            var uri = new Uri(LasCredentials.ApiEndpoint);
            RestSharpClient = new RestClient(uri.GetLeftPart(UriPartial.Authority));
        }
        
        /// <summary>
        /// Client constructor with credentials read from local file.
        /// </summary>
        public Client() : this(new Credentials()) {}

        /// <summary>Creates an asset, calls the POST /assets endpoint.</summary>
        /// <example>
        /// <code>
        /// Client client = new Client();
        /// byte[] content = File.ReadAllBytes("myScript.js");
        /// client.CreateAsset(content);
        /// </code>
        /// </example>
        /// <param name="content">Asset content</param>
        /// <param name="attributes">Additional attributes</param>
        /// <returns>Asset response from REST API</returns>
        public object CreateAsset(byte[] content, Dictionary<string, string?>? attributes) {
            string base64Content = System.Convert.ToBase64String(content);
            var body = new Dictionary<string, string?>(){
                {"content", base64Content}
            };

            if (attributes != null) {
                foreach (KeyValuePair<string, string?> entry in attributes) {
                    body.Add(entry.Key, entry.Value);
                }
            }

            RestRequest request = ClientRestRequest(Method.POST, "/assets", body);
            return ExecuteRequestResilient(RestSharpClient, request);
        }

        /// <summary> List available assets, calls the GET /assets endpoint. </summary>
        /// <example>
        /// <code>
        /// Client client = new Client();
        /// var response = client.ListAssets();
        /// </code>
        /// </example>
        /// <param name="maxResults">Number of items to show on a single page</param>
        /// <param name="nextToken">Token to retrieve the next page</param>
        /// <returns>
        /// JSON object with two keys: 
        /// - "assets" Assets response from REST API without the content of each asset
        /// - "nextToken" allowing for retrieving the next portion of data
        /// </returns>
        public object ListAssets(int? maxResults = null, string? nextToken = null) {
            var queryParams = new Dictionary<string, object?>();

            if (maxResults != null) {
                queryParams.Add("maxResults", maxResults.ToString());
            }

            if (nextToken != null) {
                queryParams.Add("nextToken", nextToken);
            }

            RestRequest request = ClientRestRequest(Method.GET, "/assets", null, queryParams);
            return ExecuteRequestResilient(RestSharpClient, request);
        }

        /// <summary>Get asset from the REST API, calls the GET /assets/{assetId} endpoint.</summary>
        /// <example>
        /// <code>
        /// Client client = new Client();
        /// var response = client.GetAsset("&lt;asset_id&gt;");
        /// </code>
        /// </example>
        /// <param name="assetId">Asset ID</param>
        /// <returns>Asset object</returns>
        public object GetAsset(string assetId) {
            RestRequest request = ClientRestRequest(Method.GET, $"/assets/{assetId}");
            return ExecuteRequestResilient(RestSharpClient, request);
        }

        /// <summary>Updates an asset, calls the PATCH /assets/{assetId} endpoint.</summary>
        /// <example>
        /// <code>
        /// Client client = new Client();
        /// byte[] newContent = File.ReadAllBytes("MyScript.js");
        /// var response = client.UpdateAsset("&lt;asset_id&gt;", newContent);
        /// </code>
        /// </example>
        /// <param name="assetId">Asset ID</param>
        /// <param name="content">New content</param>
        /// <param name="attributes">Additional attributes</param>
        /// <returns>Asset object</returns>
        public object UpdateAsset(string assetId, byte[]? content = null, Dictionary<string, string?>? attributes = null) {
            var body = new Dictionary<string, string?>();

            if (content != null) {
                string base64Content = System.Convert.ToBase64String(content);
                body.Add("content", base64Content);
            }

            if (attributes != null) {
                foreach (KeyValuePair<string, string?> entry in attributes) {
                    body.Add(entry.Key, entry.Value);
                }
            }

            RestRequest request = ClientRestRequest(Method.PATCH, $"/assets/{assetId}", body);
            return ExecuteRequestResilient(RestSharpClient, request);
        }

        /// <summary>
        /// Creates a document handle, calls the POST /documents endpoint
        /// </summary>
        /// <example>
        /// Create a document handle for a jpeg image
        /// <code>
        /// Client client = new Client();
        /// byte[] content = File.ReadAllBytes("MyReceipt.jpeg");
        /// var response = client.CreateDocument(content, "image/jpeg", "bar");
        /// </code>
        /// </example>
        /// <param name="content"> Content to POST </param>
        /// <param name="contentType"> A mime type for the document handle </param>
        /// <param name="consentId"> An identifier to mark the owner of the document handle </param>
        /// <param name="batchId"> Specifies the batch to which the document will be associated with </param>
        /// <param name="groundTruth"> A list of items {label: value},
        /// representing the ground truth values for the document </param>
        /// <returns>
        /// A deserialized object that can be interpreted as a Dictionary with the fields
        /// with batchId, documentId, contentType and consentId
        /// </returns>
        public object CreateDocument(
            byte[] content,
            string contentType,
            string? consentId = null,
            string? batchId = null,
            List<Dictionary<string, string>>? groundTruth = null)
        {
            string base64Content = System.Convert.ToBase64String(content);
            var body = new Dictionary<string, string?>()
            {
                {"content", base64Content},
                {"contentType", contentType}, 
            };

            if(consentId != null) {
                body.Add("consentId", consentId);
            }
            
            if (!string.IsNullOrEmpty(batchId)) {
                body.Add("batchId", batchId);
            }

            if (groundTruth != null) { 
                string fb = JsonConvert.SerializeObject(groundTruth);
                body.Add("groundTruth", fb);
            }
            
            RestRequest request = ClientRestRequest(Method.POST, "/documents", body);
            return ExecuteRequestResilient(RestSharpClient, request);
        }

        /// <summary>
        /// Get documents from the REST API, calls the GET /documents endpoint.
        /// </summary>
        /// <example>
        /// Create a document handle for a jpeg image
        /// <code>
        /// Client client = new Client();
        /// var response = client.ListDocuments('&lt;batchId&gt;');
        /// </code>
        /// </example>
        /// <param name="batchId"> The batch id that contains the documents of interest </param>
        /// <param name="consentId"> An identifier to mark the owner of the document handle </param>
        /// <param name="maxResults">Number of items to show on a single page</param>
        /// <param name="nextToken">Token to retrieve the next page</param>
        /// <returns> Documents from REST API contained in batch </returns>
        public object ListDocuments(
            string? batchId = null, 
            string? consentId = null, 
            int? maxResults = null, 
            string? nextToken = null
        ) {
            var queryParams = new Dictionary<string, object?>();

            if (!string.IsNullOrEmpty(batchId)) {
                queryParams.Add("batchId", batchId);
            }

            if (!string.IsNullOrEmpty(consentId)) {
                queryParams.Add("consentId", consentId);
            }

            if (maxResults != null) {
                queryParams.Add("maxResults", maxResults.ToString());
            }

            if (nextToken != null) {
                queryParams.Add("nextToken", nextToken);
            }

            RestRequest request = ClientRestRequest(Method.GET, "/documents", null, queryParams);
            return ExecuteRequestResilient(RestSharpClient, request);
        } 
        
        /// <summary>
        /// Get document from the REST API, calls the GET /documents/{documentId} endpoint.
        /// </summary>
        /// <example>
        /// Get information of document specified by &lt;documentId&gt;
        /// <code>
        /// Client client = new Client();
        /// var response = client.GetDocument('&lt;documentId&gt;');
        /// </code>
        /// </example>
        /// <param name="documentId"> The document id to run inference and create a prediction on </param>
        /// <returns> Document information from REST API </returns>
        public object GetDocument(string documentId) {
            RestRequest request = ClientRestRequest(Method.GET, $"/documents/{documentId}");
            return ExecuteRequestResilient(RestSharpClient, request);
        }

        /// <summary>
        /// Update ground truth of the document, calls the POST /documents/{documentId} endpoint.
        /// This enables the API to learn from past mistakes. 
        /// </summary> 
        /// <example>
        /// <code>
        /// Client client = new Client();
        /// var groundTruth = new List&lt;Dictionary&lt;string, string&gt;&gt;()
        /// {
        ///     new Dictionary&lt;string, string&gt;(){{"label", "total_amount"},{"value", "54.50"}},
        ///     new Dictionary&lt;string, string&gt;(){{"label", "purchase_date"},{"value", "2007-07-30"}}
        /// };
        /// var response = client.UpdateDocument('&lt;documentId&gt;', groundTruth);
        /// </code>
        /// </example>
        /// <param name="documentId"> Path to document to upload,
        /// Same as provided to <see cref="CreateDocument"/></param>
        /// <param name="groundTruth"> A list of ground truth items </param>
        /// <returns>
        /// A deserialized object that can be interpreted as a Dictionary with the fields
        /// documentId, consentId, uploadUrl, contentType and ground truth.
        /// </returns>
        ///
        public object UpdateDocument(string documentId, List<Dictionary<string, string>> groundTruth)
        {
            var bodyDict = new Dictionary<string, List<Dictionary<string,string>>>() {{"groundTruth", groundTruth}};
            
            // Doing a manual cast from Dictionary to object to help out the serialization process
            string bodyString = JsonConvert.SerializeObject(bodyDict);
            object body = JsonConvert.DeserializeObject(bodyString);
            
            RestRequest request = ClientRestRequest(Method.PATCH, $"/documents/{documentId}", body);
            return ExecuteRequestResilient(RestSharpClient, request);
        }

        /// <summary>
        /// Delete documents with specified consentId, calls DELETE /documents endpoint.
        /// </summary>
        /// <example><code>
        /// Client client = new Client();
        /// var response = client.DeleteConsent('&lt;consentId&gt;');
        /// </code></example>
        /// <param name="consentId"> Delete documents with provided consentId </param>
        /// <returns>
        /// A deserialized object that can be interpreted as a Dictionary with the fields
        /// consentId and documentIds 
        /// </returns>
        public object DeleteDocuments(string? consentId = null) {
            var queryParams = new Dictionary<string, object?>();

            if (consentId != null) {
                queryParams.Add("consentId", consentId);
            }

            RestRequest request = ClientRestRequest(Method.DELETE, "/documents", null, queryParams);
            return ExecuteRequestResilient(RestSharpClient, request);
        }

        /// <summary>
        /// Create a batch handle, calls the POST /batches endpoint.
        /// </summary>
        /// <example>
        /// Create a new batch with the provided description.
        /// on the document specified by '&lt;batchId&gt;'
        /// <code>
        /// Client client = new Client();
        /// var response = client.CreateBatch("Data gathered from the Mars Rover Invoice Scan Mission");
        /// </code>
        /// </example>
        /// <param name="name">Name of the batch</param>
        /// <param name="description"> A brief description of the purpose of the batch </param>
        /// <returns>
        /// A deserialized object that can be interpreted as a Dictionary with the fields batchId and description.
        /// batchId can be used as an input when posting documents to make them a part of this batch.
        /// </returns>
        public object CreateBatch(string? name = null, string? description = null)
        {
            var body = new Dictionary<string, string?>();

            if (name != null) {
                body.Add("name", name);
            }

            if (description != null) {
                body.Add("description", description);
            }

            RestRequest request = ClientRestRequest(Method.POST, "/batches", body);
            return ExecuteRequestResilient(RestSharpClient, request);
        }

        /// <summary>
        /// Run inference and create a prediction, calls the POST /predictions endpoint.
        /// </summary>
        /// <example>
        /// Run inference and create a prediction using the invoice model
        /// on the document specified by '&lt;documentId&gt;'
        /// <code>
        /// Client client = new Client();
        /// var response = client.CreatePrediction('&lt;documentId&gt;',"las:model:99cac468f7cf47ddad12e5e017540389");
        /// </code>
        /// </example>
        /// <param name="documentId"> Path to document to
        /// upload Same as provided to <see cref="CreateDocument"/></param>
        /// <param name="modelId"> Id of the model to use for inference </param>
        /// <param name="maxPages"> Maximum number of pages to run predictions on </param>
        /// <param name="autoRotate"> Whether or not to let the API try different 
        /// rotations on the document when running </param>
        /// <param name="extras"> Extra information to add to json body </param>
        /// <returns>
        /// A deserialized object that can be interpreted as a Dictionary with the fields documentId and predictions,
        /// the value of predictions is the output from the model.
        /// </returns>
        public object CreatePrediction(
            string documentId,
            string modelId,
            int? maxPages = null,
            bool? autoRotate = null
        )
        {
            var body = new Dictionary<string, object>() { {"documentId", documentId}, {"modelId", modelId}};
            if (maxPages != null) { body.Add("maxPages", maxPages);}
            if (autoRotate != null) { body.Add("autoRotate", autoRotate);}

            RestRequest request = ClientRestRequest(Method.POST, "/predictions", body);
            return ExecuteRequestResilient(RestSharpClient, request);
        }

        /// <summary>List predictions available, calls the GET /predictions endpoint.</summary>
        /// <example>
        /// <code>
        /// Client client = new Client();
        /// var response = client.ListPredictions();
        /// </code>
        /// </example>
        /// <param name="maxResults">Number of items to show on a single page</param>
        /// <param name="nextToken">Token to retrieve the next page</param>
        /// <returns>
        /// JSON object with two keys: 
        /// - "predictions" which contains a list of Prediction objects
        /// - "nextToken" allowing for retrieving the next portion of data
        /// </returns>
        public object ListPredictions(int? maxResults = null, string? nextToken = null) {
            var queryParams = new Dictionary<string, object?>();

            if (maxResults != null) {
                queryParams.Add("maxResults", maxResults.ToString());
            }

            if (nextToken != null) {
                queryParams.Add("nextToken", nextToken);
            }

            RestRequest request = ClientRestRequest(Method.GET, "/predictions", null, queryParams);
            return ExecuteRequestResilient(RestSharpClient, request);
        }

        /// <summary>List models available, calls the GET /models endpoint.</summary>
        /// <example>
        /// <code>
        /// Client client = new Client();
        /// var response = client.ListModels();
        /// </code>
        /// </example>
        /// <param name="maxResults">Number of items to show on a single page</param>
        /// <param name="nextToken">Token to retrieve the next page</param>
        /// <returns>
        /// JSON object with two keys: 
        /// - "models" which contains a list of Prediction objects
        /// - "nextToken" allowing for retrieving the next portion of data
        /// </returns>
        public object ListModels(int? maxResults = null, string? nextToken = null) {
            var queryParams = new Dictionary<string, object?>();

            if (maxResults != null) {
                queryParams.Add("maxResults", maxResults.ToString());
            }

            if (nextToken != null) {
                queryParams.Add("nextToken", nextToken);
            }

            RestRequest request = ClientRestRequest(Method.GET, "/models", null, queryParams);
            return ExecuteRequestResilient(RestSharpClient, request);
        }

        /// <summary>Creates an secret, calls the POST /secrets endpoint.</summary>
        /// <example>
        /// <code>
        /// Client client = new Client();
        /// var data = new Dictionary&lt;string, string&gt;{
        ///     {"key", "my_secret_value"}
        /// }
        /// var response = client.CreateSecret(data);
        /// </code>
        /// </example>
        /// <param name="data">A dictionary containing values to be hidden</param>
        /// <param name="attributes">Additional attributes</param>
        /// <returns>A Secret object</returns>
        public object CreateSecret(Dictionary<string, string> data, Dictionary<string, string?>? attributes = null) {
            var body = new Dictionary<string, object?>() {
                {"data", data}
            };

            if (attributes != null) {
                foreach (KeyValuePair<string, string?> entry in attributes) {
                    body.Add(entry.Key, entry.Value);
                }
            }

            RestRequest request = ClientRestRequest(Method.POST, "/secrets", body);
            return ExecuteRequestResilient(RestSharpClient, request);
        }

        /// <summary>List secrets available, calls the GET /secrets endpoint.</summary>
        /// <example>
        /// <code>
        /// Client client = new Client();
        /// var response = client.ListSecrets();
        /// </code>
        /// </example>
        /// <param name="maxResults">Number of items to show on a single page</param>
        /// <param name="nextToken">Token to retrieve the next page</param>
        /// <returns>
        /// JSON object with two keys: 
        /// - "secrets" which contains a list of Prediction objects
        /// - "nextToken" allowing for retrieving the next portion of data
        /// </returns>
        public object ListSecrets(int? maxResults = null, string? nextToken = null) {
            var queryParams = new Dictionary<string, object?>();

            if (maxResults != null) {
                queryParams.Add("maxResults", maxResults.ToString());
            }

            if (nextToken != null) {
                queryParams.Add("nextToken", nextToken);
            }

            RestRequest request = ClientRestRequest(Method.GET, "/secrets", null, queryParams);
            return ExecuteRequestResilient(RestSharpClient, request);
        }

        /// <summary>Updates a secret, calls the PATCH /secrets/secretId endpoint.</summary>
        /// <example>
        /// <code>
        /// Client client = new Client();
        /// var data = new Dictionary&lt;string, string&gt;{
        ///     {"key", "my_new_secret_value"}
        /// }
        /// var response = client.UpdateSecret("&lt;secretId&gt;", data);
        /// </code>
        /// </example>
        /// <param name="secretId">Secret ID</param>
        /// <param name="data">New data</param>
        /// <param name="attributes">Additional attributes</param>
        public object UpdateSecret(
            string secretId,
            Dictionary<string, string>? data,
            Dictionary<string, string?>? attributes = null
        ) {
            var body = new Dictionary<string, object?>();

            if (attributes != null) {
                foreach (KeyValuePair<string, string?> entry in attributes) {
                    body.Add(entry.Key, entry.Value);
                }
            }

            if (data != null) {
                body.Add("data", data);
            }

            RestRequest request = ClientRestRequest(Method.PATCH, $"/secrets/{secretId}", body);
            return ExecuteRequestResilient(RestSharpClient, request);
        }

        /// <summary>Creates a transition, calls the POST /transitions endpoint.</summary>
        /// <example>
        /// <code>
        /// Client client = new Client();
        /// var inputSchema = new Dictionary&lt;string, string&gt;{
        ///     {"$schema", "https://json-schema.org/draft-04/schema#"},
        ///     {"title", "input"}
        /// };
        /// var outputSchema = new Dictionary&lt;string, string&gt;{
        ///     {"$schema", "https://json-schema/draft-04/schema#"},
        ///     {"title", "output"}
        /// };
        /// var params = new Dictionary&lt;string, object&gt;{
        ///     {"imageUrl", "&lt;image_url&gt;"},
        ///     {"credentials", new Dictionary&lt;string, string&gt;{
        ///         {"username", "&lt;username&gt;"},
        ///         {"password", "&lt;password&gt;"}
        ///     }
        /// };
        /// var response = client.CreateTransition("&lt;transition_type&gt;", inputSchema, outputSchema, parameters: params);
        /// </code>
        /// </example>
        /// <param name="transitionType">Type of transition: "docker"|"manual"</param>
        /// <param name="inputJsonSchema">Json-schema that defines the input to the transition</param>
        /// <param name="outputJsonSchema">Json-schema that defines the output of the transition</param>
        /// <param name="parameters">Parameters to the corresponding transition type</param>
        /// <param name="attributes">Additional attributes</param>
        /// <returns>Transition response from REST API</returns>
        public object CreateTransition(
            string transitionType,
            Dictionary<string, string>? inputJsonSchema = null,
            Dictionary<string, string>? outputJsonSchema = null,
            Dictionary<string, object?>? parameters = null,
            Dictionary<string, string?>? attributes = null
        ) {
            var body = new Dictionary<string, object?>() {
                {"transitionType", transitionType},
            };

            if (inputJsonSchema != null) {
                body.Add("inputJsonSchema", inputJsonSchema);
            }

            if (outputJsonSchema != null) {
                body.Add("outputJsonSchema", outputJsonSchema);
            }

            if (parameters != null) {
                body.Add("parameters", parameters);
            }

            if (attributes != null) {
                foreach (KeyValuePair<string, string?> entry in attributes) {
                    body.Add(entry.Key, entry.Value);
                }
            }

            RestRequest request = ClientRestRequest(Method.POST, "/transitions", body);
            return ExecuteRequestResilient(RestSharpClient, request);
        }

        /// <summary>List transitions, calls the GET /transitions endpoint.</summary>
        /// <example>
        /// <code>
        /// Client client = new Client();
        /// var response = client.ListTransitions();
        /// </code>
        /// </example>
        /// <param name="transitionType">Type of transitions</param>
        /// <param name="maxResults">Number of items to show on a single page</param>
        /// <param name="nextToken">Token to retrieve the next page</param>
        /// <returns>Transitions response from REST API</returns>
        public object ListTransitions(string? transitionType = null, int? maxResults = null, string? nextToken = null) {
            var queryParams = new Dictionary<string, object?>();

            if (maxResults != null) {
                queryParams.Add("maxResults", maxResults.ToString());
            }

            if (nextToken != null) {
                queryParams.Add("nextToken", nextToken);
            }

            RestRequest request = ClientRestRequest(Method.GET, "/transitions", null, queryParams);
            return ExecuteRequestResilient(RestSharpClient, request);
        }

        /// <summary>Get information about a specific transition,
        /// calls the GET /transitions/{transition_id} endpoint.</summary>
        /// <example>
        /// <code>
        /// Client client = new Client();
        /// var response = client.GetTransition("&lt;transition_id&gt;");
        /// </code>
        /// </example>
        /// <param name="transitionId">Id of the transition</param>
        /// <returns>Transition response from REST API</returns>
        public object GetTransition(string transitionId) {
            var request = ClientRestRequest(Method.GET, $"/transitions/{transitionId}");
            return ExecuteRequestResilient(RestSharpClient, request);
        }

        /// <summary>Delete a transition, calls the DELETE /transitions/{transition_id} endpoint.
        /// Will fail if transition is in use by one or more workflows.</summary>
        /// <example>
        /// <code>
        /// Client client = new Client();
        /// var response = client.DeleteTransition("&lt;transition_id&gt;");
        /// </code>
        /// </example>
        /// <param name="transitionId">Id of the transition</param>
        /// <returns>Transition response from REST API</returns>
        public object DeleteTransition(string transitionId) {
            var request = ClientRestRequest(Method.DELETE, $"/transitions/{transitionId}");
            return ExecuteRequestResilient(RestSharpClient, request);
        }

        /// <summary>Get an execution of a transition, calls the GET /transitions/{transitionId}/executions/{executionId} endpoint</summary>
        /// <example>
        /// <code>
        /// Client client = new Client();
        /// var response = client.GetTransitionExecution("&lt;transition_id&gt;", "&lt;execution_id&gt;");
        /// </code>
        /// </example>
        /// <param name="transitionId">Id of the transition</param>
        /// <param name="executionId">Id of the execution</param>
        /// <returns>Transition execution response from REST API</returns>
        public object GetTransitionExecution(string transitionId, string executionId) {
            RestRequest request = ClientRestRequest(Method.GET, $"/transitions/{transitionId}/executions/{executionId}");
            return ExecuteRequestResilient(RestSharpClient, request);
        }

        /// <summary>Updates an existing transition, calls the PATCH /transitions/{transitionId} endpoint.</summary>
        /// <example>
        /// <code>
        /// Client client = new Client();
        /// var response = client.UpdateTransition("&lt;transitionId&gt;");
        /// </code>
        /// </example>
        /// <param name="transitionId">Id of the transition</param>
        /// <param name="inputJsonSchema">Json-schema that defines the input to the transition</param>
        /// <param name="outputJsonSchema">Json-schema that defines the output of the transition</param>
        /// <param name="attributes">Additional attributes</param>
        /// <returns>Transition response from REST API</returns>
        public object UpdateTransition(
            string transitionId,
            Dictionary<string, string>? inputJsonSchema,
            Dictionary<string, string>? outputJsonSchema,
            Dictionary<string, string?> attributes
        ) {
            var body = new Dictionary<string, object?>();

            if (inputJsonSchema != null) {
                body.Add("inputJsonSchema", inputJsonSchema);
            }

            if (outputJsonSchema != null) {
                body.Add("outputJsonSchema", outputJsonSchema);
            }

            if (attributes != null) {
                foreach (KeyValuePair<string, string?> entry in attributes) {
                    body.Add(entry.Key, entry.Value);
                }
            }

            RestRequest request = ClientRestRequest(Method.PATCH, $"/transitions/{transitionId}", body);
            return ExecuteRequestResilient(RestSharpClient, request);
        }

        /// <summary>Start executing a manual transition, calls the POST /transitions/{transitionId}/executions endpoint.</summary>
        /// <example>
        /// <code>
        /// Client client = new Client();
        /// var response = client.ExecuteTransition("&lt;transitionId&gt;");
        /// </code>
        /// </example>
        /// <param name="transitionId">Id of the transition</param>
        /// <returns>Transition exexution response from REST API</returns>
        public object ExecuteTransition(string transitionId) {
            var request = ClientRestRequest(Method.POST, $"/transitions/{transitionId}/executions");
            return ExecuteRequestResilient(RestSharpClient, request);
        }

        /// <summary>List executions in a transition, calls the GET /transitions/{transitionId}/executions endpoint.</summary>
        /// <example>
        /// <code>
        /// Client client = new Client();
        /// var response = client.ListTransitionExecutions("&lt;transitionId&gt;", new [] {"succeeded", "failed"});
        /// </code>
        /// </example>
        /// <param name="transitionId">Id of the transition</param>
        /// <param name="status">Status to filter by</param>
        /// <param name="executionIds">List of execution ids to filter by</param>
        /// <param name="maxResults">Maximum number of results to be returned</param>
        /// <param name="nextToken">A unique token used to retrieve the next page</param>
        /// <param name="sortBy">The sorting variable of the execution: "endTime" | "startTime"</param>
        /// <param name="order">Order of the executions: "ascending" | "descending"</param>
        /// <returns>Transition executions response from the REST API</returns>
        public object ListTransitionExecutions(
            string transitionId,
            string? status = null,
            List<string>? executionIds = null,
            int? maxResults = null,
            string? nextToken = null,
            string? sortBy = null,
            string? order = null
        ) {
            List<string>? statuses = null;
            if (status != null) {
                statuses = new List<string>{status};
            }
            return ListTransitionExecutions(transitionId, statuses, executionIds, maxResults, nextToken, sortBy, order);
         }

        /// <summary>List executions in a transition, calls the GET /transitions/{transitionId}/executions endpoint.</summary>
        /// <example>
        /// <code>
        /// Client client = new Client();
        /// var response = client.ListTransitionExecutions("&lt;transitionId&gt;", new [] {"succeeded", "failed"});
        /// </code>
        /// </example>
        /// <param name="transitionId">Id of the transition</param>
        /// <param name="statuses">List of execution statuses to filter by</param>
        /// <param name="executionIds">List of execution ids to filter by</param>
        /// <param name="maxResults">Maximum number of results to be returned</param>
        /// <param name="nextToken">A unique token used to retrieve the next page</param>
        /// <param name="sortBy">The sorting variable of the execution: "endTime" | "startTime"</param>
        /// <param name="order">Order of the executions: "ascending" | "descending"</param>
        /// <returns>Transition executions response from the REST API</returns>
        public object ListTransitionExecutions(
            string transitionId,
            List<string>? statuses = null,
            List<string>? executionIds = null,
            int? maxResults = null,
            string? nextToken = null,
            string? sortBy = null,
            string? order = null
        ) {
            var queryParams = new Dictionary<string, object?>();

            if (statuses != null) {
                queryParams.Add("status", statuses);
            }

            if (executionIds != null) {
                queryParams.Add("executionId", executionIds);
            }

            if (maxResults != null) {
                queryParams.Add("maxResults", maxResults.ToString());
            }

            if (nextToken != null) {
                queryParams.Add("nextToken", nextToken);
            }

            if (sortBy != null) {
                queryParams.Add("sortBy", sortBy);
            }

            if (order != null) {
                queryParams.Add("order", order);
            }

            var request = ClientRestRequest(Method.GET, $"/transitions/{transitionId}/executions", null, queryParams);
            return ExecuteRequestResilient(RestSharpClient, request);
        }

        /// <summary>Ends the processing of the transition execution, calls the PATCH /transitions/{transitionId}/execution/{executionId} endpoint.</summary>
        /// <example>
        /// <code>
        /// Client client = new Client();
        /// var output = new Dictionary&lt;string, string&gt;();
        /// client.UpdateTransitionExecution("&lt;transitionId&gt;", "&lt;executionId&gt;, "succeeded", output: output);
        /// </code>
        /// </example>
        /// <param name="transitionId">Id of the transition</param>
        /// <param name="executionId">Id of the execution</param>
        /// <param name="status">Status of the execution: "succeeded" | "failed"</param>
        /// <param name="output">Output from the execution, required when status is "succeeded"</param>
        /// <param name="error">Error from the execution, required when status is "failed"</param>
        /// <returns>Transition execution response from REST API</returns>
        public object UpdateTransitionExecution(
            string transitionId,
            string executionId,
            string status,
            Dictionary<string, string>? output = null,
            Dictionary<string, string>? error = null
        ) {
            var url = $"/transitions/{transitionId}/executions/{executionId}";
            var body = new Dictionary<string, object>{
                {"status", status},
            };

            if (output != null) {
                body.Add("output", output);
            }

            if (error != null) {
                body.Add("error", error);
            }
            var request = ClientRestRequest(Method.PATCH, url, body);
            return ExecuteRequestResilient(RestSharpClient, request);
        }

        /// <summary>Creates a new user, calls the POST /users endpoint.</summary>
        /// <example>
        /// <code>
        /// Client client = new Client();
        /// var response = client.CreateUser("foo@bar.com");
        /// </code>
        /// </example>
        /// <param name="email">New user's email</param>
        /// <param name="attributes">Additional attributes. Currently supported are: name, avatar</param>
        /// <returns>User response from REST API</returns>
        public object CreateUser(string email, Dictionary<string, string?>? attributes = null) {
            var body = new Dictionary<string, string>{
                {"email", email}
            };

            if (attributes != null) {
                foreach (KeyValuePair<string, string?> entry in attributes) {
                    body.Add(entry.Key, entry.Value);
                }
            }

            var request = ClientRestRequest(Method.POST, "/users", body);
            return ExecuteRequestResilient(RestSharpClient, request);
        }

        /// <summary>List users, calls the GET /users endpoint.</summary>
        /// <example>
        /// <code>
        /// Client client = new Client();
        /// var response = client.ListUsers();
        /// </code>
        /// </example>
        /// <param name="maxResults">Maximum number of results to be returned</param>
        /// <param name="nextToken">A unique token used to retrieve the next page</param>
        /// <returns>Users response from REST API</returns>
        public object ListUsers(int? maxResults = null, string? nextToken = null) {
            var queryParams = new Dictionary<string, object?>();

            if (maxResults != null) {
                queryParams.Add("maxResults", maxResults.ToString());
            }

            if (nextToken != null) {
                queryParams.Add("nextToken", nextToken);
            }

            var request = ClientRestRequest(Method.GET, "/users", null, queryParams);
            return ExecuteRequestResilient(RestSharpClient, request);
        }

        /// <summary>Get information about a specific user, calls the GET /users/{user_id} endpoint.</summary>
        /// <example>
        /// <code>
        /// Client client = new Client();
        /// var response = client.GetUser("&lt;user_id&gt;");
        /// </code>
        /// </example>
        /// <param name="userId">Id of the user</param>
        /// <returns>User response from REST API</returns>
        public object GetUser(string userId) {
            var request = ClientRestRequest(Method.GET, $"/users/{userId}");
            return ExecuteRequestResilient(RestSharpClient, request);
        }

        /// <summary>Delete the user with the provided user_id, calls the DELETE /users/{userId} endpoint.</summary>
        /// <example>
        /// <code>
        /// Client client = new Client();
        /// var response = client.DeleteUser("&lt;user_id&gt;");
        /// </code>
        /// </example>
        /// <param name="userId">Id of the user</param>
        /// <returns>User response from REST API</returns>
        public object DeleteUser(string userId) {
            var request = ClientRestRequest(Method.DELETE, $"/users/{userId}");
            return ExecuteRequestResilient(RestSharpClient, request);
        }

        /// <summary>Updates a user, calls the PATCH /users/{userId} endpoint.</summary>
        /// <example>
        /// <code>
        /// Client client = new Client();
        /// var parameters = new Dictionary&lt;string, string&gt;{
        ///     {"name", "User"}
        /// };
        /// var response = client.UpdateUser("&lt;user_id&gt;", parameters);
        /// </code>
        /// </example>
        /// <param name="userId">Id of the user</param>
        /// <param name="attributes">
        /// Attributes to update.
        /// Allowed attributes:
        ///     name (string),
        ///     avatar (base64-encoded image)
        /// </param>
        /// <returns>User response from REST API</returns>
        public object UpdateUser(string userId, Dictionary<string, object?> attributes) {
            var body = new Dictionary<string, object>();

            foreach (KeyValuePair<string, object?> entry in attributes) {
                body.Add(entry.Key, entry.Value);
            }

            var request = ClientRestRequest(Method.PATCH, $"/users/{userId}", body: body);
            return ExecuteRequestResilient(RestSharpClient, request);
        }

        /// <summary>
        /// Creates a new workflow, calls the POST /workflows endpoint.
        /// Check out Lucidtech's tutorials for more info on how to create a workflow.
        /// </summary>
        /// <example>
        /// <code>
        /// Client client = new Client();
        /// var spec = new Dictionary&lt;string, object&gt;{
        ///     {"language", "ASL"},
        ///     {"version", "1.0.0"},
        ///     {"definition", {...}}
        /// };
        /// var errorConfig = new Dictionary&lt;string, string&gt;{
        ///     {"email", "foo@bar.com}
        /// };
        /// var parameters = new Dictionary&lt;string, string&gt;{
        ///     {"name", "Name"},
        ///     {"description", "My awesome workflow"}
        /// };
        /// var response = Client.CreateWorkflow(spec, errorConfig, parameters);
        /// </code>
        /// </example>
        /// <param name="spec">Workflow specification. Currently only ASL is supported: https://states-language.net/spec.html</param>
        /// <param name="errorConfig">Error handler configuration</param>
        /// <param name="attributes">Additional attributes. Currently supported are: name, description.</param>
        /// <returns>Workflow response from REST API</returns>
        public object CreateWorkflow(
            Dictionary<string, object> spec,
            Dictionary<string, string>? errorConfig = null,
            Dictionary<string, string?>? attributes = null
        ) {
            var body = new Dictionary<string, object?>{
                {"specification", spec}
            };

            if (errorConfig != null) {
                body.Add("errorConfig", errorConfig);
            }

            if (attributes != null) {
                foreach (KeyValuePair<string, string?> entry in attributes) {
                    body.Add(entry.Key, entry.Value);
                }
            }

            var request = ClientRestRequest(Method.POST, "/workflows", body);
            return ExecuteRequestResilient(RestSharpClient, request);
        }

        /// <summary>List workflows, calls the GET /workflows endpoint.</summary>
        /// <example>
        /// <code>
        /// Client client = new Client();
        /// var response = client.ListWorkflows();
        /// </code>
        /// </example>
        /// <param name="maxResults">Maximum number of results to be returned</param>
        /// <param name="nextToken">A unique token used to retrieve the next page</param>
        /// <returns>Workflows response from REST API</returns>
        public object ListWorkflows(int? maxResults, string nextToken) {
            var queryParams = new Dictionary<string, object?>();

            if (maxResults != null) {
                queryParams.Add("maxResults", maxResults.ToString());
            }

            if (nextToken != null) {
                queryParams.Add("nextToken", nextToken);
            }

            var request = ClientRestRequest(Method.GET, "/workflows", null, queryParams);
            return ExecuteRequestResilient(RestSharpClient, request);
        }

        /// <summary>Creates a workflow handle, calls the PATCH /workflows/{workflowId} endpoint.</summary>
        /// <example>
        /// <code>
        /// Client client = new Client();
        /// var newParameters = new Dictionary&lt;string, string&gt;{
        ///     {"name", "New Name"},
        ///     {"description", "My updated awesome workflow"}
        /// };
        /// var response = client.UpdateWorkflow("&lt;workflow_id&gt;, newParameters);
        /// </code>
        /// </example>
        /// <param name="workflowId">Id of the workflow</param>
        /// <param name="attributes">Attributes to update. Currently supported are: name, description</param>
        /// <returns>Workflow response from REST API</returns>
        public object UpdateWorkflow(string workflowId, Dictionary<string, string?> attributes) {
            var request = ClientRestRequest(Method.PATCH, $"/workflows/{workflowId}", attributes);
            return ExecuteRequestResilient(RestSharpClient, request);
        }

        /// <summary>Get information about a specific workflow,
        /// calls the GET /workflows/{workflow_id} endpoint.</summary>
        /// <example>
        /// <code>
        /// Client client = new Client();
        /// var response = client.GetWorkflow("&lt;workflow_id&gt;");
        /// </code>
        /// </example>
        /// <param name="workflowId">Id of the workflow</param>
        /// <returns>Workflow response from REST API</returns>
        public object GetWorkflow(string workflowId) {
            var request = ClientRestRequest(Method.GET, $"/workflows/{workflowId}");
            return ExecuteRequestResilient(RestSharpClient, request);
        }

        /// <summary>Delete the workflow with the provided workflow_id,
        /// calls the DELETE /workflows/{workflowId} endpoint.</summary>
        /// <example>
        /// <code>
        /// Client client = new Client();
        /// var response = client.DeleteWorkflow("&lt;workflow_id&gt;");
        /// </code>
        /// </example>
        /// <param name="workflowId">Id of the workflow</param>
        /// <returns>Workflow response from REST API</param>
        public object DeleteWorkflow(string workflowId) {
            var request = ClientRestRequest(Method.DELETE, $"/workflows/{workflowId}");
            return ExecuteRequestResilient(RestSharpClient, request);
        }

        /// <summary>Start a workflow execution, calls the POST /workflows/{workflowId}/executions endpoint.</summary>
        /// <example>
        /// <code>
        /// Client client = new Client();
        /// var content = new Dictionary&lt;string, object&gt;();
        /// var response = client.ExecuteWorkflow("&lt;workflowId&gt;, content);
        /// </code>
        /// </example>
        /// <param name="workflowId">Id of the workflow</param>
        /// <param name="content">Input to the first step of the workflow</param>
        /// <returns>Workflow execution response from REST API</returns>
        public object ExecuteWorkflow(string workflowId, Dictionary<string, object> content) {
            var body = new Dictionary<string, object> {
                {"input", content}
            };
            var request = ClientRestRequest(Method.POST, $"/workflows/{workflowId}/executions", body);
            return ExecuteRequestResilient(RestSharpClient, request);
        }

        /// <summary>List executions in a workflow, calls the GET /workflows/{workflowId}/executions endpoint.</summary>
        /// <example>
        /// <code>
        /// Client client = new Client();
        /// var statuses = new [] {"running", "succeeded"};
        /// var response = client.ListWorkflowExecutions("&lt;workflow_id&gt;", statuses);
        /// </code>
        /// </example>
        /// <param name="workflowId">Id of the workflow</param>
        /// <param name="status">Workflow execution status to filter by</param>
        /// <param name="maxResults">Maximum number of results to be returned</param>
        /// <param name="nextToken">A unique token used to retrieve the next page</param>
        /// <param name="sortBy">The sorting variable of the execution: "endTime" | "startTime"</param>
        /// <param name="order">Order of the executions: "ascending" | "descending"</param>
        /// <returns>WorkflowExecutions response from REST API</returns>
        public object ListWorkflowExecutions(
            string workflowId,
            string? status = null,
            int? maxResults = null,
            string? nextToken = null,
            string? sortBy = null,
            string? order = null
        ) => ListWorkflowExecutions(workflowId, new List<string>{status}, maxResults, nextToken, sortBy, order);

        /// <summary>List executions in a workflow, calls the GET /workflows/{workflowId}/executions endpoint.</summary>
        /// <example>
        /// <code>
        /// Client client = new Client();
        /// var statuses = new [] {"running", "succeeded"};
        /// var response = client.ListWorkflowExecutions("&lt;workflow_id&gt;", statuses);
        /// </code>
        /// </example>
        /// <param name="workflowId">Id of the workflow</param>
        /// <param name="statuses">Workflow execution statuses to filter by</param>
        /// <param name="maxResults">Maximum number of results to be returned</param>
        /// <param name="nextToken">A unique token used to retrieve the next page</param>
        /// <param name="sortBy">The sorting variable of the execution: "endTime" | "startTime"</param>
        /// <param name="order">Order of the executions: "ascending" | "descending"</param>
        /// <returns>WorkflowExecutions response from REST API</returns>
        public object ListWorkflowExecutions(
            string workflowId,
            List<string>? statuses = null,
            int? maxResults = null,
            string? nextToken = null,
            string? sortBy = null,
            string? order = null
        ) {
            var queryParams = new Dictionary<string, object?> {
                {"status", statuses},
            };

            if (maxResults != null) {
                queryParams.Add("maxResults", maxResults.ToString());
            }

            if (nextToken != null) {
                queryParams.Add("nextToken", nextToken);
            }

            if (sortBy != null) {
                queryParams.Add("sortBy", sortBy);
            }

            if (order != null) {
                queryParams.Add("order", order);
            }

            var request = ClientRestRequest(Method.GET, $"/workflows/{workflowId}/executions", null, queryParams);
            return ExecuteRequestResilient(RestSharpClient, request);
        }

        /// <summary>
        /// Deletes the execution with the provided execution_id from workflow_id,
        /// calls the DELETE /workflows/{workflowId}/executions/{executionId} endpoint.
        /// </summary>
        /// <example>
        /// <code>
        /// Client client = new Client();
        /// var response = client.DeleteWorkflowExecution("&lt;workflow_id&gt;", "&lt;execution_id&gt;");
        /// </code>
        /// </example>
        /// <param name="workflowId">Id of the workflow</param>
        /// <param name="executionId">Id of the execution</param>
        /// <returns>WorkflowExecution response from REST API</returns>
        public object DeleteWorkflowExecution(string workflowId, string executionId) {
            var request = ClientRestRequest(Method.DELETE, $"/workflows/{workflowId}/executions/{executionId}");
            return ExecuteRequestResilient(RestSharpClient, request);
        }

        /// <summary>
        /// Create a HTTP web request for the REST API. 
        /// </summary>
        /// <param name="method"> The request method, e.g. POST, PUT, GET, DELETE </param>
        /// <param name="path"> The path to the domain upon which to apply the request,
        /// the total path will be <see href="Credentials.ApiEndpoint"/>path</param>
        /// <param name="body"> The content of the request </param>
        /// <param name="queryParams">Query parameters</param>
        /// <returns>
        /// An object of type <see cref="RestRequest"/> defined by the input
        /// </returns>
        private RestRequest ClientRestRequest(
            Method method,
            string path,
            object? body = null,
            Dictionary<string, object?>? queryParams = null)
        {
            Uri endpoint = new Uri(string.Concat(LasCredentials.ApiEndpoint, path));
            RestRequest request = new RestRequest(endpoint, method, DataFormat.Json);
            request.JsonSerializer = JsonSerialPublisher.Default;

            if (body == null) {
                body = new Dictionary<string, string>();
            }
            
            if (method == Method.POST || method == Method.PATCH) {
                request.AddJsonBody(body); 
            }

            if (queryParams == null) {
                queryParams = new Dictionary<string, object?>();
            }

            foreach (var entry in queryParams) {
                if (entry.Value == null) {
                    continue;
                }

                if (entry.Value is List<string?>) {
                    foreach (var item in entry.Value as List<string>) {
                        request.AddQueryParameter(entry.Key, item);
                    }
                }
            }

            var headers = CreateSigningHeaders();

            foreach (var entry in headers) {
                request.AddHeader(entry.Key, entry.Value);
            }

            return request;
        }
        

        private Dictionary<string, string> CreateSigningHeaders()
        {
            var headers = new Dictionary<string, string>()
            {
                {"Authorization", $"Bearer {LasCredentials.GetAccessToken()}"},
                {"X-Api-Key", LasCredentials.ApiKey}
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
