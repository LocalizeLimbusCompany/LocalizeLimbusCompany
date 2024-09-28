using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using MainUI;
using MainUI.Gacha;
using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using ILObject = Il2CppSystem.Object;
using UObject = UnityEngine.Object;

namespace LimbusLocalize
{
    public class LLC_Manager(IntPtr ptr) : MonoBehaviour(ptr)
    {
        static LLC_Manager()
        {
            ClassInjector.RegisterTypeInIl2Cpp<LLC_Manager>();
            GameObject obj = new(nameof(LLC_Manager));
            DontDestroyOnLoad(obj);
            obj.hideFlags |= HideFlags.HideAndDontSave;
            Instance = obj.AddComponent<LLC_Manager>();
        }
        public static LLC_Manager Instance;

        void OnApplicationQuit() => LCB_LLCMod.CopyLog();
        public static void OpenGlobalPopup(string description, string title = null, string close = "取消", string confirm = "确认", Action confirmEvent = null, Action closeEvent = null)
        {
            if (!GlobalGameManager.Instance) { return; }
            TextOkUIPopup globalPopupUI = GlobalGameManager.Instance.globalPopupUI;
            TMP_FontAsset fontAsset = LCB_Chinese_Font.tmpchinesefonts[0];
            if (fontAsset)
            {
                TextMeshProUGUI btn_canceltmp = globalPopupUI.btn_cancel.GetComponentInChildren<TextMeshProUGUI>(true);
                btn_canceltmp.font = fontAsset;
                btn_canceltmp.fontMaterial = fontAsset.material;
                UITextDataLoader btn_canceltl = globalPopupUI.btn_cancel.GetComponentInChildren<UITextDataLoader>(true);
                btn_canceltl.enabled = false;
                btn_canceltmp.text = close;
                TextMeshProUGUI btn_oktmp = globalPopupUI.btn_ok.GetComponentInChildren<TextMeshProUGUI>(true);
                btn_oktmp.font = fontAsset;
                btn_oktmp.fontMaterial = fontAsset.material;
                UITextDataLoader btn_oktl = globalPopupUI.btn_ok.GetComponentInChildren<UITextDataLoader>(true);
                btn_oktl.enabled = false;
                btn_oktmp.text = confirm;
                globalPopupUI.tmp_title.font = fontAsset;
                globalPopupUI.tmp_title.fontMaterial = fontAsset.material;
                void TextLoaderEnabled() { btn_canceltl.enabled = true; btn_oktl.enabled = true; }
                confirmEvent += TextLoaderEnabled;
                closeEvent += TextLoaderEnabled;
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
            foreach (FileInfo fileInfo in directory.GetFiles())
            {
                var value = File.ReadAllText(fileInfo.FullName);
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileInfo.FullName);
                Localizes[fileNameWithoutExtension] = value;
            }
            foreach (DirectoryInfo directoryInfo in directory.GetDirectories())
            {
                InitLocalizes(directoryInfo);
            }

        }
        public static Dictionary<string, string> Localizes = [];
        public static Action FatalErrorAction;
        public static string FatalErrorlog;
        #region 屏蔽没有意义的Warning
        [HarmonyPatch(typeof(Logger), nameof(Logger.Log),
        [
            typeof(LogType),
            typeof(ILObject)
        ])]
        [HarmonyPrefix]
        private static bool Log(Logger __instance, LogType logType, ILObject message)
        {
            if (logType == LogType.Warning)
            {
                string LogString = Logger.GetString(message);
                if (!LogString.StartsWith("<color=#0099bc><b>DOTWEEN"))
                    __instance.logHandler.LogFormat(logType, null, "{0}", LogString);
                return false;
            }
            return true;
        }
        [HarmonyPatch(typeof(Logger), nameof(Logger.Log),
        [
            typeof(LogType),
            typeof(ILObject),
            typeof(UObject)
        ])]
        [HarmonyPrefix]
        private static bool Log(Logger __instance, LogType logType, ILObject message, UObject context)
        {
            if (logType == LogType.Warning)
            {
                string LogString = Logger.GetString(message);
                if (!LogString.StartsWith("Material"))
                    __instance.logHandler.LogFormat(logType, context, "{0}", LogString);
                return false;
            }
            return true;
        }
        #endregion
        #region 修复一些弱智东西
        [HarmonyPatch(typeof(GachaEffectEventSystem), nameof(GachaEffectEventSystem.LinkToCrackPosition))]
        [HarmonyPrefix]
        private static bool LinkToCrackPosition(GachaEffectEventSystem __instance)
            => __instance._parent.EffectChainCamera;
        #endregion
        [HarmonyPatch(typeof(LoginSceneManager), nameof(LoginSceneManager.SetLoginInfo))]
        [HarmonyPostfix]
        public static void CheckModActions()
        {
            if (LLC_UpdateChecker.isUpdate)
            {
                LCB_LLCMod.LogInfo("Here is a update.");
                OpenGlobalPopup($"已更新新版本文本。\n文本版本号：{LLC_UpdateChecker.nowTextVersion}。", "更新完成");
            }
            else
            {
                LCB_LLCMod.LogInfo("No update.");
            }
        }
    }
}
