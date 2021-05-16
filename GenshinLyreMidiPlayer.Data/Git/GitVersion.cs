using System;
using System.Text.Json.Serialization;

namespace GenshinLyreMidiPlayer.Data.Git
{
    public class GitVersion
    {
        [JsonPropertyName("draft")] public bool Draft { get; set; }

        [JsonPropertyName("prerelease")] public bool Prerelease { get; set; }

        [JsonPropertyName("name")] public string Name { get; set; }

        [JsonPropertyName("tag_name")] public string TagName { get; set; }

        [JsonPropertyName("html_url")] public string Url { get; set; }

        public Version Version => new(TagName.Replace("v", string.Empty));
    }
}