using LDtkUnity;
using LDtkUnity.Editor;
using LDtkLevelManager;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
namespace LDtkLevelManagerEditor
{
    public class LDtkProjectPostProcessor : LDtkPostprocessor
    {
        protected override void OnPostprocessProject(GameObject root)
        {
            LDtkLevelManagerLevelsSyncer.SetProjectToProcess(ImportContext.assetPath);
        }

        protected override void OnPostprocessLevel(GameObject root, LdtkJson projectJson)
        {
            LDtkLevelManagerLevelsSyncer.AddProcessSubjecLevel(new ProcessedLevelEntry
            {
                levelAssetPath = ImportContext.assetPath,
                projectIid = projectJson.Iid
            });
        }
    }
}