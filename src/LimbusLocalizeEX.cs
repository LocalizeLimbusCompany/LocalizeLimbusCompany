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

            string Localizepath = LimbusLocalizeMod.path + "/Localize/";
            string text = "CN";
            foreach (string text2 in list)
            {
                var file = string.Format("{0}{1}/{1}_{2}.json", Localizepath, text, text2);
                if (File.Exists(file))
                {
                    var localizeTextData = JsonUtility.FromJson<LocalizeTextDataRoot<T>>(File.ReadAllText(file));
                    foreach (T t in localizeTextData.DataList)
                    {
                        jsonDataList._dic[t.ID.ToString()] = t;
                    }
                }
            }
        }

        public static void AbEventCharDlgRootInit(this AbEventCharDlgRoot root, List<string> jsonFiles)
        {
            root._personalityDict = new Dictionary<int, AbEventKeyDictionaryContainer>();


            string Localizepath = LimbusLocalizeMod.path + "/Localize/CN/";
            string text = "CN";
            foreach (string text2 in jsonFiles)
            {
                var file = string.Format("{0}{1}_{2}.json", Localizepath, text, text2);
                if (!File.Exists(file)) { return; }
                var localizeTextData = JsonUtility.FromJson<LocalizeTextDataRoot<TextData_AbnormalityEventCharDlg>>(File.ReadAllText(file));
                foreach (var t in localizeTextData.DataList)
                {
                    var entries = root._personalityDict._entries;
                    var Entr = root._personalityDict.FindEntry(t.PersonalityID);
                    AbEventKeyDictionaryContainer abEventKeyDictionaryContainer = Entr == -1 ? null : entries?[Entr].value;
                    if (abEventKeyDictionaryContainer == null)
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

            string Localizepath = LimbusLocalizeMod.path + "/Localize/CN/";
            string text = "CN";
            foreach (string text2 in jsonFilePathList)
            {
                var file = string.Format("{0}{1}_{2}.json", Localizepath, text, text2);
                if (!File.Exists(file)) { return; }
                var localizeTextData = JsonUtility.FromJson<LocalizeTextDataRoot<TextData_PersonalityVoice>>(File.ReadAllText(file));
                jsonDataList._voiceDictionary.Add(text2.Split('_')[^1], localizeTextData);
            }
        }
        public static void AnnouncerVoiceJsonDataListInit(this AnnouncerVoiceJsonDataList jsonDataList, List<string> jsonFilePathList)
        {
            jsonDataList._voiceDictionary = new Dictionary<string, LocalizeTextDataRoot<TextData_AnnouncerVoice>>();

            string Localizepath = LimbusLocalizeMod.path + "/Localize/CN/";
            string text = "CN";
            foreach (string text2 in jsonFilePathList)
            {
                var file = string.Format("{0}{1}_{2}.json", Localizepath, text, text2);
                if (!File.Exists(file)) { return; }
                var localizeTextData = JsonUtility.FromJson<LocalizeTextDataRoot<TextData_AnnouncerVoice>>(File.ReadAllText(file));
                jsonDataList._voiceDictionary.Add(text2.Split('_')[^1], localizeTextData);
            }
        }
        public static void BgmLyricsJsonDataListInit(this BgmLyricsJsonDataList jsonDataList, List<string> jsonFilePathList)
        {
            jsonDataList._lyricsDictionary = new Dictionary<string, LocalizeTextDataRoot<TextData_UI>>();

            string Localizepath = LimbusLocalizeMod.path + "/Localize/CN/";
            string text = "CN";
            foreach (string text2 in jsonFilePathList)
            {
                var file = string.Format("{0}{1}_{2}.json", Localizepath, text, text2);
                if (!File.Exists(file)) { return; }
                var localizeTextData = JsonUtility.FromJson<LocalizeTextDataRoot<TextData_UI>>(File.ReadAllText(file));
                jsonDataList._lyricsDictionary.Add(text2.Split('_')[^1], localizeTextData);
            }
        }
        public static void EGOVoiceJsonDataListInit(this EGOVoiceJsonDataList jsonDataList, List<string> jsonFilePathList)
        {
            jsonDataList._voiceDictionary = new Dictionary<string, LocalizeTextDataRoot<TextData_EGOVoice>>();

            string Localizepath = LimbusLocalizeMod.path + "/Localize/CN/";
            string text = "CN";
            foreach (string text2 in jsonFilePathList)
            {
                var file = string.Format("{0}{1}_{2}.json", Localizepath, text, text2);
                if (!File.Exists(file)) { return; }
                var localizeTextData = JsonUtility.FromJson<LocalizeTextDataRoot<TextData_EGOVoice>>(File.ReadAllText(file));
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
