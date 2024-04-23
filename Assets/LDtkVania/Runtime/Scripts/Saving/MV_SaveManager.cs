// using UnityEngine;
// using HandyTools.Saving;
// using System.Collections.Generic;
// using Sirenix.OdinInspector;
// using System.Threading.Tasks;
// using HandyTools.Debugging.Logging;

// namespace LDtkVania
// {
//     /// <summary>
//     /// Handles saving and loading of registered data.
//     /// For this to work data must be kept updating. Save handlers
//     /// must register data updates and when data must be saved, the method "SaveRegistry()" 
//     /// must be called. 
//     /// </summary>
//     [DefaultExecutionOrder(-10000)]
//     public class MV_SaveManager : SingleMonoBehaviour<MV_SaveManager>
//     {
//         #region Inspector

//         [BoxGroup("Dependencies")]
//         [SerializeField]
//         private SaveHandler _saveHandler;

//         [BoxGroup("Dependencies")]
//         [SerializeField]
//         private SaveSlotsHandler _saveSlotsHandler;

//         #endregion

//         #region Fields

//         private bool _running;
//         private Dictionary<SaveIdentifier, object> _registry;

//         #endregion

//         #region Singleton behaviour

//         private void OnSingletonLoaded()
//         {
//             _registry = new Dictionary<SaveIdentifier, object>();

//             if (!_saveSlotsHandler.HasSelectedSlot)
//             {
//                 MV_Logger.Error($"{name} unable to start since no save slot has been selected", this);
//                 return;
//             }

//             _running = true;
//         }

//         #endregion

//         #region Static

//         /// <summary>
//         /// Registers data so it can be subject of
//         /// the next registry save.
//         /// </summary>
//         /// <param name="key"></param>
//         /// <param name="data"></param>
//         public static void RegisterData(string key, string group, object data)
//         {
//             if (InstanceUnavailable || !Instance._running) return;

//             SaveIdentifier keyGroupPair = SaveIdentifier.Generate(key, group);
//             RegisterData(keyGroupPair, data);
//         }

//         /// <summary>
//         /// Registers data so it can be subject of
//         /// the next registry save and persists the data
//         /// afterwards.
//         /// </summary>
//         /// <param name="key"></param>
//         /// <param name="data"></param>
//         public static void RegisterAndSaveData(string key, string group, object data)
//         {
//             if (InstanceUnavailable || !Instance._running) return;

//             SaveIdentifier keyGroupPair = SaveIdentifier.Generate(key, group);
//             RegisterData(keyGroupPair, data);
//             PersistData(keyGroupPair, data);
//         }

//         /// <summary>
//         /// Registers data so it can be subject of
//         /// the next registry save.
//         /// </summary>
//         /// <param name="key"></param>
//         /// <param name="data"></param>
//         public static void RegisterData(SaveIdentifier keyGroupPair, object data)
//         {
//             if (InstanceUnavailable || !Instance._running) return;

//             if (!Instance._registry.ContainsKey(keyGroupPair))
//             {
//                 Instance._registry.Add(keyGroupPair, data);
//             }
//             else
//             {
//                 Instance._registry[keyGroupPair] = data;
//             }
//         }

//         /// <summary>
//         /// Loads data based on parameters and the given type. 
//         /// 
//         /// Note that if there is currently data registered under that 
//         /// specific key/group pair, the data will be loaded from the registry.
//         /// 
//         /// Case there is no data in the current registry, the manager will then
//         /// try loading from the disk.
//         /// 
//         /// </summary>
//         /// <param name="key"></param>
//         /// <param name="group"></param>
//         /// <typeparam name="TData"></typeparam>
//         /// <returns></returns>
//         public static TData LoadData<TData>(string key, string group)
//         {
//             if (InstanceUnavailable || !Instance._running) return default;

//             SaveIdentifier keyGroupPair = SaveIdentifier.Generate(key, group);

//             object rawData;

//             if (Instance._registry.ContainsKey(keyGroupPair))
//             {
//                 rawData = Instance._registry[keyGroupPair];
//             }
//             else
//             {
//                 // Data being loaded from save slot
//                 SaveSlot slot = Instance._saveSlotsHandler.CurrentSlot;
//                 rawData = Instance._saveHandler.LoadFromSlot<TData>(slot, key, group);
//                 // As it was just loaded, we must put it in the registry
//                 RegisterData(keyGroupPair, rawData);
//             }

//             return (TData)rawData;
//         }

//         public static bool TryLoadingData<TData>(string key, string group, out TData data)
//         {
//             data = LoadData<TData>(key, group);
//             return data != null;
//         }

//         /// <summary>
//         /// Persists all data in the registry into the disk.
//         /// The registry is cleaned after this
//         /// </summary>
//         /// <returns></returns>
//         public static async Task SaveRegistry()
//         {
//             if (InstanceUnavailable || !Instance._running) return;

//             List<Task> tasks = new List<Task>();

//             foreach (KeyValuePair<SaveIdentifier, object> keyValue in Instance._registry)
//             {
//                 SaveIdentifier keyGroupPair = keyValue.Key;
//                 object data = keyValue.Value;

//                 // Actually saving the data
//                 tasks.Add(Task.Run(() => PersistData(keyGroupPair, data)));
//             }

//             await Task.WhenAll(tasks);

//             Instance._registry.Clear();
//         }

//         private static void PersistData(SaveIdentifier pair, object data)
//         {
//             SaveSlot slot = Instance._saveSlotsHandler.CurrentSlot;
//             Instance._saveHandler.SaveIntoSlot(slot, pair.key, data, pair.group);
//         }

//         #endregion
//     }
// }