using Il2Cpp;
using Il2CppInterop.Runtime.Injection;
using Il2CppMainUI;
using Il2CppSimpleJSON;
using Il2CppSteamworks;
using Il2CppSystem.Collections.Generic;
using Il2CppSystem.Linq;
using Il2CppTMPro;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace LimbusLocalize
{
    public class TranslateJSON : MonoBehaviour
    {
        internal static void Setup()
        {
            ClassInjector.RegisterTypeInIl2Cpp<TranslateJSON>();
            GameObject obj = new GameObject("TranslateJSON");
            DontDestroyOnLoad(obj);
            obj.hideFlags |= HideFlags.HideAndDontSave;
            Instance = obj.AddComponent<TranslateJSON>();
        }
        public static TranslateJSON Instance;

        public TranslateJSON(IntPtr ptr)
            : base(ptr)
        {
            
        }

        public void Start()
        {
            __instance = this;
        }
        static TranslateJSON __instance;
        public static void OpenGlobalPopup(string description, string title = "", string close = "取消", string confirm = "确认", DelegateEvent confirmEvent = null, DelegateEvent closeEvent = null)
        {
            TextOkUIPopup globalPopupUI = GlobalGameManager.Instance.globalPopupUI;
            TMP_FontAsset fontAsset = LimbusLocalizeMod.tmpchinesefont;
            TextMeshProUGUI btn_canceltmp = globalPopupUI.btn_cancel.GetComponentInChildren<TextMeshProUGUI>(true);
            btn_canceltmp.font = fontAsset;
            btn_canceltmp.fontMaterial = fontAsset.material;
            UITextDataLoader btn_canceltl = globalPopupUI.btn_cancel.GetComponentInChildren<UITextDataLoader>(true);
            btn_canceltl.enabled = false;
            btn_canceltmp.text = close;
            TextMeshProUGUI btn_oktmp = globalPopupUI.btn_ok.GetComponentInChildren<TextMeshProUGUI>(true);
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
            Action _onconfirm = delegate () { confirmEvent?.Invoke(); btn_canceltl.enabled = true; btn_oktl.enabled = true; };
            globalPopupUI._confirmEvent = _onconfirm;
            Action _onclose = delegate () { closeEvent?.Invoke(); btn_canceltl.enabled = true; btn_oktl.enabled = true; };
            globalPopupUI._closeEvent = _onclose;
            globalPopupUI.btn_cancel.gameObject.SetActive(!string.IsNullOrEmpty(close));
            globalPopupUI._gridLayoutGroup.cellSize = new Vector2(!string.IsNullOrEmpty(close) ? 500 : 700, 100f);
            globalPopupUI.Open();
        }

        public static void CreateKR()
        {
            if (!Directory.Exists(LimbusLocalizeMod.path + "/Localize/KR"))
                Directory.CreateDirectory(LimbusLocalizeMod.path + "/Localize/KR");
            foreach (TextAsset textAsset in Resources.LoadAll<TextAsset>("Localize/KR"))
            {
                File.WriteAllText(LimbusLocalizeMod.path + "/Localize/KR/" + textAsset.name + ".json", textAsset.text);
            }
            File.WriteAllText(LimbusLocalizeMod.path + "/Localize/KR/KR_NickName.json", Resources.Load<TextAsset>("Story/ScenarioModelCode").ToString());

        }
#if false
        public static void StartTranslateText(string text, Action<string> action)
        {
            if (GoogleCanUse)
                StartTranslateTextFromGoogle(text, action);
            else
                StartTranslateTextFromYouDao(text, action);
        }
        public static void StartTranslate()
        {
            __instance.Translate().StartCoroutine();
        }
        public IEnumerator TranslateNickName(string TranslateFrom)
        {
            Dictionary<string, JSONObject> scenarioAssetDataDic = JSONNode.Parse(Resources.Load<TextAsset>("Story/ScenarioModelCode").ToString())[0].Children.ToDictionary(scenario => scenario[0].Value, scenario => scenario as JSONObject);
            Dictionary<string, JSONObject> scenarioAssetDataDic2 = JSONNode.Parse(File.ReadAllText(LimbusLocalize.path + "/Localize/CN/CN_NickName.json"))[0].Children.ToDictionary(scenario => scenario[0].Value, scenario => scenario as JSONObject);

            JSONObject cachechangejson = new JSONObject();
            JSONArray cachechangeroot = new JSONArray();
            cachechangejson["dataList"] = cachechangeroot;

            foreach (var sa in scenarioAssetDataDic)
                if (scenarioAssetDataDic2.TryGetValue(sa.Key, out var sa2))
                {
                    foreach (KeyValuePair<string, JSONNode> sas in sa.Value.m_Dict)
                        if (sa2.m_Dict[sas.Key] != sas.Value)
                            cachechangeroot.Add(sa.Value);
                }
                else
                {
                    cachechangeroot.Add(sa.Value);
                }
            if (cachechangeroot.m_List.Count == 0)
                yield break;
            File.WriteAllText(string.Format("{0}/Localize/Cache/Change/{1}/{1}_NickName.json", LimbusLocalize.path, TranslateFrom), cachechangejson.ToString());
            if (!DoTranslate)
                yield break;
            //翻译队列
            List<Action> TranslateCalls = new List<Action>();


            foreach (var krname in from JSONObject jsonroot in cachechangeroot.Children
                                   select jsonroot.m_Dict["krname"])
            {
                Action<string> action = delegate (string s)
                {
                    TranslateCalls.Add(delegate ()
                    {
                        krname.Value = s;
                    });
                };
                if (GoogleCanUse)
                    yield return TranslateTextFromGoogle(krname.Value, action);
                else
                    yield return TranslateTextFromYouDao(krname.Value, action);
            }

            foreach (Action TranslateCall in TranslateCalls)
                TranslateCall();
            File.WriteAllText(LimbusLocalize.path + "/Localize/Cache/CN/CN_NickName.json", cachechangejson.ToString());
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
            yield return TranslateNickName(TranslateFrom);
            foreach (KeyValuePair<string, TextAsset> rawkrText in rawkrdic)
            {
                string CNfilename = "CN" + rawkrText.Key.Remove(0, 2);
                string TFfilename = TranslateFrom + rawkrText.Key.Remove(0, 2);
                FileInfo fileInfo = new FileInfo(LimbusLocalize.path + "/Localize/CN/" + CNfilename);
                OpenGlobalPopup(fileInfo.Name, "当前进度,避免你无聊", default);
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
                var json2 = hasfile ? JSONNode.Parse(File.ReadAllText(LimbusLocalize.path + "/Localize/KR/" + rawkrText.Key)) : null;
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
            OpenGlobalPopup("完成", "当前进度,避免你无聊", default);
        }
        public static void StartTranslateTextFromGoogle(string text, Action<string> action)
        {
            __instance.StartCoroutine(__instance.TranslateTextFromGoogle(text, action));
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
            __instance.StartCoroutine(__instance.TranslateTextFromYouDao(text, action));
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
            __instance.StartCoroutine(__instance.CheckGoogleCanUse());
        }
#endif
    }
}
