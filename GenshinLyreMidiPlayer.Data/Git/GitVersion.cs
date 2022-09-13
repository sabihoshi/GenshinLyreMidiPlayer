using System;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace GenshinLyreMidiPlayer.Data.Git;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public class GitVersion
{
    [JsonPropertyName("draft")] public bool Draft { get; set; }

    [JsonPropertyName("prerelease")] public bool Prerelease { get; set; }

    [JsonPropertyName("name")] public string Name { get; set; } = "Unknown";

    [JsonPropertyName("tag_name")] public string TagName { get; set; } = "0.0";

    [JsonPropertyName("html_url")] public string Url { get; set; } = null!;

    public Version Version => new(TagName.Replace("v", string.Empty));
}