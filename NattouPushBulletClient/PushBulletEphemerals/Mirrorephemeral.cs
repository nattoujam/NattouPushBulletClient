using Newtonsoft.Json;

namespace NattouPushBulletClient.PushBulletEphemerals
{
    [JsonObject]
    class MirrorEphemeral
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("icon")]
        public string Icon { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("body")]
        public string Body { get; set; }

        [JsonProperty("application_name")]
        public string ApplicationName { get; set; }

        [JsonProperty("package_name")]
        public string PackageName { get; set; }

        [JsonProperty("notification_id")]
        public string Id { get; set; }

        public override string ToString()
        {
            return
                $"Type = {Type}\n" +
                $"Icon = {Icon}\n" +
                $"Title = {Title}\n" +
                $"Body = {Body}\n" +
                $"ApplicationName = {ApplicationName}\n" +
                $"PacageName = {PackageName}\n" +
                $"NotificationId = {Id}"
                ;
        }
    }
}
