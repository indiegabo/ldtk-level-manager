using UnityEditor;
using LDtkVania;
using UnityEngine;

namespace LDtkVaniaEditor
{
    class LDtkVaniaLevelsSyncer : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
        {
            string[] guids = AssetDatabase.FindAssets($"t:{nameof(MV_Project)}");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                MV_Project project = AssetDatabase.LoadAssetAtPath<MV_Project>(path);
                if (project != null && project.SyncLevelsAtCompile)
                {
                    project.SyncLevels();
                }
            }
        }
    }
}