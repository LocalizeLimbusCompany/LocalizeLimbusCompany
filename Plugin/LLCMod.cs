using System;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using LimbusLocalize.LLC;
using UnityEngine;

namespace LimbusLocalize;

[BepInPlugin(Guid, Name, Version)]
public class LLCMod : BasePlugin
{
	public enum NodeType
	{
		Auto,
		ZhenJiang,
		GitHub,
		Tianyi
	}

	public const string Guid = $"Com.{Author}.{Name}";
	public const string Name = "LocalizeLimbusCompany";
	public const string Version = "0.7.3";
	public const string Author = "Bright";
	public const string LLCLink = "https://github.com/LocalizeLimbusCompany/LocalizeLimbusCompany";
	public static ConfigFile LLCSettings;
	public static string ModPath;
	public static string GamePath;
	public static Harmony Harmony = new(Name);

	public static ConfigEntry<bool> UIImprove;
	public static Action<string, Action> LogFatalError { get; set; }
	public static Action<string> LogError { get; set; }
	public static Action<string> LogWarning { get; set; }
	public static Action<string> LogInfo { get; set; }

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
		LogInfo = log => Log.LogInfo(log);
		LogWarning = log => Log.LogWarning(log);
		LogError = log => Log.LogError(log);
		LogFatalError = (log, action) =>
		{
			Manager.FatalErrorlog += log + "\n";
			LogError(log);
			Manager.FatalErrorAction = action;
			Manager.CheckModActions();
		};
		ModPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		GamePath = new DirectoryInfo(Application.dataPath).Parent!.FullName;
		UIImprove = LLCSettings.Bind("LLC Settings", "UIImprove", true, "UI改进,若出现黑屏问题或不想使用“命定之战”之类的功能可以关闭 ( true | false )");
		InitUpdateConfig();
		try
		{
			if (ChineseSetting.IsUseChinese.Value)
			{
				Manager.InitLocalizes(new DirectoryInfo(ModPath + "/Localize/CN"));
				Harmony.PatchAll(typeof(ChineseFont));
				Harmony.PatchAll(typeof(ReadmeManager));
				Harmony.PatchAll(typeof(LoadingManager));
				if (UIImprove.Value) Harmony.PatchAll(typeof(UIImproved));
			}

			Harmony.PatchAll(typeof(Manager));
			Harmony.PatchAll(typeof(ChineseSetting));
			if (!ChineseFont.AddChineseFont(ModPath + "/tmpchinesefont"))
				LogFatalError(
					"You Not Have Chinese Font, Please Read GitHub Readme To Download\n你没有下载中文字体,请阅读GitHub的Readme下载",
					OpenLLCUrl);
			LogInfo("Startup " + DateTime.Now);
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

	private static void InitUpdateConfig()
	{
		LLCSettings.Bind("LLC Settings", "TimeOuted", 10, "自动检查并下载更新的超时时间");
		LLCSettings.Bind("LLC Settings", "AutoUpdate", true, "是否自动检查并下载更新 ( true | false )");
		LLCSettings.Bind("LLC Settings", "UpdateURI", NodeType.Auto,
			"自动更新所使用URI ( Auto：自动 | ZhenJiang：中国镇江服务器 | GitHub：GitHub | OneDrive：Onedrive For Business | Tianyi：天翼网盘 )");
	}
}