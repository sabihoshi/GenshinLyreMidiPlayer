using System;
using System.Text.Json.Serialization;

namespace GenshinLyreMidiPlayer.Models
{
    public class GitVersion
    {
        [JsonPropertyName("draft")] public bool Draft { get; set; }

        [JsonPropertyName("prerelease")] public bool Prerelease { get; set; }

        [JsonPropertyName("tag_name")] public string TagName { get; set; }

        public Version Version => new(TagName.Replace("v", string.Empty));
    }
}