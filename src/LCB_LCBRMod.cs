using LimbusLocalizeRUS;
using MelonLoader;
using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

[assembly: MelonInfo(typeof(LCB_LCBRMod), LCB_LCBRMod.NAME, LCB_LCBRMod.VERSION, LCB_LCBRMod.AUTHOR, LCB_LCBRMod.LCBRLink)]
namespace LimbusLocalizeRUS
{
    public class LCB_LCBRMod : MelonMod
    {
        public static string ModPath;
        public static string GamePath;
        public const string NAME = "LimbusCompanyBusRUS";
        public const string VERSION = "0.1.0";
        public const string AUTHOR = "Base: Bright\nRUS version: Knightey";
        public const string LCBRLink = "https://github.com/Crescent-Corporation/LimbusCompanyBusRUS";
        public static MelonPreferences_Category LCBR_Settings = MelonPreferences.CreateCategory("LCBR", "LCBR Settings");
        public static Action<string, Action> LogFatalError { get; set; }
        public static Action<string> LogError { get; set; }
        public static Action<string> LogWarning { get; set; }
        public static void OpenLCBRURL() { Application.OpenURL(LCBRLink); }
        public static void OpenGamePath() { Application.OpenURL(GamePath); }
        public override void OnInitializeMelon()
        {
            LogError = (string log) => { LoggerInstance.Error(log); Debug.LogError(log); };
            LogWarning = (string log) => { LoggerInstance.Warning(log); Debug.LogWarning(log); };
            LogFatalError = (string log, Action action) => { LCBR_Manager.FatalErrorlog += log + "\n"; LogError(log); LCBR_Manager.FatalErrorAction = action; LCBR_Manager.CheckModActions(); };
            ModPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            GamePath = new DirectoryInfo(Application.dataPath).Parent.FullName;
            try
            {
                LCBR_Manager.InitLocalizes(new DirectoryInfo(ModPath + "/Localize/RU"));
                LCBR_ReadmeManager.InitReadmeList();
                LCBR_LoadingManager.InitLoadingTexts();
                LCBR_UpdateChecker.StartCheckUpdates();
                HarmonyLib.Harmony harmony = new("LimbusLocalizeRUS");
                if (LCBR_Russian_Settings.IsUseRussian.Value)
                {
                    harmony.PatchAll(typeof(LCB_Cyrillic_Font));
                    harmony.PatchAll(typeof(LCBR_ReadmeManager));
                    harmony.PatchAll(typeof(LCBR_LoadingManager));
                    harmony.PatchAll(typeof(LCBR_SpriteUI));
                }
                harmony.PatchAll(typeof(LCBR_Russian_Settings));
                harmony.PatchAll(typeof(LCBR_Manager));
                if (!LCB_Cyrillic_Font.AddCyrillicFont(ModPath + "/tmpcyrillicfonts"))
                    LogFatalError("You have forgotten to install Font Update Mod. Please, reread README on Github.", OpenLCBRURL);
                foreach (var font in LCB_Cyrillic_Font.tmpcyrillicfontsnames)
                {
                    LoggerInstance.Msg(font);
                }
            }
            catch (Exception e)
            {
                LogFatalError("Mod has met an unknown fatal error! Contact us on Github with the log, please.", () => { OnApplicationQuit(); OpenGamePath(); OpenLCBRURL(); });
                LogError(e.ToString());
            }
        }
        public override void OnApplicationQuit()
        {
            File.Copy(GamePath + "/MelonLoader/Latest.log", GamePath + "/Framework_Log.log", true);
            var Latestlog = File.ReadAllText(GamePath + "/Framework_Log.log");
            Latestlog = Regex.Replace(Latestlog, "[0-9:\\.\\[\\]]+ During invoking native->managed trampoline(\r\n)?", "");
            File.WriteAllText(GamePath + "/Framework_Log.log", Latestlog);
            File.Copy(Application.consoleLogPath, GamePath + "/Framework_Log.log", true);
        }
    }
}
