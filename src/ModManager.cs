using Il2Cpp;
using Il2CppInterop.Runtime.Injection;
using Il2CppMainUI;
using Il2CppSystem.Collections.Generic;
using Il2CppTMPro;
using System;
using System.ComponentModel.DataAnnotations;
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

        public ModManager(IntPtr ptr)
            : base(ptr)
        {

        }

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

        public static void CreateLocalize()
        {
            if (!Directory.Exists(LimbusLocalizeMod.path + "/Localize/EN"))
                Directory.CreateDirectory(LimbusLocalizeMod.path + "/Localize/EN");
            if (!Directory.Exists(LimbusLocalizeMod.path + "/Localize/KR"))
                Directory.CreateDirectory(LimbusLocalizeMod.path + "/Localize/KR");
            if (!Directory.Exists(LimbusLocalizeMod.path + "/Localize/JP"))
                Directory.CreateDirectory(LimbusLocalizeMod.path + "/Localize/JP");
            foreach (TextAsset textAsset in Resources.LoadAll<TextAsset>("Localize/EN"))
            {
                File.WriteAllText(LimbusLocalizeMod.path + "/Localize/EN/" + textAsset.name + ".json", textAsset.text);
            }
            foreach (TextAsset textAsset in Resources.LoadAll<TextAsset>("Localize/KR"))
            {
                File.WriteAllText(LimbusLocalizeMod.path + "/Localize/KR/" + textAsset.name + ".json", textAsset.text);
            }
            foreach (TextAsset textAsset in Resources.LoadAll<TextAsset>("Localize/JP"))
            {
                File.WriteAllText(LimbusLocalizeMod.path + "/Localize/JP/" + textAsset.name + ".json", textAsset.text);
            }
            string NickName = Resources.Load<TextAsset>("Story/ScenarioModelCode").ToString();
            File.WriteAllText(LimbusLocalizeMod.path + "/Localize/NickName.json", NickName);

        }
        public static void MirrorDungeonGiveToString()
        {
            string MirrorDungeonGive = "Bright#1192 制作";
            MirrorDungeonStaticData dungeon = Singleton<StaticDataManager>.Instance.MirrorDungeonInfoList.GetDungeon(1);
            for (int i = 0; i < dungeon.floors.Count; i++)
            {
                var floor = dungeon.floors[i];
                for (int i2 = 0; i2 < floor.MirrorSector.Count; i2++)
                {
                    var Sector = floor.MirrorSector[i2];
                    var Reward = Sector.RewardList[^1];
                    MirrorDungeonGive += string.Format("\nFloor{0} Sector{1}:\n   加成:{2}\n   非加:{3}", i, i2, RewardToString(Reward.ChanceReward), RewardToString(Reward.NormalReward));

                }
            }
            File.WriteAllText(LimbusLocalizeMod.path + "/镜牢进度对应奖励一览.txt", MirrorDungeonGive);
        }
        public static string RewardToString(MirrorDungeonSectorRewardBaseData rewardBaseData)
        {
            int PASS_POINT = 0;
            string ITEM = "";
            foreach (var element in rewardBaseData.RewardList)
            {
                if (element.Type == ELEMENT_TYPE.BATTLEPASS_POINT)
                {
                    PASS_POINT += element.Num;
                }
                else if (element.Type == ELEMENT_TYPE.ITEM)
                {
                    ITEM += string.Format(" {0} {1}个", Singleton<StaticDataManager>.Instance.ItemList.GetData(element.Id).GetItemName(), element.Num);
                }
            }
            return string.Format("经验{0} 点数{1} 随机{2}自选{3} 物品{4}", rewardBaseData.acquireUserExp, PASS_POINT, rewardBaseData.randomPackagePiece, rewardBaseData.optionalPackagePiece, ITEM);
        }
    }
}
