using BepInEx.Configuration;
using Il2CppSystem.Threading;
using SimpleJSON;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using UnityEngine.Networking;

namespace LimbusLocalize
{
    public static class LLC_UpdateChecker
    {
        public static ConfigEntry<bool> AutoUpdate = LCB_LLCMod.LLC_Settings.Bind("LLC Settings", "AutoUpdate", false, "是否自动检查并下载文本更新 ( true | false )");
        public static ConfigEntry<AutoUpdateSource> UpdateURI = LCB_LLCMod.LLC_Settings.Bind("LLC Settings", "UpdateURI", AutoUpdateSource.Mirror_OneDrive, "自动更新所使用镜像源 ( 可选节点：Mirror_OneDrive：零协会OFB网盘（推荐） | Mirror_Mobile：移动网盘 | Mirror_Tianyi：天翼云盘 | Mirror_Unicom：联通云盘 )");
        public static ConfigEntry<int> AutoUpdateTimeout = LCB_LLCMod.LLC_Settings.Bind("LLC Settings", "AutoUpdateTimeout", 10, "自动更新检查超时时间（若出现Timeout，可尝试增大）");
        public static bool isUpdate = false;
        public static bool isAppOut = false;
        public static int nowTextVersion = -10001;
        public static string updateNotice = "本次文本更新没有提示。";
        public static void UpdateMod()
        {
            bool textisUpdated = CheckTextUpdate(out int web_text_version, out bool is_app_outupdated, out string update_notice);
            if (is_app_outupdated)
            {
                isAppOut = true;
                LCB_LLCMod.LogInfo("No need to update.");
                return;
            }
            if (!string.IsNullOrEmpty(update_notice))
            {
                updateNotice = update_notice.Replace("\\n", Environment.NewLine); ;
            }
            if (AutoUpdate.Value && !textisUpdated && web_text_version != -10001)
            {
                LCB_LLCMod.LogInfo("UpdateURI is " + UpdateURI.Value + ".");
                DownloadFileAsync(AutoUpdateURL[UpdateURI.Value] + $"hotupdate/LimbusLocalize_hotupdate_{web_text_version}.7z", LCB_LLCMod.ModPath + $"\\LimbusLocalize_hotupdate_{web_text_version}.7z");
                LCB_LLCMod.LogInfo("Start unarchiving.");
                Unarchive(LCB_LLCMod.ModPath + $"/LimbusLocalize_hotupdate_{web_text_version}.7z", LCB_LLCMod.GamePath);
                File.Delete(LCB_LLCMod.ModPath + $"/LimbusLocalize_hotupdate_{web_text_version}.7z");
                nowTextVersion = web_text_version;
                LCB_LLCMod.LogInfo("Mod is updated.");
                isUpdate = true;
            }
            else
            {
                LCB_LLCMod.LogInfo("No need to update.");
            }
        }
        private static void DownloadFileAsync(string uri, string filePath)
        {
            try
            {
                LCB_LLCMod.LogInfo("Download " + uri + " To " + filePath);
                using HttpClient client = new();
                using HttpResponseMessage response = client.GetAsync(uri).GetAwaiter().GetResult();
                using HttpContent content = response.Content;
                using FileStream fileStream = new(filePath, FileMode.Create);
                content.CopyToAsync(fileStream).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                if (ex is HttpRequestException httpException && httpException.StatusCode == HttpStatusCode.NotFound)
                    LCB_LLCMod.LogWarning($"{uri} 404 NotFound,No Resource");
                else
                    LCB_LLCMod.LogWarning($"{uri} Error!!!" + ex.ToString());
            }
        }
        public static bool CheckTextUpdate(out int web_text_version_out, out bool is_app_outupdated, out string update_notice)
        {
            try
            {
                UnityWebRequest www = UnityWebRequest.Get("https://hotupdate.zeroasso.top/api/version.json");
                www.timeout = AutoUpdateTimeout.Value;
                www.SendWebRequest();
                while (!www.isDone)
                {
                    Thread.Sleep(100);
                }
                if (www.result == UnityWebRequest.Result.Success)
                {
                    JSONObject json = JSONNode.Parse(www.downloadHandler.text).AsObject;
                    var local_json = JSONNode.Parse(File.ReadAllText(LCB_LLCMod.ModPath + "/version.json")).AsObject;
                    int web_app_version = json["app_version"].AsInt;
                    int local_app_version = local_json["app_version"].AsInt;
                    if (web_app_version > local_app_version)
                    {
                        LCB_LLCMod.LogInfo("App is outupdated.");
                        is_app_outupdated = true;
                    }
                    else
                    {
                        LCB_LLCMod.LogInfo("App is not outupdated.");
                        is_app_outupdated = false;
                    }
                    int web_text_version = json["text_version"].AsInt;
                    int local_text_version = local_json["text_version"].AsInt;
                    web_text_version_out = web_text_version;
                    LCB_LLCMod.LogInfo($"Local Text Version: {local_text_version}, Web Text Version: {web_text_version}");
                    update_notice = json["notice"].Value;
                    return web_text_version <= local_text_version;
                }
                else
                {
                    LCB_LLCMod.LogWarning($"Check Text Update Failed: {www.error}");
                    web_text_version_out = -10001;
                    is_app_outupdated = false;
                    update_notice = null;
                    return true;
                }
            }
            catch(Exception ex)
            {
                LCB_LLCMod.LogWarning("Check Text Update Error: " + ex.ToString());
                web_text_version_out = -10001;
                is_app_outupdated = false;
                update_notice = null;
                return true;
            }
        }
        public static void CheckReadmeUpdate()
        {
            UnityWebRequest www = UnityWebRequest.Get("https://json.zxp123.eu.org/ReadmeLatestUpdateTime.txt");
            www.timeout = 1;
            www.SendWebRequest();
            string FilePath = LCB_LLCMod.ModPath + "/Localize/Readme/Readme.json";
            var LastWriteTime = new FileInfo(FilePath).LastWriteTime;
            while (!www.isDone)
            {
                Thread.Sleep(100);
            }
            if (www.result == UnityWebRequest.Result.Success && LastWriteTime < DateTime.Parse(www.downloadHandler.text))
            {
                UnityWebRequest www2 = UnityWebRequest.Get("https://json.zxp123.eu.org/Readme.json");
                www2.SendWebRequest();
                while (!www2.isDone)
                {
                    Thread.Sleep(100);
                }
                File.WriteAllText(FilePath, www2.downloadHandler.text);
                LLC_ReadmeManager.InitReadmeList();
            }
        }
        public static void Unarchive(string archivePath, string output)
        {
            try
            {
                ProcessStartInfo processStartInfo = new ProcessStartInfo
                {
                    FileName = LCB_LLCMod.ModPath + "\\7z.exe",
                    Arguments = $"x \"{archivePath}\" -o\"{output}\" -y",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardError = true,
                };

                using (Process process = new Process { StartInfo = processStartInfo })
                {
                    process.Start();

                    process.WaitForExit();

                    if (process.ExitCode == 0)
                    {
                        Console.WriteLine("Unarchive Success!");
                    }
                    else
                    {
                        Console.WriteLine("Unarchive Failed!");
                        string error = process.StandardError.ReadToEnd();
                        LCB_LLCMod.LogError(error);
                    }
                }
            }
            catch (Exception ex)
            {
                LCB_LLCMod.LogError("Unarchive Error: " + ex.ToString());
            }
        }
        public enum AutoUpdateSource
        {
            Mirror_OneDrive,
            Mirror_Mobile,
            Mirror_Tianyi,
            Mirror_Unicom
        }
        public readonly static Dictionary<AutoUpdateSource, string> AutoUpdateURL = new()
        {
            { AutoUpdateSource.Mirror_OneDrive, "https://node.zeroasso.top/d/od/" },
            { AutoUpdateSource.Mirror_Mobile, "https://node.zeroasso.top/d/mobile/" },
            { AutoUpdateSource.Mirror_Tianyi, "https://node.zeroasso.top/d/tianyi/" },
            { AutoUpdateSource.Mirror_Unicom, "https://node.zeroasso.top/d/unicom/" }
        };
    }
}