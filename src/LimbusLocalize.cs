using HarmonyLib;
using Il2Cpp;
using Il2CppAddressable;
using Il2CppSimpleJSON;
using Il2CppSteamworks;
using Il2CppStorySystem;
using Il2CppSystem.Collections.Generic;
using Il2CppTMPro;
using Il2CppUtilityUI;
using LimbusLocalize;
using MelonLoader;
using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using UObject = UnityEngine.Object;

[assembly: MelonInfo(typeof(LimbusLocalizeMod), LimbusLocalizeMod.NAME, LimbusLocalizeMod.VERSION, LimbusLocalizeMod.AUTHOR)]
namespace LimbusLocalize
{
    public class LimbusLocalizeMod : MelonMod
    {
        public static string modpath;
        public static string gamepath;
        public static TMP_FontAsset tmpchinesefont;
        public const string NAME = "LimbusLocalizeMod";
        public const string VERSION = "0.1.9";
        public const string AUTHOR = "Bright";
        public static Action<string> LogError { get; set; }
        public static Action<string> LogWarning { get; set; }
        public override void OnInitializeMelon()
        {
            LogError = delegate (string log) { LoggerInstance.Error(log); Debug.LogError(log); };
            LogWarning = delegate (string log) { LoggerInstance.Warning(log); Debug.LogWarning(log); };
            modpath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            gamepath = new DirectoryInfo(Application.dataPath).Parent.FullName;
            if (!Directory.Exists(modpath + "/.hide"))
            {
                Directory.CreateDirectory(modpath + "/.hide");
                FileAttributes MyAttributes = File.GetAttributes(modpath + "/.hide");
                File.SetAttributes(modpath + "/.hide", MyAttributes | FileAttributes.Hidden);
            }
            try
            {
                ModManager.Setup();
                ModManager.InitLocalizes(new DirectoryInfo(modpath + "/Localize/CN"));
                ReadmeManager.InitReadmeList();
                UpdateChecker.StartCheckUpdates();
                HarmonyLib.Harmony harmony = new("LimbusLocalizeMod");
                harmony.PatchAll(typeof(ReadmeManager));
                harmony.PatchAll(typeof(LimbusLocalizeMod));
                harmony.PatchAll(typeof(GoodLifeHook));
                if (File.Exists(modpath + "/tmpchinesefont"))
                    tmpchinesefont = AssetBundle.LoadFromFile(modpath + "/tmpchinesefont").LoadAsset("assets/sourcehansanssc-heavy sdf.asset").Cast<TMP_FontAsset>();
                else
                    LogError("Fatal Error!!!\nYou Not Have Chinese Font, Please Read GitHub Readme To Download Or Use Mod Installer To Automatically Download It");
            }
            catch (Exception e)
            {
                LogError("Mod Has Unknown Fatal Error!!!\n" + e.ToString());
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
            var fontAsset = tmpchinesefont;
            __instance._text.font = fontAsset;
            __instance._text.fontMaterial = fontAsset.material;
            if (__instance._matSetter != null)
            {
                __instance._matSetter.defaultMat = fontAsset.material;
                __instance._matSetter.ResetMaterial();
                return false;
            }
            __instance.gameObject.TryGetComponent(out TextMeshProMaterialSetter textMeshProMaterialSetter);
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
            tm._threadDungeon.Init(romoteLocalizeFileList.ThreadDungeon);
            tm._railwayDungeonText.Init(romoteLocalizeFileList.RailwayDungeon);
            tm._railwayDungeonNodeText.Init(romoteLocalizeFileList.RailwayDungeonNodeInfo);
            tm._railwayDungeonStationName.Init(romoteLocalizeFileList.RailwayDungeonStationName);

            tm._abnormalityEventCharDlg.AbEventCharDlgRootInit(romoteLocalizeFileList.abnormalityCharDlgFilePath);

            tm._personalityVoiceText._voiceDictionary.JsonDataListInit(romoteLocalizeFileList.PersonalityVoice);
            tm._announcerVoiceText._voiceDictionary.JsonDataListInit(romoteLocalizeFileList.AnnouncerVoice);
            tm._bgmLyricsText._lyricsDictionary.JsonDataListInit(romoteLocalizeFileList.BgmLyrics);
            tm._egoVoiceText._voiceDictionary.JsonDataListInit(romoteLocalizeFileList.EGOVoice);

        }
        [HarmonyPatch(typeof(EGOVoiceJsonDataList), nameof(EGOVoiceJsonDataList.Init))]
        [HarmonyPrefix]
        private static bool EGOVoiceJsonDataListInit(EGOVoiceJsonDataList __instance, List<string> jsonFilePathList)
        {
            __instance._voiceDictionary = new Dictionary<string, LocalizeTextDataRoot<TextData_EGOVoice>>();
            int callcount = 0;
            foreach (string jsonFilePath in jsonFilePathList)
            {
                Action<LocalizeTextDataRoot<TextData_EGOVoice>> LoadLocalizeDel = delegate (LocalizeTextDataRoot<TextData_EGOVoice> data)
                                {
                                    if (data != null)
                                    {
                                        string[] array = jsonFilePath.Split('_');
                                        string text = array[^1];
                                        text = text.Replace(".json", "");
                                        __instance._voiceDictionary.Add(text, data);
                                    }
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
            ScenarioAssetDataList scenarioAssetDataList = JsonUtility.FromJson<ScenarioAssetDataList>(ModManager.Localizes["NickName"]);
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
            if (ModManager.Localizes.TryGetValue(scenarioID, out string file))
            {
                TextAsset textAsset = SingletonBehavior<AddressableManager>.Instance.LoadAssetSync<TextAsset>("Assets/Resources_moved/Story/Effect", scenarioID, null, null).Item1;
                if (textAsset == null)
                {
                    textAsset = SingletonBehavior<AddressableManager>.Instance.LoadAssetSync<TextAsset>("Assets/Resources_moved/Story/Effect", "SDUMMY", null, null).Item1;
                }
                string text4 = textAsset.ToString();
                Scenario scenario = new()
                {
                    ID = scenarioID
                };
                JSONArray jsonarray = JSONNode.Parse(file)[0].AsArray;
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
                LogError("Error!Can'n Find CN Story File,Use Raw Story");
                return true;
            }
        }
        [HarmonyPatch(typeof(StoryData), nameof(StoryData.GetTellerTitle))]
        [HarmonyPrefix]
        private static bool GetTellerTitle(StoryData __instance, string name, LOCALIZE_LANGUAGE lang, ref string __result)
        {
            if (__instance._modelAssetMap.TryGetValueEX(name, out var scenarioAssetData))
                __result = scenarioAssetData.nickName;
            return false;
        }
        [HarmonyPatch(typeof(StoryData), nameof(StoryData.GetTellerName))]
        [HarmonyPrefix]
        private static bool GetTellerName(StoryData __instance, string name, LOCALIZE_LANGUAGE lang, ref string __result)
        {
            if (__instance._modelAssetMap.TryGetValueEX(name,out var scenarioAssetData))
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
            var fontAsset = tmpchinesefont;
            __instance.tmp_loginAccount.font = fontAsset;
            __instance.tmp_loginAccount.fontMaterial = fontAsset.material;
            __instance.tmp_loginAccount.text = "LimbusLocalizeMod v." + VERSION;
            if (UpdateChecker.UpdateCall != null)
            {
                ModManager.OpenGlobalPopup("模组更新已下载,点击确认将打开下载路径并退出游戏", default, default, "确认", UpdateChecker.UpdateCall);
                return;
            }
            if (File.Exists(modpath + "/.hide/checkisfirstuse"))
                if (File.ReadAllText(modpath + "/.hide/checkisfirstuse") == SteamID + " true")
                    return;
            UserAgreementUI userAgreementUI = UObject.Instantiate(__instance._userAgreementUI, __instance._userAgreementUI.transform.parent);
            userAgreementUI.gameObject.SetActive(true);
            userAgreementUI.tmp_popupTitle.GetComponent<UITextDataLoader>().enabled = false;
            userAgreementUI.tmp_popupTitle.text = "首次使用提示";
            var textMeshProUGUI = userAgreementUI._userAgreementContent._agreementJP.GetComponentInChildren<TextMeshProUGUI>(true);
            Action<bool> _ontogglevaluechange = delegate (bool on)
            {
                if (userAgreementUI._userAgreementContent.Agreed())
                {
                    textMeshProUGUI.text = "模因封号触媒启动\r\n\r\n检测到存活迹象\r\n\r\n解开安全锁";
                    userAgreementUI._userAgreementContent.toggle_userAgreements.gameObject.SetActive(false);
                    userAgreementUI.btn_confirm.interactable = true;
                }
            };
            userAgreementUI._userAgreementContent.Init(_ontogglevaluechange);
            Action _onclose = delegate ()
            {
                File.WriteAllText(modpath + "/.hide/checkisfirstuse", SteamID + " true");
                userAgreementUI.gameObject.SetActive(false);
                UObject.Destroy(userAgreementUI);
                UObject.Destroy(userAgreementUI.gameObject);
            };
            userAgreementUI._panel.closeEvent.AddListener(_onclose);
            Action _oncancel = delegate ()
            {
                SteamClient.Shutdown();
                Application.Quit();
            };
            userAgreementUI.btn_cancel._onClick.AddListener(_oncancel);
            userAgreementUI.btn_confirm.interactable = false;
            Action _onconfirm = userAgreementUI.OnConfirmClicked;
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
        
    }
}
