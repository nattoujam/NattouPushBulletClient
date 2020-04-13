using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NattouPushBulletClient.PushBulletEphemerals
{
    [JsonObject]
    class Message
    {
        [JsonProperty("type")]
        public string Type { set; get; }

        [JsonProperty("push")]
        public JObject Push { get; set; }

        [JsonProperty("subtype")]
        public string SubType { get; set; }

        public override string ToString()
        {
            return
                $"Type = {Type}\n" +
                $"SubType = {SubType}\n" +
                $"Push = {Push}"
                ;
        }
    }
}
