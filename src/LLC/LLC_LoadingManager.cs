using BepInEx.Configuration;
using HarmonyLib;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace LimbusLocalize
{
    public static class LLC_LoadingManager
    {
        static List<string> LoadingTexts = [];
        static string Touhou;
        static readonly string Raw = "<bounce f=0.5>NOW LOADING...</bounce>";
        public static ConfigEntry<bool> RandomLoadText = LCB_LLCMod.LLC_Settings.Bind("LLC Settings", "RandomLoadText", true, "是否随机选择载入标语,即右下角的[NOW LOADING...] ( true | false )");
        static LLC_LoadingManager() => InitLoadingTexts();
        public static void InitLoadingTexts()
        {
            LoadingTexts = [.. File.ReadAllLines(LCB_LLCMod.ModPath + "/Localize/Readme/LoadingTexts.md")];
            for (int i = 0; i < LoadingTexts.Count; i++)
            {
                string LoadingText = LoadingTexts[i];
                LoadingTexts[i] = "<bounce f=0.5>" + LoadingText.Remove(0, 2) + "</bounce>";
            }
            Touhou = LoadingTexts[0];
            LoadingTexts.RemoveAt(0);
        }
        public static T SelectOne<T>(List<T> list)
            => list.Count == 0 ? default : list[Random.Range(0, list.Count)];
        [HarmonyPatch(typeof(LoadingSceneManager), nameof(LoadingSceneManager.Start))]
        [HarmonyPostfix]
        private static void LSM_Start(LoadingSceneManager __instance)
        {
            if (!RandomLoadText.Value)
                return;
            var loadingText = __instance._loadingText;
            loadingText.font = LCB_Chinese_Font.tmpchinesefonts[0];
            loadingText.fontMaterial = LCB_Chinese_Font.tmpchinesefonts[0].material;
            loadingText.fontSize = 40;
            int random = Random.Range(0, 100);
            if (random < 25)
                loadingText.text = Raw;
            else if (random < 50)
                loadingText.text = Touhou;
            else
                loadingText.text = SelectOne(LoadingTexts);
        }
    }
}
