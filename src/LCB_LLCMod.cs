using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace LimbusLocalize
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class LCB_LLCMod : BasePlugin
    {
        public static ConfigFile LLC_Settings;
        public static string ModPath;
        public static string GamePath;
        public const string GUID = "Com.Bright.LocalizeLimbusCompany";
        public const string NAME = "LimbusLocalizeMod";
        public const string VERSION = "0.6.52";
        public const string AUTHOR = "Bright";
        public const string LLCLink = "https://github.com/LocalizeLimbusCompany/LocalizeLimbusCompany";
        public static Action<string, Action> LogFatalError { get; set; }
        public static Action<string> LogError { get; set; }
        public static Action<string> LogWarning { get; set; }
        public static Action<string> LogInfo { get; set; }
        public static void OpenLLCURL() => Application.OpenURL(LLCLink);
        public static void OpenGamePath() => Application.OpenURL(GamePath);
        public override void Load()
        {
            LLC_Settings = Config;
            LogError = (string log) => { Log.LogError(log); Debug.LogError(log); };
            LogWarning = (string log) => { Log.LogWarning(log); Debug.LogWarning(log); };
            LogInfo = (string log) => { Log.LogInfo(log); Debug.Log(log); };
            LogFatalError = (string log, Action action) => { LLC_Manager.FatalErrorlog += log + "\n"; LogError(log); LLC_Manager.FatalErrorAction = action; LLC_Manager.CheckModActions(); };
            ModPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            GamePath = new DirectoryInfo(Application.dataPath).Parent.FullName;
            if (File.Exists(ModPath + "/version.json"))
            {
                LogInfo("Exist version.json, start update checker.");
                try
                {
                    LLC_UpdateChecker.UpdateMod();
                }
                catch (Exception e)
                {
                    LogWarning("Update checker error: " + e.ToString());
                    LogInfo("Skip update.");
                }
            }
            else
            {
                LogInfo("Not Exist version.json, skip update checker.");
            }
            try
            {
                Harmony harmony = new(NAME);
                if (LLC_Chinese_Setting.IsUseChinese.Value)
                {
                    LLC_Manager.InitLocalizes(new DirectoryInfo(ModPath + "/Localize/CN"));
                    harmony.PatchAll(typeof(LCB_Chinese_Font));
                    harmony.PatchAll(typeof(LLC_ReadmeManager));
                    harmony.PatchAll(typeof(LLC_LoadingManager));
                    harmony.PatchAll(typeof(LLC_SpriteUI));
                }
                harmony.PatchAll(typeof(LLC_Manager));
                harmony.PatchAll(typeof(LLC_Chinese_Setting));
                if (!LCB_Chinese_Font.AddChineseFont(ModPath + "/tmpchinesefont"))
                    LogFatalError("You Not Have Chinese Font, Please Read GitHub Readme To Download", OpenLLCURL);
            }
            catch (Exception e)
            {
                LogFatalError("Mod Has Unknown Fatal Error!!!", () => { CopyLog(); OpenGamePath(); OpenLLCURL(); });
                LogError(e.ToString());
            }
            LogInfo("Limbus Localize Mod v" + VERSION + " is loaded.");
        }
        public static void CopyLog()
        {
            File.Copy(GamePath + "/BepInEx/LogOutput.log", GamePath + "/Latest(框架日志).log", true);
            File.Copy(Application.consoleLogPath, GamePath + "/Player(游戏日志).log", true);
        }
    }
}
