using BattleUI;
using HarmonyLib;

namespace LimbusLocalize.LLC;

public static class SpriteUI
{
    [HarmonyPatch(typeof(ParryingTypoUI), nameof(ParryingTypoUI.SetParryingTypoData))]
    [HarmonyPrefix]
    private static void ParryingTypoUI_SetParryingTypoData(ParryingTypoUI __instance)
    {
        __instance.img_parryingTypo.sprite = ReadmeManager.ReadmeSprites["LLC_Combo"];
    }
}