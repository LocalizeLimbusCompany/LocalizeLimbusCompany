using System;
using System.IO;
using System.Net;
using System.Net.Http;
using BepInEx.Configuration;
using Il2CppSystem.Threading;
using SimpleJSON;
using UnityEngine;

namespace LimbusLocalize.LLC;

public static class UpdateChecker
{
    public enum Uri
    {
        GitHub,
        MirrorOneDrive
    }

    public static ConfigEntry<bool> AutoUpdate =
        LLCMod.LLCSettings.Bind("LLC Settings", "AutoUpdate", false, "是否自动检查并下载更新 ( true | false )");

    public static ConfigEntry<Uri> UpdateUri = LLCMod.LLCSettings.Bind("LLC Settings", "UpdateURI", Uri.MirrorOneDrive,
        "自动更新所使用URI ( GitHub:默认 | MirrorOneDrive:镜像,更新可能有延迟,但下载速度更快 )");

    public static string Updatelog;
    public static Action UpdateCall;
    private static readonly HttpClient Client = new();

    public static void StartAutoUpdate()
    {
        if (!AutoUpdate.Value) return;
        LLCMod.LogWarning($"Check Mod Update From {UpdateUri.Value}");
        var modUpdate = ModUpdate;
        new Thread(modUpdate).Start();
    }

    public static void ModUpdate()
    {
        try
        {
            var releaseUri = UpdateUri.Value == Uri.GitHub
                ? "https://api.github.com/repos/LocalizeLimbusCompany/LocalizeLimbusCompany/releases/latest"
                : "https://json.zxp123.eu.org/LatestMod_Release.json";
            Client.Timeout = TimeSpan.FromSeconds(10);
            Client.DefaultRequestHeaders.Add("User-Agent", "User-Agent");
            var response = Client.GetStringAsync(releaseUri).GetAwaiter().GetResult();
            var latest = JSONNode.Parse(response).AsObject;
            var tag = latest["tag_name"].Value;
            if (Version.Parse(LLCMod.Version) < Version.Parse(tag.Remove(0, 1)))
            {
                var updatelog = $"LimbusLocalize_BIE_{tag}.7z";
                Updatelog += updatelog + " ";
                var downloadUri = UpdateUri.Value == Uri.GitHub
                    ? $"https://github.com/LocalizeLimbusCompany/LocalizeLimbusCompany/releases/download/{tag}/{updatelog}"
                    : $"https://node.zeroasso.top/d/od/{updatelog}";
                var filename = Path.Combine(LLCMod.GamePath, updatelog);
                if (!File.Exists(filename)) DownloadFile(downloadUri, filename);
                UpdateCall = UpdateDel;
            }

            LLCMod.LogWarning("Check Chinese Font Asset Update");
            ChineseFontUpdate();
        }
        catch (Exception ex)
        {
            LLCMod.LogWarning($"Mod update failed:\n{ex}");
        }
    }

    public static void ChineseFontUpdate()
    {
        try
        {
            var releaseUri = UpdateUri.Value == Uri.GitHub
                ? "https://api.github.com/repos/LocalizeLimbusCompany/LLC_ChineseFontAsset/releases/latest"
                : "https://json.zxp123.eu.org/LatestTmp_Release.json";
            var response = Client.GetStringAsync(releaseUri).GetAwaiter().GetResult();
            var latest = JSONNode.Parse(response).AsObject;
            var latestReleaseTag = int.Parse(latest["tag_name"].Value);
            var filePath = Path.Combine(LLCMod.ModPath, "tmpchinesefont");
            var lastWriteTime = File.Exists(filePath)
                ? int.Parse(TimeZoneInfo.ConvertTime(new FileInfo(filePath).LastWriteTime,
                    TimeZoneInfo.FindSystemTimeZoneById("China Standard Time")).ToString("yyMMdd"))
                : 0;
            if (lastWriteTime >= latestReleaseTag) return;
            var updatelog = $"tmpchinesefont_BIE_{latestReleaseTag}.7z";
            Updatelog += updatelog + " ";
            var downloadUri = UpdateUri.Value == Uri.GitHub
                ? $"https://github.com/LocalizeLimbusCompany/LLC_ChineseFontAsset/releases/download/{latestReleaseTag}/{updatelog}"
                : $"https://node.zeroasso.top/d/od/{updatelog}";
            var filename = Path.Combine(LLCMod.GamePath, updatelog);
            if (!File.Exists(filename)) DownloadFile(downloadUri, filename);
            UpdateCall = UpdateDel;
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

    private static void UpdateDel()
    {
        LLCMod.OpenGamePath();
        Application.Quit();
    }
}
