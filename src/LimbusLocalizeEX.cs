using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace LimbusLocalize
{
    public static class LimbusLocalizeEX
    {
        public static void Init<T>(this JsonDataList<T> jsonDataList, List<string> list) where T : LocalizeTextData, new()
        {
            string Localizepath = LimbusLocalize.path + "/Localize/CN/";
            string text = "CN";
            foreach (string text2 in list)
            {
                var localizeTextData = JsonUtility.FromJson<LocalizeTextDataRoot<T>>(File.ReadAllText(string.Format("{0}{1}_{2}.json", Localizepath, text, text2)));
                foreach (T t in localizeTextData.DataList)
                {
                    jsonDataList._dic[t.ID.ToString()]= t;
                }

            }
        }
        
        public static void AbEventCharDlgRootInit(this AbEventCharDlgRoot root, List<string> jsonFiles)
        {
            root._personalityDict = new Dictionary<int, AbEventKeyDictionaryContainer>();


            string Localizepath = LimbusLocalize.path + "/Localize/CN/";
            string text = "CN";
            foreach (string text2 in jsonFiles)
            {
                var localizeTextData = JsonUtility.FromJson<LocalizeTextDataRoot<TextData_AbnormalityEventCharDlg>>(File.ReadAllText(string.Format("{0}{1}_{2}.json", Localizepath, text, text2)));
                foreach (var t in localizeTextData.DataList)
                {
                    if (!root._personalityDict.TryGetValue(t.PersonalityID, out AbEventKeyDictionaryContainer abEventKeyDictionaryContainer))
                    {
                        abEventKeyDictionaryContainer = new AbEventKeyDictionaryContainer();
                        root._personalityDict[t.PersonalityID] = abEventKeyDictionaryContainer;
                    }
                    string[] array = t.Usage.Trim().Split(new char[] { '(', ')' });
                    for (int i = 1; i < array.Length; i += 2)
                    {
                        string[] array2 = array[i].Split(',', StringSplitOptions.None);
                        int num = int.Parse(array2[0].Trim());
                        AB_DLG_EVENT_TYPE ab_DLG_EVENT_TYPE = (AB_DLG_EVENT_TYPE)Enum.Parse(typeof(AB_DLG_EVENT_TYPE), array2[1].Trim());
                        AbEventKey abEventKey = new AbEventKey(num, ab_DLG_EVENT_TYPE);
                        abEventKeyDictionaryContainer.AddDlgWithEvent(abEventKey, t);
                    }
                }

            }
        }
        public static void PersonalityVoiceJsonDataListInit(this PersonalityVoiceJsonDataList jsonDataList, List<string> jsonFilePathList)
        {
            jsonDataList._voiceDictionary = new Dictionary<string, LocalizeTextDataRoot<TextData_PersonalityVoice>>();

            string Localizepath = LimbusLocalize.path + "/Localize/CN/";
            string text = "CN";
            foreach (string text2 in jsonFilePathList)
            {
                var localizeTextData = JsonUtility.FromJson<LocalizeTextDataRoot<TextData_PersonalityVoice>>(File.ReadAllText(string.Format("{0}{1}_{2}.json", Localizepath, text, text2)));
                string[] array = text2.Split('_', StringSplitOptions.None);
                string text3 = array[array.Length - 1];
                jsonDataList._voiceDictionary.Add(text3, localizeTextData);
            }
        }
        public static void AnnouncerVoiceJsonDataListInit(this AnnouncerVoiceJsonDataList jsonDataList, List<string> jsonFilePathList)
        {
            jsonDataList._voiceDictionary = new Dictionary<string, LocalizeTextDataRoot<TextData_AnnouncerVoice>>();

            string Localizepath = LimbusLocalize.path + "/Localize/CN/";
            string text = "CN";
            foreach (string text2 in jsonFilePathList)
            {
                var localizeTextData = JsonUtility.FromJson<LocalizeTextDataRoot<TextData_AnnouncerVoice>>(File.ReadAllText(string.Format("{0}{1}_{2}.json", Localizepath, text, text2)));
                string[] array = text2.Split('_', StringSplitOptions.None);
                string text3 = array[array.Length - 1];
                jsonDataList._voiceDictionary.Add(text3, localizeTextData);
            }
        }
        public static void BgmLyricsJsonDataListInit(this BgmLyricsJsonDataList jsonDataList, List<string> jsonFilePathList)
        {
            jsonDataList._lyricsDictionary = new Dictionary<string, LocalizeTextDataRoot<TextData_UI>>();

            string Localizepath = LimbusLocalize.path + "/Localize/CN/";
            string text = "CN";
            foreach (string text2 in jsonFilePathList)
            {
                var localizeTextData = JsonUtility.FromJson<LocalizeTextDataRoot<TextData_UI>>(File.ReadAllText(string.Format("{0}{1}_{2}.json", Localizepath, text, text2)));
                string[] array = text2.Split('_', StringSplitOptions.None);
                string text3 = array[array.Length - 1];
                jsonDataList._lyricsDictionary.Add(text3, localizeTextData);
            }
        }
        public static void EGOVoiceJsonDataListInit(this EGOVoiceJsonDataList jsonDataList, List<string> jsonFilePathList)
        {
            jsonDataList._voiceDictionary = new Dictionary<string, LocalizeTextDataRoot<TextData_EGOVoice>>();

            string Localizepath = LimbusLocalize.path + "/Localize/CN/";
            string text = "CN";
            foreach (string text2 in jsonFilePathList)
            {
                var localizeTextData = JsonUtility.FromJson<LocalizeTextDataRoot<TextData_EGOVoice>>(File.ReadAllText(string.Format("{0}{1}_{2}.json", Localizepath, text, text2)));
                string[] array = text2.Split('_', StringSplitOptions.None);
                string text3 = array[array.Length - 1];
                jsonDataList._voiceDictionary.Add(text3, localizeTextData);
            }
        }
    }
}