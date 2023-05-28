using HarmonyLib;
using Il2Cpp;
using Il2CppTMPro;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace LimbusLocalizeRUS
{
    public static class LCBR_LoadingManager
    {
        static List<string> LoadingTexts = new();
        static string Angela;
        static readonly string Raw = "<bounce f=0.5>NOW LOADING...</bounce>";
        public static void InitLoadingTexts()
        {
            LoadingTexts = File.ReadAllLines(LCB_LCBRMod.ModPath + "/Localize/Readme/LoadingTexts.md").ToList();
            for (int i = 0; i < LoadingTexts.Count; i++)
            {
                string LoadingText = LoadingTexts[i];
                LoadingTexts[i] = "<bounce f=0.5>" + LoadingText.Remove(0, 2) + "</bounce>";
            }
            Angela = LoadingTexts[0];
            LoadingTexts.RemoveAt(0);
        }
        public static T SelectOne<T>(List<T> list)
        {
            if (list.Count == 0)
            {
                return default;
            }
            return list[Random.Range(0, list.Count - 1)];
        }
        [HarmonyPatch(typeof(LoadingSceneManager), nameof(LoadingSceneManager.Start))]
        [HarmonyPostfix]
        private static void LSM_Start(LoadingSceneManager __instance)
        {
            var loadingText = __instance._loadingText;
            loadingText.font = LCB_Cyrillic_Font.tmpcyrillicfonts[3];
            loadingText.fontMaterial = LCB_Cyrillic_Font.tmpcyrillicfonts[3].material;
            loadingText.fontSize = 46;
            int random = Random.Range(0, 100);
            if (random < 25)
                loadingText.text = Raw;
            else if (random < 50)
                loadingText.text = Angela;
            else
                loadingText.text = SelectOne(LoadingTexts);
        }
    }
}
