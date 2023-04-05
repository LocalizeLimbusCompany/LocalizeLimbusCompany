using Il2CppSimpleJSON;
using Il2CppSystem.Threading;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
namespace LimbusLocalize
{
    public static class UpdateChecker
    {
        static UpdateChecker()
        {
        }
        public static void StartCheckUpdates()
        {
            LimbusLocalizeMod.OnLogWarning("Check Mod Update");
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
                LimbusLocalizeMod.OnLogWarning("Can't access GitHub!!!" + www.error);
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
                    string filename = dirs[^1];
                    UnityWebRequest wwwdownload = UnityWebRequest.Get(download);
                    wwwdownload.SendWebRequest();
                    while (!wwwdownload.isDone)
                    {
                        Thread.Sleep(100);
                    }
                    var NativeData = wwwdownload.downloadHandler.GetNativeData();
                    List<byte> datas = new();
                    foreach (var file in NativeData)
                    {
                        datas.Add(file);
                    }
                    File.WriteAllBytes(LimbusLocalizeMod.gamepath + "/" + filename, datas.ToArray());
                    UpdateCall = UpdateDel;
                }
                LimbusLocalizeMod.OnLogWarning("Check Chinese Font Asset Update");
                Action FontAssetUpdate = CheckChineseFontAssetUpdate;
                new Thread(FontAssetUpdate).Start();
            }
        }
        static void CheckChineseFontAssetUpdate()
        {
            UnityWebRequest www = UnityWebRequest.Get("https://api.github.com/repos/LocalizeLimbusCompany/LLC_ChineseFontAsset/releases/latest");
            string FilePath = LimbusLocalizeMod.path + "/tmpchinesefont";
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
                string filename = dirs[^1];
                if (!File.Exists(filename))
                {
                    UnityWebRequest wwwdownload = UnityWebRequest.Get(download);
                    wwwdownload.SendWebRequest();
                    while (!wwwdownload.isDone)
                    {
                        Thread.Sleep(100);
                    }
                    var NativeData = wwwdownload.downloadHandler.GetNativeData();
                    List<byte> datas = new();
                    foreach (var file in NativeData)
                    {
                        datas.Add(file);
                    }
                    File.WriteAllBytes(LimbusLocalizeMod.gamepath + "/" + filename, datas.ToArray());
                }
                UpdateCall = UpdateDel;
            }
            LimbusLocalizeMod.OnLogWarning("Check Readme Update");
            Action ReadmeUpdate = CheckReadmeUpdate;
            new Thread(ReadmeUpdate).Start();
        }
        static void UpdateDel()
        {
            Application.OpenURL(LimbusLocalizeMod.gamepath);
            Application.Quit();
        }
        static void CheckChineseLocalizeUpdate()
        {
            UnityWebRequest www = UnityWebRequest.Get("https://api.github.com/repos/LocalizeLimbusCompany/LLC_ChineseLocalize/releases");
        }
        static void CheckReadmeUpdate()
        {
            UnityWebRequest www = UnityWebRequest.Get("https://LocalizeLimbusCompany.github.io/LocalizeLimbusCompany/LatestUpdateTime.txt");
            string FilePath = LimbusLocalizeMod.path + "/Localize/Readme/Readme.json";
            var LastWriteTime = new FileInfo(FilePath).LastWriteTime;
            www.SendWebRequest();
            while (!www.isDone)
            {
                Thread.Sleep(100);
            }
            if(LastWriteTime < DateTime.Parse(www.downloadHandler.text))
            {
                UnityWebRequest www2 = UnityWebRequest.Get("https://LocalizeLimbusCompany.github.io/LocalizeLimbusCompany/Readme.json"); 
                www2.SendWebRequest();
                while (!www2.isDone)
                {
                    Thread.Sleep(100);
                }
                ReadmeManager.ReadmeList.Clear();
                File.WriteAllText(FilePath, www.downloadHandler.text);
                ReadmeManager.InitReadmeList();
            }
        }
        public static Action UpdateCall;
    }
}