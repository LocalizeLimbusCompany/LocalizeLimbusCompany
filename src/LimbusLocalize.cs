using Addressable;
using BepInEx;
using HarmonyLib;
using SimpleJSON;
using StorySystem;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using TMPro;
using UnityEngine;
using UtilityUI;

namespace LimbusLocalize
{
    [BepInPlugin("Bright.LimbusLocalizeMod", "LimbusLocalizeMod", "0.1")]
    public class LimbusLocalize : BaseUnityPlugin
    {
        public static string path;
        public void Awake()
        {
            path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            DontDestroyOnLoad(gameObject);
            gameObject.hideFlags |= HideFlags.HideAndDontSave;
            gameObject.layer = 5;
            Harmony harmony = new Harmony("LimbusLocalizeMod");
            MethodInfo method = typeof(LimbusLocalize).GetMethod("LoadRemote", AccessTools.all);
            harmony.Patch(typeof(TextDataManager).GetMethod("LoadRemote", AccessTools.all), new HarmonyMethod(method));
            method = typeof(LimbusLocalize).GetMethod("LoadLocal", AccessTools.all);
            harmony.Patch(typeof(TextDataManager).GetMethod("LoadLocal", AccessTools.all), new HarmonyMethod(method));

            method = typeof(LimbusLocalize).GetMethod("UpdateTMP", AccessTools.all);
            harmony.Patch(typeof(TextMeshProLanguageSetter).GetMethod("UpdateTMP", AccessTools.all), new HarmonyMethod(method));
            method = typeof(LimbusLocalize).GetMethod("set_fontMaterial", AccessTools.all);
            harmony.Patch(typeof(TextMeshProUGUI).GetMethod("set_fontMaterial", AccessTools.all), new HarmonyMethod(method));
            method = typeof(LimbusLocalize).GetMethod("StoryDataInit", AccessTools.all);
            harmony.Patch(typeof(StoryData).GetMethod("Init", AccessTools.all), new HarmonyMethod(method));
            method = typeof(LimbusLocalize).GetMethod("GetTellerTitle", AccessTools.all);
            harmony.Patch(typeof(StoryData).GetMethod("GetTellerTitle", AccessTools.all), new HarmonyMethod(method));
            method = typeof(LimbusLocalize).GetMethod("GetTellerName", AccessTools.all);
            harmony.Patch(typeof(StoryData).GetMethod("GetTellerName", AccessTools.all), new HarmonyMethod(method));
            method = typeof(LimbusLocalize).GetMethod("GetScenario", AccessTools.all);
            harmony.Patch(typeof(StoryData).GetMethod("GetScenario", AccessTools.all), new HarmonyMethod(method));

            foreach (TMP_FontAsset fontAsset in AssetBundle.LoadFromFile(path + "/tmpchinesefont").LoadAllAssets<TMP_FontAsset>())
            {
                TMP_FontAssets.Add(fontAsset);
            }
        }
        public static List<TMP_FontAsset> TMP_FontAssets = new List<TMP_FontAsset>();

        private static bool set_fontMaterial(TextMeshProUGUI __instance, Material value)
        {
            value = __instance.font.material;
            if (__instance.fontSize == 50f)
            {
                __instance.fontSize = 35f;
            }
            if (__instance.fontSize == 60f)
            {
                __instance.fontSize = 32f;
            }
            if (__instance.fontSize == 70f)
            {
                __instance.fontSize = 42f;
            }
            if (__instance.fontSize == 80f)
            {
                __instance.fontSize = 56f;
            }
            if (__instance.fontSize == 100f)
            {
                __instance.fontSize = 53f;
            }
            if (__instance.fontSize == 160f)
            {
                __instance.fontSize = 115f;
            }
            if (__instance.m_sharedMaterial != null && __instance.m_sharedMaterial.GetInstanceID() == value.GetInstanceID())
            {
                return false;
            }
            __instance.m_sharedMaterial = value;
            __instance.m_padding = __instance.GetPaddingForMaterial();
            __instance.m_havePropertiesChanged = true;
            __instance.SetVerticesDirty();
            __instance.SetMaterialDirty();
            return false;
        }
        private static bool UpdateTMP(TextMeshProLanguageSetter __instance, LOCALIZE_LANGUAGE lang)
        {
            var fontAsset = TMP_FontAssets[0];
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
        private static bool LoadRemote(LOCALIZE_LANGUAGE lang)
        {
            var tm = TextDataManager.Instance;
            tm._isLoadedRemote = true;
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
            tm._userInfoBannerDesc.Init(romoteLocalizeFileList.UserInfoBannerDesc);
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
        private static bool StoryDataInit(StoryData __instance)
        {
            ScenarioAssetDataList scenarioAssetDataList = JsonUtility.FromJson<ScenarioAssetDataList>(File.ReadAllText(LimbusLocalize.path + "/Localize/CN/CN_NickName.json"));
            __instance._modelAssetMap = new Dictionary<string, ScenarioAssetData>();
            __instance._standingAssetMap = new Dictionary<string, StandingAsset>();
            __instance._standingAssetPathMap = new Dictionary<string, string>();
            foreach (ScenarioAssetData scenarioAssetData in scenarioAssetDataList.assetData)
            {
                string name = scenarioAssetData.name;
                __instance._modelAssetMap.Add(name, scenarioAssetData);
                if (!string.IsNullOrEmpty(scenarioAssetData.fileName) && !__instance._standingAssetPathMap.ContainsKey(scenarioAssetData.fileName))
                {
                    __instance._standingAssetPathMap.Add(scenarioAssetData.fileName, "Story_StandingModel" + scenarioAssetData.fileName);
                }
            }
            ScenarioMapAssetDataList scenarioMapAssetDataList = JsonUtility.FromJson<ScenarioMapAssetDataList>(Resources.Load<TextAsset>("Story/ScenarioMapCode").ToString());
            __instance._mapAssetMap = new Dictionary<string, ScenarioMapAssetData>();
            foreach (ScenarioMapAssetData scenarioMapAssetData in scenarioMapAssetDataList.assetData)
            {
                __instance._mapAssetMap.Add(scenarioMapAssetData.id, scenarioMapAssetData);
            }
            __instance._emotionMap = new Dictionary<string, EmotionAsset>();
            for (int i = 0; i < __instance._emotions.Count; i++)
            {
                __instance._emotionMap.Add(__instance._emotions[i].prefab.Name.ToLower(), __instance._emotions[i]);
            }
            return false;
        }
        private static bool GetScenario(StoryData __instance, string scenarioID, LOCALIZE_LANGUAGE lang, ref Scenario __result)
        {
            string item = File.ReadAllText(LimbusLocalize.path + "/Localize/CN/CN_" + scenarioID + ".json");
            TextAsset textAsset = SingletonBehavior<AddressableManager>.Instance.LoadAssetSync<TextAsset>("Assets/Resources_moved/Story/Effect", scenarioID, null, null).Item1;
            if (textAsset == null)
            {
                textAsset = SingletonBehavior<AddressableManager>.Instance.LoadAssetSync<TextAsset>("Assets/Resources_moved/Story/Effect", "SDUMMY", null, null).Item1;
            }
            string text3 = item;
            string text4 = textAsset.ToString();
            Scenario scenario = new Scenario();
            scenario.ID = scenarioID;
            JSONArray jsonarray = (JSONArray)JSONNode.Parse(text3)["dataList"];
            JSONArray jsonarray2 = (JSONArray)JSONNode.Parse(text4)["dataList"];
            for (int i = 0; i < jsonarray.Count; i++)
            {
                int num = jsonarray[i]["id"];
                if (num >= 0)
                {
                    JSONNode jsonnode = new JSONObject();
                    if (jsonarray2[i]["id"] == num)
                    {
                        jsonnode = jsonarray2[i];
                    }
                    scenario.Scenarios.Add(new Dialog(num, jsonarray[i], jsonnode));
                }
            }
            __result = scenario;
            return false;
        }
        private static bool GetTellerTitle(StoryData __instance, string name, LOCALIZE_LANGUAGE lang, ref string __result)
        {
            if (__instance._modelAssetMap.TryGetValue(name, out ScenarioAssetData scenarioAssetData))
            {
                __result = scenarioAssetData.nickName;
            }

            return false;
        }
        private static bool GetTellerName(StoryData __instance, string name, LOCALIZE_LANGUAGE lang, ref string __result)
        {
            if (__instance._modelAssetMap.TryGetValue(name, out ScenarioAssetData scenarioAssetData))
            {
                __result = scenarioAssetData.krname;
            }

            return false;
        }
    }
}
