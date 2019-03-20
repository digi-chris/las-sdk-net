using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Lucidtech.Las.Utils;

namespace Lucidtech.Las.Core
{
	/// <summary>
	/// A class that contains all the necessary information regarding a prediction performed by <see cref="ApiClient"/>.
	/// </summary>
	public class Prediction
	{

		/// Dictionary containing document id, consent id and model name
		public Dictionary<string, string> Essentials { get; }
		/// A list of the responses from a prediction
		public List<Dictionary<string, object>> Fields { get; }

		/// <summary>
		/// Constructor of s Prediction object
		/// </summary>
		/// <param name="documentId"> The id of the document used in the prediction</param>
		/// <param name="consentId"> The consent id</param>
		/// <param name="modelName"> The name of the model used</param>
		/// <param name="predictionResponse"> The response from prediction </param>
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

		/// <summary>
		/// Make the members of Prediction accessible as if it was a dictionary
		/// </summary>
		/// <param name="s"> a string that needs to match either one of the keys in <c> Essentials</c> or "fields" </param>
		/// <exception cref="KeyNotFoundException"> Will throw an exception if <paramref name="s"/> is invalid</exception>
		public object this[string s]
		{
			get
			{
				if (string.Equals("fields", s)) { return Fields; }

				foreach (var entry in Essentials)
				{
					if (string.Equals(entry.Key, s)) { return entry.Value; }
				}

				throw new KeyNotFoundException($"{s} is not present in the Prediction class");
			}
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
    }

	/// <summary>
	/// The structured format of the response from a send feedback request.
	/// </summary>
    public class FeedbackResponse
    {
	    /// <summary>
	    /// Dictionary that contains document Id, consent Id, upload url and content type.
	    /// </summary>
	    public Dictionary<string, string> Essentials { get; }
	    /// <summary>
	    /// The same information as was uploaded as feedback.
	    /// </summary>
	    public List<Dictionary<string, string>> Feedback { get; }

	    public FeedbackResponse(object response)
	    {
			JObject jsonResponse = JObject.Parse(response.ToString());
			
			Essentials = new Dictionary<string, string>( )
			{
				{"documentId", jsonResponse["documentId"].ToString()},
					
				{"consentId", jsonResponse["consentId"].ToString()},
				{"uploadUrl", jsonResponse["uploadUrl"].ToString()},
				{"contentType", jsonResponse["contentType"].ToString()}
			};
			
		    Feedback = JsonSerialPublisher.ObjectToDict<List<Dictionary<string,string>>>(jsonResponse["feedback"]);
	    }
    }
    
}
