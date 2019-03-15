using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using RestSharp;
using Newtonsoft.Json;
using RestSharp.Deserializers;
using RestSharp.Serializers;



namespace Lucidtech.Las.Serializer
{
    public class LasSerializer : ISerializer, IDeserializer
    {
        private Newtonsoft.Json.JsonSerializer Serializer;
        
        public string RootElement { get; set; }

        public string Namespace { get; set; }

        public string DateFormat { get; set; }

        public string ContentType
        {
            get { return "application/json";}
            set { }
        }

        public static LasSerializer Default => new LasSerializer(new Newtonsoft.Json.JsonSerializer(){ NullValueHandling = NullValueHandling.Ignore,});  
        //public static LasSerializer Default => new LasSerializer(new Newtonsoft.Json.JsonSerializer());  
        public LasSerializer(Newtonsoft.Json.JsonSerializer serializer)
        {
            Serializer = serializer;
        }

        /*
        public string Serialize(object obj)
        {
            StringBuilder sb = new StringBuilder();
            using (var sw = new StringWriter(sb))
            {
                using (var jtw = new JsonTextWriter(sw))
                {
                    Serializer.Serialize(jtw, obj);
                    return sw.ToString();
                }
            }
        }
        */
        public string Serialize(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        public T Deserialize<T>(IRestResponse response)
        {
			return JsonConvert.DeserializeObject<T>(response.Content);
        }
        
        public object DeserializeObject(string response)
        {
			return JsonConvert.DeserializeObject(response);
        }
    }
}
