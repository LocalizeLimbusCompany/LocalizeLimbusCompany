using BattleUI;
using HarmonyLib;

namespace LimbusLocalize
{
    public static class LLC_SpriteUI
    {
        [HarmonyPatch(typeof(ParryingTypoUI), nameof(ParryingTypoUI.Init))]
        [HarmonyPostfix]
        private static void ParryingTypoUI_Init(ParryingTypoUI __instance)
          => __instance.img_parryingTypo.sprite = LLC_ReadmeManager.ReadmeSprites["LLC_Combo"];
    }
}
