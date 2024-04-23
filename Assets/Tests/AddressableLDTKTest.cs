// using UnityEngine;
// using Sirenix.OdinInspector;
// using UnityEngine.AddressableAssets;
// using UnityEngine.ResourceManagement.AsyncOperations;
// using UnityEngine.ResourceManagement.ResourceLocations;
// using System.Collections.Generic;
// using System.Threading.Tasks;

// namespace HandyTools
// {
//     public class AddressableLDTKTest : MonoBehaviour
//     {
//         #region Inspector

//         [BoxGroup("Dependencies")]
//         [SerializeField]
//         private string _iid;

//         [BoxGroup("Dependencies")]
//         [SerializeField]
//         private AssetReference _assetReference;

//         [BoxGroup("Dependencies")]
//         [SerializeField]
//         private AssetLabelReference _labelsReference;

//         #endregion

//         #region Behaviour

//         private void Awake()
//         {
//             MV_Level mV_Level = MV_Project.Instance.GetLevel(_iid);
//             // _ = LoadLevel(_assetReference);
//             _ = LoadLevel(mV_Level);
//             // _assetReference.LoadAssetAsync<GameObject>().Completed += OnLoadAssetCompleted;
//             // Addressables.LoadResourceLocationsAsync(_labelsReference.labelString, null).Completed += OnLoadResourceLocations;
//         }

//         [Button]
//         private void Test()
//         {
//             Addressables.LoadResourceLocationsAsync(_labelsReference.labelString, null).Completed += x =>
//             {
//                 foreach (IResourceLocation item in x.Result)
//                 {
//                     // Addressables.LoadAssetAsync<GameObject>(item).Completed += OnLoadAssetCompleted;
//                 }
//             };
//         }

//         private void OnLoadResourceLocations(AsyncOperationHandle<IList<IResourceLocation>> handle)
//         {
//             foreach (IResourceLocation location in handle.Result)
//             {
//                 try
//                 {
//                     Addressables.LoadAssetAsync<GameObject>(location).Completed += OnLoadAssetCompleted;
//                 }
//                 catch (System.Exception e)
//                 {
//                     MV_Logger.Error(e.Message, this);
//                 }
//             }
//         }

//         private void OnLoadAssetCompleted(AsyncOperationHandle<GameObject> handle)
//         {
//             if (handle.Status == AsyncOperationStatus.Succeeded)
//             {
//                 GameObject obj = Instantiate(handle.Result);
//                 MV_LevelBehaviour level = obj.GetComponent<MV_LevelBehaviour>();
//                 Debug.Log($"Loaded {level} from {handle.Result.name}");
//             }
//             else
//             {
//                 MV_Logger.Error("Failed to load asset", this);
//             }

//         }

//         private async Task LoadLevel(MV_Level mV_Level)
//         {
//             if (mV_Level == null) return;
//             AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(mV_Level.AddressableKey);
//             await handle.Task;
//             if (handle.Status == AsyncOperationStatus.Succeeded)
//             {
//                 GameObject obj = Instantiate(handle.Result);
//                 MV_LevelBehaviour level = obj.GetComponent<MV_LevelBehaviour>();
//                 Debug.Log(level);
//             }
//             else
//             {
//                 MV_Logger.Error("Failed to load asset", this);
//             }
//         }

//         private async Task LoadLevel(AssetReference aRef)
//         {
//             AsyncOperationHandle<GameObject> handle = aRef.LoadAssetAsync<GameObject>();
//             await handle.Task;
//             if (handle.Status == AsyncOperationStatus.Succeeded)
//             {
//                 GameObject obj = Instantiate(handle.Result);
//                 MV_LevelBehaviour level = obj.GetComponent<MV_LevelBehaviour>();
//                 Debug.Log(level);
//             }
//             else
//             {
//                 MV_Logger.Error("Failed to load asset", this);
//             }
//         }

//         #endregion
//     }
// }