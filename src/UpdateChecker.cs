using Il2CppSimpleJSON;
using Il2CppSystem.Threading;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
namespace LimbusLocalize
{
    public static class UpdateChecker
    {
        public static void StartCheckUpdates()
        {
            LimbusLocalizeMod.LogWarning("Check Mod Update");
            Action ModUpdate = CheckModUpdate;
            new Thread(ModUpdate).Start();
        }
        static void CheckModUpdate()
        {
            UnityWebRequest www = UnityWebRequest.Get("https://api.github.com/repos/LocalizeLimbusCompany/LocalizeLimbusCompany/releases");
            www.timeout = 4;
            www.SendWebRequest();
            while (!www.isDone)
            {
                Thread.Sleep(100);
            }
            if (www.result != UnityWebRequest.Result.Success)
            {
                LimbusLocalizeMod.LogWarning("Can't access GitHub!!!" + www.error);
            }
            else
            {
                JSONArray releases = JSONNode.Parse(www.downloadHandler.text).AsArray;

                string latestReleaseTag = releases[0]["tag_name"].Value;
                string latest2ReleaseTag = releases.m_List.Count > 1 ? releases[1]["tag_name"].Value : string.Empty;

                string download = string.Empty;
                var ver = "v" + LimbusLocalizeMod.VERSION;
                if (latest2ReleaseTag == ver)
                {
                    download = "https://github.com/LocalizeLimbusCompany/LocalizeLimbusCompany/releases/download/" + latestReleaseTag + "/LimbusLocalize_OTA_" + latestReleaseTag + ".7z";
                }
                else if (latestReleaseTag != ver)
                {
                    download = "https://github.com/LocalizeLimbusCompany/LocalizeLimbusCompany/releases/download/" + latestReleaseTag + "/LimbusLocalize_" + latestReleaseTag + ".7z";
                }
                if (!string.IsNullOrEmpty(download))
                {
                    var dirs = download.Split('/');
                    string filename = LimbusLocalizeMod.gamepath + "/" + dirs[^1];
                    if (!File.Exists(filename))
                    {
                        DownloadFileAsync(download, filename).GetAwaiter().GetResult();
                    }
                    UpdateCall = UpdateDel;
                }
                LimbusLocalizeMod.LogWarning("Check Chinese Font Asset Update");
                Action FontAssetUpdate = CheckChineseFontAssetUpdate;
                new Thread(FontAssetUpdate).Start();
            }
            LimbusLocalizeMod.LogWarning("Check Readme Update");
            Action ReadmeUpdate = CheckReadmeUpdate;
            new Thread(ReadmeUpdate).Start();
        }
        static void CheckChineseFontAssetUpdate()
        {
            UnityWebRequest www = UnityWebRequest.Get("https://api.github.com/repos/LocalizeLimbusCompany/LLC_ChineseFontAsset/releases/latest");
            string FilePath = LimbusLocalizeMod.modpath + "/tmpchinesefont";
            var LastWriteTime = File.Exists(FilePath) ? new FileInfo(FilePath).LastWriteTime.ToString("yyMMdd") : string.Empty;
            www.SendWebRequest();
            while (!www.isDone)
            {
                Thread.Sleep(100);
            }
            var latest = JSONNode.Parse(www.downloadHandler.text).AsObject;
            string latestReleaseTag = latest["tag_name"].Value;
            if (LastWriteTime != latestReleaseTag)
            {
                string download = "https://github.com/LocalizeLimbusCompany/LLC_ChineseFontAsset/releases/download/" + latestReleaseTag + "/tmpchinesefont_" + latestReleaseTag + ".7z";
                var dirs = download.Split('/');
                string filename = LimbusLocalizeMod.gamepath + "/" + dirs[^1];
                if (!File.Exists(filename))
                {
                    DownloadFileAsync(download, filename).GetAwaiter().GetResult();
                }
                UpdateCall = UpdateDel;
            }
            if (UpdateCall != null)
            {
                ModManager.OpenGlobalPopup("模组更新已下载,点击确认将打开下载路径并退出游戏", default, "取消", "确认", UpdateCall);
            }

        }
        static void UpdateDel()
        {
            Application.OpenURL(LimbusLocalizeMod.gamepath);
            Application.Quit();
        }
        static async Task DownloadFileAsync(string url, string filePath)
        {
            using HttpClient client = new();
            using HttpResponseMessage response = await client.GetAsync(url);
            using HttpContent content = response.Content;
            using FileStream fileStream = new(filePath, FileMode.Create);
            await content.CopyToAsync(fileStream);
        }
        public static void CheckReadmeUpdate()
        {
            UnityWebRequest www = UnityWebRequest.Get("https://LocalizeLimbusCompany.github.io/LocalizeLimbusCompany/LatestUpdateTime.txt");
            www.timeout = 1;
            www.SendWebRequest();
            string FilePath = LimbusLocalizeMod.modpath + "/Localize/Readme/Readme.json";
            var LastWriteTime = new FileInfo(FilePath).LastWriteTime;
            while (!www.isDone)
            {
                Thread.Sleep(100);
            }
            if (www.result == UnityWebRequest.Result.Success && LastWriteTime < DateTime.Parse(www.downloadHandler.text))
            {
                UnityWebRequest www2 = UnityWebRequest.Get("https://LocalizeLimbusCompany.github.io/LocalizeLimbusCompany/Readme.json");
                www2.SendWebRequest();
                while (!www2.isDone)
                {
                    Thread.Sleep(100);
                }
                File.WriteAllText(FilePath, www2.downloadHandler.text);
                ReadmeManager.InitReadmeList();
            }
        }
        public static Action UpdateCall;
    }
}