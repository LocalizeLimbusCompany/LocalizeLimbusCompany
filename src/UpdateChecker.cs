using Il2Cpp;
using Il2CppSimpleJSON;
using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
namespace LimbusLocalize
{
    public static class UpdateChecker
    {
        static UpdateChecker()
        {
            CheckModUpdates().StartCoroutine();
        }
        static IEnumerator CheckModUpdates()
        {
            UnityWebRequest www = UnityWebRequest.Get("https://api.github.com/repos/Bright1192/LimbusLocalize/releases");
            www.timeout = 4;
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                JSONArray releases = (JSONArray)JSONNode.Parse(www.downloadHandler.text);

                string latestReleaseTag = releases[0]["tag_name"].Value;
                string latest2ReleaseTag = releases.m_List.Count > 1 ? releases[1]["tag_name"].Value : string.Empty;

                string download = string.Empty;
                var ver = "v" + LimbusLocalizeMod.VERSION;
                if (latest2ReleaseTag == ver)
                {
                    download = "https://github.com/Bright1192/LimbusLocalize/releases/download/" + latestReleaseTag + "/LimbusLocalize_OTA_" + latestReleaseTag + ".rar";
                }
                else if (latestReleaseTag != ver)
                {
                    download = "https://github.com/Bright1192/LimbusLocalize/releases/download/" + latestReleaseTag + "/LimbusLocalize_" + latestReleaseTag + ".rar";
                }
                if (!string.IsNullOrEmpty(download))
                {
                    var dirs = download.Split('/');
                    string filename = dirs[dirs.Length - 1];
                    UnityWebRequest wwwdownload = UnityWebRequest.Get(download);
                    yield return wwwdownload.SendWebRequest();
                    var dir = new DirectoryInfo(Application.dataPath).Parent.FullName;

                    File.WriteAllBytes(dir + "/" + filename, wwwdownload.downloadHandler.GetData());
                    UpdateCall = delegate () { Application.OpenURL(dir); Application.Quit(); };
                }
            }
        }
        public static Action UpdateCall;
    }
}