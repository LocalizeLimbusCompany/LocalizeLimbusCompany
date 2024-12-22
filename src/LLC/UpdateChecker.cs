using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using BepInEx.Configuration;
using Il2CppSystem.Threading;
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

    public static ConfigEntry<bool> AutoUpdate =
        LLCMod.LLCSettings.Bind("LLC Settings", "AutoUpdate", true, "是否自动检查并下载更新 ( true | false )");

    public static ConfigEntry<NodeType> UpdateUri = LLCMod.LLCSettings.Bind("LLC Settings", "UpdateURI",
        NodeType.Auto,
        "自动更新所使用URI ( Auto：自动,优先使用GitHub | GitHub：GitHub | OneDrive：Onedrive For Business | Tianyi：天翼网盘 )");

    public static readonly Dictionary<NodeType, string> UrlDictionary = new()
    {
        { NodeType.OneDrive, "https://node.zeroasso.top/d/od/" },
        { NodeType.Tianyi, "https://node.zeroasso.top/d/tianyi/" }
    };

    private static readonly HttpClient Client = new();

    public static NodeType UpdateUriTemp;

    public static bool NeedPopup;

    public static string TMPOldVersion = string.Empty;

    public static string TMPUpdateVersion = string.Empty;

    public static string ResourceOldVersion = string.Empty;

    public static string ResourceUpdateVersion = string.Empty;

    public static string UpdateMessage = string.Empty;

    public static bool IsAppOutdated;

    public static void StartAutoUpdate()
    {
        if (!AutoUpdate.Value) return;
        if (!File.Exists(LLCMod.ModPath + "/version.json") ||
            !File.Exists(LLCMod.ModPath + "/7z.exe"))
        {
            LLCMod.LogWarning("Can't Find HotUpdate Need File. Skip Mod Update.");
            return;
        }

        UpdateUriTemp = UpdateUri.Value == NodeType.Auto
            ? IsChinaIP() ? NodeType.OneDrive : NodeType.GitHub
            : UpdateUri.Value;
        LLCMod.LogWarning($"Check Mod Update From {UpdateUriTemp}");
        var modUpdate = ModUpdate;
        new Thread(modUpdate).Start();
    }

    public static void ModUpdate()
    {
        try
        {
            var versionPath = LLCMod.ModPath + "/version.json";
            var localJson = JSONNode.Parse(File.ReadAllText(versionPath)).AsObject;
            Client.Timeout = TimeSpan.FromSeconds(10);
            Client.DefaultRequestHeaders.Add("User-Agent", "User-Agent");
            var response = Client.GetStringAsync("https://hotupdate.zeroasso.top/api/version.json").GetAwaiter()
                .GetResult();
            var serverJson = JSONNode.Parse(response).AsObject;
            var tag = serverJson["version"].Value;
            if (Version.Parse(localJson["version"].Value) < Version.Parse(tag))
            {
                var updatelog = $"LimbusLocalize_BIE_{tag}.7z";
                var downloadUri = UpdateUriTemp == NodeType.GitHub
                    ? $"https://github.com/LocalizeLimbusCompany/LocalizeLimbusCompany/releases/download/{tag}/{updatelog}"
                    : $"{UrlDictionary[UpdateUriTemp]}{updatelog}";
                var filename = Path.Combine(LLCMod.GamePath, updatelog);
                if (!File.Exists(filename)) DownloadFile(downloadUri, filename);
                NeedPopup = true;
                IsAppOutdated = true;
                UpdateMessage = updatelog;
                LLCMod.LogWarning("New mod version found. Download full mod.");
                return;
            }

            var latestTextVersion = int.Parse(serverJson["resource_version"].Value);
            var localTextVersion = int.Parse(localJson["resource_version"].Value);
            if (latestTextVersion > localTextVersion)
            {
                var updatelog = $"LimbusLocalize_Resource_{latestTextVersion}.7z";
                var downloadUri = UpdateUriTemp == NodeType.GitHub
                    ? $"https://raw.githubusercontent.com/ZengXiaoPi/LLC-CF-source/refs/heads/main/files/resource/{updatelog}"
                    : $"{UrlDictionary[UpdateUriTemp]}Resource/{updatelog}";
                var filename = Path.Combine(LLCMod.GamePath, updatelog);
                if (!File.Exists(filename))
                    DownloadFile(downloadUri, filename);
                UnarchiveFile(filename, LLCMod.GamePath);
                ChineseFont.LoadLocal();
                ChineseFont.LoadRemote2();
                NeedPopup = true;
                ResourceOldVersion = localTextVersion.ToString();
                ResourceUpdateVersion = latestTextVersion.ToString();
                UpdateMessage = serverJson["notice"].Value.Replace("\\n", "\n");
                LLCMod.LogWarning("Mod Update Success.");
            }

            File.WriteAllText(versionPath, response);
            LLCMod.LogWarning("Check Chinese Font Asset Update");
            ChineseFontUpdate();
        }
        catch (Exception ex)
        {
            LLCMod.LogWarning($"Mod update failed::\n{ex}");
        }
    }

    public static void ChineseFontUpdate()
    {
        try
        {
            var releaseUri = UpdateUriTemp == NodeType.GitHub
                ? "https://api.github.com/repos/LocalizeLimbusCompany/LLC_ChineseFontAsset/releases/latest"
                : "https://json.zxp123.eu.org/LatestTmp_Release.json";
            var response = Client.GetStringAsync(releaseUri).GetAwaiter().GetResult();
            var latest = JSONNode.Parse(response).AsObject;
            var latestReleaseTag = int.Parse(latest["tag_name"].Value);
            var fontPath = LLCMod.ModPath + "/tmpchinesefont";
            var lastWriteTime = File.Exists(fontPath)
                ? int.Parse(TimeZoneInfo.ConvertTime(new FileInfo(fontPath).LastWriteTime,
                    TimeZoneInfo.FindSystemTimeZoneById("China Standard Time")).ToString("yyMMdd"))
                : 0;
            if (lastWriteTime >= latestReleaseTag) return;
            string updatelog;
            string downloadUri;
            if (UpdateUriTemp == NodeType.GitHub)
            {
                updatelog = $"tmpchinesefont_BIE_{latestReleaseTag}.7z";
                downloadUri =
                    $"https://github.com/LocalizeLimbusCompany/LLC_ChineseFontAsset/releases/download/{latestReleaseTag}/{updatelog}";
            }
            else
            {
                updatelog = "tmpchinesefont_BIE.7z";
                downloadUri = $"{UrlDictionary[UpdateUriTemp]}{updatelog}";
            }

            var filename = Path.Combine(LLCMod.GamePath, updatelog);
            if (!File.Exists(filename))
                DownloadFile(downloadUri, filename);
            UnarchiveFile(filename, LLCMod.GamePath);
            ChineseFont.Tmpchinesefonts.Clear();
            ChineseFont.AddChineseFont(fontPath);
            NeedPopup = true;
            TMPUpdateVersion = latestReleaseTag.ToString();
            TMPOldVersion = lastWriteTime.ToString();
            LLCMod.LogWarning("Chinese Font Asset Update Success.");
        }
        catch (Exception ex)
        {
            LLCMod.LogWarning($"Font asset update failed:\n{ex}");
        }
    }

    public static void ReadmeUpdate()
    {
        try
        {
            var lastUpdateTimeText =
                Client.GetStringAsync("https://json.zxp123.eu.org/ReadmeLatestUpdateTime.txt").GetAwaiter().GetResult();
            var filePath = LLCMod.ModPath + "/Localize/Readme/Readme.json";
            var lastWriteTime = new FileInfo(filePath).LastWriteTime;
            if (lastWriteTime >= DateTime.Parse(lastUpdateTimeText))
                return;
            File.WriteAllText(filePath,
                Client.GetStringAsync("https://json.zxp123.eu.org/Readme.json").GetAwaiter().GetResult());
            ReadmeManager.InitReadmeList();
        }
        catch (Exception ex)
        {
            LLCMod.LogWarning($"Readme update failed:\n{ex}");
        }
    }

    public static void DownloadFile(string uri, string filePath)
    {
        try
        {
            LLCMod.LogWarning($"Download {uri} To {filePath}");
            using var response = Client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead).GetAwaiter()
                .GetResult();
            using FileStream fileStream = new(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
            response.Content.CopyToAsync(fileStream).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            LLCMod.LogWarning(ex is HttpRequestException { StatusCode: HttpStatusCode.NotFound }
                ? $"{uri} 404 NotFound,No Resource"
                : $"{uri} Error!!!:\n{ex}");
        }
    }
    
    private static void UnarchiveFile(string sourceFile, string destinationPath)
    {
        LLCMod.LogWarning($"Unarchiving {sourceFile} To {destinationPath}");
        var processStartInfo = new ProcessStartInfo
        {
            FileName = LLCMod.ModPath + "/7z.exe",
            Arguments = $"""x "{sourceFile}" -o"{destinationPath}" -y""",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
        using var process = Process.Start(processStartInfo);
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
        File.Delete(sourceFile);
    }

    public static bool IsChinaIP()
    {
        var response = Client.GetStringAsync("http://ip-api.com/json").GetAwaiter().GetResult();
        var json = JSONNode.Parse(response).AsObject;
        return json["country"] == "China";
    }
}