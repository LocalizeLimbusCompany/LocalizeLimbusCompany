using HarmonyLib;
using Il2CppSystem.Collections.Generic;
using LocalSave;
using MainUI;
using MainUI.NoticeUI;
using Server;
using SimpleJSON;
using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UObject = UnityEngine.Object;

namespace LimbusLocalize
{
    public static class LLC_ReadmeManager
    {
        public static NoticeUIPopup NoticeUIInstance;
        public static RedDotWriggler _redDot_Notice;
        static LLC_ReadmeManager()
        {
            InitReadmeList();
            InitReadmeSprites();
            ReadmeActions = new()
            {
                {
                    "Action_Issues",()=>
                    {
                        LCB_LLCMod.CopyLog();
                        LCB_LLCMod.OpenGamePath();
                        Application.OpenURL(LCB_LLCMod.LLCLink + "/issues?q=is:issue");
                    }
                }
            };
        }
        public static void UIInitialize()
        {
            Action _close = () => { Close(); };
            NoticeUIInstance._popupPanel.closeEvent.AddListener(_close);
            NoticeUIInstance._arrowScroll.Initialize();
            NoticeUIInstance._titleViewManager.Initialized();
            NoticeUIInstance._contentViewManager.Initialized();
            NoticeUIInstance.btn_back._onClick.AddListener(_close);
            Action eventNotice_onClick = () => { NoticeUIInstance.EventTapClickEvent(); };
            Action systemNotice_onClick = () => { NoticeUIInstance.SystemTapClickEvent(); };
            NoticeUIInstance.btn_eventNotice._onClick.AddListener(eventNotice_onClick);
            NoticeUIInstance.btn_systemNotice._onClick.AddListener(systemNotice_onClick);
            NoticeUIInstance.btn_systemNotice.GetComponentInChildren<UITextDataLoader>(true).enabled = false;
            NoticeUIInstance.btn_systemNotice.GetComponentInChildren<TextMeshProUGUI>(true).text = "更新公告";
            NoticeUIInstance.btn_eventNotice.GetComponentInChildren<UITextDataLoader>(true).enabled = false;
            NoticeUIInstance.btn_eventNotice.GetComponentInChildren<TextMeshProUGUI>(true).text = "公告";
        }
        public static void Open()
        {
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
            ReadmeSprites = new();

            foreach (FileInfo fileInfo in new DirectoryInfo(LCB_LLCMod.ModPath + "/Localize/Readme").GetFiles().Where(f => f.Extension == ".jpg" || f.Extension == ".png"))
            {
                Texture2D texture2D = new(2, 2);
                ImageConversion.LoadImage(texture2D, File.ReadAllBytes(fileInfo.FullName));
                Sprite sprite = Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f));
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileInfo.FullName);
                texture2D.name = fileNameWithoutExtension;
                sprite.name = fileNameWithoutExtension;
                UObject.DontDestroyOnLoad(sprite);
                sprite.hideFlags |= HideFlags.HideAndDontSave;
                ReadmeSprites[fileNameWithoutExtension] = sprite;
            }
        }
        public static void InitReadmeList()
        {
            ReadmeList.Clear();
            foreach (var notices in JSONNode.Parse(File.ReadAllText(LCB_LLCMod.ModPath + "/Localize/Readme/Readme.json"))[0].AsArray.m_List)
            {
                ReadmeList.Add(new Notice(JsonUtility.FromJson<NoticeFormat>(notices.ToString()), LOCALIZE_LANGUAGE.KR));
            }
        }
        public static List<Notice> ReadmeList = new();
        public static Dictionary<string, Sprite> ReadmeSprites;
        public static System.Collections.Generic.Dictionary<string, Action> ReadmeActions;

        public static void Close()
        {
            Singleton<UserLocalSaveDataRoot>.Instance.NoticeRedDotSaveModel.Save();
            NoticeUIInstance._popupPanel.Close();
            UpdateNoticeRedDot();
        }
        public static void UpdateNoticeRedDot()
           => _redDot_Notice?.gameObject.SetActive(IsValidRedDot());
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
        private static bool InitNoticeList(UserLocalNoticeRedDotModel __instance, List<int> severNoticeList)
        {
            LLC_UpdateChecker.CheckReadmeUpdate();
            for (int i = 0; i < __instance.GetDataList().Count; i++)
            {
                Func<int, bool> func = x =>
                {
                    Func<Notice, bool> func2 = x2 => x2.ID == x;
                    return !severNoticeList.Contains(x) && ReadmeList.FindAll(func2).Count == 0;
                };
                __instance.idList.RemoveAll(func);
            }
            __instance.Save();
            UpdateNoticeRedDot();
            return false;
        }
        [HarmonyPatch(typeof(NoticeUIPopup), nameof(NoticeUIPopup.Initialize))]
        [HarmonyPostfix]
        private static void NoticeUIPopupInitialize(NoticeUIPopup __instance)
        {
            if (!NoticeUIInstance)
            {
                var NoticeUIPopupInstance = UObject.Instantiate(__instance, __instance.transform.parent);
                NoticeUIInstance = NoticeUIPopupInstance;
                UIInitialize();
            }
        }
        [HarmonyPatch(typeof(MainLobbyUIPanel), nameof(MainLobbyUIPanel.Initialize))]
        [HarmonyPostfix]
        private static void MainLobbyUIPanelInitialize(MainLobbyUIPanel __instance)
        {
            var UIButtonInstance = UObject.Instantiate(__instance.button_notice, __instance.button_notice.transform.parent).Cast<MainLobbyRightUpperUIButton>();
            _redDot_Notice = UIButtonInstance.gameObject.GetComponentInChildren<RedDotWriggler>(true);
            UpdateNoticeRedDot();
            UIButtonInstance._onClick.RemoveAllListeners();
            Action onClick = delegate
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
                transform.GetChild(i).localScale = new Vector3(0.77f, 0.77f, 1f);
            }
        }
        [HarmonyPatch(typeof(NoticeUIContentImage), nameof(NoticeUIContentImage.SetData))]
        [HarmonyPrefix]
        private static bool ImageSetData(NoticeUIContentImage __instance, string formatValue)
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
        private static bool HyperLinkOnPointerClick(NoticeUIContentHyperLink __instance)
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
