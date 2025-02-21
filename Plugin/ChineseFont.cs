using System;
using System.IO;
using Addressable;
using HarmonyLib;
using Il2CppSystem.Collections.Generic;
using LimbusLocalize.LLC;
using SimpleJSON;
using StorySystem;
using TMPro;
using UnityEngine;
using UtilityUI;
using Object = UnityEngine.Object;

namespace LimbusLocalize;

public static class ChineseFont
{
	public static readonly List<TMP_FontAsset> Tmpchinesefonts = new();
	public static readonly List<string> Tmpchinesefontnames = new();

	public static bool TryGetValueEx<TKey, TValue>(this Dictionary<TKey, TValue> dic, TKey key, out TValue value)
	{
		var entries = dic._entries;
		var entr = dic.FindEntry(key);
		value = entr == -1 || entries == null ? default : entries[entr].value;
		return value != null;
	}

	#region 字体

	public static bool AddChineseFont(string path)
	{
		if (!File.Exists(path)) return false;

		var assetBundle = AssetBundle.LoadFromFile(path);
		if (!assetBundle) return false;

		var fontFound = false;
		var allAssets = assetBundle.LoadAllAssets();

		foreach (var asset in allAssets)
		{
			var fontAsset = asset.TryCast<TMP_FontAsset>();
			if (!fontAsset) continue;

			Object.DontDestroyOnLoad(fontAsset);
			fontAsset.hideFlags |= HideFlags.HideAndDontSave;

			if (Tmpchinesefonts.Contains(fontAsset)) continue;
			Tmpchinesefonts.Add(fontAsset);
			Tmpchinesefontnames.Add(fontAsset.name);
			fontFound = true;
		}

		return fontFound;
	}

	public static bool IsChineseFont(TMP_FontAsset fontAsset)
	{
		return Tmpchinesefontnames.Contains(fontAsset.name);
	}

	[HarmonyPatch(typeof(TMP_Text), nameof(TMP_Text.font), MethodType.Setter)]
	[HarmonyPrefix]
	private static bool Set_font(TMP_Text __instance, ref TMP_FontAsset value)
	{
		if (!value || !IsChineseFont(value))
			value = Tmpchinesefonts[0];
		return true;
	}

	[HarmonyPatch(typeof(TextMeshProLanguageSetter), nameof(TextMeshProLanguageSetter.UpdateTMP),
		typeof(LOCALIZE_LANGUAGE))]
	[HarmonyPostfix]
	private static void UpdateTMP(TextMeshProLanguageSetter __instance)
	{
		__instance._text.font = Tmpchinesefonts[0];
		if (__instance._text.overflowMode == TextOverflowModes.Ellipsis)
			__instance._text.overflowMode = TextOverflowModes.Overflow;
	}

	[HarmonyPatch(typeof(TextMeshProLanguageSetter), nameof(TextMeshProLanguageSetter.Awake))]
	[HarmonyPrefix]
	private static void Awake(TextMeshProLanguageSetter __instance)
	{
		if (!__instance._text && __instance.TryGetComponent<TextMeshProUGUI>(out var textMeshProUGUI))
			__instance._text = textMeshProUGUI;
		if (!__instance._matSetter &&
		    __instance.TryGetComponent<TextMeshProMaterialSetter>(out var textMeshProMaterialSetter))
			__instance._matSetter = textMeshProMaterialSetter;
	}

	[HarmonyPatch(typeof(TextMeshProMaterialSetter), nameof(TextMeshProMaterialSetter.WriteMaterialProperty))]
	[HarmonyPrefix]
	private static bool WriteMaterialProperty(TextMeshProMaterialSetter __instance)
	{
		if (!__instance._fontMaterialInstance) return false;
		if (!IsChineseFont(__instance._text.font)) return true;
		if (__instance.underlayOffsetX != 0f)
			__instance.underlayOn = true;
		return false;
	}

	#endregion

	#region 载入汉化

	public static void LoadLocal()
	{
		var tm = Singleton<TextDataSet>.Instance;
		var localizeFileList =
			JsonUtility.FromJson<TextDataSet.LocalizeFileList>(Resources
				.Load<TextAsset>("Localize/LocalizeFileList").ToString());
		tm._loginUIList.Init(localizeFileList.LoginUIFilePaths);
		tm._fileDownloadDesc.Init(localizeFileList.FileDownloadDesc);
		tm._battleHint._dic.Clear();
		tm._battleHint.Init(localizeFileList.BattleHint);
	}

	public static void LoadRemote2()
	{
		var tm = Singleton<TextDataSet>.Instance;
		var romoteLocalizeFileList = JsonUtility.FromJson<TextDataSet.RomoteLocalizeFileList>(AddressableManager
			.Instance.LoadAssetSync<TextAsset>("Assets/Resources_moved/Localize", "RemoteLocalizeFileList").Item1
			.ToString());
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
		tm._userTicket_L.Init(romoteLocalizeFileList.UserTicketL);
		tm._userTicket_R.Init(romoteLocalizeFileList.UserTicketR);
		tm._userTicket_EGOBg.Init(romoteLocalizeFileList.UserTicketEGOBg);
		tm._panicInfo.Init(romoteLocalizeFileList.PanicInfo);
		tm._mentalConditionList.Init(romoteLocalizeFileList.mentalCondition);
		tm._dungeonStartBuffs.Init(romoteLocalizeFileList.DungeonStartBuffs);
		tm._railwayDungeonBuffText.Init(romoteLocalizeFileList.RailwayDungeonBuff);
		tm._buffAbilityList.Init(romoteLocalizeFileList.buffAbilities);
		tm._egoGiftCategory.Init(romoteLocalizeFileList.EgoGiftCategory);
		tm._mirrorDungeonEgoGiftLockedDescList.Init(romoteLocalizeFileList.MirrorDungeonEgoGiftLockedDesc);
		tm._mirrorDungeonEnemyBuffDescList.Init(romoteLocalizeFileList.MirrorDungeonEnemyBuffDesc);
		tm._iapStickerText.Init(romoteLocalizeFileList.IAPSticker);
		tm._danteAbilityDataList.Init(romoteLocalizeFileList.DanteAbility);
		tm._mirrorDungeonThemeList.Init(romoteLocalizeFileList.MirrorDungeonTheme);
		tm._unlockCodeList.Init(romoteLocalizeFileList.UnlockCode);
		tm._battleSpeechBubbleText.Init(romoteLocalizeFileList.BattleSpeechBubble);
		tm._gachaNotice.Init(romoteLocalizeFileList.GachaNotice);

		tm._abnormalityEventCharDlg.AbEventCharDlgRootInit(romoteLocalizeFileList.abnormalityCharDlgFilePath);

		tm._personalityVoiceText._voiceDictionary.JsonDataListInit(romoteLocalizeFileList.PersonalityVoice);
		tm._announcerVoiceText._voiceDictionary.JsonDataListInit(romoteLocalizeFileList.AnnouncerVoice);
		tm._bgmLyricsText._lyricsDictionary.JsonDataListInit(romoteLocalizeFileList.BgmLyrics);
		tm._egoVoiceText._voiceDictionary.JsonDataListInit(romoteLocalizeFileList.EGOVoice);
	}

	[HarmonyPatch(typeof(EGOVoiceJsonDataList), nameof(EGOVoiceJsonDataList.Init))]
	[HarmonyPrefix]
	private static bool EgoVoiceJsonDataListInit(EGOVoiceJsonDataList __instance, List<string> jsonFilePathList)
	{
		__instance._voiceDictionary = new Dictionary<string, LocalizeTextDataRoot<TextData_EGOVoice>>();
		var callcount = 0;
		foreach (var jsonFilePath in jsonFilePathList)
		{
			AddressableManager.Instance.LoadLocalizeJsonAssetAsync<TextData_EGOVoice>(jsonFilePath,
				(Action<LocalizeTextDataRoot<TextData_EGOVoice>>)LoadLocalizeDel);
			continue;

			void LoadLocalizeDel(LocalizeTextDataRoot<TextData_EGOVoice> data)
			{
				if (data != null)
				{
					var array = jsonFilePath.Split('_');
					var text = array[^1];
					text = text.Replace(".json", "");
					__instance._voiceDictionary[text] = data;
				}

				callcount++;
				if (callcount == jsonFilePathList.Count) LoadRemote2();
			}
		}

		return false;
	}

	[HarmonyPatch(typeof(StoryDataParser), nameof(StoryDataParser.GetScenario))]
	[HarmonyPrefix]
	private static bool GetScenario(string scenarioID, LOCALIZE_LANGUAGE lang, ref Scenario __result)
	{
		var textAsset = AddressableManager.Instance
			.LoadAssetSync<TextAsset>("Assets/Resources_moved/Story/Effect", scenarioID).Item1;
		if (!textAsset)
		{
			LLCMod.LogError("Story Unknown Error!Call Story: Dirty Hacker");
			scenarioID = "SDUMMY";
			textAsset = AddressableManager.Instance
				.LoadAssetSync<TextAsset>("Assets/Resources_moved/Story/Effect", scenarioID).Item1;
		}

		if (!Manager.Localizes.TryGetValue(scenarioID, out var text))
		{
			LLCMod.LogError("Story Error!Can'n Find CN Story File,Use Raw EN Story");
			text = AddressableManager.Instance
				.LoadAssetSync<TextAsset>("Assets/Resources_moved/Localize/en/StoryData", "EN_" + scenarioID).Item1
				.ToString();
		}

		var text2 = textAsset.ToString();
		Scenario scenario = new()
		{
			ID = scenarioID
		};
		var jsonarray = JSONNode.Parse(text)[0].AsArray;
		var jsonarray2 = JSONNode.Parse(text2)[0].AsArray;
		var s = 0;
		for (var i = 0; i < jsonarray.Count; i++)
		{
			var jsonNode = jsonarray[i];
			if (jsonNode.Count < 1)
			{
				s++;
				continue;
			}

			if (jsonNode[0].IsNumber && jsonNode[0].AsInt < 0)
				continue;
			var num = i - s;
			var effectToken = jsonarray2[num];
			if ("IsNotPlayDialog".Sniatnoc(effectToken["effectv2"]))
			{
				scenario.Scenarios.Add(new Dialog(num, new JSONNode(), effectToken));
				if (jsonNode.Count == 1)
					continue;
				s--;
				effectToken = jsonarray2[num + 1];
			}

			scenario.Scenarios.Add(new Dialog(num, jsonNode, effectToken));
		}

		__result = scenario;
		return false;
	}

	public static bool Sniatnoc(this string text, string value)
	{
		if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(value))
			return false;
		return value.Contains(text);
	}

	[HarmonyPatch(typeof(StoryAssetLoader), nameof(StoryAssetLoader.GetTellerName))]
	[HarmonyPrefix]
	private static bool GetTellerName(StoryAssetLoader __instance, string name, LOCALIZE_LANGUAGE lang,
		ref string __result)
	{
		if (__instance._modelAssetMap.TryGetValueEx(name, out var scenarioAssetData))
			__result = scenarioAssetData.krname ?? string.Empty;
		return false;
	}

	[HarmonyPatch(typeof(StoryAssetLoader), nameof(StoryAssetLoader.GetTellerTitle))]
	[HarmonyPrefix]
	private static bool GetTellerTitle(StoryAssetLoader __instance, string name, LOCALIZE_LANGUAGE lang,
		ref string __result)
	{
		if (__instance._modelAssetMap.TryGetValueEx(name, out var scenarioAssetData))
			__result = scenarioAssetData.nickName ?? string.Empty;
		return false;
	}

	[HarmonyPatch(typeof(TextDataSet), nameof(TextDataSet.LoadRemote))]
	[HarmonyPrefix]
	private static void LoadRemote(ref LOCALIZE_LANGUAGE lang)
	{
		lang = LOCALIZE_LANGUAGE.EN;
	}

	[HarmonyPatch(typeof(StoryAssetLoader), nameof(StoryAssetLoader.Init))]
	[HarmonyPostfix]
	private static void StoryDataInit(StoryAssetLoader __instance)
	{
		foreach (var scenarioAssetData in JsonUtility.FromJson<ScenarioAssetDataList>(Manager.Localizes["NickName"])
			         .assetData)
			__instance._modelAssetMap[scenarioAssetData.name] = scenarioAssetData;
	}

	[HarmonyPatch(typeof(LoginSceneManager), nameof(LoginSceneManager.SetLoginInfo))]
	[HarmonyPostfix]
	private static void SetLoginInfo(LoginSceneManager __instance)
	{
		LoadLocal();
		__instance.tmp_loginAccount.text = "LimbusLocalizeMod v" + LLCMod.Version;
	}

	private static void Init<T>(this JsonDataList<T> jsonDataList, List<string> jsonFilePathList)
		where T : LocalizeTextData, new()
	{
		foreach (var text in jsonFilePathList)
		{
			if (!Manager.Localizes.TryGetValue(text, out var text2)) continue;
			var localizeTextData = JsonUtility.FromJson<LocalizeTextDataRoot<T>>(text2);
			foreach (var t in localizeTextData.DataList) jsonDataList._dic[t.ID] = t;
		}
	}

	private static void AbEventCharDlgRootInit(this AbEventCharDlgRoot root, List<string> jsonFilePathList)
	{
		root._personalityDict = new Dictionary<int, AbEventKeyDictionaryContainer>();
		foreach (var text in jsonFilePathList)
		{
			if (!Manager.Localizes.TryGetValue(text, out var text2)) continue;
			var localizeTextData = JsonUtility.FromJson<LocalizeTextDataRoot<TextData_AbnormalityEventCharDlg>>(text2);
			foreach (var t in localizeTextData.DataList)
			{
				if (!root._personalityDict.TryGetValueEx(t.PersonalityID, out var abEventKeyDictionaryContainer))
				{
					abEventKeyDictionaryContainer = new AbEventKeyDictionaryContainer();
					root._personalityDict[t.PersonalityID] = abEventKeyDictionaryContainer;
				}

				var array = t.Usage.Trim().Split('(', ')');
				for (var i = 1; i < array.Length; i += 2)
				{
					var array2 = array[i].Split(',');
					var num = int.Parse(array2[0].Trim());
					var eventType = (AB_DLG_EVENT_TYPE)Enum.Parse(typeof(AB_DLG_EVENT_TYPE), array2[1].Trim());
					AbEventKey abEventKey = new(num, eventType);
					abEventKeyDictionaryContainer.AddDlgWithEvent(abEventKey, t);
				}
			}
		}
	}

	private static void JsonDataListInit<T>(this Dictionary<string, LocalizeTextDataRoot<T>> jsonDataList,
		List<string> jsonFilePathList)
	{
		foreach (var text in jsonFilePathList)
		{
			if (!Manager.Localizes.TryGetValue(text, out var text2)) continue;
			var localizeTextData = JsonUtility.FromJson<LocalizeTextDataRoot<T>>(text2);
			jsonDataList[text.Split('_')[^1]] = localizeTextData;
		}
	}

	#endregion
}