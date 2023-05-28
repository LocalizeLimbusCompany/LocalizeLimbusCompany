using LimbusLocalizeRUS;
using HarmonyLib;
using Il2Cpp;
using Il2CppAddressable;
using Il2CppSimpleJSON;
using Il2CppStorySystem;
using Il2CppSystem.Collections.Generic;
using Il2CppTMPro;
using Il2CppUtilityUI;
using System;
using System.IO;
using UnityEngine;

namespace LimbusLocalizeRUS
{
    public static class LCB_Cyrillic_Font
    {
        public static List<TMP_FontAsset> tmpcyrillicfonts = new();
        public static List<string> tmpcyrillicfontsnames = new();
        #region Handwriting
        public static bool AddCyrillicFont(string path)
        {
            if (File.Exists(path))
            {
                var AllAssets = AssetBundle.LoadFromFile(path).LoadAllAssets();
                foreach (var Asset in AllAssets)
                {
                    var TryCastFontAsset = Asset.TryCast<TMP_FontAsset>();
                    if (TryCastFontAsset)
                    {
                        UnityEngine.Object.DontDestroyOnLoad(TryCastFontAsset);
                        TryCastFontAsset.hideFlags |= HideFlags.HideAndDontSave;
                        tmpcyrillicfonts.Add(TryCastFontAsset);
                        tmpcyrillicfontsnames.Add(TryCastFontAsset.name);
                    }
                }
                return true;
            }
            return false;
        }
        public static bool GetCyrillicFonts(string fontname, out TMP_FontAsset fontAsset)
        {
            fontAsset = null;
            if (tmpcyrillicfonts.Count == 0)
                return false;
            if (fontname == "BebasKai SDF")
            {
                fontAsset = tmpcyrillicfonts[0];
                return true;
            }
            if (fontname == "Caveat Semibold SDF")
            {
                fontAsset = tmpcyrillicfonts[1];
                return true;
            }
            if (fontname == "ExcelsiorSans SDF")
            {
                fontAsset = tmpcyrillicfonts[2];
                return true;
            }
            if (fontname.StartsWith("HigashiOme - Gothic - C"))
            {
                fontAsset = tmpcyrillicfonts[3];
                return true;
            }
            if (fontname.StartsWith("Corporate-Logo-Bold") || fontname == "Mikodacs SDF" || fontname == "KOTRA_BOLD SDF")
            {
                fontAsset = tmpcyrillicfonts[5];
                return true;
            }
            if (fontname == "Pretendard-Regular SDF" || fontname.StartsWith("SCDream5") || fontname == "LiberationSans SDF")
            {
                fontAsset = tmpcyrillicfonts[6];
                return true;
            }
            return false;
        }
        public static bool IsCyrillicFont(TMP_FontAsset fontAsset)
        {
            return tmpcyrillicfontsnames.Contains(fontAsset.name);
        }

        [HarmonyPatch(typeof(TMP_Text), nameof(TMP_Text.font), MethodType.Setter)]
        [HarmonyPrefix]
        private static bool set_font(TMP_Text __instance, ref TMP_FontAsset value)
        {
            if (IsCyrillicFont(__instance.m_fontAsset))
                return false;
            string fontname = __instance.m_fontAsset.name;
            if (GetCyrillicFonts(fontname, out TMP_FontAsset font))
                value = font;
            return true;
        }
        [HarmonyPatch(typeof(TMP_Text), nameof(TMP_Text.fontMaterial), MethodType.Setter)]
        [HarmonyPrefix]
        private static void set_fontMaterial(TMP_Text __instance, ref Material value)
        {
            if (IsCyrillicFont(__instance.m_fontAsset))
            value = __instance.m_fontAsset.material;
        }
        [HarmonyPatch(typeof(TextMeshProLanguageSetter), nameof(TextMeshProLanguageSetter.UpdateTMP))]
        [HarmonyPrefix]
        private static bool UpdateTMP(TextMeshProLanguageSetter __instance, LOCALIZE_LANGUAGE lang)
        {
            FontInformation fontInformation = __instance._fontInformation.Count > 0 ? __instance._fontInformation[0] : null;
                if (fontInformation == null)
                    return false;
                if (fontInformation.fontAsset == null)
                    return false;
                if (__instance._text == null)
                    return false;
                var raw_fontAsset = fontInformation.fontAsset;
                bool use_ru = GetCyrillicFonts(raw_fontAsset.name, out var ru_fontAsset);

            var fontAsset = use_ru ? ru_fontAsset : fontInformation.fontAsset;
            var fontMaterial = use_ru ? ru_fontAsset.material : fontInformation.fontMaterial ?? fontInformation.fontAsset.material;

            __instance._text.font = fontAsset;
            __instance._text.fontMaterial = fontMaterial;
            if (__instance._matSetter != null)
            {
                __instance._matSetter.defaultMat = fontMaterial;
                __instance._matSetter.ResetMaterial();
                return false;
            }
            __instance.gameObject.TryGetComponent(out TextMeshProMaterialSetter textMeshProMaterialSetter);
            if (textMeshProMaterialSetter != null)
            {
                textMeshProMaterialSetter.defaultMat = fontMaterial;
                textMeshProMaterialSetter.ResetMaterial();
            }
            return false;
        }
        #endregion
        #region Я заебался переводить китайский
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
            tm._dungeonName.Init(romoteLocalizeFileList.DungeonName);
            tm._danteNoteDesc.Init(romoteLocalizeFileList.DanteNote);
            tm._danteNoteCategoryKeyword.Init(romoteLocalizeFileList.DanteNoteCategoryKeyword);

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
        [HarmonyPatch(typeof(StoryData), nameof(StoryData.GetScenario))]
        [HarmonyPrefix]
        private static bool GetScenario(StoryData __instance, string scenarioID, ref LOCALIZE_LANGUAGE lang, ref Scenario __result)
        {
            TextAsset textAsset = SingletonBehavior<AddressableManager>.Instance.LoadAssetSync<TextAsset>("Assets/Resources_moved/Story/Effect", scenarioID, null, null).Item1;
            if (!textAsset)
            {
                LCB_LCBRMod.LogError("Story Unknown Error! Call Story: Dirty Hacker");
                scenarioID = "SDUMMY";
                textAsset = SingletonBehavior<AddressableManager>.Instance.LoadAssetSync<TextAsset>("Assets/Resources_moved/Story/Effect", scenarioID, null, null).Item1;
            }
            if (!LCBR_Manager.Localizes.TryGetValue(scenarioID, out string text))
            {
                LCB_LCBRMod.LogError("Story error! We can't find the RU story file, so we'll use EN story");
                text = AddressableManager.Instance.LoadAssetSync<TextAsset>("Assets/Resources_moved/Localize/EN/StoryData", "EN_" + scenarioID, null, null).Item1.ToString();
            }
            string text2 = textAsset.ToString();
            Scenario scenario = new()
            {
                ID = scenarioID
            };
            JSONArray jsonarray = JSONNode.Parse(text)[0].AsArray;
            JSONArray jsonarray2 = JSONNode.Parse(text2)[0].AsArray;
            for (int i = 0; i < jsonarray.Count; i++)
            {
                int num = jsonarray[i][0].AsInt;
                if (num >= 0)
                {
                    JSONNode jsonnode;
                    if (jsonarray2[i][0].AsInt == num)
                        jsonnode = jsonarray2[i];
                    else
                        jsonnode = new JSONObject();
                    scenario.Scenarios.Add(new Dialog(num, jsonarray[i], jsonnode));
                }
            }
            __result = scenario;
            return false;
        }
        [HarmonyPatch(typeof(StoryData), nameof(StoryData.GetTellerName))]
        [HarmonyPrefix]
        private static bool GetTellerName(StoryData __instance, string name, LOCALIZE_LANGUAGE lang, ref string __result)
        {
            if (__instance._modelAssetMap.TryGetValueEX(name, out var scenarioAssetData))
                __result = scenarioAssetData.krname ?? string.Empty;
            return false;
        }
        [HarmonyPatch(typeof(StoryData), nameof(StoryData.GetTellerTitle))]
        [HarmonyPrefix]
        private static bool GetTellerTitle(StoryData __instance, string name, LOCALIZE_LANGUAGE lang, ref string __result)
        {
            if (__instance._modelAssetMap.TryGetValueEX(name, out var scenarioAssetData))
                __result = scenarioAssetData.nickName ?? string.Empty;
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
        [HarmonyPatch(typeof(TextDataManager), nameof(TextDataManager.LoadRemote))]
        [HarmonyPrefix]
        private static void LoadRemote(ref LOCALIZE_LANGUAGE lang)
        {
            lang = LOCALIZE_LANGUAGE.EN;
        }
        [HarmonyPatch(typeof(StoryData), nameof(StoryData.Init))]
        [HarmonyPrefix]
        private static bool StoryDataInit(StoryData __instance)
        {
            ScenarioAssetDataList scenarioAssetDataList = JsonUtility.FromJson<ScenarioAssetDataList>(LCBR_Manager.Localizes["NickName"]);
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
        [HarmonyPatch(typeof(LoginSceneManager), nameof(LoginSceneManager.SetLoginInfo))]
        [HarmonyPostfix]
        private static void SetLoginInfo(LoginSceneManager __instance)
        {
            LoadLocal(LOCALIZE_LANGUAGE.EN);
            __instance.tmp_loginAccount.text = "Localize LCB v" + LCB_LCBRMod.VERSION;
        }
        private static void Init<T>(this JsonDataList<T> jsonDataList, List<string> jsonFilePathList) where T : LocalizeTextData, new()
        {
            foreach (string text in jsonFilePathList)
            {
                if (!LCBR_Manager.Localizes.TryGetValue(text, out var text2)) { continue; }
                var localizeTextData = JsonUtility.FromJson<LocalizeTextDataRoot<T>>(text2);
                foreach (T t in localizeTextData.DataList)
                {
                    jsonDataList._dic[t.ID.ToString()] = t;
                }
            }
        }

        private static void AbEventCharDlgRootInit(this AbEventCharDlgRoot root, List<string> jsonFilePathList)
        {
            root._personalityDict = new();
            foreach (string text in jsonFilePathList)
            {
                if (!LCBR_Manager.Localizes.TryGetValue(text, out var text2)) { continue; }
                var localizeTextData = JsonUtility.FromJson<LocalizeTextDataRoot<TextData_AbnormalityEventCharDlg>>(text2);
                foreach (var t in localizeTextData.DataList)
                {
                    if (!root._personalityDict.TryGetValueEX(t.PersonalityID, out var abEventKeyDictionaryContainer))
                    {
                        abEventKeyDictionaryContainer = new AbEventKeyDictionaryContainer();
                        root._personalityDict[t.PersonalityID] = abEventKeyDictionaryContainer;
                    }
                    string[] array = t.Usage.Trim().Split(new char[] { '(', ')' });
                    for (int i = 1; i < array.Length; i += 2)
                    {
                        string[] array2 = array[i].Split(',');
                        int num = int.Parse(array2[0].Trim());
                        AB_DLG_EVENT_TYPE ab_DLG_EVENT_TYPE = (AB_DLG_EVENT_TYPE)Enum.Parse(typeof(AB_DLG_EVENT_TYPE), array2[1].Trim());
                        AbEventKey abEventKey = new(num, ab_DLG_EVENT_TYPE);
                        abEventKeyDictionaryContainer.AddDlgWithEvent(abEventKey, t);
                    }
                }

            }
        }
        private static void JsonDataListInit<T>(this Dictionary<string, LocalizeTextDataRoot<T>> jsonDataList, List<string> jsonFilePathList)
        {
            foreach (string text in jsonFilePathList)
            {
                if (!LCBR_Manager.Localizes.TryGetValue(text, out var text2)) { continue; }
                var localizeTextData = JsonUtility.FromJson<LocalizeTextDataRoot<T>>(text2);
                jsonDataList[text.Split('_')[^1]] = localizeTextData;
            }
        }

        #endregion
        public static bool TryGetValueEX<TKey, TValue>(this Dictionary<TKey, TValue> dic, TKey key, out TValue value)
        {
            var entries = dic._entries;
            var Entr = dic.FindEntry(key);
            value = Entr == -1 ? default : entries == null ? default : entries[Entr].value;
            return value != null;
        }
    }
}