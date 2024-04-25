using BepInEx.Configuration;
using HarmonyLib;
using LocalSave;
using MainUI;
using StorySystem;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LimbusLocalize
{
    public static class LLC_Chinese_Setting
    {
        public static ConfigEntry<bool> IsUseChinese = LCB_LLCMod.LLC_Settings.Bind("LLC Settings", "IsUseChinese", true, "是否使用汉化 ( true | false )");
        static bool _isusechinese;
        static Toggle Chinese_Setting;
        [HarmonyPatch(typeof(SettingsPanelGame), nameof(SettingsPanelGame.InitLanguage))]
        [HarmonyPrefix]
        private static bool InitLanguage(SettingsPanelGame __instance, LocalGameOptionData option)
        {
            if (!Chinese_Setting)
            {
                Toggle original = __instance._languageToggles[0];
                Transform parent = original.transform.parent;
                var _languageToggle = UnityEngine.Object.Instantiate(original, parent);
                var cntmp = _languageToggle.GetComponentInChildren<TextMeshProUGUI>(true);
                cntmp.font = LCB_Chinese_Font.tmpchinesefonts[0];
                cntmp.fontMaterial = LCB_Chinese_Font.tmpchinesefonts[0].material;
                cntmp.text = "中文";
                Chinese_Setting = _languageToggle;
                parent.localPosition = new Vector3(parent.localPosition.x - 306f, parent.localPosition.y, parent.localPosition.z);
                while (__instance._languageToggles.Count > 3)
                    __instance._languageToggles.RemoveAt(__instance._languageToggles.Count - 1);
                __instance._languageToggles.Add(_languageToggle);
            }
            foreach (Toggle tg in __instance._languageToggles)
            {
                tg.onValueChanged.RemoveAllListeners();
                Action<bool> onValueChanged = (bool isOn) =>
                {
                    if (!isOn)
                        return;
                    __instance.OnClickLanguageToggleEx(__instance._languageToggles.IndexOf(tg));
                };
                tg.onValueChanged.AddListener(onValueChanged);
                tg.SetIsOnWithoutNotify(false);
            }
            LOCALIZE_LANGUAGE language = option.GetLanguage();
            if (_isusechinese = IsUseChinese.Value)
                Chinese_Setting.SetIsOnWithoutNotify(true);
            else if (language == LOCALIZE_LANGUAGE.KR)
                __instance._languageToggles[0].SetIsOnWithoutNotify(true);
            else if (language == LOCALIZE_LANGUAGE.EN)
                __instance._languageToggles[1].SetIsOnWithoutNotify(true);
            else if (language == LOCALIZE_LANGUAGE.JP)
                __instance._languageToggles[2].SetIsOnWithoutNotify(true);
            __instance._lang = language;
            return false;
        }
        [HarmonyPatch(typeof(SettingsPanelGame), nameof(SettingsPanelGame.ApplySetting))]
        [HarmonyPostfix]
        private static void ApplySetting() => IsUseChinese.Value = _isusechinese;
        private static void OnClickLanguageToggleEx(this SettingsPanelGame __instance, int tgIdx)
        {
            if (tgIdx == 3)
            {
                _isusechinese = true;
                return;
            }
            _isusechinese = false;
            if (tgIdx == 0)
                __instance._lang = LOCALIZE_LANGUAGE.KR;
            else if (tgIdx == 1)
                __instance._lang = LOCALIZE_LANGUAGE.EN;
            else if (tgIdx == 2)
                __instance._lang = LOCALIZE_LANGUAGE.JP;
        }
        [HarmonyPatch(typeof(DateUtil), nameof(DateUtil.TimeZoneOffset), MethodType.Getter)]
        [HarmonyPrefix]
        private static bool TimeZoneOffset(ref int __result)
        {
            if (IsUseChinese.Value)
            {
                __result = 8;
                return false;
            }
            return true;
        }
        [HarmonyPatch(typeof(DateUtil), nameof(DateUtil.TimeZoneString), MethodType.Getter)]
        [HarmonyPrefix]
        private static bool TimeZoneString(ref string __result)
        {
            if (IsUseChinese.Value)
            {
                __result = "CST";
                return false;
            }
            return true;
        }
        [HarmonyPatch(typeof(StoryPlayData), nameof(StoryPlayData.GetDialogAfterClearingAllCathy))]
        [HarmonyPrefix]
        private static bool GetDialogAfterClearingAllCathy(Scenario curStory, Dialog dialog, ref string __result)
        {
            if (IsUseChinese.Value)
            {
                __result = dialog.Content;
                UserDataManager instance = Singleton<UserDataManager>.Instance;
                if ("P10704".Equals(curStory.ID) && instance != null && instance._unlockCodeData != null && instance._unlockCodeData.CheckUnlockStatus(106) && dialog.Id == 3)
                {
                    __result = __result.Replace("凯茜", "■■■■■");
                }
                return false;
            }
            return true;
        }
        [HarmonyPatch(typeof(Util), nameof(Util.GetDlgAfterClearingAllCathy))]
        [HarmonyPrefix]
        private static bool GetDlgAfterClearingAllCathy(string dlgId, string originString, ref string __result)
        {
            if (IsUseChinese.Value)
            {
                __result = originString;
                UserDataManager instance = Singleton<UserDataManager>.Instance;
                if (instance == null || instance._unlockCodeData == null || !instance._unlockCodeData.CheckUnlockStatus(106))
                    return false;
                if ("battle_defeat_10707_1".Equals(dlgId))
                    __result = __result.Replace("凯茜", "■■■■■");
                else if ("battle_dead_10704_1".Equals(dlgId))
                    __result = __result.Replace("凯瑟琳", "■■■■■");
                return false;
            }
            return true;
        }
    }
}