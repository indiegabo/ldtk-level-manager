using LDtkUnity;
using LDtkUnity.Editor;
using LDtkLevelManager;
using UnityEngine;
using UnityEditor;

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