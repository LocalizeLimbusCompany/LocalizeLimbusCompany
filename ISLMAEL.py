# -*- coding: UTF-8 -*-
import os
import time


oldtext = """private static void BossBattleStartInit(ActBossBattleStartUI __instance)
    {
        if (!IsUseChinese.Value)
            return;
        var textGroup = __instance.transform.GetChild(2).GetChild(1);
        var tmp = textGroup.GetChild(1).GetComponentInChildren<TextMeshProUGUI>();
        if (!tmp.text.Equals("Proelium Fatale"))
            return;
        tmp.font = ChineseFont.Tmpchinesefonts[0];
        tmp.text = "<b>命定之战</b>";
        tmp = textGroup.GetChild(2).GetComponentInChildren<TextMeshProUGUI>();
        tmp.font = ChineseFont.Tmpchinesefonts[0];
        tmp.text = "凡跨入此门之人，当放弃一切希望";
    }"""
newtext = """private static void BossBattleStartInit(ActBossBattleStartUI __instance)
    {
        if (!IsUseChinese.Value)
            return;
        System.Collections.Generic.List<string> _loadingTexts;
        System.Collections.Generic.List<string> _loadingTextsTitles;
        _loadingTexts = [.. File.ReadAllLines(LLCMod.ModPath + "/Localize/Readme/BossBattleStartInitTexts.md")];
        _loadingTextsTitles = [.. File.ReadAllLines(LLCMod.ModPath + "/Localize/Readme/BossBattleStartInitTextsTitles.md")];
        var textGroup = __instance.transform.GetChild(2).GetChild(1);
        var tmp = textGroup.GetChild(1).GetComponentInChildren<TextMeshProUGUI>();
        if (_loadingTexts.Count == 0|| _loadingTextsTitles.Count == 0){
            LLCMod.LogWarning("nothing in BossBattleStartInitTextsTitles.md or BossBattleStartInitTextsTitles.md,using default.");
            return;
        }
        if (!tmp.text.Equals("Proelium Fatale"))
            return;
        {
            int i = UnityEngine.Random.RandomRangeInt(0,_loadingTexts.Count);
            tmp.font = ChineseFont.Tmpchinesefonts[0];
            if(i>_loadingTextsTitles.Count-1){
                tmp.text = "<b>"+SelectOne(_loadingTextsTitles)+"</b>";
            } else{
                tmp.text = "<b>"+SelectOne(_loadingTextsTitles,i)+"</b>";
            }
            tmp = textGroup.GetChild(2).GetComponentInChildren<TextMeshProUGUI>();
            tmp.font = ChineseFont.Tmpchinesefonts[0];
            if(i>_loadingTexts.Count-1){
                tmp.text = "<b>"+SelectOne(_loadingTexts)+"</b>";
            } else{
                tmp.text = "<b>"+SelectOne(_loadingTexts,i)+"</b>";
            }
        }
    }
    public static T SelectOne<T>(System.Collections.Generic.List<T> list,int i = -1){
        if (i != -1) return list[i]; else {
            UnityEngine.Random.seed = (int)(Time.deltaTime+ Time.timeSinceLevelLoad + DateTime.Today.Day + DateTime.Now.Minute);
            UnityEngine.Random.InitState((int)(Time.deltaTime+ Time.timeSinceLevelLoad + DateTime.Today.Day + DateTime.Now.Minute));
            LLCMod.LogWarning((Time.deltaTime+ Time.timeSinceLevelLoad + DateTime.Today.Day + DateTime.Now.Minute).ToString());
            return list.Count == 0 ? default : list[UnityEngine.Random.Range(0, list.Count)];
            }
        }"""
oldusingtext = """using System;
using BattleUI.Dialog;
using BattleUI.Typo;
using BepInEx.Configuration;
using HarmonyLib;
using LocalSave;
using MainUI;
using StorySystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Voice;
using Object = UnityEngine.Object;"""
newusingtext = """using System;
using BattleUI.Dialog;
using BattleUI.Typo;
using BepInEx.Configuration;
using HarmonyLib;
using LocalSave;
using MainUI;
using StorySystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Voice;
using Object = UnityEngine.Object;
//new add
using System.IO;
using System.Collections.Generic;
using Il2CppSystem.Collections.Generic;
"""
oldInittext= """if (!_chineseSetting)
        {
            Toggle original = __instance._languageToggles[0];
            var parent = original.transform.parent;
            var languageToggle = Object.Instantiate(original, parent);
            var cntmp = languageToggle.GetComponentInChildren<TextMeshProUGUI>(true);
            cntmp.font = ChineseFont.Tmpchinesefonts[0];
            cntmp.fontMaterial = ChineseFont.Tmpchinesefonts[0].material;
            cntmp.text = "中文";
            _chineseSetting = languageToggle;
            parent.localPosition =
                new Vector3(parent.localPosition.x - 306f, parent.localPosition.y, parent.localPosition.z);
            while (__instance._languageToggles.Count > 3)
                __instance._languageToggles.RemoveAt(__instance._languageToggles.Count - 1);
            __instance._languageToggles.Add(languageToggle);
        }"""
newInittext="""if (!_chineseSetting)
        {
            Toggle original = __instance._languageToggles[0];
            var parent = original.transform.parent;
            var languageToggle = Object.Instantiate(original, parent);
            var cntmp = languageToggle.GetComponentInChildren<TextMeshProUGUI>(true);
            cntmp.font = ChineseFont.Tmpchinesefonts[0];
            cntmp.fontMaterial = ChineseFont.Tmpchinesefonts[0].material;
            cntmp.text = "中文";
            _chineseSetting = languageToggle;
            parent.localPosition =
                new Vector3(parent.localPosition.x - 306f, parent.localPosition.y, parent.localPosition.z);
            while (__instance._languageToggles.Count > 3)
                __instance._languageToggles.RemoveAt(__instance._languageToggles.Count - 1);
            __instance._languageToggles.Add(languageToggle);
            //Heathcliff Fools
            var readmeActions = ReadmeManager.ReadmeActions;
            readmeActions.Add("Action_AprilFools_Ten-Heathcliff", () =>
            {
                ReadmeManager.Close();
                Il2CppSystem.Collections.Generic.List<GachaLogDetail> list = new();
                for (var i = 0; i < 10; i++)
                    list.Add(new GachaLogDetail(ELEMENT_TYPE.PERSONALITY, 10705)
                    {
                        ex = new Element(ELEMENT_TYPE.ITEM, 10701, 50)
                    });

                UIPresenter.Controller.GetPanel(MAINUI_PANEL_TYPE.LOWER_CONTROL).Cast<LowerControlUIPanel>()
                    .OnClickLowerControllButton(4);
                UIController.Instance.GetPresenter(MAINUI_PHASE_TYPE.Gacha).Cast<GachaUIPresenter>()
                    .OpenGachaResultUI(list);
                GlobalGameManager.Instance.StartTutorialManager.ProgressTutorial();
            });
        }"""
texts = []
text = ''
filePath = "./Plugin/LLC/ChineseSetting.cs"
filePath2 = "./build.ps1"
#运行更新(Localize)
os.system("git remote add upstream https://github.com/LocalizeLimbusCompany/LocalizeLimbusCompany.git")
os.system("git fetch upstream")
os.system("git checkout main")
os.system("git merge upstream/main --allow-unrelated-histories")
os.system("git checkout upstream/main .")
os.system("git add .")
os.system("git commit -m 'Sync with upstream repository'")
os.removedirs("./Localize")
os.system("git clone https://github.com/LocalizeLimbusCompany/LLC_Release ./Localize")
os.system('copy Boss* .\Localize\Readme') 
os.system("""git add ./Localize""")
os.system("""git add .""")
os.system(""" git commit -m "更新 Localize 子模块到最新版本" """)


with open(filePath,"r+",encoding='utf-8') as file:
    texts = file.readlines()
    text = ''
    for n in texts:
        text += n
    text = text.replace(oldtext,newtext)
    text = text.replace(oldusingtext,newusingtext)
    text = text.replace(oldInittext,newInittext)
with open(filePath,"w",encoding='utf-8') as file:
    file.write(text)

with open(filePath2,"r+",encoding='utf-8') as file:
    texts = file.readlines()
    text = ''
    for n in texts:
        text += n
    text = text.replace("7z a","..\Patcher\7z.exe a")
with open(filePath2,"w",encoding='utf-8') as file:
    file.write(text)
# -*- coding: UTF-8 -*-
import json
j = {
      "id": 1191,
      "version": 0,
      "type": 1,
      "startDate": "2023-03-20T00:00:00.000Z",
      "endDate": "2098-12-31T23:00:00.000Z",
      "sprNameList": [],
      "title_KR": "伞神我们敬佩你啊!",
      "content_KR": "{\"list\":[{\"formatKey\":\"SubTitle\",\"formatValue\":\"伞神我们敬佩你啊\"},{\"formatKey\":\"HyperLink\",\"formatValue\":\"<link=Action_AprilFools_Ten-Heathcliff>点我</link>\"}]}"}
with open("Localize\Readme\Readme.json", "r+", encoding="utf-8") as f:
    data = json.load(f)
    data["noticeList"].append(j)
with open("Localize\Readme\Readme.json", "w", encoding="utf-8") as f:
        json.dump(data, f, ensure_ascii=False, indent=4)
os.system("""git pull origin main""")
os.system("""git push origin main""")
output_file = "build_" + time.strftime("%Y-%m-%d-%H-%M-%S", time.localtime()) + ".7z"
os.system(f"""powershell .\\build.ps1 {output_file}""")
with open(os.environ['GITHUB_OUTPUT'], 'a') as fh:
    print(f'output_file={output_file}', file=fh)