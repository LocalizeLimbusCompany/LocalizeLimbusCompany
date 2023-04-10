using Il2Cpp;
using Il2CppInterop.Runtime.Injection;
using Il2CppMainUI;
using Il2CppTMPro;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace LimbusLocalize
{
    public class ModManager : MonoBehaviour
    {
        internal static void Setup()
        {
            ClassInjector.RegisterTypeInIl2Cpp<ModManager>();
            GameObject obj = new("ModManager");
            DontDestroyOnLoad(obj);
            obj.hideFlags |= HideFlags.HideAndDontSave;
            Instance = obj.AddComponent<ModManager>();
        }
        public static ModManager Instance;

        public ModManager(IntPtr ptr) : base(ptr) { }

        public static void OpenGlobalPopup(string description, string title = "", string close = "取消", string confirm = "确认", DelegateEvent confirmEvent = null, DelegateEvent closeEvent = null)
        {
            TextOkUIPopup globalPopupUI = GlobalGameManager.Instance.globalPopupUI;
            TMP_FontAsset fontAsset = LimbusLocalizeMod.tmpchinesefont;
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
            globalPopupUI._titleObject.SetActive(!string.IsNullOrEmpty(title));
            globalPopupUI.tmp_title.text = title;
            globalPopupUI.tmp_description.text = description;
            Action _onconfirm = delegate () { confirmEvent?.Invoke(); btn_canceltl.enabled = true; btn_oktl.enabled = true; };
            globalPopupUI._confirmEvent = _onconfirm;
            Action _onclose = delegate () { closeEvent?.Invoke(); btn_canceltl.enabled = true; btn_oktl.enabled = true; };
            globalPopupUI._closeEvent = _onclose;
            globalPopupUI.btn_cancel.gameObject.SetActive(!string.IsNullOrEmpty(close));
            globalPopupUI._gridLayoutGroup.cellSize = new Vector2(!string.IsNullOrEmpty(close) ? 500 : 700, 100f);
            globalPopupUI.Open();
        }
        public static void InitLocalizes(DirectoryInfo directory)
        {
            foreach (FileInfo fileInfo in directory.GetFiles())
            {
                var value = File.ReadAllText(fileInfo.FullName);
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileInfo.FullName).Remove(0,3);
                Localizes[fileNameWithoutExtension] = value;
            }
            foreach (DirectoryInfo directoryInfo in directory.GetDirectories())
            {
                InitLocalizes(directoryInfo);
            }

        }
        public static Dictionary<string, string> Localizes = new();
    }
}
