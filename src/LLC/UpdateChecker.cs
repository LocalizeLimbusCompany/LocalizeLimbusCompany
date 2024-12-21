using BepInEx.Configuration;
using Il2CppSystem.Threading;
using JetBrains.Annotations;
using SimpleJSON;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using UnityEngine;
using UnityEngine.Networking;

namespace LimbusLocalize.LLC;

public static class UpdateChecker
{
    // 存储节点类型
    public enum NodeType
    {
        OneDrive,
        Tianyi
    }
    // 存储节点类型所对应的EndPoint
    // 服务器目前在以下节点托管了热更新文件
    // OneDrive For Business: https://node.zeroasso.top/od/
    // 天翼网盘: https://node.zeroasso.top/tianyi/
    public static readonly Dictionary<NodeType, string> UrlDictionary = new()
    {
        {NodeType.OneDrive, "https://node.zeroasso.top/d/od/"},
        {NodeType.Tianyi, "https://node.zeroasso.top/d/tianyi/"}
    };

    public static ConfigEntry<bool> AutoUpdate =
        LLCMod.LLCSettings.Bind("LLC Settings", "AutoUpdate", false, "是否自动检查并下载更新 ( true | false )");

    public static ConfigEntry<NodeType> UpdateUri = LLCMod.LLCSettings.Bind("LLC Settings", "UpdateURI", NodeType.OneDrive,
        "自动更新所使用URI ( OneDrive：Onedrive For Business | Tianyi：天翼网盘 )");

    // 是否需要弹出更新提示
    public static bool needPopup = false;
    // 字体文件旧版本号，用于PopUp提示
    public static string TMPOldVersion = string.Empty;
    // 字体文件最新版本号，用于PopUp提示
    public static string TMPUpdateVersion = string.Empty;
    // 模组文件旧版本号，用于PopUp提示
    public static string ModOldVersion = string.Empty;
    // 模组最新版本号，用于PopUp提示
    public static string ModUpdateVersion = string.Empty;
    // 更新提示，用于PopUp提示
    public static string HotUpdateMessage = string.Empty;
    // 是否有新版本的程序文件
    public static bool isAppOutdated = false;

    /// <summary>
    /// 检查模组更新入口函数
    /// </summary>
    public static void StartAutoUpdate()
    {
        if (!AutoUpdate.Value) return;
        LLCMod.LogInfo($"Check Mod Update From {UpdateUri.Value}");
        // 此处必须进行同步操作，否则会进行后续操作，然后就寄了
        CheckModUpdate();
    }
    /// <summary>
    /// 检查模组更新实际操作函数
    /// </summary>
    private static void CheckModUpdate()
    {
        // 首先检测是否有version.json文件存在，如果这个文件不存在，就无法获取到当前本地的版本信息，因此直接跳过检查
        if (!File.Exists(LLCMod.ModPath + "/version.json"))
        {
            LLCMod.LogWarning("Can't find version.json. Skip Mod Update Check.");
            return;
        }
        // 其次检查是否有7z.exe文件存在，如果这个文件不存在，就无法解压，因此直接跳过检查
        if (!File.Exists(LLCMod.ModPath + "/7z.exe"))
        {
            LLCMod.LogWarning("Can't find 7z.exe. Skip Mod Update Check.");
            return;
        }

        // 读取本地的version.json文件
        JSONObject localJson = JSONNode.Parse(File.ReadAllText(LLCMod.ModPath + "/version.json")).AsObject;
        // 用Dictionary获取当前配置的EndPoint
        string updateEndpoint = UrlDictionary[UpdateUri.Value];
        // 获取托管在服务器上的版本文件
        UnityWebRequest www = UnityWebRequest.Get("https://hotupdate.zeroasso.top/api/version.json");
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
            JSONObject response = JSONNode.Parse(www.downloadHandler.text).AsObject;

            // 比较本地版本号和服务器上的程序版本号，如果服务器上的程序版本号较新，则不进行自动更新，弹出提示要求用户手动更新
            // 程序版本号代表dll的更新
            int latestAPPVersion = int.Parse(response["app_version"].Value);
            int localAPPVersion = int.Parse(localJson["app_version"].Value);
            if (localAPPVersion < latestAPPVersion)
            {
                needPopup = true;
                isAppOutdated = true;
                LLCMod.LogWarning("New appliction version found. Cannot update mod.");
                return;
            }

            // 复写本地的version.json文件，以更新本地的版本信息
            File.WriteAllText(LLCMod.ModPath + "/version.json", www.downloadHandler.text);
            // 程序版本若已经为最新，则比较文本版本号，如果服务器上的文本版本较新，则下载并解压更新文本资源
            int latestTextVersion = int.Parse(response["text_version"].Value);
            int localTextVersion = int.Parse(localJson["text_version"].Value);

            if (latestTextVersion > localTextVersion)
            {
                // 需要弹出提示
                needPopup = true;
                // 存储模组版本
                ModOldVersion = localTextVersion.ToString();
                ModUpdateVersion = latestTextVersion.ToString();
                // 存储模组更新日志
                HotUpdateMessage = response["notice"].Value.Replace("\\n", "\n");

                // 在服务器上的资源文件格式为：LimbusLocalize_hotupdate_版本号.7z
                // 例如LimbusLocalize_hotupdate_10.7z
                string updatelog = "LimbusLocalize_hotupdate_" + latestTextVersion;

                // 拼接Endpoint和文件名并下载
                // 拼接完应该是例如：https://node.zeroasso.top/d/od/hotupdate/LimbusLocalize_hotupdate_10.7z
                var downloadUri = $"{updateEndpoint}hotupdate/{updatelog}.7z";
                var dirs = downloadUri.Split('/');
                var filename = LLCMod.GamePath + "/" + dirs[^1];
                if (!File.Exists(filename))
                    DownloadFileAsync(downloadUri, filename);

                // 解压
                UnarchiveFile(filename, LLCMod.GamePath);
                LLCMod.LogInfo("Mod Update Success.");
            }
            // 检查字体资源更新
            LLCMod.LogInfo("Check Chinese Font Asset Update");
            CheckChineseFontAssetUpdate();
        }
    }
    /// <summary>
    /// 检查字体资源更新
    /// </summary>
    private static void CheckChineseFontAssetUpdate()
    {
        var releaseUri = "https://api.kr.zeroasso.top/LatestTmp_Release.json";
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
        // 需要弹出提示
        needPopup = true;

        // 在此处存储字体资源的版本号
        TMPUpdateVersion = latestReleaseTag.ToString();
        TMPOldVersion = lastWriteTime.ToString();

        var updatelog = "tmpchinesefont_BIE";
        string updateEndpoint = UrlDictionary[UpdateUri.Value];
        var download = $"{updateEndpoint}{updatelog}.7z";
        var dirs = download.Split('/');
        var filename = LLCMod.GamePath + "/" + dirs[^1];
        if (!File.Exists(filename))
            DownloadFileAsync(download, filename);

        // 在此处改为直接解压
        UnarchiveFile(filename, LLCMod.GamePath);
        LLCMod.LogInfo("Chinese Font Asset Update Success.");
    }

    private static void UnarchiveFile(string sourceFile, string destinationPath)
    {
        LLCMod.LogInfo("Unarchive " + sourceFile + " To " + destinationPath);
        ProcessStartInfo processStartInfo = new ProcessStartInfo
        {
            FileName = LLCMod.ModPath + "/7z.exe",
            Arguments = $"x \"{sourceFile}\" -o\"{destinationPath}\" -y",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
        using (Process process = new())
        {
            process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    LLCMod.LogInfo("Output: " + e.Data);
                }
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    LLCMod.LogError("Error: " + e.Data);
                }
            };
            process.StartInfo = processStartInfo;
            process.Start();
            process.WaitForExit();
        }
        File.Delete(sourceFile);
    }

    private static void DownloadFileAsync(string uri, string filePath)
    {
        try
        {
            LLCMod.LogInfo("Download " + uri + " To " + filePath);
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