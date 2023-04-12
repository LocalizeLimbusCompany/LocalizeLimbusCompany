using HarmonyLib;
using Il2Cpp;
using Il2CppDungeon.Mirror;
using Il2CppDungeon.Mirror.Data;
using Il2CppMainUI.Gacha;
using Il2CppSystem.Collections.Generic;
using Il2CppUI.Utility;
using System;
using UnityEngine;
using ILObject = Il2CppSystem.Object;
using UObject = UnityEngine.Object;

namespace LimbusLocalize
{
    public static class GoodLifeHook
    {
        #region 去tmd文字描述成功率
        [HarmonyPatch(typeof(UITextMaker), nameof(UITextMaker.GetSuccessRateText))]
        [HarmonyPrefix]
        public static bool GetSuccessRateText(float rate, ref string __result)
        {
            __result = ((int)(rate * 100.0f)).ToString() + "%";
            return false;
        }
        [HarmonyPatch(typeof(UITextMaker), nameof(UITextMaker.GetSuccessRateToText))]
        [HarmonyPrefix]
        public static bool GetSuccessRateToText(float rate, ref string __result)
        {
            __result = ((int)(rate * 100.0f)).ToString() + "%";
            return false;
        }
        [HarmonyPatch(typeof(PossibleResultData), nameof(PossibleResultData.GetProb))]
        [HarmonyPrefix]
        public static bool GetProb(PossibleResultData __instance, float probAdder, ref float __result)
        {
            float num = 0f;
            float num2 = __instance._defaultProb + probAdder;
            float num3 = 1f - num2;
            int i = 0;
            int count = __instance._headTailCntList.Count;
            while (i < count)
            {
                num += Mathf.Pow(num2, __instance._headTailCntList[i].HeadCnt) * Mathf.Pow(num3, __instance._headTailCntList[i].TailCnt);
                i++;
            }
            __result = num;
            return false;
        }
        #endregion
        #region 屏蔽没有意义的Warning
        [HarmonyPatch(typeof(Logger), nameof(Logger.Log), new Type[]
        {
            typeof(LogType),
            typeof(ILObject)
        })]
        [HarmonyPrefix]
        private static bool Log(Logger __instance, LogType __0, ILObject __1)
        {
            if (__0 == LogType.Warning)
            {
                string LogString = Logger.GetString(__1);
                if (!LogString.Contains("DOTWEEN"))
                    __instance.logHandler.LogFormat(__0, null, "{0}", new ILObject[] { LogString });
                return false;
            }
            return true;
        }
        [HarmonyPatch(typeof(Logger), nameof(Logger.Log), new Type[]
        {
            typeof(LogType),
            typeof(ILObject),
            typeof(UObject)
        })]
        [HarmonyPrefix]
        private static bool Log(Logger __instance, LogType logType, ILObject message, UObject context)
        {
            if (logType == LogType.Warning)
            {
                string LogString = Logger.GetString(message);
                if (!LogString.Contains("Material"))
                    __instance.logHandler.LogFormat(logType, context, "{0}", new ILObject[] { LogString });
                return false;
            }
            return true;
        }
        #endregion
        #region 修复一些弱智东西
        [HarmonyPatch(typeof(GachaEffectEventSystem), nameof(GachaEffectEventSystem.LinkToCrackPosition))]
        [HarmonyPrefix]
        private static bool LinkToCrackPosition(GachaEffectEventSystem __instance, GachaCrackController[] crackList)
        {
            return __instance._parent.EffectChainCamera;
        }

        [HarmonyPatch(typeof(PersonalityVoiceJsonDataList), nameof(PersonalityVoiceJsonDataList.GetDataList))]
        [HarmonyPrefix]
        public static bool PersonalityVoiceGetDataList(PersonalityVoiceJsonDataList __instance, int personalityId, ref LocalizeTextDataRoot<TextData_PersonalityVoice> __result)
        {
            if (!__instance._voiceDictionary.TryGetValueEX(personalityId.ToString(), out LocalizeTextDataRoot<TextData_PersonalityVoice> localizeTextDataRoot))
            {
                Debug.LogError("PersonalityVoice no id:" + personalityId.ToString());
                localizeTextDataRoot = new LocalizeTextDataRoot<TextData_PersonalityVoice>() { dataList = new List<TextData_PersonalityVoice>() };
            }
            __result = localizeTextDataRoot;
            return false;
        }
        #endregion
        #region 人格ego跟随编队
        [HarmonyPatch(typeof(FormationSwitchableMirrorPersonalityMediator), nameof(FormationSwitchableMirrorPersonalityMediator.OpenAcquireNewCharacter))]
        [HarmonyPrefix]
        private static bool OpenAcquireNewCharacter(FormationSwitchableMirrorPersonalityMediator __instance, int acquiredIndex, int characterId, int grade, ref MirrorDungeonAcquireCharacterInfo formerInfo, DelegateEvent backEvent, DelegateEvent confirmEvent)
        {
            if (formerInfo == null)
            {
                var currentFormation = Singleton<UserDataManager>.Instance.Formations.GetCurrentFormation().GetFormationDetailInfo(characterId);
                var egos = currentFormation.GetEgos();
                PersonalityStaticData defaultDataByCharacterId = Singleton<StaticDataManager>.Instance.PersonalityStaticDataList.GetData(currentFormation.PersonalityId);
                __instance._personalityId = defaultDataByCharacterId.ID;
                __instance._egoArray = new int[5];
                List<EgoContainIndex> list = new List<EgoContainIndex>();
                for (int i = 0; i < grade; i++)
                {
                    int num = egos[i];
                    __instance._egoArray[i] = num;
                    if (num != 0)
                    {
                        Ego ego = Singleton<UserDataManager>.Instance.Egos.GetEgo(num);
                        if (ego != null)
                        {
                            list.Add(new EgoContainIndex(num, ego.Gacksung, i));
                        }
                        else
                        {
                            list.Add(new EgoContainIndex(0, 0, i));
                        }
                    }
                }
                formerInfo = new MirrorDungeonAcquireCharacterInfo(currentFormation.PersonalityId, list, 1);
            }
            return true;
        }
        [HarmonyPatch(typeof(FormationSwitchableMirrorPersonalityMediator), nameof(FormationSwitchableMirrorPersonalityMediator.OpenLevelUpCharacter))]
        [HarmonyPrefix]
        private static bool OpenLevelUpCharacter(FormationSwitchableMirrorPersonalityMediator __instance, MirrorDungeonLevelUpData data, DelegateEvent confirmEvent)
        {
            __instance._characterId = (int)data.CharacterType;
            __instance._personalityId = data.PersonalityId;
            MirrorDungeonSaveUnitInfo unit = Singleton<UserDataManager>.Instance.MirrorDungeonSaveData.currentInfo.GetUnit(__instance._characterId);

            var currentFormation = Singleton<UserDataManager>.Instance.Formations.GetCurrentFormation().GetFormationDetailInfo(__instance._characterId);
            var egos = currentFormation.GetEgos();
            int grade = data.NextGrade - 1;
            var egoid = egos[grade];
            if (egoid != 0)
            {
                Ego ego = Singleton<UserDataManager>.Instance.Egos.GetEgo(egoid);
                unit.ConvertedEgos[grade] = ego;
            }
            return true;
        }
        #endregion
    }
}
