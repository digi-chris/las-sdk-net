using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Lucidtech.Las.Utils;

namespace Lucidtech.Las.Core
{
    /// <summary>
    /// A class that contains all the necessary information regarding a prediction performed by <see cref="ApiClient"/>.
    /// </summary>
    public class Prediction
    {
        /// <summary>
        /// Document id
        /// </summary>
        public string DocumentId { get; }
        /// <summary>
        /// Consent id
        /// </summary>
        public string ConsentId { get; }
        /// <summary>
        /// Upload url
        /// </summary>
        public string ModelName { get; }
        /// A list of the responses from a prediction
        public List<Dictionary<string, object>> Fields { get; }

        /// <summary>
        /// Constructor of s Prediction object
        /// </summary>
        /// <param name="documentId"> The id of the document used in the prediction </param>
        /// <param name="consentId"> The consent id </param>
        /// <param name="modelName"> The name of the model used </param>
        /// <param name="predictionResponse"> The response from prediction </param>
        public Prediction(string documentId, string consentId, string modelName,
            List<Dictionary<string, object>> predictionResponse)
        {
            DocumentId = documentId;
            ConsentId = consentId;
            ModelName = modelName;
            Fields = predictionResponse;
        }
        
        /// <summary>
        /// Convert an object of this class to a string ready to be interpreted as a json object.
        /// </summary>
        /// <param name="format"> The format of the string,
        /// either <c>Formatting.None</c> or <c>Formatting.Indented</c> </param>
        /// <returns> A string that is formatted as a json object </returns>
        public string ToJsonString(Formatting format = Formatting.None) 
        {
            return JsonConvert.SerializeObject(this, format);
        }
    }

    /// <summary>
    /// The structured format of the response from a revoke consent request.
    /// </summary>
    public class RevokeResponse
    {
        /// <summary>
        /// The consent Id where documents where deleted. 
        /// </summary>
        public string ConsentId { get; }
        /// <summary>
        /// The document Ids of the deleted documents.
        /// </summary>
        public List<string> DocumentIds { get; }
        
        public RevokeResponse(object deleteConsentResponse)
        {
            JObject jsonResponse = JObject.Parse(deleteConsentResponse.ToString());
            ConsentId = jsonResponse["consentId"].ToString();
            DocumentIds = JsonSerialPublisher.ObjectToDict<List<string>>(jsonResponse["documentIds"]);
        }
        /// <summary>
        /// Convert an object of this class to a string ready to be interpreted as a json object.
        /// </summary>
        /// <param name="format"> The format of the string,
        /// either <c>Formatting.None</c> or <c>Formatting.Indented</c> </param>
        /// <returns> A string that is formatted as a json object </returns>
        public string ToJsonString(Formatting format = Formatting.None) 
        {
            return JsonConvert.SerializeObject(this, format);
        }
    }

    /// <summary>
    /// The structured format of the response from a send feedback request.
    /// </summary>
    public class FeedbackResponse
    {
        /// <summary>
        /// Document id
        /// </summary>
        public string DocumentId { get; }
        /// <summary>
        /// Consent id
        /// </summary>
        public string ConsentId { get; }
        /// <summary>
        /// Upload url
        /// </summary>
        public string UploadUrl { get; }
        /// <summary>
        /// Content type
        /// </summary>
        public string ContentType { get; }
        /// <summary>
        /// The same information as was uploaded as feedback.
        /// </summary>
        public List<Dictionary<string, string>> Feedback { get; }

        public FeedbackResponse(object response)
        {
            JObject jsonResponse = JObject.Parse(response.ToString());
            
            DocumentId = jsonResponse["documentId"].ToString();
            ConsentId = jsonResponse["consentId"].ToString();
            UploadUrl = jsonResponse["uploadUrl"].ToString();
            ContentType = jsonResponse["contentType"].ToString();
            Feedback = JsonSerialPublisher.ObjectToDict<List<Dictionary<string,string>>>(jsonResponse["feedback"]);
        }
        
        /// <summary>
        /// Convert an object of this class to a string ready to be interpreted as a json object.
        /// </summary>
        /// <param name="format"> The format of the string,
        /// either <c>Formatting.None</c> or <c>Formatting.Indented</c> </param>
        /// <returns> A string that is formatted as a json object </returns>
        public string ToJsonString(Formatting format = Formatting.None) 
        {
            return JsonConvert.SerializeObject(this, format);
        }
    }
    
}
