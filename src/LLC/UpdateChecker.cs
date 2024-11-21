using System;
using System.IO;
using System.Net;
using System.Net.Http;
using BepInEx.Configuration;
using Il2CppSystem.Threading;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Networking;

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

    public static ConfigEntry<Uri> UpdateUri = LLCMod.LLCSettings.Bind("LLC Settings", "UpdateURI", Uri.GitHub,
        "自动更新所使用URI ( GitHub:默认 | Mirror_OneDrive:镜像,更新可能有延迟,但下载速度更快 )");

    public static string Updatelog;
    public static Action UpdateCall;

    public static void StartAutoUpdate()
    {
        if (!AutoUpdate.Value) return;
        LLCMod.LogWarning($"Check Mod Update From {UpdateUri.Value}");
        var modUpdate = CheckModUpdate;
        new Thread(modUpdate).Start();
    }

    private static void CheckModUpdate()
    {
        var releaseUri = UpdateUri.Value == Uri.GitHub
            ? "https://api.github.com/repos/LocalizeLimbusCompany/LocalizeLimbusCompany/releases/latest"
            : "https://json.zxp123.eu.org/LatestMod_Release.json";
        var www = UnityWebRequest.Get(releaseUri);
        www.timeout = 4;
        www.SendWebRequest();
        while (!www.isDone)
            Thread.Sleep(100);
        if (www.result != UnityWebRequest.Result.Success)
        {
            LLCMod.LogWarning($"Can't access {UpdateUri.Value}!!!" + www.error);
        }
        else
        {
            var latest = JSONNode.Parse(www.downloadHandler.text).AsObject;
            var latestReleaseTag = latest["tag_name"].Value;
            if (Version.Parse(LLCMod.Version) < Version.Parse(latestReleaseTag.Remove(0, 1)))
            {
                var updatelog = "LimbusLocalize_BIE_" + latestReleaseTag;
                Updatelog += updatelog + ".7z ";
                var downloadUri = UpdateUri.Value == Uri.GitHub
                    ? $"https://github.com/LocalizeLimbusCompany/LocalizeLimbusCompany/releases/download/{latestReleaseTag}/{updatelog}.7z"
                    : $"https://node.zeroasso.top/d/od/{updatelog}.7z";
                var dirs = downloadUri.Split('/');
                var filename = LLCMod.GamePath + "/" + dirs[^1];
                if (!File.Exists(filename))
                    DownloadFileAsync(downloadUri, filename);
                UpdateCall = UpdateDel;
            }

            LLCMod.LogWarning("Check Chinese Font Asset Update");
            var fontAssetUpdate = CheckChineseFontAssetUpdate;
            new Thread(fontAssetUpdate).Start();
        }
    }

    private static void CheckChineseFontAssetUpdate()
    {
        var releaseUri = UpdateUri.Value == Uri.GitHub
            ? "https://api.github.com/repos/LocalizeLimbusCompany/LLC_ChineseFontAsset/releases/latest"
            : "https://json.zxp123.eu.org/LatestTmp_Release.json";
        var www = UnityWebRequest.Get(releaseUri);
        var filePath = LLCMod.ModPath + "/tmpchinesefont";
        var lastWriteTime = File.Exists(filePath)
            ? int.Parse(TimeZoneInfo.ConvertTime(new FileInfo(filePath).LastWriteTime,
                TimeZoneInfo.FindSystemTimeZoneById("China Standard Time")).ToString("yyMMdd"))
            : 0;
        www.SendWebRequest();
        while (!www.isDone)
            Thread.Sleep(100);
        var latest = JSONNode.Parse(www.downloadHandler.text).AsObject;
        var latestReleaseTag = int.Parse(latest["tag_name"].Value);
        if (lastWriteTime >= latestReleaseTag) return;
        var updatelog = "tmpchinesefont_BIE_" + latestReleaseTag;
        Updatelog += updatelog + ".7z ";
        var download = UpdateUri.Value == Uri.GitHub
            ? $"https://github.com/LocalizeLimbusCompany/LLC_ChineseFontAsset/releases/download/{latestReleaseTag}/{updatelog}.7z"
            : $"https://node.zeroasso.top/d/od/{updatelog}.7z";
        var dirs = download.Split('/');
        var filename = LLCMod.GamePath + "/" + dirs[^1];
        if (!File.Exists(filename))
            DownloadFileAsync(download, filename);
        UpdateCall = UpdateDel;
    }

    private static void UpdateDel()
    {
        LLCMod.OpenGamePath();
        Application.Quit();
    }

    private static void DownloadFileAsync(string uri, string filePath)
    {
        try
        {
            LLCMod.LogWarning("Download " + uri + " To " + filePath);
            using HttpClient client = new();
            using var response = client.GetAsync(uri).GetAwaiter().GetResult();
            using var content = response.Content;
            using FileStream fileStream = new(filePath, FileMode.Create);
            content.CopyToAsync(fileStream).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            if (ex is HttpRequestException { StatusCode: HttpStatusCode.NotFound })
                LLCMod.LogWarning($"{uri} 404 NotFound,No Resource");
            else
                LLCMod.LogWarning($"{uri} Error!!!" + ex);
        }
    }

    public static void CheckReadmeUpdate()
    {
        var www = UnityWebRequest.Get("https://json.zxp123.eu.org/ReadmeLatestUpdateTime.txt");
        www.timeout = 1;
        www.SendWebRequest();
        var filePath = LLCMod.ModPath + "/Localize/Readme/Readme.json";
        var lastWriteTime = new FileInfo(filePath).LastWriteTime;
        while (!www.isDone) Thread.Sleep(100);
        if (www.result != UnityWebRequest.Result.Success ||
            lastWriteTime >= DateTime.Parse(www.downloadHandler.text)) return;
        var www2 = UnityWebRequest.Get("https://json.zxp123.eu.org/Readme.json");
        www2.SendWebRequest();
        while (!www2.isDone) Thread.Sleep(100);
        File.WriteAllText(filePath, www2.downloadHandler.text);
        ReadmeManager.InitReadmeList();
    }
}