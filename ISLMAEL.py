# -*- coding: UTF-8 -*-
import io
import os
import sys
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
newtext = R"""private static void BossBattleStartInit(ActBossBattleStartUI __instance)
    {
        if (!IsUseChinese.Value)
            return;
        System.Collections.Generic.List<string> _loadingTexts;
        System.Collections.Generic.List<string> _loadingTextsTitles;
        _loadingTexts = [.. File.ReadAllLines(LLCMod.ModPath + "/Localize/Readme/BossBattleStartInitTexts.md")];
        _loadingTextsTitles = [.. File.ReadAllLines(LLCMod.ModPath + "/Localize/Readme/BossBattleStartInitTextsTitles.md")];
        var textGroup = __instance.transform.GetChild(2).GetChild(1);
        var tmp = textGroup.GetChild(1).GetComponentInChildren<TextMeshProUGUI>();
        if (_loadingTexts.Count == 0 || _loadingTextsTitles.Count == 0)
        {
            LLCMod.LogWarning("nothing in BossBattleStartInitTextsTitles.md or BossBattleStartInitTextsTitles.md,using default.");
            return;
        }
        if (!tmp.text.Equals("Proelium Fatale"))
            return;
        {
            int i = UnityEngine.Random.RandomRangeInt(0, _loadingTexts.Count);
            tmp.font = ChineseFont.Tmpchinesefonts[0];
            if (i > _loadingTextsTitles.Count - 1)
            {
                tmp.text = "<b>" + SelectOne(_loadingTextsTitles) + "</b>";
            }
            else
            {
                tmp.text = "<b>" + SelectOne(_loadingTextsTitles, i) + "</b>";
            }
            tmp = textGroup.GetChild(2).GetComponentInChildren<TextMeshProUGUI>();
            tmp.font = ChineseFont.Tmpchinesefonts[0];
            if (i > _loadingTexts.Count - 1)
            {
                tmp.text = "<b>" + SelectOne(_loadingTexts) + "</b>";
            }
            else
            {
                tmp.text = "<b>" + SelectOne(_loadingTexts, i) + "</b>";
            }
        }
    }
    public static T SelectOne<T>(System.Collections.Generic.List<T> list, int i = -1)
    {
        if (i != -1) return list[i];
        else
        {
            UnityEngine.Random.seed = (int)(Time.deltaTime + Time.timeSinceLevelLoad + DateTime.Today.Day + DateTime.Now.Minute);
            UnityEngine.Random.InitState((int)(Time.deltaTime + Time.timeSinceLevelLoad + DateTime.Today.Day + DateTime.Now.Minute));
            LLCMod.LogWarning((Time.deltaTime + Time.timeSinceLevelLoad + DateTime.Today.Day + DateTime.Now.Minute).ToString());
            return list.Count == 0 ? default : list[UnityEngine.Random.Range(0, list.Count)];
        }
    }
    public class LyricLine
    {
        [JsonPropertyName("from")]
        public double from { get; set; }

        [JsonPropertyName("to")]
        public double to { get; set; }

        [JsonPropertyName("content")]
        public string content { get; set; }
    }//anti replace 

    private static TextMeshProUGUI lyricText;
    private static List<LyricLine>  lyrics;
    private static bool inLoginScene = false;
    [HarmonyPatch(typeof(FMODUnity.RuntimeManager),
nameof(FMODUnity.RuntimeManager.PlayOneShot),
new[] { typeof(FMOD.GUID), typeof(Vector3) })]
    [HarmonyPrefix]
    static bool PlayOneShotPrefix(FMOD.GUID guid, Vector3 position)
    {
        LLCMod.LogInfo($"PlayOneShotPrefix guid");

        // RuntimeManager.PlayOneShot
        if (guid.IsNull) return true; // 继续执行原方法

        // 获取事件路径
        string eventPath;
        FMOD.Studio.EventDescription eventDescription;
        FMODUnity.RuntimeManager.StudioSystem.getEventByID(guid, out eventDescription);
        eventDescription.getPath(out eventPath);

        // 检测目标路径
        if (eventPath == "event:/BGM/TitleBgm")
        {
            // 替换为本地 MP3 文件
            PlayLocalMP3(position);
            return false; // 阻止原方法执行
        }
        LLCMod.LogInfo($"[FMOD] 播放: {eventPath}");
        return true; // 继续执行原方法

    }
    static void PlayLocalMP3(Vector3 position)
    {
        string filePath = Path.Combine(LLCMod.ModPath, "Localize/TitleBgm.mp3");

        try
        {
            FMOD.Sound sound;
            // FMOD.Channel channel;

            // 创建声音并播放
            FMOD.RESULT rs = FMODUnity.RuntimeManager.CoreSystem.createSound(filePath, FMOD.MODE.LOOP_NORMAL, out sound);
            if (rs != FMOD.RESULT.OK)
            {
                LLCMod.LogError($"创建声音失败: {rs}");
                return;
            }
            FMODUnity.RuntimeManager.CoreSystem.playSound(sound, default, false, out channel);
            // 设置 3D 位置（如果需要）
            FMOD.VECTOR pos = new FMOD.VECTOR
            {
                x = position.x,
                y = position.y,
                z = position.z
            };
            FMOD.VECTOR vel = new FMOD.VECTOR { x = 0, y = 0, z = 0 }; // 速度设置为 0
            channel.set3DAttributes(ref pos, ref vel);


            LLCMod.LogInfo($"[FMOD] enjoy it {filePath} !");
        }
        catch (Exception e)
        {
            LLCMod.LogError($"播放本地文件失败: {e.Message}");
        }
    }
    [HarmonyPatch(typeof(FMODUnity.RuntimeManager), nameof(FMODUnity.RuntimeManager.CreateInstance), new[] { typeof(string) })]
    [HarmonyPrefix]
    static bool CreateInstancePrefix(string path, ref FMOD.Studio.EventInstance __result)
    {
        if (path == "event:/BGM/TitleBgm")
        {
            PlayLocalMP3(Vector3.zero);
            __result = default; // 返回空实例
            return false; // 阻止原方法执行
        }

        // 其他事件正常记录
        LLCMod.LogInfo($"[FMOD] CreateInstance: {path}");
        return true;
    }
    [HarmonyPatch(typeof(SceneManager), "Internal_SceneLoaded")]
    [HarmonyPostfix]
    public static void Postfix(Scene scene, LoadSceneMode mode)
    {
        try
        {
            if (scene.name == "LoginScene")
            {
                Canvas canvas = GameObject.FindObjectOfType<Canvas>();
                if (canvas == null)
                {
                    GameObject canvasObject = new GameObject("Canvas");
                    canvas = canvasObject.AddComponent<Canvas>();
                    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    canvasObject.AddComponent<CanvasScaler>();
                    canvasObject.AddComponent<GraphicRaycaster>();
                }

                // 创建一个新的TextMeshProUGUI对象
                GameObject textObject = new GameObject("LyricText");
                lyricText = textObject.AddComponent<TextMeshProUGUI>();

                // 设置父对象为Canvas
                textObject.transform.SetParent(canvas.transform, false);

                // 设置锚点和轴心点以确保文字始终居中
                lyricText.rectTransform.anchorMin = new Vector2(0.5f, 0.9f);
                lyricText.rectTransform.anchorMax = new Vector2(0.5f, 0.95f);
                lyricText.rectTransform.pivot = new Vector2(0.5f, 0);
                lyricText.rectTransform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
                // 设置文本的位置
                lyricText.rectTransform.anchoredPosition = new Vector2(0, 10);
                lyricText.rectTransform.sizeDelta = new Vector2(10000, 65.5f);  // 这里高度可以根据需要调整

                lyricText.font = ChineseFont.Tmpchinesefonts[0];
                lyricText.fontStyle = FontStyles.Italic;
                lyricText.fontSize = 40f;

                // 设置文本对齐方式
                lyricText.alignment = TextAlignmentOptions.Center;
                if (lyrics == null)
                {
                    json = File.ReadAllText(Path.Combine(LLCMod.ModPath, "Localize/lyrics.json"), System.Text.Encoding.UTF8);         
                    lyrics = System.Text.Json.JsonSerializer.Deserialize<List<LyricLine>>(json);
                }
                StartSinging();
            }
            else if (scene.name != "LogoScene")
            {
                StopSinging();
            }
        }
        catch (Exception ex)
        {
            LLCMod.LogError($"Error in BGMPostfix: {ex}");
        }
    }
    public static void StartSinging()
    {
        string json = File.ReadAllText(LLCMod.ModPath + "\\Localize\\lyrics.json");
        if (!inLoginScene)
        {
            inLoginScene = true;
            new Thread((ThreadStart)UpdateLyrics).Start();
        }
    }
    private static void UpdateLyrics()
    {
        while (inLoginScene)
        {
            uint timeMs;
            channel.getPosition(out timeMs, FMOD.TIMEUNIT.MS);
            double currentTime = (double)timeMs / 1000.0; // 转换为秒
            if (lyrics == null) return;

            foreach (var lyric in lyrics)
            {
                if (currentTime >= (double)lyric.from && currentTime < (double)lyric.to)
                {
                    // 使用RichText来支持颜色
                    lyricText.text = $"{lyric.content}";
                    break;
                } else {
                    lyricText.text = "";
                }
            }
            Thread.Sleep(50); // 控制刷新率
        }
    }
    public static void StopSinging()
    {
        inLoginScene = false;
        channel.stop();
        lyricText.text = "";
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
using Object = UnityEngine.Object;

namespace LimbusLocalize.LLC;

public static class ChineseSetting
{"""
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
using UnityEngine.SceneManagement;
using Il2CppSystem.Threading;
using System.Collections.Generic;
using System.Text.Json.Serialization;
//anti replace 

namespace LimbusLocalize.LLC;

public static class ChineseSetting
{
    static FMOD.Channel channel = new FMOD.Channel();
    public static string  json = "";
"""
oldInittext= R"""if (!_chineseSetting)
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
newInittext=R"""if (!_chineseSetting)
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
csprojold = """        <PackageReference Include="HarmonyX" Version="2.5.2" IncludeAssets="compile"/>
        <PackageReference Include="Il2CppInterop.Runtime" Version="1.0.0"/>
        <Reference Include="Assembly-CSharp">
            <HintPath>..\\lib\\Assembly-CSharp.dll</HintPath>
        </Reference>"""
csprojnew = """        <PackageReference Include="HarmonyX" Version="2.5.2" IncludeAssets="compile" />
        <PackageReference Include="Il2CppInterop.Runtime" Version="1.0.0" />
        <Reference Include="Assembly-CSharp">
            <HintPath>..\\lib\\Assembly-CSharp.dll</HintPath>
        </Reference>
        <Reference Include="FMODUnity">
          <HintPath>..\\lib\\FMODUnity.dll</HintPath>
        </Reference>
        <Reference Include="FMODUnityResonance">
          <HintPath>.\\lib\\FMODUnityResonance.dll</HintPath>
        </Reference>"""
texts = []
text = ''
filePath = "./Plugin/LLC/ChineseSetting.cs"
filePath2 = "./build.ps1"
#运行更新(Localize)
os.system("git remote add upstream https://github.com/LocalizeLimbusCompany/LocalizeLimbusCompany.git")
os.system("git fetch upstream")
os.system("git checkout main")
os.system("git checkout --ours .github/workflows/release.yml")
os.system("git checkout --ours ISLMAEL.py")

os.system("git add .github/workflows/release.yml")
os.system("git add Plugin/ChineseFont.cs") 
os.system("git add Plugin/LLC/ChineseSetting.cs")
os.system("git add Plugin/LimbusLocalize.csproj")
os.system("git add build.ps1")

os.system("git merge upstream/main --allow-unrelated-histories")
os.system("git checkout upstream/main .")
os.system("git add .")
os.system("git commit -m 'Sync with upstream repository'")
import shutil
shutil.rmtree("./Localize")
os.system("git clone https://github.com/LocalizeLimbusCompany/LLC_Release ./Localize")
os.system('copy Boss* .\\Localize\\Readme') 
os.system("""git add ./Localize""")
os.system("""git add .""")
os.system(""" git commit -m "更新 Localize 子模块到最新版本" """)

os.system("""copy .\\TitleBgm.mp3 .\\Localize\\TitleBgm.mp3""")
os.system("""copy .\\lyrics.json .\\Localize\\lyrics.json""")

os.system("""copy .\\FMODUnity.dll .\\lib""")
os.system("""copy .\\FMODUnityResonance.dll .\\lib""")

os.system("""chcp 65001""")

sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')

buildfilePath = "./build.ps1"
mainfilePath = "./Plugin/LLC/ChineseSetting.cs"
csfilePath = "./Plugin/LimbusLocalize.csproj"


with open(mainfilePath,"r+",encoding='utf-8') as file:
    texts = file.readlines()
    text = ''
    for n in texts:
        text += n
    text = text.replace(oldtext,newtext)
    text = text.replace(oldusingtext,newusingtext)
    text = text.replace(oldInittext,newInittext)
with open(mainfilePath,"w",encoding='utf-8') as file:
    file.write(text)

with open(csfilePath,"r+",encoding='utf-8') as file:
    texts = file.readlines()
    text = ''
    for n in texts:
        text += n
    text = text.replace(csprojold,csprojnew)
with open(csfilePath,"w",encoding='utf-8') as file:
    file.write(text)


with open(buildfilePath,"r+",encoding='utf-8') as file:
    texts = file.readlines()
    text = ''
    for n in texts:
        text += n
    text = text.replace("""Copy-Item -Path Localize/CN $BIE_LLC_Path/Localize -Force -Recurse
Copy-Item -Path Localize/Readme $BIE_LLC_Path/Localize -Force -Recurse
if ($version)
	{
	 Set-Location "$Path"
	 7z a -t7z "./LimbusLocalize_BIE_$version.7z" "BepInEx/" -mx=9 -ms
	}""","""Copy-Item -Path Localize/CN $BIE_LLC_Path/Localize -Force -Recurse
Copy-Item -Path Localize/Readme $BIE_LLC_Path/Localize -Force -Recurse
Copy-Item -Path .\\TitleBgm.mp3 $BIE_LLC_Path/Localize -Force -Recurse
Copy-Item -Path .\\lyrics.json $BIE_LLC_Path/Localize -Force -Recurse
if ($version)
	{
	 Set-Location "$Path"
	 ..\\Patcher\\7z.exe a -t7z "./LimbusLocalize_BIE_$version.7z" "BepInEx/" -mx=9 -ms
	}""")
with open(buildfilePath,"w",encoding='utf-8') as file:
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
with open("Localize\\Readme\\Readme.json", "r+", encoding="utf-8") as f:
    data = json.load(f)
    data["noticeList"].append(j)
with open("Localize\\Readme\\Readme.json", "w", encoding="utf-8") as f:
        json.dump(data, f, ensure_ascii=False, indent=4)
# os.system("""git pull origin main""")
# os.system("""git push origin main -f""")