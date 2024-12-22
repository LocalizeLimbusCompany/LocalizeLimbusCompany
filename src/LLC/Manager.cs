using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using MainUI.Gacha;
using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using ILObject = Il2CppSystem.Object;
using ULogger = UnityEngine.Logger;
using UObject = UnityEngine.Object;

namespace LimbusLocalize.LLC;

public class Manager(IntPtr ptr) : MonoBehaviour(ptr)
{
    public static Manager Instance;
    public static Dictionary<string, string> Localizes = [];
    public static Action FatalErrorAction;
    public static string FatalErrorlog;

    static Manager()
    {
        ClassInjector.RegisterTypeInIl2Cpp<Manager>();
        GameObject obj = new(nameof(Manager));
        DontDestroyOnLoad(obj);
        obj.hideFlags |= HideFlags.HideAndDontSave;
        Instance = obj.AddComponent<Manager>();
    }

    private void OnApplicationQuit()
    {
        LLCMod.CopyLog();
    }

    public static void OpenGlobalPopup(string description, string title = null, string close = "取消",
        string confirm = "确认", Action confirmEvent = null, Action closeEvent = null)
    {
        if (!GlobalGameManager.Instance) return;
        var globalPopupUI = GlobalGameManager.Instance.globalPopupUI;
        TMP_FontAsset fontAsset = ChineseFont.Tmpchinesefonts[0];
        if (fontAsset)
        {
            var btnCanceltmp = globalPopupUI.btn_cancel.GetComponentInChildren<TextMeshProUGUI>(true);
            btnCanceltmp.font = fontAsset;
            btnCanceltmp.fontMaterial = fontAsset.material;
            var btnCanceltl = globalPopupUI.btn_cancel.GetComponentInChildren<UITextDataLoader>(true);
            btnCanceltl.enabled = false;
            btnCanceltmp.text = close;
            var btnOktmp = globalPopupUI.btn_ok.GetComponentInChildren<TextMeshProUGUI>(true);
            btnOktmp.font = fontAsset;
            btnOktmp.fontMaterial = fontAsset.material;
            var btnOktl = globalPopupUI.btn_ok.GetComponentInChildren<UITextDataLoader>(true);
            btnOktl.enabled = false;
            btnOktmp.text = confirm;
            globalPopupUI.tmp_title.font = fontAsset;
            globalPopupUI.tmp_title.fontMaterial = fontAsset.material;

            confirmEvent += TextLoaderEnabled;
            closeEvent += TextLoaderEnabled;

            void TextLoaderEnabled()
            {
                btnCanceltl.enabled = true;
                btnOktl.enabled = true;
            }
        }

        globalPopupUI._titleObject.SetActive(!string.IsNullOrEmpty(title));
        globalPopupUI.tmp_title.text = title;
        globalPopupUI.tmp_description.text = description;
        globalPopupUI._confirmEvent = confirmEvent;
        globalPopupUI._closeEvent = closeEvent;
        globalPopupUI.btn_cancel.gameObject.SetActive(!string.IsNullOrEmpty(close));
        globalPopupUI._gridLayoutGroup.cellSize = new Vector2(!string.IsNullOrEmpty(close) ? 500 : 700, 100f);
        globalPopupUI.Open();
    }

    public static void InitLocalizes(DirectoryInfo directory)
    {
        foreach (var fileInfo in directory.GetFiles())
        {
            var value = File.ReadAllText(fileInfo.FullName);
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileInfo.FullName);
            Localizes[fileNameWithoutExtension] = value;
        }

        foreach (var directoryInfo in directory.GetDirectories()) InitLocalizes(directoryInfo);
    }

    #region 修复一些弱智东西

    [HarmonyPatch(typeof(GachaEffectEventSystem), nameof(GachaEffectEventSystem.LinkToCrackPosition))]
    [HarmonyPrefix]
    private static bool LinkToCrackPosition(GachaEffectEventSystem __instance)
    {
        return __instance._parent.EffectChainCamera;
    }

    #endregion

    [HarmonyPatch(typeof(LoginSceneManager), nameof(LoginSceneManager.SetLoginInfo))]
    [HarmonyPostfix]
    public static void CheckModActions()
    {
        if (!UpdateChecker.needPopup)
        {
            return;
        }
        // 首先判断程序版本是否落后
        if (UpdateChecker.isAppOutdated)
        {
            OpenGlobalPopup("您的模组程序版本已落后！\n请使用工具箱或手动更新模组文件到最新版本。", "模组程序已落后", "关闭游戏", "忽略",null, Application.Quit);
            return;
        }
        string UpdateMessage = "您的模组已更新至最新版本！\n更新内容：";
        if (!string.IsNullOrEmpty(UpdateChecker.ResourceOldVersion) && !string.IsNullOrEmpty(UpdateChecker.ResourceUpdateVersion))
        {
            UpdateMessage += $"\n资源更新：v{UpdateChecker.ResourceOldVersion} => v{UpdateChecker.ResourceUpdateVersion}";
        }
        if (!string.IsNullOrEmpty(UpdateChecker.TMPUpdateVersion) && !string.IsNullOrEmpty(UpdateChecker.TMPOldVersion))
        {
            UpdateMessage += $"\n字体更新：v{UpdateChecker.TMPOldVersion} => v{UpdateChecker.TMPUpdateVersion}";
        }
        if (!string.IsNullOrEmpty(UpdateChecker.HotUpdateMessage)){
            UpdateMessage += "\n更新提示：\n" + UpdateChecker.HotUpdateMessage;
        }
        OpenGlobalPopup(UpdateMessage, "模组更新完成");
    }

    #region 屏蔽没有意义的Warning

    [HarmonyPatch(typeof(ULogger), nameof(ULogger.Log), typeof(LogType), typeof(ILObject))]
    [HarmonyPrefix]
    private static bool Log(ULogger __instance, LogType logType, ILObject message)
    {
        if (logType != LogType.Warning) return true;
        var logString = ULogger.GetString(message);
        if (!logString.StartsWith("<color=#0099bc><b>DOTWEEN"))
            __instance.logHandler.LogFormat(logType, null, "{0}", logString);
        return false;
    }

    [HarmonyPatch(typeof(ULogger), nameof(ULogger.Log), typeof(LogType), typeof(ILObject), typeof(UObject))]
    [HarmonyPrefix]
    private static bool Log(ULogger __instance, LogType logType, ILObject message, UObject context)
    {
        if (logType != LogType.Warning) return true;
        var logString = ULogger.GetString(message);
        if (!logString.StartsWith("Material"))
            __instance.logHandler.LogFormat(logType, context, "{0}", logString);
        return false;
    }

    #endregion
}