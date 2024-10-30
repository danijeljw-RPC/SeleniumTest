using SeleniumTest.Models;
using System.Text.Json;

namespace SeleniumTest.Services
{
    public static class VersionService
    {
        public static async Task<ChromeDriverInfo?> GetSelectedVersion(string jsonUrl, string requestedVersion, string platform)
        {
            using HttpClient client = new HttpClient();
            var response = await client.GetStringAsync(jsonUrl);
            var jsonDoc = JsonDocument.Parse(response);

            var versions = jsonDoc.RootElement.GetProperty("versions").EnumerateArray();
            var selectedVersion = string.IsNullOrEmpty(requestedVersion)
                ? versions.OrderByDescending(v => Version.Parse(v.GetProperty("version").GetString()!)).FirstOrDefault()
                : versions.FirstOrDefault(v => v.GetProperty("version").GetString() == requestedVersion);

            if (selectedVersion.ValueKind == JsonValueKind.Undefined)
                return null;

            if (!selectedVersion.TryGetProperty("downloads", out JsonElement downloads) ||
                !downloads.TryGetProperty("chrome", out JsonElement chrome) ||
                !downloads.TryGetProperty("chromedriver", out JsonElement chromedriver))
                return null;

            var chromeElement = chrome.EnumerateArray().FirstOrDefault(d => d.GetProperty("platform").GetString() == platform);
            var chromeDriverElement = chromedriver.EnumerateArray().FirstOrDefault(d => d.GetProperty("platform").GetString() == platform);

            string? chromeUrl = chromeElement.ValueKind != JsonValueKind.Undefined
                ? chromeElement.GetProperty("url").GetString()
                : null;
            string? chromeDriverUrl = chromeDriverElement.ValueKind != JsonValueKind.Undefined
                ? chromeDriverElement.GetProperty("url").GetString()
                : null;

            return chromeUrl != null && chromeDriverUrl != null
                ? new ChromeDriverInfo
                {
                    Version = selectedVersion.GetProperty("version").GetString(),
                    ChromeUrl = chromeUrl,
                    ChromeDriverUrl = chromeDriverUrl
                }
                : null;
        }
    }
}

