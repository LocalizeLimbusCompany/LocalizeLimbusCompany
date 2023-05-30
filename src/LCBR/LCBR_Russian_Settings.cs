using HarmonyLib;
using Il2Cpp;
using Il2CppLocalSave;
using Il2CppMainUI;
using Il2CppTMPro;
using MelonLoader;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace LimbusLocalizeRUS
{
    public static class LCBR_Russian_Settings
    {
        public static MelonPreferences_Entry<bool> IsUseRussian = LCB_LCBRMod.LCBR_Settings.CreateEntry("IsUseRussian", true, null, "Use true, false is optional");
        static bool _isuserussian;
        static Toggle Russian_Settings;
        [HarmonyPatch(typeof(SettingsPanelGame), nameof(SettingsPanelGame.InitLanguage))]
        [HarmonyPrefix]
        private static bool InitLanguage(SettingsPanelGame __instance, LocalGameOptionData option)
        {
            if (!Russian_Settings)
            {
                Toggle original = __instance._languageToggles[0];
                Transform parent = original.transform.parent;
                var _languageToggle = UnityEngine.Object.Instantiate(original, parent);
                var rutmp = _languageToggle.GetComponentInChildren<TextMeshProUGUI>(true);
                rutmp.font = LCB_Cyrillic_Font.GetCyrillicFonts(5);
                rutmp.fontMaterial = LCB_Cyrillic_Font.GetCyrillicFonts(4).material;
                rutmp.text = "<size=40>Русский</size>";
                Russian_Settings = _languageToggle;
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
            if (_isuserussian = IsUseRussian.Value)
                Russian_Settings.SetIsOnWithoutNotify(true);
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
        private static void ApplySetting()
        {
            IsUseRussian.Value = _isuserussian;
        }
        private static void OnClickLanguageToggleEx(this SettingsPanelGame __instance, int tgIdx)
        {
            if (tgIdx == 3)
            {
                _isuserussian = true;
                return;
            }
            _isuserussian = false;
            if (tgIdx == 0)
                __instance._lang = LOCALIZE_LANGUAGE.KR;
            else if (tgIdx == 1)
                __instance._lang = LOCALIZE_LANGUAGE.EN;
            else if (tgIdx == 2)
                __instance._lang = LOCALIZE_LANGUAGE.JP;
        }
    }
}