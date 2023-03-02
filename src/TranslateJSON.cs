using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine.Networking;
using System;

namespace LimbusLocalize
{
    public class TranslateJSON : MonoBehaviour
    {
        public void Start()
        {
            __instance = this;
        }
        public static void StartTranslateText(string text, Action<string> action)
        {
            TranslateJSON.__instance.StartCoroutine(__instance.TranslateText(text, action));
        }
        static TranslateJSON __instance;
        private IEnumerator TranslateText(string text, Action<string> action)
        {

            // Build the request URI

            string uri = "https://translate.googleapis.com/translate_a/single?client=gtx&dt=t&sl=ko&tl=zh-CN&q=" + WebUtility.UrlEncode(text);

            UnityWebRequest www = UnityWebRequest.Get(uri);

            yield return www.SendWebRequest();

            string responseBody = www.downloadHandler.text;
            // Find the first occurrence of a double quote
            int startIndex = responseBody.IndexOf('\"');

            if (startIndex != -1)
            {
                // Find the next occurrence of a double quote
                int endIndex = responseBody.IndexOf('\"', startIndex + 1);

                if (endIndex != -1)
                {
                    // Extract the contents between the quotes
                    string content = responseBody.Substring(startIndex + 1, endIndex - startIndex - 1);

                    action(content);

                    Debug.Log("Extracted content: " + content);
                }
            }
            yield break;
        }
        static List<string> keyValuePairs = new List<string>();
    }
}