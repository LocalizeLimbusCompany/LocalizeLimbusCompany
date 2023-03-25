using Il2CppSimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
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
        public static void CheckModUpdate()
        {
            LimbusLocalizeMod.OnLogWarning("Check Mod Update");
            var CheckModUpdatesIE = CheckModUpdates();
            while (CheckModUpdatesIE.MoveNext())
            {
                if (CheckModUpdatesIE.Current is UnityWebRequestAsyncOperation)
                {
                    var SendWebRequest = CheckModUpdatesIE.Current as UnityWebRequestAsyncOperation;
                    while (!SendWebRequest.isDone)
                    { }
                }
            }
        }
        static IEnumerator CheckModUpdates()
        {
            UnityWebRequest www = UnityWebRequest.Get("https://api.github.com/repos/Bright1192/LimbusLocalize/releases");
            www.timeout = 4;
            yield return www.SendWebRequest();

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
                    download = "https://github.com/Bright1192/LimbusLocalize/releases/download/" + latestReleaseTag + "/LimbusLocalize_OTA_" + latestReleaseTag + ".7z";
                }
                else if (latestReleaseTag != ver)
                {
                    download = "https://github.com/Bright1192/LimbusLocalize/releases/download/" + latestReleaseTag + "/LimbusLocalize_" + latestReleaseTag + ".7z";
                }
                if (!string.IsNullOrEmpty(download))
                {
                    var dirs = download.Split('/');
                    string filename = dirs[^1];
                    UnityWebRequest wwwdownload = UnityWebRequest.Get(download);
                    yield return wwwdownload.SendWebRequest();
                    var dir = new DirectoryInfo(Application.dataPath).Parent.FullName;
                    var NativeData = wwwdownload.downloadHandler.GetNativeData();
                    List<byte> datas = new();
                    foreach (var file in NativeData)
                    {
                        datas.Add(file);
                    }
                    File.WriteAllBytes(dir + "/" + filename, datas.ToArray());
                    UpdateCall = delegate () { Application.OpenURL(dir); Application.Quit(); };
                }
            }
        }
        public static Action UpdateCall;
    }
}