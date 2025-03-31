using BattleUI;
using BattleUI.Dialog;
using BattleUI.Typo;
using BepInEx.Configuration;
using HarmonyLib;
using MainUI;
using TMPro;
using UnityEngine.UI;
using Voice;

namespace LimbusLocalize.LLC;

public static class UIImproved
{
	public static ConfigEntry<bool> ShowDialog = LLCMod.LLCSettings.Bind("LLC Settings", "ShowDialog", false,
		"将罪人在战斗内的语音文本翻译以头顶气泡的形式呈现 ( true | false )");

	private static BattleUnitView _unitView;

	[HarmonyPatch(typeof(ParryingTypoUI), nameof(ParryingTypoUI.SetParryingTypoData))]
	[HarmonyPrefix]
	private static void ParryingTypoUI_SetParryingTypoData(ParryingTypoUI __instance)
	{
		__instance.img_parryingTypo.sprite = ReadmeManager.ReadmeSprites["LLC_Combo"];
	}

	[HarmonyPatch(typeof(ActBossBattleStartUI), nameof(ActBossBattleStartUI.Init))]
	[HarmonyPostfix]
	private static void BossBattleStartInit(ActBossBattleStartUI __instance)
	{
		var textGroup = __instance.transform.GetChild(2).GetChild(1);
		var tmp = textGroup.GetChild(1).GetComponentInChildren<TextMeshProUGUI>();
		if (!tmp.text.Equals("Proelium Fatale"))
			return;
		textGroup.GetChild(1).GetComponentInChildren<Image>().sprite = ReadmeManager.ReadmeSprites["LLC_BossBattle"];
		tmp.font = ChineseFont.Tmpchinesefonts[1];
		tmp.text = "<b>命定之战</b>";
		tmp = textGroup.GetChild(2).GetComponentInChildren<TextMeshProUGUI>();
		tmp.text = "凡跨入此门之人，当放弃一切希望";
		if (!tmp.font.fallbackFontAssetTable.Contains(ChineseFont.Tmpchinesefonts[1]))
			tmp.font.fallbackFontAssetTable.Add(ChineseFont.Tmpchinesefonts[1]);
	}

	[HarmonyPatch(typeof(ActTypoUnlockDanteAbilityUI), nameof(ActTypoUnlockDanteAbilityUI.Open))]
	[HarmonyPostfix]
	private static void UnlockDanteAbilityUIInit(ActTypoUnlockDanteAbilityUI __instance)
	{
		var textGroup = __instance.transform.GetChild(0).GetChild(1).GetChild(6);
		var tmp = textGroup.GetComponentInChildren<TextMeshProUGUI>();
		var mask = textGroup.GetComponentInChildren<Mask>();
		tmp.font = ChineseFont.Tmpchinesefonts[1];
		mask.enabled = false;
		textGroup.GetChild(1).GetComponentInChildren<Image>().enabled = false;
		textGroup.GetChild(2).GetComponentInChildren<Image>().enabled = false;
	}

	[HarmonyPatch(typeof(StageChapterAreaSlot), "Init")]
	[HarmonyPostfix]
	private static void AreaSlotInit(StageChapterAreaSlot __instance)
	{
		var tmproArea = __instance.tmpro_area;
		if (!tmproArea.text.StartsWith("DISTRICT ")) return;
		if (!tmproArea.font.fallbackFontAssetTable.Contains(ChineseFont.Tmpchinesefonts[1]))
			tmproArea.font.fallbackFontAssetTable.Add(ChineseFont.Tmpchinesefonts[1]);
		tmproArea.text = tmproArea.text.Replace("DISTRICT ", "") + "<size=25>区";
	}

	[HarmonyPatch(typeof(FormationPersonalityUI_Label), "Reload")]
	[HarmonyPostfix]
	private static void PersonalityUILabel(FormationPersonalityUI_Label __instance)
	{
		switch (__instance._model._status)
		{
			case FormationPersonalityUI_LabelTypes.Participated:
				__instance.img_label.sprite = ReadmeManager.ReadmeSprites["LLC_Selected"];
				break;
			case FormationPersonalityUI_LabelTypes.Changed:
				__instance.tmp_text.text = "<size=45>已更改";
				break;
			case FormationPersonalityUI_LabelTypes.Baton:
				__instance.img_label.sprite = ReadmeManager.ReadmeSprites["LLC_Backup"];
				break;
		}
	}

	[HarmonyPatch(typeof(VoiceGenerator), nameof(VoiceGenerator.CreateVoiceInstance))]
	[HarmonyPostfix]
	private static void CreateVoiceInstance(string path, bool isSpecial)
	{
		if (!ShowDialog.Value || !_unitView || !path.StartsWith(VoiceGenerator.VOICE_EVENT_PATH + "battle_"))
			return;
		path = path[VoiceGenerator.VOICE_EVENT_PATH.Length..];
		if (!Singleton<TextDataSet>.Instance.personalityVoiceText._voiceDictionary.TryGetValue(path.Split('_')[^2],
			    out var dataList)) return;
		foreach (var data in dataList.dataList)
			if (path.Equals(data.id))
			{
				_unitView._uiManager.ShowDialog(new BattleDialogLine(data.dlg, null));
				break;
			}
	}

	[HarmonyPatch(typeof(BattleUnitView), nameof(BattleUnitView.SetPlayVoice))]
	[HarmonyPrefix]
	private static void BattleUnitView_Func(BattleUnitView __instance, BattleCharacterVoiceType key, bool isSpecial,
		BattleSkillViewer skillViewer)
	{
		_unitView = __instance;
	}
}