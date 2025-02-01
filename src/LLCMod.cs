using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using LimbusLocalize.LLC;
using Microsoft.Win32;
using UnityEngine;

namespace LimbusLocalize;

[BepInPlugin(Guid, Name, Version)]
public class LLCMod : BasePlugin
{
    public const string Guid = $"Com.{Author}.{Name}";
    public const string Name = "LocalizeLimbusCompany";
    public const string Version = "0.6.66";
    public const string Author = "Bright";
    public const string LLCLink = "https://github.com/LocalizeLimbusCompany/LocalizeLimbusCompany";
    public static ConfigFile LLCSettings;
    public static string ModPath;
    public static string GamePath;
    public static Harmony Harmony = new(Name);
    public static Action<string, Action> LogFatalError { get; set; }
    public static Action<string> LogError { get; set; }
    public static Action<string> LogWarning { get; set; }

    public static void OpenLLCUrl()
    {
        Application.OpenURL(LLCLink);
    }

    public static void OpenGamePath()
    {
        Application.OpenURL(GamePath);
    }

    public override void Load()
    {
        LLCSettings = Config;
        LogError = log =>
        {
            Log.LogError(log);
            Debug.LogError(log);
        };
        LogWarning = log =>
        {
            Log.LogWarning(log);
            Debug.LogWarning(log);
        };
        LogFatalError = (log, action) =>
        {
            Manager.FatalErrorlog += log + "\n";
            LogError(log);
            Manager.FatalErrorAction = action;
            Manager.CheckModActions();
        };
        ModPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        GamePath = new DirectoryInfo(Application.dataPath).Parent!.FullName;
        UpdateChecker.StartAutoUpdate();
        try
        {
            if (ChineseSetting.IsUseChinese.Value)
            {
                Manager.InitLocalizes(new DirectoryInfo(ModPath + "/Localize/CN"));
                Harmony.PatchAll(typeof(ChineseFont));
                Harmony.PatchAll(typeof(ReadmeManager));
                Harmony.PatchAll(typeof(LoadingManager));
                Harmony.PatchAll(typeof(SpriteUI));
            }

            Harmony.PatchAll(typeof(Manager));
            Harmony.PatchAll(typeof(ChineseSetting));
            if (!ChineseFont.AddChineseFont(ModPath + "/tmpchinesefont"))
                LogFatalError(
                    "You Not Have Chinese Font, Please Read GitHub Readme To Download\n你没有下载中文字体,请阅读GitHub的Readme下载",
                    OpenLLCUrl);
            LogWarning("Startup" + DateTime.Now);
        }
        catch (Exception e)
        {
            LogFatalError("Mod Has Unknown Fatal Error!!!\n模组部分功能出现致命错误,即将打开GitHub,请根据Issues流程反馈", () =>
            {
                CopyLog();
                OpenGamePath();
                OpenLLCUrl();
            });
            LogError(e.ToString());
        }
    }

    public static void CopyLog()
    {
        File.Copy(GamePath + "/BepInEx/LogOutput.log", GamePath + "/Latest(框架日志).log", true);
        File.Copy(Application.consoleLogPath, GamePath + "/Player(游戏日志).log", true);
    }

    public static bool IncompatibleMod()
    {
        try
        {
            //Check BepInEx mod
            if (IL2CPPChainloader.Instance.Plugins.Keys.Any(guid => !guid.StartsWith("Com.Bright.")))
                return OpenGlobalPopup("Invalid assemblies.\nUnable to run game.");
            //Check visual mod
            if (OperatingSystem.IsWindows())
            {
                var steamPath =
                    Registry.GetValue(@"HKEY_CURRENT_USER\Software\Valve\Steam", "SteamPath", null) as string;
                var userdata = long.Parse(LoginInfoManager.Instance.SteamID) - 76561197960265728;
                var lines = File.ReadAllLines($"{steamPath}/userdata/{userdata}/config/localconfig.vdf");
                var start = false;
                var end = false;
                var has = false;
                foreach (var line in lines)
                {
                    if (!start)
                    {
                        if ("\t\t\t\t\t\"1973530\"".Equals(line))
                            start = true;
                        continue;
                    }

                    if (line.Contains("\"LastPlayed\""))
                        if (!end)
                            end = true;
                        else
                            break;
                    var launchOptions = "\t\t\t\t\t\t\"LaunchOptions\"\t\t";
                    if (!line.Contains(launchOptions)) continue;
                    launchOptions = line[launchOptions.Length..];
                    if (launchOptions.Contains("%command%"))
                        has = true;
                    break;
                }

                if (has) return OpenGlobalPopup("Invalid parameters.\nUnable to run game.");
            }

            //Check private server
            var hostEntry = Dns.GetHostEntry("www.limbuscompanyapi.com");
            var ip = hostEntry.AddressList[0].ToString();
            if (ip.StartsWith("127.0.") || ip.StartsWith("192.168."))
                return OpenGlobalPopup("Remote address loopback error.\nUnable to run game.");
        }
        catch (Exception e)
        {
            LogError("IncompatibleMod Error" + e);
        }

        return true;

        bool OpenGlobalPopup(string description)
        {
            Action action = Application.Quit;
            Manager.OpenGlobalPopup(description, null,
                null, "OK", action, action);
            return false;
        }
    }
}