using Il2CppSteamworks;
using Il2CppSystem.Runtime.CompilerServices;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace LimbusLocalize
{
    public class ReflectionUtility
    {

        public static bool TryGetEnumerator(object list, out IEnumerator enumerator)
        {
            enumerator = (list as IEnumerable).GetEnumerator();
            return true;
        }


        public static bool TryGetEntryType(Type enumerableType, out Type type)
        {
            bool isArray = enumerableType.IsArray;
            bool flag;
            if (isArray)
            {
                type = enumerableType.GetElementType();
                flag = true;
            }
            else
            {
                foreach (Type t in enumerableType.GetInterfaces())
                {
                    bool isGenericType = t.IsGenericType;
                    if (isGenericType)
                    {
                        Type typeDef = t.GetGenericTypeDefinition();
                        bool flag2 = typeDef == typeof(IEnumerable<>) || typeDef == typeof(IList<>) || typeDef == typeof(ICollection<>);
                        if (flag2)
                        {
                            type = t.GetGenericArguments()[0];
                            return true;
                        }
                    }
                }
                type = typeof(object);
                flag = false;
            }
            return flag;
        }

        public static bool TryGetDictEnumerator(object dictionary, out IEnumerator<DictionaryEntry> dictEnumerator)
        {
            dictEnumerator = EnumerateDictionary((IDictionary)dictionary);
            return true;
        }

        private static IEnumerator<DictionaryEntry> EnumerateDictionary(IDictionary dict)
        {
            IDictionaryEnumerator enumerator = dict.GetEnumerator();
            while (enumerator.MoveNext())
            {
                yield return new DictionaryEntry(enumerator.Key, enumerator.Value);
            }
            yield break;
        }


        public static bool TryGetEntryTypes(Type dictionaryType, out Type keys, out Type values)
        {
            foreach (Type t in dictionaryType.GetInterfaces())
            {
                bool flag = t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IDictionary<,>);
                if (flag)
                {
                    Type[] args = t.GetGenericArguments();
                    keys = args[0];
                    values = args[1];
                    return true;
                }
            }
            keys = typeof(object);
            values = typeof(object);
            return false;
        }
    }
}
