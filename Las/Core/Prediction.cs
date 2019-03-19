using System.Collections.Generic;

namespace Lucidtech.Las.Core
{
	/// <summary>
	/// A class that contains all the necessary information regarding a prediction performed by <see cref="ApiClient"/>.
	/// </summary
	public class Prediction
	{

		/// Dictionary containing document id, consent id and model name
		private Dictionary<string, string> Essentials { get; }
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
}
