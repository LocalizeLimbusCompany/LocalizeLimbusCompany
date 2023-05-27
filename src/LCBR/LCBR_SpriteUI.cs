using HarmonyLib;
using Il2CppBattleUI;

namespace LimbusLocalizeRUS
{
    public static class LCBR_SpriteUI
    {
        [HarmonyPatch(typeof(ParryingTypoUI), nameof(ParryingTypoUI.Init))]
        [HarmonyPostfix]
        private static void ParryingTypoUI_Init(ParryingTypoUI __instance)
        {
            __instance.img_parryingTypo.sprite = LCBR_ReadmeManager.ReadmeSprites["LCBR_Combo"];
        }
    }
}
