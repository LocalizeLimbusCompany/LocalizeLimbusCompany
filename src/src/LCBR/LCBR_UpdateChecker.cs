using Il2CppSimpleJSON;
using Il2CppSystem.Threading;
using Semver;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace LimbusLocalizeRUS
{
    public static class LCBR_UpdateChecker
    {
        public static void StartCheckUpdates()
        {
            LCB_LCBRMod.LogWarning("Mod update check");
            Action ModUpdate = CheckModUpdate;
            new Thread(ModUpdate).Start();
        }
        static void CheckModUpdate()
        {
            UnityWebRequest www = UnityWebRequest.Get("");
            www.timeout = 4;
            www.SendWebRequest();
            while (!www.isDone)
                Thread.Sleep(100);
            if (www.result != UnityWebRequest.Result.Success)
                LCB_LCBRMod.LogWarning("Не удаётся полключиться к GitHub!" + www.error);
            else
            {
                JSONArray releases = JSONNode.Parse(www.downloadHandler.text).AsArray;
                string latestReleaseTag = releases[0]["tag_name"].Value;
                string latest2ReleaseTag = releases.m_List.Count > 1 ? releases[1]["tag_name"].Value : string.Empty;
                if (SemVersion.Parse(LCB_LCBRMod.VERSION) < SemVersion.Parse(latestReleaseTag.Remove(0, 1)))
                {
                    string updatelog = (latest2ReleaseTag == "v" + LCB_LCBRMod.VERSION ? "LimbusLocalizeRus_OTA_" : "LimbusLocalizeRus_") + latestReleaseTag;
                    Updatelog += updatelog + ".7z ";
                    string download = "" + latestReleaseTag + "/" + updatelog + ".7z";
                    var dirs = download.Split('/');
                    string filename = LCB_LCBRMod.GamePath + "/" + dirs[^1];
                    if (!File.Exists(filename))
                        DownloadFileAsync(download, filename).GetAwaiter().GetResult();
                    UpdateCall = UpdateDel;
                }
                LCB_LCBRMod.LogWarning("Check Cyrillic font asset update");
                Action FontAssetUpdate = CheckChineseFontAssetUpdate;
                new Thread(FontAssetUpdate).Start();
            }
            LCB_LCBRMod.LogWarning("Check readme update");
            Action ReadmeUpdate = CheckReadmeUpdate;
            new Thread(ReadmeUpdate).Start();
        }
        static void CheckChineseFontAssetUpdate()
        {
            UnityWebRequest www = UnityWebRequest.Get("");
            string FilePath = LCB_LCBRMod.ModPath + "/tmpcyrillicfont";
            var LastWriteTime = File.Exists(FilePath) ? int.Parse(new FileInfo(FilePath).LastWriteTime.ToString("ddmmyy")) : 0;
            www.SendWebRequest();
            while (!www.isDone)
                Thread.Sleep(100);
            var latest = JSONNode.Parse(www.downloadHandler.text).AsObject;
            int latestReleaseTag = int.Parse(latest["tag_name"].Value);
            if (LastWriteTime < latestReleaseTag)
            {
                string updatelog = "tmpcyrillicfont_" + latestReleaseTag;
                Updatelog += updatelog + ".7z ";
                string download = "" + latestReleaseTag + "/" + updatelog + ".7z";
                var dirs = download.Split('/');
                string filename = LCB_LCBRMod.GamePath + "/" + dirs[^1];
                if (!File.Exists(filename))
                    DownloadFileAsync(download, filename).GetAwaiter().GetResult();
                UpdateCall = UpdateDel;
            }
        }
        static void UpdateDel()
        {
            LCB_LCBRMod.OpenGamePath();
            Application.Quit();
        }
        static async Task DownloadFileAsync(string url, string filePath)
        {
            LCB_LCBRMod.LogWarning("Download " + url + " to " + filePath);
            using HttpClient client = new();
            using HttpResponseMessage response = await client.GetAsync(url);
            using HttpContent content = response.Content;
            using FileStream fileStream = new(filePath, FileMode.Create);
            await content.CopyToAsync(fileStream);
        }
        public static void CheckReadmeUpdate()
        {
            UnityWebRequest www = UnityWebRequest.Get("");
            www.timeout = 1;
            www.SendWebRequest();
            string FilePath = LCB_LCBRMod.ModPath + "/Localize/Readme/Readme.json";
            var LastWriteTime = new FileInfo(FilePath).LastWriteTime;
            while (!www.isDone)
            {
                Thread.Sleep(100);
            }
            if (www.result == UnityWebRequest.Result.Success && LastWriteTime < DateTime.Parse(www.downloadHandler.text))
            {
                UnityWebRequest www2 = UnityWebRequest.Get("");
                www2.SendWebRequest();
                while (!www2.isDone)
                {
                    Thread.Sleep(100);
                }
                File.WriteAllText(FilePath, www2.downloadHandler.text);
                LCBR_ReadmeManager.InitReadmeList();
            }
        }
        public static string Updatelog;
        public static Action UpdateCall;
    }
}
