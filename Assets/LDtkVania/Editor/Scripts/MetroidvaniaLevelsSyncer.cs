using LDtkVania;
using UnityEditor;
using UnityEngine;

namespace LDtkVaniaEditor
{
    class MetroidvaniaLevelsSyncer : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
        {
            if (!MV_Project.SilentInstanceCheck) return;
            if (!MV_Project.Instance.SyncLevelsAtCompile) return;
            // Debug.Log($"OnPostprocessAllAssets {MV_Project.Instance.SyncLevelsAtCompile}");
            // MV_Project.Instance.Clear();
            MV_Project.Instance.SyncLevels();
        }
    }
}