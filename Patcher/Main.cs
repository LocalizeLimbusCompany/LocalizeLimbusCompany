using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using BepInEx.Preloader.Core.Patching;
using UnityEngine;

namespace LimbusLocalize_Updater;
using System;
using System.Dynamic;
using System.Text.Json;

[PatcherPluginInfo(Guid, Name, VERSION)]
public class UpdaterPatcher : BasePatcher
{
    public enum NodeType
    {
        Auto,
        ZhenJiang,
        GitHub,
        OneDrive,
        Tianyi
    }

    public const string Guid = $"Com.{Author}.{Name}";
    public const string Author = "ZengXiaoPi-Bright";
    public const string Name = "LLC-AutoUpdater";
    public const string VERSION = "1.0.0";

    public static string ModPath = "";
    public static string GamePath = "";
    public static string PatcherPath = "";

    public static int TimeOuted = 10;
    public static bool AutoUpdate;
    public static bool IsUseChinese;
    public static NodeType UpdateUri = NodeType.Auto;

    public static readonly Dictionary<NodeType, string> UrlDictionary = new()
    {
        { NodeType.Auto, "https://api.zeroasso.top/v2/download/files?file_name={0}" },
        { NodeType.ZhenJiang, "https://download.zeroasso.top/files/{0}" },
        { NodeType.OneDrive, "https://node.zeroasso.top/d/od/{0}" },
        { NodeType.Tianyi, "https://node.zeroasso.top/d/tianyi/{0}" }
    };

    private static readonly HttpClient Client = new();

    public static bool NeedPopup;
    public static string AppOldVersion = string.Empty;
    public static string AppUpdateVersion = string.Empty;
    public static string TMPOldVersion = string.Empty;
    public static string TMPUpdateVersion = string.Empty;
    public static string ResourceOldVersion = string.Empty;
    public static string ResourceUpdateVersion = string.Empty;

    public static string UpdateMessage = string.Empty;

    public static Action<string> LogError { get; set; }
    public static Action<string> LogWarning { get; set; }
    public static Action<string> LogInfo { get; set; }

    public override void Initialize()
    {
        PatcherPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        GamePath = new DirectoryInfo(Application.dataPath).Parent!.FullName;
        ModPath = Path.Combine(GamePath, "BepInEx", "plugins", "LLC");

        if (!File.Exists(ModPath + "/version.json") ||
            !File.Exists(PatcherPath + "/7z.exe"))
        {
            LogWarning("Can't Find HotUpdate Need File. Skip Mod Update.");
            return;
        }

        LogInfo = log => Log.LogInfo(log);
        LogWarning = log => Log.LogWarning(log);
        LogError = log => Log.LogError(log);

        var configPath = Path.Combine(GamePath, "BepInEx", "config", "Com.Bright.LocalizeLimbusCompany.cfg");
        if (!File.Exists(configPath))
        {
            TimeOuted = 10;
            AutoUpdate = true;
            UpdateUri = NodeType.Auto;
        }
        else
        {
            // Read Config
            SimpleConfigLoader.Load(configPath);
            IsUseChinese = SimpleConfigLoader.Get("LLC Settings", "IsUseChinese", true);
            if (!IsUseChinese) return;
            TimeOuted = SimpleConfigLoader.Get("LLC Settings", "TimeOuted", 10);
            AutoUpdate = SimpleConfigLoader.Get("LLC Settings", "AutoUpdate", true);
            UpdateUri = SimpleConfigLoader.GetEnum("LLC Settings", "UpdateURI", NodeType.Auto);
        }

        if (!AutoUpdate) return;
        StartAutoUpdate();
    }

    public static void StartAutoUpdate()
    {
        Client.Timeout = TimeSpan.FromSeconds(TimeOuted);
        Client.DefaultRequestHeaders.Add("User-Agent", "LLC-GameClient");
        LogInfo($"Check Mod Update From {UpdateUri}");
        ModUpdate();
        LogInfo("Check Chinese Font Asset Update");
        ChineseFontUpdate();
        if (NeedPopup) GenUpdateText();
    }

    private static void ModUpdate()
    {
        try
        {
            var versionPath = ModPath + "/version.json";
            var localJson = JsonNode.Parse(File.ReadAllText(versionPath)).AsObject;
            var response = Client.GetStringAsync("https://api.github.com/repos/killjsj/LLCMod-mod-something-/releases/latest").GetAwaiter()
                .GetResult();

            var jsonDoc = JsonDocument.Parse(response);
            var root = jsonDoc.RootElement;

            var release = new ExpandoObject();
            release.Url = root.GetProperty("url").GetString();
            release.TagName = root.GetProperty("tag_name").GetString();
            release.Author = new ExpandoObject();
            release.Author.Login = root.GetProperty("author").GetProperty("login").GetString();

            var tag = serverJson["name"].Value;
            var appOldVersion = localJson["version"].Value;
            var latestTextVersion = int.Parse(serverJson["resource_version"].Value);
            var localTextVersion = int.Parse(localJson["resource_version"].Value);
            if (Version.Parse(appOldVersion) < Version.Parse(tag))
            {
                LogInfo("New mod version found. Download full mod.");
                var updatelog = $"LimbusLocalize_BIE_v{tag}.7z";
                var downloadUri = $"https://github.com/LocalizeLimbusCompany/LocalizeLimbusCompany/releases/download/v{tag}/{updatelog}";
                var filename = Path.Combine(GamePath, updatelog);
                if (!File.Exists(filename)) DownloadFile(downloadUri, filename);
                UnarchiveFile(filename, GamePath);
                NeedPopup = true;
                UpdateMessage = updatelog;
                AppOldVersion = appOldVersion;
                AppUpdateVersion = tag;
            }
            else if (latestTextVersion > localTextVersion)
            {
                LogInfo("New text resource found. Download resource.");
                var updatelog = $"LimbusLocalize_Resource_{latestTextVersion}.7z";
                var downloadUri = UpdateUri == NodeType.GitHub
                    ? $"https://github.com/LocalizeLimbusCompany/LLC_Release/releases/download/{latestTextVersion}/{updatelog}"
                    : string.Format(UrlDictionary[UpdateUri], "Resource/" + updatelog);
                var filename = Path.Combine(GamePath, updatelog);
                if (!File.Exists(filename))
                    DownloadFile(downloadUri, filename);
                UnarchiveFile(filename, GamePath);
                NeedPopup = true;
                ResourceOldVersion = localTextVersion.ToString();
                ResourceUpdateVersion = latestTextVersion.ToString();
                UpdateMessage = serverJson["notice"].Value.Replace("\\n", "\n");
                LogInfo("Mod Update Success.");
            }
            else
            {
                LogInfo("No new mod or resource found.");
            }
        }
        catch (Exception ex)
        {
            if (ex is OperationCanceledException)
                LogWarning(
                    "Maybe the timeout time is too short? Please try to change the timeout amount in Com.Bright.LocalizeLimbusCompany.cfg file.");
            LogWarning($"Mod update failed::\n{ex}");
        }
    }

    private static void ChineseFontUpdate()
    {
        try
        {
            var releaseUri = UpdateUri == NodeType.GitHub
                ? "https://api.github.com/repos/LocalizeLimbusCompany/LLC_ChineseFontAsset/releases/latest"
                : "https://api.zeroasso.top/v2/get_api/get/repos/LocalizeLimbusCompany/LLC_ChineseFontAsset/releases/latest";
            var response = Client.GetStringAsync(releaseUri).GetAwaiter().GetResult();
            var latest = JsonNode.Parse(response).AsObject;
            var latestReleaseTag = int.Parse(latest["tag_name"].Value);
            var fontPath = ModPath + "/tmpchinesefont";
            var lastWriteTime = File.Exists(fontPath)
                ? int.Parse(TimeZoneInfo.ConvertTime(new FileInfo(fontPath).LastWriteTime,
                    TimeZoneInfo.FindSystemTimeZoneById("China Standard Time")).ToString("yyMMdd"))
                : 0;
            if (lastWriteTime < latestReleaseTag)
            {
                string updatelog;
                string downloadUri;
                if (UpdateUri == NodeType.GitHub)
                {
                    updatelog = $"tmpchinesefont_BIE_{latestReleaseTag}.7z";
                    downloadUri =
                        $"https://github.com/LocalizeLimbusCompany/LLC_ChineseFontAsset/releases/download/{latestReleaseTag}/{updatelog}";
                }
                else
                {
                    updatelog = "tmpchinesefont_BIE.7z";
                    downloadUri = string.Format(UrlDictionary[UpdateUri], updatelog);
                }

                var filename = Path.Combine(GamePath, updatelog);
                if (!File.Exists(filename))
                    DownloadFile(downloadUri, filename);
                UnarchiveFile(filename, GamePath);
                NeedPopup = true;
                TMPUpdateVersion = latestReleaseTag.ToString();
                TMPOldVersion = lastWriteTime.ToString();
                LogInfo("Chinese Font Asset Update Success.");
            }
            else
            {
                LogInfo("No new font asset found.");
            }
        }
        catch (Exception ex)
        {
            LogWarning($"Font asset update failed:\n{ex}");
        }
    }

    private static void DownloadFile(string uri, string filePath)
    {
        try
        {
            LogInfo($"Download {uri} To {filePath}");
            using var response = Client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead).GetAwaiter()
                .GetResult();
            using FileStream fileStream = new(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
            response.Content.CopyToAsync(fileStream).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            LogWarning(ex is HttpRequestException { StatusCode: HttpStatusCode.NotFound }
                ? $"{uri} 404 NotFound,No Resource"
                : $"{uri} Error!!!:\n{ex}");
            throw;
        }
    }

    private static void UnarchiveFile(string sourceFile, string destinationPath)
    {
        try
        {
            LogInfo($"Unarchiving {sourceFile} To {destinationPath}");
            var processStartInfo = new ProcessStartInfo
            {
                FileName = PatcherPath + "/7z.exe",
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
                if (!string.IsNullOrEmpty(e.Data)) LogInfo("Output: " + e.Data);
            };
            process.ErrorDataReceived += (_, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data)) LogError("Error: " + e.Data);
            };
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
            File.Delete(sourceFile);
        }
        catch (Exception ex)
        {
            LogWarning($"Unarchive file failed:\n{ex}");
        }
    }

    public static void GenUpdateText()
    {
        var updateMessage = "您的模组已更新至最新版本！\n更新内容：";
        if (!string.IsNullOrEmpty(AppOldVersion) && !string.IsNullOrEmpty(AppUpdateVersion))
            updateMessage +=
                $"\n程序更新：v{AppOldVersion} => v{AppUpdateVersion}";
        if (!string.IsNullOrEmpty(ResourceUpdateVersion) &&
            !string.IsNullOrEmpty(ResourceOldVersion))
            updateMessage +=
                $"\n文本更新：v{ResourceOldVersion} => v{ResourceUpdateVersion}";
        if (!string.IsNullOrEmpty(TMPUpdateVersion) &&
            !string.IsNullOrEmpty(TMPOldVersion))
            updateMessage += $"\n字体更新：v{TMPOldVersion} => v{TMPUpdateVersion}";
        if (!string.IsNullOrEmpty(UpdateMessage))
            updateMessage += "\n更新提示：\n" + UpdateMessage;
        File.WriteAllText(Path.Combine(ModPath, "UPDATE_TEMP_INFO"), updateMessage);
    }
}