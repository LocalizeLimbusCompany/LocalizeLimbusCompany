using System;
using System.IO;
using HarmonyLib;
using Il2CppSystem.Collections.Generic;
using MainUI;
using StorySystem;
using UnityEngine;

namespace LimbusLocalize.LLC;

public static class AprilFools
{
	public static void Init()
	{
		var readmeActions = ReadmeManager.ReadmeActions;
		readmeActions.Add("Action_AprilFools_Ten-Yi-Sang", () =>
		{
			ReadmeManager.Close();
			List<GachaLogDetail> list = new();
			for (var i = 0; i < 10; i++)
				list.Add(new GachaLogDetail(ELEMENT_TYPE.PERSONALITY, 10103)
				{
					ex = new Element(ELEMENT_TYPE.ITEM, 10101, 50)
				});

			UIPresenter.Controller.GetPanel(MAINUI_PANEL_TYPE.LOWER_CONTROL).Cast<LowerControlUIPanel>()
				.OnClickLowerControllButton(4);
			UIController.Instance.GetPresenter(MAINUI_PHASE_TYPE.Gacha).Cast<GachaUIPresenter>()
				.OpenGachaResultUI(list);
			GlobalGameManager.Instance.StartTutorialManager.ProgressTutorial();
		});
		readmeActions.Add("Action_AprilFools_Pretend-Get-Lunacy", () =>
		{
			List<Element> elements = new();
			elements.Add(new Element(ELEMENT_TYPE.ITEM, 2, 300));
			UIController.Instance.GetPopup(MAINUI_POPUP_TYPE.ELEMENTLIST).Cast<ElementListUIPopup>()
				.SetDataOpen("未获得", elements);
		});
		readmeActions.Add("Action_AprilFools_Show-Credit", Credits.ShowCredit);
	}

	public static class Credits
	{
		private const int SubChapterID = 107;
		private static CreditsData _copy;
		private static readonly Harmony Harmony = new(LLCMod.Name + nameof(Credits));

		public static void ShowCredit()
		{
			Harmony.PatchAll(typeof(Credits));
			var json = $$"""
			             {"id":{{SubChapterID}},"creditsDataList":[
			             """;
			foreach (var line in File.ReadAllLines(LLCMod.ModPath + "/Localize/Readme/Credits.md"))
				if (line.StartsWith("- "))
				{
					json += $$"""
					          {"type":2,"division":"{{line[2..]}}","name":"","title":""},
					          """;
				}
				else if (line.StartsWith("  - "))
				{
					var split = line[4..].Split('：');
					json += $$"""
					          {"type":3,"division":"","name":"{{split[0]}}","title":"{{split[^1]}}"},
					          """;
				}

			json = json[..^1] + "]}";

			var creditsDataDic = StaticDataManager.Instance.CreditsDataList._creditsDataDic;
			_copy = creditsDataDic[SubChapterID];
			creditsDataDic[SubChapterID] = JsonUtility.FromJson<CreditsData>(json);

			GlobalGameManager.Instance.LoadScene(SCENE_STATE.Story);
		}

		[HarmonyPatch(typeof(StorySceneManager), nameof(StorySceneManager.Start))]
		[HarmonyPrefix]
		private static bool StorySceneManagerStart(StorySceneManager __instance)
		{
			__instance._storyManager._creditCon.SetCredit(SubChapterID, (Action<bool>)End);
			return false;

			void End(bool f)
			{
				if (!f) return;
				GlobalGameManager.Instance.LoadScene(SCENE_STATE.Main);
				StaticDataManager.Instance.CreditsDataList._creditsDataDic[SubChapterID] = _copy;
				Harmony.UnpatchSelf();
			}
		}

		[HarmonyPatch(typeof(CreditsSlot), nameof(CreditsSlot.SetData))]
		[HarmonyPostfix]
		private static void SetData(CreditsSlot __instance)
		{
			var tmpFontAsset = ChineseFont.Tmpchinesefonts[0];
			__instance._subDivisionText.font = tmpFontAsset;
			__instance._nameText.font = tmpFontAsset;
			__instance._titleText.font = tmpFontAsset;
		}
	}
}