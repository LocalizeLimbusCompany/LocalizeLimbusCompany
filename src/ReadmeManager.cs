using HarmonyLib;
using Il2Cpp;
using Il2CppLocalSave;
using Il2CppMainUI;
using Il2CppMainUI.NoticeUI;
using Il2CppServer;
using Il2CppSimpleJSON;
using Il2CppSystem.Collections.Generic;
using Il2CppTMPro;
using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UObject = UnityEngine.Object;

namespace LimbusLocalize
{
    public static class ReadmeManager
    {
        public static NoticeUIPopup NoticeUIInstance;
        public static RedDotWriggler _redDot_Notice;
        public static void Initialize()
        {
            InitReadmeSprites();
            NoticeUIInstance.Initialize();
            NoticeUIInstance.btn_systemNotice.GetComponentInChildren<UITextDataLoader>(true).enabled = false;
            NoticeUIInstance.btn_systemNotice.GetComponentInChildren<TextMeshProUGUI>(true).text = "更新公告";
            NoticeUIInstance.btn_eventNotice.GetComponentInChildren<UITextDataLoader>(true).enabled = false;
            NoticeUIInstance.btn_eventNotice.GetComponentInChildren<TextMeshProUGUI>(true).text = "贡献,反馈,赞助";
        }
        public static void AddClosedel()
        {
            NoticeUIInstance.btn_back._onClick.RemoveAllListeners();
            Action _back = delegate () { Close(); };
            NoticeUIInstance.btn_back._onClick.AddListener(_back);
        }
        public static void Open()
        {
            AddClosedel();
            NoticeUIInstance.Open();
            NoticeUIInstance._popupPanel.Open();
            List<Notice> notices = ReadmeList;
            Func<Notice, bool> findsys = (Notice x) => x.noticeType == NOTICE_TYPE.System;
            NoticeUIInstance._systemNotices = notices.FindAll(findsys);
            Func<Notice, bool> findeve = (Notice x) => x.noticeType == NOTICE_TYPE.Event;
            NoticeUIInstance._eventNotices = notices.FindAll(findeve);
            NoticeUIInstance.EventTapClickEvent();
            NoticeUIInstance.btn_eventNotice.Cast<UISelectedButton>().SetSelected(true);
        }
        public static void InitReadmeSprites()
        {
            ReadmeSprites = new Dictionary<string, Sprite>();

            foreach (FileInfo fileInfo in new DirectoryInfo(LimbusLocalizeMod.modpath + "/Localize/Readme").GetFiles().Where(f => f.Extension != ".json"))
            {
                Texture2D texture2D = new(2, 2);
                ImageConversion.LoadImage(texture2D, File.ReadAllBytes(fileInfo.FullName));
                Sprite value = Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f));
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileInfo.FullName);
                texture2D.name = fileNameWithoutExtension;
                value.name = fileNameWithoutExtension;
                ReadmeSprites[fileNameWithoutExtension] = value;
            }

        }
        public static void InitReadmeList()
        {
            ReadmeList.Clear();
            foreach (var notices in JSONNode.Parse(File.ReadAllText(LimbusLocalizeMod.modpath + "/Localize/Readme/Readme.json"))[0].AsArray.m_List)
            {
                ReadmeList.Add(new Notice(JsonUtility.FromJson<NoticeFormat>(notices.ToString()), LOCALIZE_LANGUAGE.KR));
            }
        }
        public static List<Notice> ReadmeList = new();
        public static Dictionary<string, Sprite> ReadmeSprites = new();
        public static System.Collections.Generic.Dictionary<string, Action> ReadmeActions = new();

        public static void Close()
        {
            Singleton<UserLocalSaveDataRoot>.Instance.NoticeRedDotSaveModel.Save();
            NoticeUIInstance._popupPanel.Close();
            UpdateNoticeRedDot();
        }
        public static void UpdateNoticeRedDot()
        {
            _redDot_Notice.gameObject.SetActive(IsValidRedDot());
        }
        public static bool IsValidRedDot()
        {
            int i = 0;
            int count = ReadmeList.Count;
            while (i < count)
            {
                if (!Singleton<UserLocalSaveDataRoot>.Instance.NoticeRedDotSaveModel.TryCheckId(ReadmeList[i].ID))
                {
                    return true;
                }
                i++;
            }
            return false;
        }
        #region 公告相关
        [HarmonyPatch(typeof(UserLocalNoticeRedDotModel), nameof(UserLocalNoticeRedDotModel.InitNoticeList))]
        [HarmonyPrefix]
        public static bool InitNoticeList(UserLocalNoticeRedDotModel __instance, List<int> severNoticeList)
        {
            for (int i = 0; i < __instance.GetDataList().Count; i++)
            {
                Func<int, bool> func = x =>
                {
                    Func<Notice, bool> value1 = x2 => x2.ID == x;
                    return !severNoticeList.Contains(x) && ReadmeList.FindAll(value1).Count == 0;
                };
                __instance.idList.RemoveAll(func);
            }
            __instance.Save();
            UpdateNoticeRedDot();
            return false;
        }
        [HarmonyPatch(typeof(NoticeUIPopup), nameof(NoticeUIPopup.Initialize))]
        [HarmonyPostfix]
        public static void NoticeUIPopupInitialize(NoticeUIPopup __instance)
        {
            if (!NoticeUIInstance)
            {
                var NoticeUIPopupInstance = UObject.Instantiate(__instance, __instance.transform.parent);
                NoticeUIInstance = NoticeUIPopupInstance;
                Initialize();
                AddClosedel();
            }
        }
        [HarmonyPatch(typeof(MainLobbyUIPanel), nameof(MainLobbyUIPanel.Initialize))]
        [HarmonyPostfix]
        public static void MainLobbyUIPanelInitialize(MainLobbyUIPanel __instance)
        {
            var UIButtonInstance = UObject.Instantiate(__instance.button_notice, __instance.button_notice.transform.parent).Cast<MainLobbyRightUpperUIButton>();
            _redDot_Notice = UIButtonInstance.gameObject.GetComponentInChildren<RedDotWriggler>(true);
            UpdateNoticeRedDot();
            UIButtonInstance._onClick.RemoveAllListeners();
            System.Action onClick = delegate
            {
                Open();
            };
            UIButtonInstance._onClick.AddListener(onClick);
            UIButtonInstance.transform.SetSiblingIndex(1);
            var spriteSetting = new ButtonSprites()
            {
                _enabled = ReadmeSprites["Readme_Zero_Button"],
                _hover = ReadmeSprites["Readme_Zero_Button"]
            };
            UIButtonInstance.spriteSetting = spriteSetting;
            var transform = __instance.button_notice.transform.parent;
            var layoutGroup = transform.GetComponent<HorizontalLayoutGroup>();
            layoutGroup.childScaleHeight = true;
            layoutGroup.childScaleWidth = true;
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).localScale = new Vector3(0.8f, 0.8f, 1f);
            }
        }
        [HarmonyPatch(typeof(NoticeUIContentImage), nameof(NoticeUIContentImage.SetData))]
        [HarmonyPrefix]
        public static bool ImageSetData(NoticeUIContentImage __instance, string formatValue)
        {
            if (formatValue.StartsWith("Readme_"))
            {
                Sprite image = ReadmeSprites[formatValue];
                __instance.gameObject.SetActive(true);
                __instance.SetImage(image);
                return false;
            }
            return true;
        }
        [HarmonyPatch(typeof(NoticeUIContentHyperLink), nameof(NoticeUIContentHyperLink.OnPointerClick))]
        [HarmonyPrefix]
        public static bool HyperLinkOnPointerClick(NoticeUIContentHyperLink __instance, PointerEventData eventData)
        {
            string URL = __instance.tmp_main.text;
            if (URL.StartsWith("<link"))
            {
                int startIndex = URL.IndexOf('=');
                if (startIndex != -1)
                {
                    int endIndex = URL.IndexOf('>', startIndex + 1);
                    if (endIndex != -1)
                    {
                        URL = URL.Substring(startIndex + 1, endIndex - startIndex - 1);
                    }
                }
                if (URL.StartsWith("Action_"))
                {
                    ReadmeActions[URL]?.Invoke();
                    return false;
                }
            }
            Application.OpenURL(URL);
            return false;
        }
        #endregion
    }
}
