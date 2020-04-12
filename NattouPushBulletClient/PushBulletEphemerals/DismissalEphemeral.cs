using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NattouPushBulletClient.PushBulletEphemerals
{
    [JsonObject]
    class DismissalEphemeral
    {
        [JsonProperty("type")]
        public string Type { set; get; }

        [JsonProperty("package_name")]
        public string PackageName { get; set; }

        [JsonProperty("notification_id")]
        public string Id { get; set; }

        public override string ToString()
        {
            return
                $"Type = {Type}\n" +
                $"PackageName = {PackageName}\n" +
                $"NotificationId = {Id}"
                ;
        }
    }
}
