using BattleUI;
using BattleUI.Typo;
using HarmonyLib;
using MainUI;
using TMPro;
using UnityEngine.UI;

namespace LimbusLocalize.LLC;

public static class UIImproved
{
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
			case FormationPersonalityUI_LabelTypes.Changed:
				__instance.tmp_text.text = "<size=45>已更改";
				break;
			case FormationPersonalityUI_LabelTypes.Participated:
				__instance.img_label.sprite = ReadmeManager.ReadmeSprites["LLC_Selected"];
				break;
		}
	}
}