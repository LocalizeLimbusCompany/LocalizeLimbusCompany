using LimbusLocalize;
using MelonLoader;
using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

[assembly: MelonInfo(typeof(LCB_LLCMod), LCB_LLCMod.NAME, LCB_LLCMod.VERSION, LCB_LLCMod.AUTHOR, LCB_LLCMod.LLCLink)]
namespace LimbusLocalize
{
    public class LCB_LLCMod : MelonMod
    {
        public static string ModPath;
        public static string GamePath;
        public const string NAME = "LimbusLocalizeMod";
        public const string VERSION = "0.5.10";
        public const string AUTHOR = "Bright&SmallYuan";
        public const string LLCLink = "https://github.com/SmallYuanSY/LocalizeLimbusCompany";
        public static MelonPreferences_Category LLC_Settings = MelonPreferences.CreateCategory("LLC", "LLC Settings");
        public static Action<string, Action> LogFatalError { get; set; }
        public static Action<string> LogError { get; set; }
        public static Action<string> LogWarning { get; set; }
        public static void OpenLLCURL() { Application.OpenURL(LLCLink); }
        public static void OpenGamePath() { Application.OpenURL(GamePath); }
        public override void OnInitializeMelon()
        {
            LogError = (string log) => { LoggerInstance.Error(log); Debug.LogError(log); };
            LogWarning = (string log) => { LoggerInstance.Warning(log); Debug.LogWarning(log); };
            LogFatalError = (string log, Action action) => { LLC_Manager.FatalErrorlog += log + "\n"; LogError(log); LLC_Manager.FatalErrorAction = action; LLC_Manager.CheckModActions(); };
            ModPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            GamePath = new DirectoryInfo(Application.dataPath).Parent.FullName;
            try
            {
                LLC_UpdateChecker.StartCheckUpdates();
                HarmonyLib.Harmony harmony = new("LimbusLocalizeMod");
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
                    LogFatalError("You Not Have Chinese Font, Please Read GitHub Readme To Download\n你没有下载中文字體,請閱讀GitHub的Readme下載", OpenLLCURL);
            }
            catch (Exception e)
            {
                LogFatalError("Mod Has Unknown Fatal Error!!!\n模组部分功能出現致命錯誤,即將打開GitHub,請根據Issues流程反饋", () => { OnApplicationQuit(); OpenGamePath(); OpenLLCURL(); });
                LogError(e.ToString());
            }
        }
        public override void OnApplicationQuit()
        {
            File.Copy(GamePath + "/MelonLoader/Latest.log", GamePath + "/Latest(框架日志).log", true);
            var Latestlog = File.ReadAllText(GamePath + "/Latest(框架日志).log");
            Latestlog = Regex.Replace(Latestlog, "[0-9:\\.\\[\\]]+ During invoking native->managed trampoline(\r\n)?", "");
            File.WriteAllText(GamePath + "/Latest(框架日志).log", Latestlog);
            File.Copy(Application.consoleLogPath, GamePath + "/Player(游戏日志).log", true);
        }
    }
}
