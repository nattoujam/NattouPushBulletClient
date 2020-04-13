using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NattouPushBulletClient.PushBulletEphemerals
{
    [JsonObject]
    class SmsEpemeral
    {
        [JsonProperty("type")]
        public string Type { set; get; }

        [JsonProperty("conversation_iden")]
        public string ConversationId { set; get; }

        [JsonProperty("message")]
        public string Message { set; get; }

        [JsonProperty("package_name")]
        public string PackageName { set; get; }

        [JsonProperty("source_user_iden")]
        public string SourceUserId { set; get; }

        [JsonProperty("target_device_iden")]
        public string TargetDeviceId { set; get; }

        public override string ToString()
        {
            return
                $"Type = {Type}\n" +
                $"ConversationId = {ConversationId}\n" +
                $"Message = {Message}\n" +
                $"PackageName = {PackageName}\n" +
                $"SourceUserId = {SourceUserId}\n" +
                $"TargetDeviceId = {TargetDeviceId}"
                ;
        }
    }
}
