using Addressable;
using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;
using static System.Collections.Specialized.GenericAdapter;

namespace LimbusLocalize
{
    public class TranslateJSON : MonoBehaviour
    {
        public void Start()
        {
            __instance = this;
        }
        static TranslateJSON __instance;
        public static void StartTranslateText(string text, Action<string> action)
        {
            if (GoogleCanUse)
                StartTranslateTextFromGoogle(text, action);
            else
                StartTranslateTextFromYouDao(text, action);
        }
        public static void StartTranslate()
        {
            TranslateJSON.__instance.StartCoroutine(__instance.Translate());
        }
        public static void OpenGlobalPopup(string description, string title = "", string close = "取消", string confirm = "确认", DelegateEvent confirmEvent = null, DelegateEvent closeEvent = null)
        {
            MainUI.TextOkUIPopup globalPopupUI = GlobalGameManager.Instance.globalPopupUI;
            TMPro.TMP_FontAsset fontAsset = LimbusLocalize.TMP_FontAssets[0];
            TMPro.TextMeshProUGUI btn_canceltmp = globalPopupUI.btn_cancel.GetComponentInChildren<TMPro.TextMeshProUGUI>(true);
            btn_canceltmp.font = fontAsset;
            btn_canceltmp.fontMaterial = fontAsset.material;
            UITextDataLoader btn_canceltl = globalPopupUI.btn_cancel.GetComponentInChildren<UITextDataLoader>(true);
            btn_canceltl.enabled = false;
            btn_canceltmp.text = close;
            TMPro.TextMeshProUGUI btn_oktmp = globalPopupUI.btn_ok.GetComponentInChildren<TMPro.TextMeshProUGUI>(true);
            btn_oktmp.font = fontAsset;
            btn_oktmp.fontMaterial = fontAsset.material;
            UITextDataLoader btn_oktl = globalPopupUI.btn_ok.GetComponentInChildren<UITextDataLoader>(true);
            btn_oktl.enabled = false;
            btn_oktmp.text = confirm;
            globalPopupUI.tmp_title.font = fontAsset;
            globalPopupUI.tmp_title.fontMaterial = fontAsset.material;
            globalPopupUI._titleObject.SetActive(!string.IsNullOrEmpty(title));
            globalPopupUI.tmp_title.text = title;
            globalPopupUI.tmp_description.text = description;
            globalPopupUI._confirmEvent = delegate () { confirmEvent?.Invoke(); btn_canceltl.enabled = true; btn_oktl.enabled = true; };
            globalPopupUI._closeEvent = delegate () { closeEvent?.Invoke(); btn_canceltl.enabled = true; btn_oktl.enabled = true; };
            globalPopupUI.btn_cancel.gameObject.SetActive(confirmEvent != null);
            globalPopupUI._gridLayoutGroup.cellSize = new Vector2(confirmEvent != null ? 500 : 700, 100f);
            globalPopupUI.Open();
        }
        public static void CreateKR()
        {
            if (!Directory.Exists(LimbusLocalize.path + "/Localize/KR"))
                Directory.CreateDirectory(LimbusLocalize.path + "/Localize/KR");
            foreach ((string, TextAsset raw) LocalizeKR in (from TextAsset raw in Resources.LoadAll<TextAsset>("Localize/KR")
                                                            select (raw.name + ".json", raw)))
            {
                File.WriteAllText(LimbusLocalize.path + "/Localize/KR/" + LocalizeKR.Item1, LocalizeKR.raw.text);
            }

        }
        public IEnumerator Translate()
        {
            yield return CheckGoogleCanUse();
            string TranslateFrom = GoogleCanUse ? "KR" : "EN";
            if (!Directory.Exists(LimbusLocalize.path + "/Localize/Cache"))
                Directory.CreateDirectory(LimbusLocalize.path + "/Localize/Cache");
            if (!Directory.Exists(LimbusLocalize.path + "/Localize/Cache/Change"))
                Directory.CreateDirectory(LimbusLocalize.path + "/Localize/Cache/Change");
            if (!Directory.Exists(LimbusLocalize.path + "/Localize/Cache/Change/" + TranslateFrom))
                Directory.CreateDirectory(LimbusLocalize.path + "/Localize/Cache/Change/" + TranslateFrom);
            if (!Directory.Exists(LimbusLocalize.path + "/Localize/Cache/CN"))
                Directory.CreateDirectory(LimbusLocalize.path + "/Localize/Cache/CN");
            if (DoTranslate)
            {
                File.WriteAllText(LimbusLocalize.path + "/Localize/Cache/HowToLoadCache", LimbusLocalize.VERSION + " " + "Change/" + TranslateFrom);
            }
            else
            {
                File.WriteAllText(LimbusLocalize.path + "/Localize/Cache/HowToLoadCache", LimbusLocalize.VERSION + " " + "CN");
            }
            Dictionary<string, TextAsset> rawkrdic = Resources.LoadAll<TextAsset>("Localize/KR").ToDictionary(raw => raw.name + ".json", raw => raw);
            Dictionary<string, TextAsset> TranslateFromDic = Resources.LoadAll<TextAsset>("Localize/" + TranslateFrom).ToDictionary(raw => raw.name + ".json", raw => raw);

            foreach (KeyValuePair<string, TextAsset> rawkrText in rawkrdic)
            {
                string CNfilename = "CN" + rawkrText.Key.Remove(0, 2);
                string TFfilename = TranslateFrom + rawkrText.Key.Remove(0, 2);
                FileInfo fileInfo = new FileInfo(LimbusLocalize.path + "/Localize/CN/" + CNfilename);
                TranslateJSON.OpenGlobalPopup(fileInfo.Name, "当前进度,避免你无聊");
                //从本地加载汉化数据
                var json = JSONNode.Parse(File.ReadAllText(fileInfo.FullName));
                Dictionary<string, JSONObject> cn = json[0].Children.ToDictionary(jsonroot =>
                {
                    JSONObject obj = (jsonroot as JSONObject);
                    string objValue = obj.m_Dict.TryGetValue("id", out var c) ? c.Value : "-1";
                    return objValue == "-1" ? obj.m_Dict.TryGetValue("content", out var v) ? v.Value : obj.m_Dict["model"].Value : objValue;
                }, jsonroot => jsonroot as JSONObject);

                //从本地加载KR对比用数据
                bool hasfile = File.Exists(LimbusLocalize.path + "/Localize/KR/" + rawkrText.Key);
                var json2 = JSONNode.Parse(File.ReadAllText(LimbusLocalize.path + "/Localize/KR/" + rawkrText.Key));
                Dictionary<string, JSONObject> cachekr = hasfile ? json2[0].Children.ToDictionary(jsonroot =>
                {
                    JSONObject obj = (jsonroot as JSONObject);
                    string objValue = obj.m_Dict.TryGetValue("id", out var c) ? c.Value : "-1";
                    return objValue == "-1" ? obj.m_Dict.TryGetValue("content", out var v) ? v.Value : obj.m_Dict["model"].Value : objValue;
                }, jsonroot => jsonroot as JSONObject) : null;
                //从Resources加载原始KR数据
                var json3 = JSONNode.Parse(rawkrText.Value.text);
                Dictionary<string, JSONObject> rawkr = json3[0].Children.ToDictionary(jsonroot =>
                {
                    JSONObject obj = (jsonroot as JSONObject);
                    string objValue = obj.m_Dict.TryGetValue("id", out var c) ? c.Value : "-1";
                    return objValue == "-1" ? obj.m_Dict.TryGetValue("content", out var v) ? v.Value : obj.m_Dict["model"].Value : objValue;
                }, jsonroot => jsonroot as JSONObject);
                //从Resources加载机翻用数据
                var json4 = JSONNode.Parse(TranslateFromDic[TFfilename].text);
                Dictionary<string, JSONObject> tf = json4[0].Children.ToDictionary(jsonroot =>
                {
                    JSONObject obj = (jsonroot as JSONObject);
                    string objValue = obj.m_Dict.TryGetValue("id", out var c) ? c.Value : "-1";
                    return objValue == "-1" ? obj.m_Dict.TryGetValue("content", out var v) ? v.Value : obj.m_Dict["model"].Value : objValue;
                }, jsonroot => jsonroot as JSONObject);

                JSONObject cachechangejson = new JSONObject();
                JSONArray cachechangeroot = new JSONArray();
                cachechangejson["dataList"] = cachechangeroot;

                //将改动的值添加翻译队列
                foreach (KeyValuePair<string, JSONObject> rawkrchild in rawkr)
                    if (hasfile && cachekr.TryGetValue(rawkrchild.Key, out var ckrchild))
                    {
                        foreach (KeyValuePair<string, JSONNode> racs in rawkrchild.Value.m_Dict)
                        {
                            if (racs.Value.IsObject)
                            {
                                if (ckrchild.m_Dict[racs.Key] != racs.Value)
                                    cachechangeroot.Add(tf[rawkrchild.Key]);
                            }
                            else
                            {
                                if (ckrchild.m_Dict[racs.Key].ToString() != racs.Value.ToString())
                                    cachechangeroot.Add(tf[rawkrchild.Key]);
                            }
                        }
                    }
                    else
                    {
                        cachechangeroot.Add(tf[rawkrchild.Key]);
                    }
                if (cachechangeroot.m_List.Count == 0)
                    continue;
                File.WriteAllText(LimbusLocalize.path + "/Localize/Cache/Change/" + TranslateFrom + "/" + TFfilename, cachechangejson.ToString());
                if (!DoTranslate)
                    continue;
                //翻译队列
                List<Action> TranslateCalls = new List<Action>();

                foreach ((JSONObject jsonroot, KeyValuePair<string, JSONNode> keyValue, string Key, string Value) in from JSONObject jsonroot in cachechangeroot.Children
                                                                                                                     from keyValue in jsonroot.m_Dict
                                                                                                                     let Key = keyValue.Key.ToString()
                                                                                                                     let Value = keyValue.Value.ToString()
                                                                                                                     select (jsonroot, keyValue, Key, Value))
                {

                    if (Regex.Matches(Value, "[0-9-]+").GetMatch(0)?.Value != Value && Key != "usage" && Key != "model" && Key != "id")
                    {
                        Action<string> action = delegate (string s)
                        {
                            TranslateCalls.Add(delegate ()
                            {
                                jsonroot.m_Dict[Key] = s;
                            });
                        };
                        if (GoogleCanUse)
                            yield return TranslateTextFromGoogle(jsonroot[Key].IsObject ? jsonroot[Key].Value : jsonroot[Key].ToString(), action);
                        else
                            yield return TranslateTextFromYouDao(jsonroot[Key].IsObject ? jsonroot[Key].Value : jsonroot[Key].ToString(), action);
                    }

                }

                foreach (Action TranslateCall in TranslateCalls)
                    TranslateCall();
                File.WriteAllText(LimbusLocalize.path + "/Localize/Cache/CN/" + fileInfo.Name, cachechangejson.ToString());

            }
            TranslateCall();
            TranslateJSON.OpenGlobalPopup("完成", "当前进度,避免你无聊");
        }
        public static void StartTranslateTextFromGoogle(string text, Action<string> action)
        {
            TranslateJSON.__instance.StartCoroutine(__instance.TranslateTextFromGoogle(text, action));
        }
        private IEnumerator TranslateTextFromGoogle(string text, Action<string> action)
        {
            string uri = "https://translate.googleapis.com/translate_a/single?client=gtx&dt=t&sl=ko&tl=zh-CN&q=" + WebUtility.UrlEncode(text);
            UnityWebRequest www = UnityWebRequest.Get(uri);
            yield return www.SendWebRequest();
            string responseBody = www.downloadHandler.text;
            int startIndex = responseBody.IndexOf('\"');
            if (startIndex != -1)
            {
                int endIndex = responseBody.IndexOf('\"', startIndex + 1);
                if (endIndex != -1)
                    action(responseBody.Substring(startIndex + 1, endIndex - startIndex - 1));
            }
        }
        public static void StartTranslateTextFromYouDao(string text, Action<string> action)
        {
            TranslateJSON.__instance.StartCoroutine(__instance.TranslateTextFromYouDao(text, action));
        }
        private IEnumerator TranslateTextFromYouDao(string text, Action<string> action)
        {
            string uri = "https://fanyi.youdao.com/translate?&doctype=json&type=en2zh_cn&i=" + WebUtility.UrlEncode(text);
            UnityWebRequest www = UnityWebRequest.Get(uri);
            yield return www.SendWebRequest();
            string responseBody = www.downloadHandler.text;
            action(JSONNode.Parse(responseBody)[3][0][0][1]);
        }
        private IEnumerator CheckGoogleCanUse()
        {
            string url = "https://translate.googleapis.com/translate_a/single?client=gtx&dt=t&sl=ko&tl=zh-CN&q=이스마엘";
            UnityWebRequest request = UnityWebRequest.Get(url);
            request.timeout = 4;
            yield return request.SendWebRequest();
            GoogleCanUse = request.result == UnityWebRequest.Result.Success;
        }
        static bool GoogleCanUse;
        public static Action TranslateCall;
        public static bool DoTranslate;
        public static void UpdateGoogleCanUse()
        {
            TranslateJSON.__instance.StartCoroutine(__instance.CheckGoogleCanUse());
        }
    }
}