using RestSharp;
using Newtonsoft.Json;
using RestSharp.Deserializers;
using RestSharp.Serializers;

namespace Lucidtech.Las.Utils
{
    /// <summary>
    /// A Json publishes that allows the user to serialize and deserialize
    /// back and forth between serialized json objects
    /// and deserialized general objects and specific Dictionaries.
    /// </summary>
    public class JsonSerialPublisher: ISerializer, IDeserializer
    {
        public string RootElement { get; set; }
        public string Namespace { get; set; }
        public string DateFormat { get; set; }
        public string ContentType
        {
            get { return "application/json"; }
            set { }
        }
        private Newtonsoft.Json.JsonSerializer _serializer;

        /// <summary>
        /// A default Serializer that can be used by <see cref="RestSharp"/>.
        /// </summary>
        public static JsonSerialPublisher Default => new JsonSerialPublisher(
            new Newtonsoft.Json.JsonSerializer(){ NullValueHandling = NullValueHandling.Ignore});  
        public JsonSerialPublisher(Newtonsoft.Json.JsonSerializer serializer)
        {
            _serializer = serializer;
        }

        /// <summary>
        /// Serialize a general object.
        /// </summary>
        /// <param name="obj"> A general object to be serialized </param>
        /// <returns> A string ready to be interpreted as a json file </returns>
        public string Serialize(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        /// <summary>
        /// Deserialize the content of an IRestResponse.
        /// </summary>
        /// <param name="response"> The response from a request performed by <c> RestSharp.RestClient </c> </param>
        /// <typeparam name="T"> The type of the output, e.g. Dictionary or a List of some sort </typeparam>
        /// <returns> A deserialized object of type <typeparamref name="T"/> </returns>
        public T Deserialize<T>(IRestResponse response)
        {
            return JsonConvert.DeserializeObject<T>(response.Content);
        }
        
        /// <summary>
        /// Deserialize a string that is on a json format.
        /// </summary>
        /// <param name="response"> A json formatted string </param>
        /// <returns> A general deserialized object </returns>
        public object DeserializeObject(string response)
        {
            return JsonConvert.DeserializeObject(response);
        }
        
        /// <summary>
        /// Deserialize a string that is on a json format.
        /// </summary>
        /// <param name="response"> A json formatted string</param>
        /// <typeparam name="T"> The type of the output, e.g. Dictionary or a List of some sort </typeparam>
        /// <returns> A deserialized object of type <typeparamref name="T"/> </returns>
        public T DeserializeObject<T>(string response)
        {
            return JsonConvert.DeserializeObject<T>(response);
        }
        
        /// <summary>
        /// Convert a general object to a Dictionary of a specific type.
        /// </summary>
        /// <param name="obj"> A general object with a structure
        /// that can be described by <typeparamref name="T"/> </param>
        /// <typeparam name="T"> The type of the output, e.g. Dictionary or a List of some sort </typeparam>
        /// <returns> An object of type <typeparamref name="T"/> </returns>
        public static T ObjectToDict<T>(object obj)
        {
            var serial = JsonConvert.SerializeObject(obj); 
            return JsonConvert.DeserializeObject<T>(serial);
        }
    }
}
