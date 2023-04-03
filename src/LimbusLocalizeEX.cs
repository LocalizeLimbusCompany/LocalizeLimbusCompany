using Il2Cpp;
using Il2CppSystem.Collections.Generic;
using Il2CppSystem.Linq;
using System;
using System.IO;
using UnityEngine;

namespace LimbusLocalize
{
    public static class LimbusLocalizeEX
    {
        public static void Init<T>(this JsonDataList<T> jsonDataList, List<string> list) where T : LocalizeTextData, new()
        {
            foreach (string text2 in list)
            {
                var file = ModManager.Localizes[text2];
                if (string.IsNullOrEmpty(file)) { continue; }
                var localizeTextData = JsonUtility.FromJson<LocalizeTextDataRoot<T>>(file);
                foreach (T t in localizeTextData.DataList)
                {
                    jsonDataList._dic[t.ID.ToString()] = t;
                }
            }
        }

        public static void AbEventCharDlgRootInit(this AbEventCharDlgRoot root, List<string> jsonFiles)
        {
            root._personalityDict = new Dictionary<int, AbEventKeyDictionaryContainer>();

            foreach (string text2 in jsonFiles)
            {
                var file = ModManager.Localizes[text2];
                if (string.IsNullOrEmpty(file)) { continue; }
                var localizeTextData = JsonUtility.FromJson<LocalizeTextDataRoot<TextData_AbnormalityEventCharDlg>>(file);
                foreach (var t in localizeTextData.DataList)
                {
                    if (!root._personalityDict.TryGetValueEX(t.PersonalityID, out var abEventKeyDictionaryContainer))
                    {
                        abEventKeyDictionaryContainer = new AbEventKeyDictionaryContainer();
                        root._personalityDict[t.PersonalityID] = abEventKeyDictionaryContainer;
                    }
                    string[] array = t.Usage.Trim().Split(new char[] { '(', ')' });
                    for (int i = 1; i < array.Length; i += 2)
                    {
                        string[] array2 = array[i].Split(',');
                        int num = int.Parse(array2[0].Trim());
                        AB_DLG_EVENT_TYPE ab_DLG_EVENT_TYPE = (AB_DLG_EVENT_TYPE)Enum.Parse(typeof(AB_DLG_EVENT_TYPE), array2[1].Trim());
                        AbEventKey abEventKey = new(num, ab_DLG_EVENT_TYPE);
                        abEventKeyDictionaryContainer.AddDlgWithEvent(abEventKey, t);
                    }
                }

            }
        }
        public static void PersonalityVoiceJsonDataListInit(this PersonalityVoiceJsonDataList jsonDataList, List<string> jsonFilePathList)
        {
            jsonDataList._voiceDictionary = new Dictionary<string, LocalizeTextDataRoot<TextData_PersonalityVoice>>();

            foreach (string text2 in jsonFilePathList)
            {
                var file = ModManager.Localizes[text2];
                if (string.IsNullOrEmpty(file)) { continue; }
                var localizeTextData = JsonUtility.FromJson<LocalizeTextDataRoot<TextData_PersonalityVoice>>(file);
                jsonDataList._voiceDictionary.Add(text2.Split('_')[^1], localizeTextData);
            }
        }
        public static void AnnouncerVoiceJsonDataListInit(this AnnouncerVoiceJsonDataList jsonDataList, List<string> jsonFilePathList)
        {
            jsonDataList._voiceDictionary = new Dictionary<string, LocalizeTextDataRoot<TextData_AnnouncerVoice>>();

            foreach (string text2 in jsonFilePathList)
            {
                var file = ModManager.Localizes[text2];
                if (string.IsNullOrEmpty(file)) { continue; }
                var localizeTextData = JsonUtility.FromJson<LocalizeTextDataRoot<TextData_AnnouncerVoice>>(file);
                jsonDataList._voiceDictionary.Add(text2.Split('_')[^1], localizeTextData);
            }
        }
        public static void BgmLyricsJsonDataListInit(this BgmLyricsJsonDataList jsonDataList, List<string> jsonFilePathList)
        {
            jsonDataList._lyricsDictionary = new Dictionary<string, LocalizeTextDataRoot<TextData_UI>>();

            foreach (string text2 in jsonFilePathList)
            {
                var file = ModManager.Localizes[text2];
                if (string.IsNullOrEmpty(file)) { continue; }
                var localizeTextData = JsonUtility.FromJson<LocalizeTextDataRoot<TextData_UI>>(file);
                jsonDataList._lyricsDictionary.Add(text2.Split('_')[^1], localizeTextData);
            }
        }
        public static void EGOVoiceJsonDataListInit(this EGOVoiceJsonDataList jsonDataList, List<string> jsonFilePathList)
        {
            jsonDataList._voiceDictionary = new Dictionary<string, LocalizeTextDataRoot<TextData_EGOVoice>>();

            foreach (string text2 in jsonFilePathList)
            {
                var file = ModManager.Localizes[text2];
                if (string.IsNullOrEmpty(file)) { continue; }
                var localizeTextData = JsonUtility.FromJson<LocalizeTextDataRoot<TextData_EGOVoice>>(file);
                jsonDataList._voiceDictionary.Add(text2.Split('_')[^1], localizeTextData);
            }
        }

        public static Dictionary<TKey, TElement> ToDictionary<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
        {
            return Enumerable.ToDictionary<TSource, TKey, TElement>(source, keySelector, elementSelector, null);
        }
        public static List<T> AddEX<T>(this List<T> list, params T[] values)
        {
            foreach (var value in values)
                list.Add(value);
            return list;
        }
        public static bool TryGetValueEX<TKey, TValue>(this Dictionary<TKey, TValue> dic, TKey key, out TValue value)
        {
            var entries = dic._entries;
            var Entr = dic.FindEntry(key);
            value = Entr == -1 ? default : entries == null ? default : entries[Entr].value;
            return value != null;
        }
    }
}
