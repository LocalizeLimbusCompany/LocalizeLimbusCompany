using HarmonyLib;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Injection;
using Il2CppSystem;
using Il2CppSystem.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace LimbusLocalize
{
    public class Il2CppManagedEnumerator : Il2CppSystem.Object
    {
        static Il2CppManagedEnumerator()
        {
            try
            {
                ClassInjector.RegisterTypeInIl2Cpp<Il2CppManagedEnumerator>(new RegisterTypeOptions
                {
                    Interfaces = new System.Type[] { typeof(Il2CppSystem.Collections.IEnumerator) }
                });
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogWarning(ex.ToString());
            }
        }

        public Il2CppManagedEnumerator(System.IntPtr ptr)
            : base(ptr)
        {
        }

        public Il2CppManagedEnumerator(System.Collections.IEnumerator enumerator)
            : base(ClassInjector.DerivedConstructorPointer<Il2CppManagedEnumerator>())
        {
            if (enumerator == null)
            {
                throw new System.ArgumentNullException("enumerator");
            }
            this.enumerator = enumerator;
            ClassInjector.DerivedConstructorBody(this);
        }

        public Il2CppSystem.Object Current
        {
            get
            {
                object obj = enumerator.Current;
                if (!true)
                {
                }
                Il2CppSystem.Collections.IEnumerator i = obj as Il2CppSystem.Collections.IEnumerator;
                Il2CppSystem.Object @object;
                if (i == null)
                {
                    System.Collections.IEnumerator e = obj as System.Collections.IEnumerator;
                    if (e == null)
                    {
                        Il2CppSystem.Object il2cppObj = obj as Il2CppSystem.Object;
                        if (il2cppObj == null)
                        {
                            if (obj == null)
                            {
                                @object = null;
                            }
                            else
                            {
                                @object = Il2CppManagedEnumerator.ManagedToIl2CppObject(obj);
                            }
                        }
                        else
                        {
                            @object = il2cppObj;
                        }
                    }
                    else
                    {
                        @object = new Il2CppManagedEnumerator(e);
                    }
                }
                else
                {
                    @object = i.Cast<Il2CppSystem.Object>();
                }
                if (!true)
                {
                }
                return @object;
            }
        }

        public bool MoveNext()
        {
            return enumerator.MoveNext();
        }

        public void Reset()
        {
            enumerator.Reset();
        }

        private static Il2CppSystem.Object ManagedToIl2CppObject(object obj)
        {
            System.Type t = obj.GetType();
            string s = obj as string;
            bool flag = s != null;
            Il2CppSystem.Object @object;
            if (flag)
            {
                @object = new Il2CppSystem.Object(IL2CPP.ManagedStringToIl2Cpp(s));
            }
            else
            {
                bool isPrimitive = t.IsPrimitive;
                if (!isPrimitive)
                {
                    System.Runtime.CompilerServices.DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new System.Runtime.CompilerServices.DefaultInterpolatedStringHandler(54, 1);
                    defaultInterpolatedStringHandler.AppendLiteral("Type ");
                    defaultInterpolatedStringHandler.AppendFormatted<System.Type>(t);
                    defaultInterpolatedStringHandler.AppendLiteral(" cannot be converted directly to an Il2Cpp object");
                    throw new System.NotSupportedException(defaultInterpolatedStringHandler.ToStringAndClear());
                }
                @object = Il2CppManagedEnumerator.GetValueBoxer(t)(obj);
            }
            return @object;
        }

        private static System.Func<object, Il2CppSystem.Object> GetValueBoxer(System.Type t)
        {
            System.Func<object, Il2CppSystem.Object> conv;
            bool flag = Il2CppManagedEnumerator.boxers.TryGetValue(t, out conv);
            System.Func<object, Il2CppSystem.Object> func;
            if (flag)
            {
                func = conv;
            }
            else
            {
                DynamicMethod dm = new DynamicMethod("Il2CppUnbox_" + t.FullDescription(), typeof(Il2CppSystem.Object), new System.Type[] { typeof(object) });
                ILGenerator il = dm.GetILGenerator();
                LocalBuilder loc = il.DeclareLocal(t);
                FieldInfo classField = typeof(Il2CppClassPointerStore<>).MakeGenericType(new System.Type[] { t }).GetField("NativeClassPtr");
                il.Emit(OpCodes.Ldsfld, classField);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Unbox_Any, t);
                il.Emit(OpCodes.Stloc, loc);
                il.Emit(OpCodes.Ldloca, loc);
                il.Emit(OpCodes.Call, typeof(IL2CPP).GetMethod("il2cpp_value_box"));
                il.Emit(OpCodes.Newobj, typeof(Il2CppSystem.Object).GetConstructor(new System.Type[] { typeof(System.IntPtr) }));
                il.Emit(OpCodes.Ret);
                System.Func<object, Il2CppSystem.Object> converter = dm.CreateDelegate(typeof(System.Func<object, Il2CppSystem.Object>)) as System.Func<object, Il2CppSystem.Object>;
                Il2CppManagedEnumerator.boxers[t] = converter;
                func = converter;
            }
            return func;
        }

        private static readonly Dictionary<System.Type, System.Func<object, Il2CppSystem.Object>> boxers = new Dictionary<System.Type, System.Func<object, Il2CppSystem.Object>>();

        private readonly System.Collections.IEnumerator enumerator;
    }
}
