using System;
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

    public static ConfigEntry<bool> IsUseChinese =
        LLCMod.LLCSettings.Bind("LLC Settings", "IsUseChinese", true, "是否使用汉化 ( true | false )");

    public static ConfigEntry<bool> ShowDialog = LLCMod.LLCSettings.Bind("LLC Settings", "ShowDialog", false,
        "将罪人在战斗内的语音文本翻译以头顶气泡的形式呈现 ( true | false )");

    private static bool _isusechinese;
    private static Toggle _chineseSetting;
    private static BattleUnitView _unitView;

    [HarmonyPatch(typeof(SettingsPanelGame), nameof(SettingsPanelGame.InitLanguage))]
    [HarmonyPrefix]
    private static bool InitLanguage(SettingsPanelGame __instance, LocalGameOptionData option)
    {
        if (!_chineseSetting)
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
        }

        foreach (var tg in __instance._languageToggles)
        {
            tg.onValueChanged.RemoveAllListeners();

            tg.onValueChanged.AddListener((Action<bool>)OnValueChanged);
            tg.SetIsOnWithoutNotify(false);
            continue;

            void OnValueChanged(bool isOn)
            {
                if (!isOn) return;
                __instance.OnClickLanguageToggleEx(__instance._languageToggles.IndexOf(tg));
            }
        }

        var language = option.GetLanguage();
        if (_isusechinese = IsUseChinese.Value)
            _chineseSetting.SetIsOnWithoutNotify(true);
        else
            switch (language)
            {
                case LOCALIZE_LANGUAGE.KR:
                    __instance._languageToggles[0].SetIsOnWithoutNotify(true);
                    break;
                case LOCALIZE_LANGUAGE.EN:
                    __instance._languageToggles[1].SetIsOnWithoutNotify(true);
                    break;
                case LOCALIZE_LANGUAGE.JP:
                    __instance._languageToggles[2].SetIsOnWithoutNotify(true);
                    break;
            }

        __instance._lang = language;
        return false;
    }

    [HarmonyPatch(typeof(SettingsPanelGame), nameof(SettingsPanelGame.ApplySetting))]
    [HarmonyPostfix]
    private static void ApplySetting()
    {
        IsUseChinese.Value = _isusechinese;
    }

    private static void OnClickLanguageToggleEx(this SettingsPanelGame instance, int tgIdx)
    {
        if (tgIdx == 3)
        {
            _isusechinese = true;
            return;
        }

        _isusechinese = false;
        instance._lang = tgIdx switch
        {
            0 => LOCALIZE_LANGUAGE.KR,
            1 => LOCALIZE_LANGUAGE.EN,
            2 => LOCALIZE_LANGUAGE.JP,
            _ => instance._lang
        };
    }

    [HarmonyPatch(typeof(DateUtil), nameof(DateUtil.TimeZoneOffset), MethodType.Getter)]
    [HarmonyPrefix]
    private static bool TimeZoneOffset(ref int __result)
    {
        if (!IsUseChinese.Value)
            return true;
        __result = 8;
        return false;
    }

    [HarmonyPatch(typeof(DateUtil), nameof(DateUtil.TimeZoneString), MethodType.Getter)]
    [HarmonyPrefix]
    private static bool TimeZoneString(ref string __result)
    {
        if (!IsUseChinese.Value)
            return true;
        __result = "CST";
        return false;
    }

    [HarmonyPatch(typeof(PriceText), nameof(PriceText.SetPrice))]
    [HarmonyPrefix]
    private static bool SetPrice(PriceText __instance, IAPProductStaticData productStaticData)
    {
        if (!IsUseChinese.Value)
            return true;
        __instance.tmp_unit.text = "CNY";
        var priceTier = StaticDataManager.Instance._iapProductStaticDataList
            .GetDataByProductID(productStaticData.productId).priceTier;
        var usdCent = StaticDataManager.Instance._iapPriceTierStaticDataList.list
            .Find((Func<IAPPriceTierStaticData, bool>)(price => price.PriceTier == priceTier)).usd_cent;
        __instance.tmp_price.text = ((usdCent + 1) * 7 / 100).ToString();
        return false;
    }

    [HarmonyPatch(typeof(BattleUnitView), nameof(BattleUnitView.ViewCancelTextTypo_Lack))]
    [HarmonyPrefix]
    private static bool ViewCancelTextTypo_Lack(BattleUnitView __instance, CanceledData data)
    {
        if (!IsUseChinese.Value)
            return true;
        if (data?._lackOfBuffs?.Count > 0)
            __instance.UIManager.bufTypoUI.OpenBufTypo(BUF_TYPE.Negative,
                Singleton<TextDataSet>.Instance.BufList.GetData(data._lackOfBuffs[0].ToString()).GetName() + " 不足",
                data._lackOfBuffs[0]);
        return false;
    }

    [HarmonyPatch(typeof(StoryPlayData), nameof(StoryPlayData.GetDialogAfterClearingAllCathy))]
    [HarmonyPrefix]
    private static bool GetDialogAfterClearingAllCathy(Scenario curStory, Dialog dialog, ref string __result)
    {
        if (!IsUseChinese.Value)
            return true;
        __result = dialog.Content;
        var instance = UserDataManager.Instance;
        if ("P10704".Equals(curStory.ID) && instance?._unlockCodeData != null &&
            instance._unlockCodeData.CheckUnlockStatus(106) &&
            dialog.Id == 3)
            __result = __result.Replace("凯茜", "■■■■■");
        return false;
    }

    [HarmonyPatch(typeof(Util), nameof(Util.GetDlgAfterClearingAllCathy))]
    [HarmonyPrefix]
    private static bool GetDlgAfterClearingAllCathy(string dlgId, string originString, ref string __result)
    {
        if (!IsUseChinese.Value)
            return true;
        __result = originString;
        var instance = UserDataManager.Instance;
        if (instance?._unlockCodeData == null || !instance._unlockCodeData.CheckUnlockStatus(106))
            return false;
        __result = dlgId switch
        {
            "battle_defeat_10707_1" => __result.Replace("凯茜", "■■■■■"),
            "battle_dead_10704_1" => __result.Replace("凯瑟琳", "■■■■■"),
            _ => __result
        };
        return false;
    }

    [HarmonyPatch(typeof(UnitInfoBreakSectionTooltipUI), nameof(UnitInfoBreakSectionTooltipUI.SetDataAndOpen))]
    [HarmonyPostfix]
    private static void SetDataAndOpen(UnitInfoBreakSectionTooltipUI __instance)
    {
        if (!IsUseChinese.Value)
            return;
        __instance.tmp_tooltipContent.font = ChineseFont.Tmpchinesefonts[0];
        __instance.tmp_tooltipContent.fontSize = 35f;
    }

    [HarmonyPatch(typeof(ActBossBattleStartUI), nameof(ActBossBattleStartUI.Init))]
    [HarmonyPostfix]
    private static void BossBattleStartInit(ActBossBattleStartUI __instance)
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
    }

    [HarmonyPatch(typeof(VoiceGenerator), nameof(VoiceGenerator.CreateVoiceInstance))]
    [HarmonyPostfix]
    [HarmonyDebug]
    private static void CreateVoiceInstance(string path, bool isSpecial)
    {
        if (!ShowDialog.Value || !_unitView || !path.StartsWith(VoiceGenerator.VOICE_EVENT_PATH + "battle_"))
            return;
        path = path[VoiceGenerator.VOICE_EVENT_PATH.Length..];
        if (!Singleton<TextDataSet>.Instance.personalityVoiceText._voiceDictionary.TryGetValue(path.Split('_')[^2],
                out var dataList)) return;
        foreach (var data in dataList.dataList)
            if (path.Equals(data.id))
            {
                _unitView._uiManager.ShowDialog(new BattleDialogLine(data.dlg, null));
                break;
            }
    }

    [HarmonyPatch(typeof(BattleUnitView), nameof(BattleUnitView.SetPlayVoice))]
    [HarmonyPrefix]
    private static void BattleUnitView_Func(BattleUnitView __instance, BattleCharacterVoiceType key, bool isSpecial,
        BattleSkillViewer skillViewer)
    {
        _unitView = __instance;
    }
}