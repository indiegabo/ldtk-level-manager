using LDtkUnity;
using LDtkUnity.Editor;
using LDtkVania;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
namespace LDtkVaniaEditor
{
    public class LDtkProjectPostProcessor : LDtkPostprocessor
    {
        protected override void OnPostprocessProject(GameObject root)
        {
            LDtkVaniaLevelsSyncer.SetProjectToProcess(ImportContext.assetPath);
        }

        protected override void OnPostprocessLevel(GameObject root, LdtkJson projectJson)
        {
            LDtkVaniaLevelsSyncer.AddProcessSubjecLevel(new ProcessedLevelEntry
            {
                levelAssetPath = ImportContext.assetPath,
                projectIid = projectJson.Iid
            });
        }
    }
}