using System;
using System.Linq;
using LDtkLevelManager;
using UnityEditor;

namespace LDtkLevelManagerEditor
{
    public class SceneDeletionProcessor : AssetModificationProcessor
    {
        public static AssetDeleteResult OnWillDeleteAsset(string path, RemoveAssetOptions options)
        {
            var asset = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
            if (asset == null) return AssetDeleteResult.DidNotDelete;

            var labelList = AssetDatabase.GetLabels(asset);

            if (labelList.Contains(LevelScene.SceneLabelName))
            {
                Logger.Error($"Scene Level wrappers should only be deleted using the <color=#FFFFFF>LDtkLevelManager Level Inspector</color> tool.", asset);
                return AssetDeleteResult.FailedDelete;
            }

            return AssetDeleteResult.DidNotDelete;
        }
    }
}