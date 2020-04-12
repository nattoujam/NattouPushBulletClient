using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NattouPushBulletClient.PushBulletEphemerals
{
    [JsonObject]
    class Ephemeral
    {
        [JsonProperty("type")]
        public string Type { set; get; }

        [JsonProperty("push")]
        public JObject Message { get; set; }

        public override string ToString()
        {
            return
                $"Type = {Type}\n" +
                $"Push = {Message}"
                ;
        }
    }
}
