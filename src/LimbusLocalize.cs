using HarmonyLib;
using Il2Cpp;
using Il2CppAddressable;
using Il2CppMainUI;
using Il2CppMainUI.Gacha;
using Il2CppMainUI.NoticeUI;
using Il2CppServer;
using Il2CppSimpleJSON;
using Il2CppSteamworks;
using Il2CppStorySystem;
using Il2CppSystem.Collections.Generic;
using Il2CppTMPro;
using Il2CppUI.Utility;
using Il2CppUtilityUI;
using LimbusLocalize;
using MelonLoader;
using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Playables;
using UnityEngine.UI;
using ILObject = Il2CppSystem.Object;
using RawObject = System.Object;
using UObject = UnityEngine.Object;

[assembly: MelonInfo(typeof(LimbusLocalizeMod), LimbusLocalizeMod.NAME, LimbusLocalizeMod.VERSION, LimbusLocalizeMod.AUTHOR)]
namespace LimbusLocalize
{
    public class LimbusLocalizeMod : MelonMod
    {
        public static string path;
        public static string gamepath;
        public static TMP_FontAsset tmpchinesefont;
        public const string NAME = "LimbusLocalizeMod";
        public const string VERSION = "0.1.7";
        public const string AUTHOR = "Bright";
        public static Action<string> OnLogError { get; set; }
        public static Action<string> OnLogWarning { get; set; }
        public override void OnInitializeMelon()
        {
            OnLogError = delegate (string log) { base.LoggerInstance.Error(log); Debug.LogError(log); };
            OnLogWarning = delegate (string log) { base.LoggerInstance.Warning(log); Debug.LogWarning(log); };
            path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            gamepath = new DirectoryInfo(Application.dataPath).Parent.FullName;
            if (!Directory.Exists(path + "/.hide"))
            {
                Directory.CreateDirectory(path + "/.hide");
                FileAttributes MyAttributes = File.GetAttributes(path + "/.hide");
                File.SetAttributes(path + "/.hide", MyAttributes | FileAttributes.Hidden);
            }
            try
            {
                ModManager.Setup();
                HarmonyLib.Harmony harmony = new("LimbusLocalizeMod");
                harmony.PatchAll(typeof(LimbusLocalizeMod));
                UpdateChecker.StartCheckUpdates();
                if (File.Exists(path + "/tmpchinesefont"))
                    //使用AssetBundle技术载入中文字库
                    tmpchinesefont = AssetBundle.LoadFromFile(path + "/tmpchinesefont").LoadAsset("assets/sourcehansanssc-heavy sdf.asset").Cast<TMP_FontAsset>();
                else
                    OnLogError("Fatal Error!!!\nYou Not Have Chinese Font, Please Read GitHub Readme To Download Or Use Mod Installer To Automatically Download It");
            }
            catch (Exception e)
            {
                OnLogError("Mod Has Unknown Fatal Error!!!\n" + e.ToString());
            }
        }
        public override void OnApplicationQuit()
        {
            File.Copy(gamepath + "/MelonLoader/Latest.log", gamepath + "/框架日志.log", true);
            var Latestlog = File.ReadAllText(gamepath + "/框架日志.log");
            Latestlog = Regex.Replace(Latestlog, "[0-9:\\.\\[\\]]+ During invoking native->managed trampoline(\r\n)?", "");
            File.WriteAllText(gamepath + "/框架日志.log", Latestlog);
            File.Copy(Application.consoleLogPath, gamepath + "/游戏日志.log", true);
        }
        #region 去tmd文字描述成功率
        [HarmonyPatch(typeof(UITextMaker), nameof(UITextMaker.GetSuccessRateText))]
        [HarmonyPrefix]
        public static bool GetSuccessRateText(float rate, ref string __result)
        {
            __result = ((int)(rate * 100.0f)).ToString() + "%";
            return false;
        }
        [HarmonyPatch(typeof(UITextMaker), nameof(UITextMaker.GetSuccessRateToText))]
        [HarmonyPrefix]
        public static bool GetSuccessRateToText(float rate, ref string __result)
        {
            __result = ((int)(rate * 100.0f)).ToString() + "%";
            return false;
        }
        [HarmonyPatch(typeof(PossibleResultData), nameof(PossibleResultData.GetProb))]
        [HarmonyPrefix]
        public static bool GetProb(PossibleResultData __instance, float probAdder, ref float __result)
        {
            float num = 0f;
            float num2 = __instance._defaultProb + probAdder;
            float num3 = 1f - num2;
            int i = 0;
            int count = __instance._headTailCntList.Count;
            while (i < count)
            {
                num += Mathf.Pow(num2, __instance._headTailCntList[i].HeadCnt) * Mathf.Pow(num3, __instance._headTailCntList[i].TailCnt);
                i++;
            }
            __result = num;
            return false;
        }
        #endregion
        #region 屏蔽没有意义的Warning
        [HarmonyPatch(typeof(Logger), nameof(Logger.Log), new System.Type[]
        {
            typeof(LogType),
            typeof(ILObject)
        })]
        [HarmonyPrefix]
        private static bool Log(Logger __instance, LogType __0, ILObject __1)
        {
            if (__0 == LogType.Warning)
            {
                string LogString = Logger.GetString(__1);
                if (!LogString.Contains("DOTWEEN"))
                    __instance.logHandler.LogFormat(__0, null, "{0}", new ILObject[] { LogString });
                return false;
            }
            return true;
        }
        [HarmonyPatch(typeof(Logger), nameof(Logger.Log), new System.Type[]
        {
            typeof(LogType),
            typeof(ILObject),
            typeof(UObject)
        })]
        [HarmonyPrefix]
        private static bool Log(Logger __instance, LogType logType, ILObject message, UObject context)
        {
            if (logType == LogType.Warning)
            {
                string LogString = Logger.GetString(message);
                if (!LogString.Contains("Material"))
                    __instance.logHandler.LogFormat(logType, context, "{0}", new ILObject[] { LogString });
                return false;
            }
            return true;
        }
        #endregion
        [HarmonyPatch(typeof(GachaEffectEventSystem), nameof(GachaEffectEventSystem.LinkToCrackPosition))]
        [HarmonyPrefix]
        private static bool LinkToCrackPosition(GachaEffectEventSystem __instance, GachaCrackController[] crackList)
        {
            return __instance._parent.EffectChainCamera;
        }
        #region 字体
        [HarmonyPatch(typeof(TMP_Text), nameof(TMP_Text.font), MethodType.Setter)]
        [HarmonyPrefix]
        private static bool set_font(TMP_Text __instance, TMP_FontAsset value)
        {
            if (__instance.m_fontAsset == tmpchinesefont)
                return false;
            if (__instance.font.name == "KOTRA_BOLD SDF" || __instance.font.name.StartsWith("Corporate-Logo-Bold") || __instance.font.name.StartsWith("HigashiOme-Gothic-C") || __instance.font.name == "Pretendard-Regular SDF" || __instance.font.name.StartsWith("SCDream") || __instance.font.name == "LiberationSans SDF" || __instance.font.name == "Mikodacs SDF" || __instance.font.name == "BebasKai SDF")
                value = tmpchinesefont;
            if (__instance.m_fontAsset == value)
                return false;
            __instance.m_fontAsset = value;
            __instance.LoadFontAsset();
            __instance.m_havePropertiesChanged = true;
            __instance.SetVerticesDirty();
            __instance.SetLayoutDirty();
            return false;
        }
        [HarmonyPatch(typeof(TMP_Text), nameof(TMP_Text.fontMaterial), MethodType.Setter)]
        [HarmonyPrefix]
        private static bool set_fontMaterial(TMP_Text __instance, Material value)
        {
            bool check = __instance.gameObject.name.StartsWith("[Tmpro]SkillMinPower") || __instance.gameObject.name.StartsWith("[Tmpro]SkillMaxPower");
            //处理不正确大小
            if (!check && __instance.fontSize >= 50f)
                __instance.fontSize -= __instance.fontSize / 50f * 20f;
            value = __instance.font.material;
            if (__instance.m_sharedMaterial != null && __instance.m_sharedMaterial.GetInstanceID() == value.GetInstanceID())
                return false;
            __instance.m_sharedMaterial = value;
            __instance.m_padding = __instance.GetPaddingForMaterial();
            __instance.m_havePropertiesChanged = true;
            __instance.SetVerticesDirty();
            __instance.SetMaterialDirty();
            return false;
        }
        [HarmonyPatch(typeof(TextMeshProLanguageSetter), nameof(TextMeshProLanguageSetter.UpdateTMP))]
        [HarmonyPrefix]
        private static bool UpdateTMP(TextMeshProLanguageSetter __instance, LOCALIZE_LANGUAGE lang)
        {
            //使用中文字库
            var fontAsset = tmpchinesefont;
            __instance._text.font = fontAsset;
            __instance._text.fontMaterial = fontAsset.material;
            if (__instance._matSetter != null)
            {
                __instance._matSetter.defaultMat = fontAsset.material;
                __instance._matSetter.ResetMaterial();
                return false;
            }
            __instance.gameObject.TryGetComponent<TextMeshProMaterialSetter>(out TextMeshProMaterialSetter textMeshProMaterialSetter);
            if (textMeshProMaterialSetter != null)
            {
                textMeshProMaterialSetter.defaultMat = fontAsset.material;
                textMeshProMaterialSetter.ResetMaterial();
            }
            return false;
        }
        #endregion
        #region 载入,应用汉化
        [HarmonyPatch(typeof(TextDataManager), nameof(TextDataManager.LoadRemote))]
        [HarmonyPrefix]
        private static void LoadRemote(ref LOCALIZE_LANGUAGE lang)
        {
            lang = LOCALIZE_LANGUAGE.EN;
        }
        private static void LoadRemote2(LOCALIZE_LANGUAGE lang)
        {
            //载入所有文本
            var tm = TextDataManager.Instance;
            TextDataManager.RomoteLocalizeFileList romoteLocalizeFileList = JsonUtility.FromJson<TextDataManager.RomoteLocalizeFileList>(SingletonBehavior<AddressableManager>.Instance.LoadAssetSync<TextAsset>("Assets/Resources_moved/Localize", "RemoteLocalizeFileList", null, null).Item1.ToString());
            tm._uiList.Init(romoteLocalizeFileList.UIFilePaths);
            tm._characterList.Init(romoteLocalizeFileList.CharacterFilePaths);
            tm._personalityList.Init(romoteLocalizeFileList.PersonalityFilePaths);
            tm._enemyList.Init(romoteLocalizeFileList.EnemyFilePaths);
            tm._egoList.Init(romoteLocalizeFileList.EgoFilePaths);
            tm._skillList.Init(romoteLocalizeFileList.SkillFilePaths);
            tm._passiveList.Init(romoteLocalizeFileList.PassiveFilePaths);
            tm._bufList.Init(romoteLocalizeFileList.BufFilePaths);
            tm._itemList.Init(romoteLocalizeFileList.ItemFilePaths);
            tm._keywordList.Init(romoteLocalizeFileList.keywordFilePaths);
            tm._skillTagList.Init(romoteLocalizeFileList.skillTagFilePaths);
            tm._abnormalityEventList.Init(romoteLocalizeFileList.abnormalityEventsFilePath);
            tm._attributeList.Init(romoteLocalizeFileList.attributeTextFilePath);
            tm._abnormalityCotentData.Init(romoteLocalizeFileList.abnormalityGuideContentFilePath);
            tm._keywordDictionary.Init(romoteLocalizeFileList.keywordDictionaryFilePath);
            tm._actionEvents.Init(romoteLocalizeFileList.actionEventsFilePath);
            tm._egoGiftData.Init(romoteLocalizeFileList.egoGiftFilePath);
            tm._stageChapter.Init(romoteLocalizeFileList.stageChapterPath);
            tm._stagePart.Init(romoteLocalizeFileList.stagePartPath);
            tm._stageNodeText.Init(romoteLocalizeFileList.stageNodeInfoPath);
            tm._dungeonNodeText.Init(romoteLocalizeFileList.dungeonNodeInfoPath);
            tm._storyDungeonNodeText.Init(romoteLocalizeFileList.storyDungeonNodeInfoPath);
            tm._quest.Init(romoteLocalizeFileList.Quest);
            tm._dungeonArea.Init(romoteLocalizeFileList.dungeonAreaPath);
            tm._battlePass.Init(romoteLocalizeFileList.BattlePassPath);
            tm._storyTheater.Init(romoteLocalizeFileList.StoryTheater);
            tm._announcer.Init(romoteLocalizeFileList.Announcer);
            tm._normalBattleResultHint.Init(romoteLocalizeFileList.NormalBattleHint);
            tm._abBattleResultHint.Init(romoteLocalizeFileList.AbBattleHint);
            tm._tutorialDesc.Init(romoteLocalizeFileList.TutorialDesc);
            tm._iapProductText.Init(romoteLocalizeFileList.IAPProduct);
            tm._illustGetConditionText.Init(romoteLocalizeFileList.GetConditionText);
            tm._choiceEventResultDesc.Init(romoteLocalizeFileList.ChoiceEventResult);
            tm._battlePassMission.Init(romoteLocalizeFileList.BattlePassMission);
            tm._gachaTitle.Init(romoteLocalizeFileList.GachaTitle);
            tm._introduceCharacter.Init(romoteLocalizeFileList.IntroduceCharacter);
            tm._userBanner.Init(romoteLocalizeFileList.UserBanner);

            tm._abnormalityEventCharDlg.AbEventCharDlgRootInit(romoteLocalizeFileList.abnormalityCharDlgFilePath);
            tm._personalityVoiceText.PersonalityVoiceJsonDataListInit(romoteLocalizeFileList.PersonalityVoice);
            tm._announcerVoiceText.AnnouncerVoiceJsonDataListInit(romoteLocalizeFileList.AnnouncerVoice);
            tm._bgmLyricsText.BgmLyricsJsonDataListInit(romoteLocalizeFileList.BgmLyrics);
            tm._egoVoiceText.EGOVoiceJsonDataListInit(romoteLocalizeFileList.EGOVoice);

        }
        [HarmonyPatch(typeof(PersonalityVoiceJsonDataList), nameof(PersonalityVoiceJsonDataList.GetDataList))]
        [HarmonyPrefix]
        public static bool PersonalityVoiceGetDataList(PersonalityVoiceJsonDataList __instance, int personalityId, ref LocalizeTextDataRoot<TextData_PersonalityVoice> __result)
        {
            if (!__instance._voiceDictionary.TryGetValueEX(personalityId.ToString(), out LocalizeTextDataRoot<TextData_PersonalityVoice> localizeTextDataRoot))
            {
                Debug.LogError("PersonalityVoice no id:" + personalityId.ToString());
                localizeTextDataRoot = new LocalizeTextDataRoot<TextData_PersonalityVoice>() { dataList = new List<TextData_PersonalityVoice>() };
            }
            __result = localizeTextDataRoot;
            return false;
        }
        [HarmonyPatch(typeof(EGOVoiceJsonDataList), nameof(EGOVoiceJsonDataList.Init))]
        [HarmonyPrefix]
        private static bool EGOVoiceJsonDataListInit(EGOVoiceJsonDataList __instance, List<string> jsonFilePathList)
        {
            __instance._voiceDictionary = new Dictionary<string, LocalizeTextDataRoot<TextData_EGOVoice>>();
            int callcount = 0;
            foreach (string jsonFilePath in jsonFilePathList)
            {
                System.Action<LocalizeTextDataRoot<TextData_EGOVoice>> LoadLocalizeDel = delegate (LocalizeTextDataRoot<TextData_EGOVoice> data)
                                {
                                    if (data != null)
                                    {
                                        string[] array = jsonFilePath.Split('_');
                                        string text = array[^1];
                                        text = text.Replace(".json", "");
                                        __instance._voiceDictionary.Add(text, data);
                                    }
                                    else
                                        Util.DebugLog("There is no VoiceData: " + jsonFilePath);
                                    callcount++;
                                    if (callcount == jsonFilePathList.Count)
                                        LoadRemote2(LOCALIZE_LANGUAGE.EN);
                                };
                AddressableManager.Instance.LoadLocalizeJsonAssetAsync<TextData_EGOVoice>(jsonFilePath, LoadLocalizeDel);
            }
            return false;
        }
        private static bool LoadLocal(LOCALIZE_LANGUAGE lang)
        {
            var tm = TextDataManager.Instance;
            TextDataManager.LocalizeFileList localizeFileList = JsonUtility.FromJson<TextDataManager.LocalizeFileList>(Resources.Load<TextAsset>("Localize/LocalizeFileList").ToString());
            tm._loginUIList.Init(localizeFileList.LoginUIFilePaths);
            tm._fileDownloadDesc.Init(localizeFileList.FileDownloadDesc);
            tm._battleHint.Init(localizeFileList.BattleHint);
            return false;
        }
        [HarmonyPatch(typeof(StoryData), nameof(StoryData.Init))]
        [HarmonyPrefix]
        private static bool StoryDataInit(StoryData __instance)
        {
            //载入所有剧情
            ScenarioAssetDataList scenarioAssetDataList = JsonUtility.FromJson<ScenarioAssetDataList>(File.ReadAllText(LimbusLocalizeMod.path + "/Localize/CN/CN_NickName.json"));
            __instance._modelAssetMap = new Dictionary<string, ScenarioAssetData>();
            __instance._standingAssetMap = new Dictionary<string, StandingAsset>();
            __instance._standingAssetPathMap = new Dictionary<string, string>();
            foreach (ScenarioAssetData scenarioAssetData in scenarioAssetDataList.assetData)
            {
                string name = scenarioAssetData.name;
                __instance._modelAssetMap.Add(name, scenarioAssetData);
                if (!string.IsNullOrEmpty(scenarioAssetData.fileName) && !__instance._standingAssetPathMap.ContainsKey(scenarioAssetData.fileName))
                    __instance._standingAssetPathMap.Add(scenarioAssetData.fileName, "Story_StandingModel" + scenarioAssetData.fileName);
            }
            ScenarioMapAssetDataList scenarioMapAssetDataList = JsonUtility.FromJson<ScenarioMapAssetDataList>(Resources.Load<TextAsset>("Story/ScenarioMapCode").ToString());
            __instance._mapAssetMap = new Dictionary<string, ScenarioMapAssetData>();
            foreach (ScenarioMapAssetData scenarioMapAssetData in scenarioMapAssetDataList.assetData)
                __instance._mapAssetMap.Add(scenarioMapAssetData.id, scenarioMapAssetData);
            __instance._emotionMap = new Dictionary<string, EmotionAsset>();
            for (int i = 0; i < __instance._emotions.Count; i++)
                __instance._emotionMap.Add(__instance._emotions[i].prefab.Name.ToLower(), __instance._emotions[i]);
            return false;
        }
        [HarmonyPatch(typeof(StoryData), nameof(StoryData.GetScenario))]
        [HarmonyPrefix]
        private static bool GetScenario(StoryData __instance, string scenarioID, LOCALIZE_LANGUAGE lang, ref Scenario __result)
        {
            //读取剧情
            string file = LimbusLocalizeMod.path + "/Localize/CN/CN_" + scenarioID + ".json";
            if (File.Exists(file))
            {
                string item = File.ReadAllText(file);
                TextAsset textAsset = SingletonBehavior<AddressableManager>.Instance.LoadAssetSync<TextAsset>("Assets/Resources_moved/Story/Effect", scenarioID, null, null).Item1;
                if (textAsset == null)
                {
                    textAsset = SingletonBehavior<AddressableManager>.Instance.LoadAssetSync<TextAsset>("Assets/Resources_moved/Story/Effect", "SDUMMY", null, null).Item1;
                }
                string text3 = item;
                string text4 = textAsset.ToString();
                Scenario scenario = new()
                {
                    ID = scenarioID
                };
                JSONArray jsonarray = JSONNode.Parse(text3)[0].AsArray;
                JSONArray jsonarray2 = JSONNode.Parse(text4)[0].AsArray;
                for (int i = 0; i < jsonarray.Count; i++)
                {
                    int num = jsonarray[i][0].AsInt;
                    if (num >= 0)
                    {
                        JSONNode jsonnode;
                        if (jsonarray2[i][0].AsInt == num)
                        {
                            jsonnode = jsonarray2[i];
                        }
                        else
                        {
                            jsonnode = new JSONObject();
                        }
                        scenario.Scenarios.Add(new Dialog(num, jsonarray[i], jsonnode));
                    }
                }
                __result = scenario;
                return false;
            }
            else
            {
                OnLogError("Error!Can'n Find CN Story File,Use Raw Story");
                return true;
            }
        }
        [HarmonyPatch(typeof(StoryData), nameof(StoryData.GetTellerTitle))]
        [HarmonyPrefix]
        private static bool GetTellerTitle(StoryData __instance, string name, LOCALIZE_LANGUAGE lang, ref string __result)
        {
            //剧情称号
            var entries = __instance._modelAssetMap._entries;
            var Entr = __instance._modelAssetMap.FindEntry(name);
            ScenarioAssetData scenarioAssetData = Entr == -1 ? null : entries?[Entr].value;
            if (scenarioAssetData != null)
                __result = scenarioAssetData.nickName;
            return false;
        }
        [HarmonyPatch(typeof(StoryData), nameof(StoryData.GetTellerName))]
        [HarmonyPrefix]
        private static bool GetTellerName(StoryData __instance, string name, LOCALIZE_LANGUAGE lang, ref string __result)
        {
            //剧情名字
            var entries = __instance._modelAssetMap._entries;
            var Entr = __instance._modelAssetMap.FindEntry(name);
            ScenarioAssetData scenarioAssetData = Entr == -1 ? null : entries?[Entr].value;
            if (scenarioAssetData != null)
                __result = scenarioAssetData.krname;
            return false;
        }
        #endregion
        [HarmonyPatch(typeof(LoginSceneManager), nameof(LoginSceneManager.SetLoginInfo))]
        [HarmonyPostfix]
        private static void SetLoginInfo(LoginSceneManager __instance)
        {
            string SteamID = SteamClient.SteamId.ToString();
            LoadLocal(LOCALIZE_LANGUAGE.EN);
            //在主页右下角增加一段文本，用于指示版本号和其他内容
            var fontAsset = tmpchinesefont;
            __instance.tmp_loginAccount.font = fontAsset;
            __instance.tmp_loginAccount.fontMaterial = fontAsset.material;
            __instance.tmp_loginAccount.text = "LimbusLocalizeMod v." + VERSION;

            ReadmeManager.InitReadmeList();
            //增加首次使用弹窗，告知使用者不用花钱买/使用可能有封号概率等
            if (UpdateChecker.UpdateCall != null)
            {
                ModManager.OpenGlobalPopup("模组更新已下载,点击确认将打开下载路径并退出游戏", default, default, "确认", UpdateChecker.UpdateCall);
                return;
            }
            if (File.Exists(LimbusLocalizeMod.path + "/.hide/checkisfirstuse"))
                if (File.ReadAllText(LimbusLocalizeMod.path + "/.hide/checkisfirstuse") == SteamID + " true")
                    return;
            UserAgreementUI userAgreementUI = UnityEngine.Object.Instantiate(__instance._userAgreementUI, __instance._userAgreementUI.transform.parent);
            userAgreementUI.gameObject.SetActive(true);
            userAgreementUI.tmp_popupTitle.GetComponent<UITextDataLoader>().enabled = false;
            userAgreementUI.tmp_popupTitle.text = "首次使用提示";
            var textMeshProUGUI = userAgreementUI._userAgreementContent._agreementJP.GetComponentInChildren<TextMeshProUGUI>(true);
            System.Action<bool> _ontogglevaluechange = delegate (bool on)
            {
                if (userAgreementUI._userAgreementContent.Agreed())
                {
                    textMeshProUGUI.text = "模因封号触媒启动\r\n\r\n检测到存活迹象\r\n\r\n解开安全锁";
                    userAgreementUI._userAgreementContent.toggle_userAgreements.gameObject.SetActive(false);
                    userAgreementUI.btn_confirm.interactable = true;
                }
            };
            userAgreementUI._userAgreementContent.Init(_ontogglevaluechange);
            System.Action _onclose = delegate ()
            {
                File.WriteAllText(LimbusLocalizeMod.path + "/.hide/checkisfirstuse", SteamID + " true");
                userAgreementUI.gameObject.SetActive(false);
                UnityEngine.Object.Destroy(userAgreementUI);
                UnityEngine.Object.Destroy(userAgreementUI.gameObject);
            };
            userAgreementUI._panel.closeEvent.AddListener(_onclose);
            System.Action _oncancel = delegate ()
            {
                SteamClient.Shutdown();
                Application.Quit();
            };
            userAgreementUI.btn_cancel._onClick.AddListener(_oncancel);
            userAgreementUI.btn_confirm.interactable = false;
            System.Action _onconfirm = userAgreementUI.OnConfirmClicked;
            userAgreementUI.btn_confirm._onClick.AddListener(_onconfirm);
            userAgreementUI._collectionOfPersonalityInfo.gameObject.SetActive(false);
            userAgreementUI._userAgreementContent._scrollRect.content = userAgreementUI._userAgreementContent._agreementJP;
            textMeshProUGUI.font = fontAsset;
            textMeshProUGUI.fontMaterial = fontAsset.material;
            textMeshProUGUI.text = "<link=\"https://github.com/Bright1192/LimbusLocalize\">点我进入Github链接</link>\n该mod完全免费\n零协会是唯一授权发布对象\n警告：使用模组会有微乎其微的封号概率(如果他们检测这个的话)\n你已经被警告过了";
            var textMeshProUGUI2 = userAgreementUI._userAgreementContent.toggle_userAgreements.GetComponentInChildren<TextMeshProUGUI>(true);
            textMeshProUGUI2.GetComponent<UITextDataLoader>().enabled = false;
            textMeshProUGUI2.font = fontAsset;
            textMeshProUGUI2.fontMaterial = fontAsset.material;
            textMeshProUGUI2.text = "点击进行身份认证";
            userAgreementUI._userAgreementContent.transform.localPosition = new Vector3(510f, 77f);
            userAgreementUI._userAgreementContent.toggle_userAgreements.gameObject.SetActive(true);
            userAgreementUI._userAgreementContent._agreementJP.gameObject.SetActive(true);
            userAgreementUI._userAgreementContent.img_titleBg.gameObject.SetActive(false);
            float preferredWidth = userAgreementUI._userAgreementContent.tmp_title.preferredWidth;
            Vector2 sizeDelta = userAgreementUI._userAgreementContent.img_titleBg.rectTransform.sizeDelta;
            sizeDelta.x = preferredWidth + 60f;
            userAgreementUI._userAgreementContent.img_titleBg.rectTransform.sizeDelta = sizeDelta;
            userAgreementUI._userAgreementContent._userAgreementsScrollbar.value = 1f;
            userAgreementUI._userAgreementContent._userAgreementsScrollbar.size = 0.3f;
        }
        #region 公告相关
        [HarmonyPatch(typeof(NoticeUIPopup), nameof(NoticeUIPopup.Initialize))]
        [HarmonyPostfix]
        public static void NoticeUIPopupInitialize(NoticeUIPopup __instance)
        {
            if (!ReadmeManager.NoticeUIInstance)
            {
                var NoticeUIPopupInstance = UObject.Instantiate(__instance, __instance.transform.parent);
                ReadmeManager.NoticeUIInstance = NoticeUIPopupInstance;
                ReadmeManager.Initialize();
            }
        }
        [HarmonyPatch(typeof(MainLobbyUIPanel), nameof(MainLobbyUIPanel.Initialize))]
        [HarmonyPostfix]
        public static void MainLobbyUIPanelInitialize(MainLobbyUIPanel __instance)
        {
            var UIButtonInstance = UObject.Instantiate(__instance.button_notice, __instance.button_notice.transform.parent).Cast<MainLobbyRightUpperUIButton>();
            ReadmeManager._redDot_Notice = UIButtonInstance.gameObject.GetComponentInChildren<RedDotWriggler>(true);
            ReadmeManager.UpdateNoticeRedDot();
            UIButtonInstance._onClick.RemoveAllListeners();
            System.Action onClick = delegate
                        {
                            ReadmeManager.Open();
                        };
            UIButtonInstance._onClick.AddListener(onClick);
            UIButtonInstance.transform.SetSiblingIndex(1);
            var spriteSetting = new ButtonSprites()
            {
                _enabled = ReadmeManager.GetReadmeSprite("Readme_Zero_Button"),
                _hover = ReadmeManager.GetReadmeSprite("Readme_Zero_Button")
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
                Sprite image = ReadmeManager.GetReadmeSprite(formatValue);
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
                    ReadmeManager.ReadmeActions[URL]?.Invoke();
                    return false;
                }
            }
            Application.OpenURL(URL);
            return false;
        }
        #endregion
    }
}
