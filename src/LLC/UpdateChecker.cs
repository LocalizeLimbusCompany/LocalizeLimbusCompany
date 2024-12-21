using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using BepInEx.Configuration;
using SimpleJSON;

namespace LimbusLocalize.LLC;

public static class UpdateChecker
{
    public enum NodeType
    {
        Auto,
        GitHub,
        OneDrive,
        Tianyi
    }

    public static readonly Dictionary<NodeType, string> UrlDictionary = new()
    {
        { NodeType.OneDrive, "https://node.zeroasso.top/d/od/" },
        { NodeType.Tianyi, "https://node.zeroasso.top/d/tianyi/" }
    };

    private static readonly HttpClient Client = new();

    public static ConfigEntry<bool> AutoUpdate =
        LLCMod.LLCSettings.Bind("LLC Settings", "AutoUpdate", true, "是否自动检查并下载更新 ( true | false )");

    public static ConfigEntry<NodeType> UpdateUri = LLCMod.LLCSettings.Bind("LLC Settings", "UpdateURI",
        NodeType.OneDrive,
        "自动更新所使用URI ( Auto：自动,优先使用GitHub | GitHub：GitHub | OneDrive：Onedrive For Business | Tianyi：天翼网盘 )");

    public static NodeType UpdateUriTemp;

    public static bool NeedPopup;

    public static string TMPOldVersion = string.Empty;

    public static string TMPUpdateVersion = string.Empty;

    public static string ModOldVersion = string.Empty;

    public static string ModUpdateVersion = string.Empty;

    public static string HotUpdateMessage = string.Empty;

    public static bool IsAppOutdated;

    public static void StartAutoUpdate()
    {
        if (!AutoUpdate.Value) return;

        UpdateUriTemp = UpdateUri.Value == NodeType.Auto
            ? GetUrlLatencyAsync(NodeType.GitHub, LLCMod.LLCLink).GetAwaiter()
                .GetResult().Item2 < 500L
                ? NodeType.GitHub
                : GetFastestNodeTypeAsync().GetAwaiter().GetResult()
            : UpdateUri.Value;
        LLCMod.LogWarning($"Check Mod Update From {UpdateUriTemp}");
        Client.Timeout = TimeSpan.FromSeconds(10);
        ModUpdateAsync().GetAwaiter().GetResult();
    }

    public static async Task<NodeType> GetFastestNodeTypeAsync()
    {
        var tasks = UrlDictionary.Select(kvp => GetUrlLatencyAsync(kvp.Key, kvp.Value)).ToList();
        var results = await Task.WhenAll(tasks);
        var fastest = results[0];
        foreach (var result in results)
            if (result.Item2 < fastest.Item2)
                fastest = result;

        return fastest.Item1;
    }

    public static async Task<(NodeType, long)> GetUrlLatencyAsync(NodeType nodeType, string url)
    {
        var stopwatch = new Stopwatch();
        try
        {
            stopwatch.Start();
            var response = await Client.GetAsync(url);
            stopwatch.Stop();
            return response.IsSuccessStatusCode ? (nodeType, stopwatch.ElapsedMilliseconds) : (nodeType, long.MaxValue);
        }
        catch
        {
            return (nodeType, long.MaxValue);
        }
    }

    public static async Task ModUpdateAsync()
    {
        try
        {
            if (!AutoUpdate.Value) return;
            if (!File.Exists(LLCMod.ModPath + "/version.json") || !File.Exists(LLCMod.ModPath + "/7z.exe"))
            {
                LLCMod.LogWarning("Can't Find HotUpdate Need File. Skip Mod Update.");
                return;
            }
            var localJson = JSONNode.Parse(await File.ReadAllTextAsync(LLCMod.ModPath + "/version.json")).AsObject;
            var response = await Client.GetStringAsync("https://hotupdate.zeroasso.top/api/version.json");
            var serverJson = JSONNode.Parse(response).AsObject;
            var latestAppVersion = int.Parse(serverJson["app_version"].Value);
            var localAppVersion = int.Parse(localJson["app_version"].Value);
            if (localAppVersion < latestAppVersion)
            {
                NeedPopup = true;
                IsAppOutdated = true;
                LLCMod.LogWarning("New application version found. Cannot update mod.");
                return;
            }

            var latestTextVersion = int.Parse(serverJson["text_version"].Value);
            var localTextVersion = int.Parse(localJson["text_version"].Value);
            if (latestTextVersion > localTextVersion)
            {
                NeedPopup = true;
                ModOldVersion = localTextVersion.ToString();
                ModUpdateVersion = latestTextVersion.ToString();
                HotUpdateMessage = serverJson["notice"].Value.Replace("\\n", "\n");
                var updatelog = $"LimbusLocalize_hotupdate_{latestTextVersion}.7z";
                var downloadUri = $"{UrlDictionary[UpdateUriTemp]}hotupdate/{updatelog}";
                var filename = Path.Combine(LLCMod.GamePath, updatelog);
                if (!File.Exists(filename))
                    await DownloadFileAsync(downloadUri, filename);
                UnarchiveFile(filename, LLCMod.GamePath);
                LLCMod.LogWarning("Mod Update Success.");
            }

            await File.WriteAllTextAsync(LLCMod.ModPath + "/version.json", response);
            LLCMod.LogWarning("Check Chinese Font Asset Update");
            await ChineseFontUpdateAsync();
        }
        catch (Exception ex)
        {
            LLCMod.LogWarning($"Mod Update failed:\n{ex}");
        }
    }

    private static async Task ChineseFontUpdateAsync()
    {
        try
        {
            var response = await Client.GetStringAsync("https://api.kr.zeroasso.top/LatestTmp_Release.json");

            var latest = JSONNode.Parse(response).AsObject;
            var latestReleaseTag = int.Parse(latest["tag_name"].Value);
            var filePath = LLCMod.ModPath + "/tmpchinesefont";
            var lastWriteTime = File.Exists(filePath)
                ? int.Parse(TimeZoneInfo.ConvertTime(new FileInfo(filePath).LastWriteTime,
                    TimeZoneInfo.FindSystemTimeZoneById("China Standard Time")).ToString("yyMMdd"))
                : 0;

            if (lastWriteTime >= latestReleaseTag) return;

            NeedPopup = true;
            TMPUpdateVersion = latestReleaseTag.ToString();
            TMPOldVersion = lastWriteTime.ToString();

            var updatelog = "tmpchinesefont_BIE.7z";
            var downloadUri = $"{UrlDictionary[UpdateUriTemp]}{updatelog}";
            var filename = Path.Combine(LLCMod.GamePath, updatelog);

            if (!File.Exists(filename))
                await DownloadFileAsync(downloadUri, filename);

            UnarchiveFile(filename, LLCMod.GamePath);
            LLCMod.LogWarning("Chinese Font Asset Update Success.");
        }
        catch (Exception ex)
        {
            LLCMod.LogWarning($"Chinese Font Asset Update failed:\n{ex}");
        }
    }

    private static async Task DownloadFileAsync(string uri, string filePath)
    {
        try
        {
            LLCMod.LogWarning($"Download {uri} To {filePath}");
            using var response = await Client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();
            using var content = response.Content;
            await using var fileStream = new FileStream(filePath, FileMode.Create);
            await content.CopyToAsync(fileStream);
        }
        catch (Exception ex)
        {
            LLCMod.LogWarning($"Error downloading {uri}:\n{ex}");
        }
    }

    public static async Task ReadmeUpdateAsync()
    {
        try
        {
            var lastUpdateTimeText =
                await Client.GetStringAsync("https://json.zxp123.eu.org/ReadmeLatestUpdateTime.txt");
            var filePath = LLCMod.ModPath + "/Localize/Readme/Readme.json";
            var lastWriteTime = new FileInfo(filePath).LastWriteTime;
            if (lastWriteTime >= DateTime.Parse(lastUpdateTimeText))
                return;
            await File.WriteAllTextAsync(filePath,
                await Client.GetStringAsync("https://json.zxp123.eu.org/Readme.json"));
            ReadmeManager.InitReadmeList();
        }
        catch (Exception ex)
        {
            LLCMod.LogWarning($"Readme update failed:\n{ex}");
        }
    }

    private static void UnarchiveFile(string sourceFile, string destinationPath)
    {
        LLCMod.LogWarning($"Unarchive {sourceFile} To {destinationPath}");
        var processStartInfo = new ProcessStartInfo
        {
            FileName = LLCMod.ModPath + "/7z.exe",
            Arguments = $"""x "{sourceFile}" -o"{destinationPath}" -y""",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        using (var process = Process.Start(processStartInfo))
        {
            if (process == null) return;

            process.OutputDataReceived += (_, e) =>
            {
                var eData = e.Data;
                if (!string.IsNullOrEmpty(eData)) LLCMod.LogWarning("Output: " + eData);
            };

            process.ErrorDataReceived += (_, e) =>
            {
                var eData = e.Data;
                if (!string.IsNullOrEmpty(eData)) LLCMod.LogError("Error: " + eData);
            };

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
        }

        File.Delete(sourceFile);
    }
}