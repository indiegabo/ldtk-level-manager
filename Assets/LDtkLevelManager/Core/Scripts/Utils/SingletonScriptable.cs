using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace LDtkLevelManager.Utils
{
    public class SingletonScriptable<T> : ScriptableObject where T : SingletonScriptable<T>
    {
        private static T _instance;
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    var op = Addressables.LoadAssetAsync<T>(typeof(T).Name);
                    _instance = op.WaitForCompletion(); //Forces synchronous load so that we can return immediately
                }
                return _instance;
            }
        }
    }
}