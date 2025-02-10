using System;
using BattleUI.Dialog;
using BattleUI.Typo;
using BepInEx.Configuration;
using HarmonyLib;
using LocalSave;
using MainUI;
using StorySystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Voice;
using Object = UnityEngine.Object;

namespace LimbusLocalize.LLC;

public static class ChineseSetting
{
    public static ConfigEntry<bool> IsUseChinese =
        LLCMod.LLCSettings.Bind("LLC Settings", "IsUseChinese", true, "是否使用汉化 ( true | false )");

    public static ConfigEntry<bool> ShowDialog = LLCMod.LLCSettings.Bind("LLC Settings", "ShowDialog", false,
        "将罪人在战斗内的语音文本翻译以头顶气泡的形式呈现 ( true | false )");

    private static bool _isusechinese;
    private static Toggle _chineseSetting;
    private static BattleUnitView _unitView;

    [HarmonyPatch(typeof(SettingsPanelGame), nameof(SettingsPanelGame.InitLanguage))]
    [HarmonyPrefix]
    private static bool InitLanguage(SettingsPanelGame __instance, LocalGameOptionData option)
    {
        if (!_chineseSetting)
        {
            Toggle original = __instance._languageToggles[0];
            var parent = original.transform.parent;
            var languageToggle = Object.Instantiate(original, parent);
            var cntmp = languageToggle.GetComponentInChildren<TextMeshProUGUI>(true);
            cntmp.font = ChineseFont.Tmpchinesefonts[0];
            cntmp.fontMaterial = ChineseFont.Tmpchinesefonts[0].material;
            cntmp.text = "中文";
            _chineseSetting = languageToggle;
            parent.localPosition =
                new Vector3(parent.localPosition.x - 306f, parent.localPosition.y, parent.localPosition.z);
            while (__instance._languageToggles.Count > 3)
                __instance._languageToggles.RemoveAt(__instance._languageToggles.Count - 1);
            __instance._languageToggles.Add(languageToggle);
        }

        foreach (var tg in __instance._languageToggles)
        {
            tg.onValueChanged.RemoveAllListeners();

            tg.onValueChanged.AddListener((Action<bool>)OnValueChanged);
            tg.SetIsOnWithoutNotify(false);
            continue;

            void OnValueChanged(bool isOn)
            {
                if (!isOn) return;
                __instance.OnClickLanguageToggleEx(__instance._languageToggles.IndexOf(tg));
            }
        }

        var language = option.GetLanguage();
        if (_isusechinese = IsUseChinese.Value)
            _chineseSetting.SetIsOnWithoutNotify(true);
        else
            switch (language)
            {
                case LOCALIZE_LANGUAGE.KR:
                    __instance._languageToggles[0].SetIsOnWithoutNotify(true);
                    break;
                case LOCALIZE_LANGUAGE.EN:
                    __instance._languageToggles[1].SetIsOnWithoutNotify(true);
                    break;
                case LOCALIZE_LANGUAGE.JP:
                    __instance._languageToggles[2].SetIsOnWithoutNotify(true);
                    break;
            }

        __instance._lang = language;
        return false;
    }

    [HarmonyPatch(typeof(SettingsPanelGame), nameof(SettingsPanelGame.ApplySetting))]
    [HarmonyPostfix]
    private static void ApplySetting()
    {
        IsUseChinese.Value = _isusechinese;
    }

    private static void OnClickLanguageToggleEx(this SettingsPanelGame instance, int tgIdx)
    {
        if (tgIdx == 3)
        {
            _isusechinese = true;
            return;
        }

        _isusechinese = false;
        instance._lang = tgIdx switch
        {
            0 => LOCALIZE_LANGUAGE.KR,
            1 => LOCALIZE_LANGUAGE.EN,
            2 => LOCALIZE_LANGUAGE.JP,
            _ => instance._lang
        };
    }

    [HarmonyPatch(typeof(DateUtil), nameof(DateUtil.TimeZoneOffset), MethodType.Getter)]
    [HarmonyPrefix]
    private static bool TimeZoneOffset(ref int __result)
    {
        if (!IsUseChinese.Value)
            return true;
        __result = 8;
        return false;
    }

    [HarmonyPatch(typeof(DateUtil), nameof(DateUtil.TimeZoneString), MethodType.Getter)]
    [HarmonyPrefix]
    private static bool TimeZoneString(ref string __result)
    {
        if (!IsUseChinese.Value)
            return true;
        __result = "CST";
        return false;
    }

    [HarmonyPatch(typeof(PriceText), nameof(PriceText.SetPrice))]
    [HarmonyPrefix]
    private static bool SetPrice(PriceText __instance, IAPProductStaticData productStaticData)
    {
        if (!IsUseChinese.Value)
            return true;
        __instance.tmp_unit.text = "CNY";
        var priceTier = StaticDataManager.Instance._iapProductStaticDataList
            .GetDataByProductID(productStaticData.productId).priceTier;
        var usdCent = StaticDataManager.Instance._iapPriceTierStaticDataList.list
            .Find((Func<IAPPriceTierStaticData, bool>)(price => price.PriceTier == priceTier)).usd_cent;
        __instance.tmp_price.text = ((usdCent + 1) * 7 / 100).ToString();
        return false;
    }

    [HarmonyPatch(typeof(BattleUnitView), nameof(BattleUnitView.ViewCancelTextTypo_Lack))]
    [HarmonyPrefix]
    private static bool ViewCancelTextTypo_Lack(BattleUnitView __instance, CanceledData data)
    {
        if (!IsUseChinese.Value)
            return true;
        if (data?._lackOfBuffs?.Count > 0)
            __instance.UIManager.bufTypoUI.OpenBufTypo(BUF_TYPE.Negative,
                Singleton<TextDataSet>.Instance.BufList.GetData(data._lackOfBuffs[0].ToString()).GetName() + " 不足",
                data._lackOfBuffs[0]);
        return false;
    }

    [HarmonyPatch(typeof(StoryPlayData), nameof(StoryPlayData.GetDialogAfterClearingAllCathy))]
    [HarmonyPrefix]
    private static bool GetDialogAfterClearingAllCathy(Scenario curStory, Dialog dialog, ref string __result)
    {
        if (!IsUseChinese.Value)
            return true;
        __result = dialog.Content;
        var instance = UserDataManager.Instance;
        if ("P10704".Equals(curStory.ID) && instance?._unlockCodeData != null &&
            instance._unlockCodeData.CheckUnlockStatus(106) &&
            dialog.Id == 3)
            __result = __result.Replace("凯茜", "■■■■■");
        return false;
    }

    [HarmonyPatch(typeof(Util), nameof(Util.GetDlgAfterClearingAllCathy))]
    [HarmonyPrefix]
    private static bool GetDlgAfterClearingAllCathy(string dlgId, string originString, ref string __result)
    {
        if (!IsUseChinese.Value)
            return true;
        __result = originString;
        var instance = UserDataManager.Instance;
        if (instance?._unlockCodeData == null || !instance._unlockCodeData.CheckUnlockStatus(106))
            return false;
        __result = dlgId switch
        {
            "battle_defeat_10707_1" => __result.Replace("凯茜", "■■■■■"),
            "battle_dead_10704_1" => __result.Replace("凯瑟琳", "■■■■■"),
            _ => __result
        };
        return false;
    }

    [HarmonyPatch(typeof(UnitInfoBreakSectionTooltipUI), nameof(UnitInfoBreakSectionTooltipUI.SetDataAndOpen))]
    [HarmonyPostfix]
    private static void SetDataAndOpen(UnitInfoBreakSectionTooltipUI __instance)
    {
        if (!IsUseChinese.Value)
            return;
        __instance.tmp_tooltipContent.font = ChineseFont.Tmpchinesefonts[0];
        __instance.tmp_tooltipContent.fontSize = 35f;
    }

    [HarmonyPatch(typeof(ActBossBattleStartUI), nameof(ActBossBattleStartUI.Init))]
    [HarmonyPostfix]
    private static void BossBattleStartInit(ActBossBattleStartUI __instance)
    {
        if (!IsUseChinese.Value)
            return;
        var textGroup = __instance.transform.GetChild(2).GetChild(1);
        var tmp = textGroup.GetChild(1).GetComponentInChildren<TextMeshProUGUI>();
        if (!tmp.text.Equals("Proelium Fatale"))
            return;
        tmp.font = ChineseFont.Tmpchinesefonts[0];
        tmp.text = "<b>命定之战</b>";
        tmp = textGroup.GetChild(2).GetComponentInChildren<TextMeshProUGUI>();
        tmp.font = ChineseFont.Tmpchinesefonts[0];
        tmp.text = "凡跨入此门之人，当放弃一切希望";
    }

    [HarmonyPatch(typeof(VoiceGenerator), nameof(VoiceGenerator.CreateVoiceInstance))]
    [HarmonyPostfix]
    [HarmonyDebug]
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