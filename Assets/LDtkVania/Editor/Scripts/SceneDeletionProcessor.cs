using System;
using System.Linq;
using LDtkVania;
using UnityEditor;

namespace LDtkVaniaEditor
{
    public class SceneDeletionProcessor : AssetModificationProcessor
    {
        public static AssetDeleteResult OnWillDeleteAsset(string path, RemoveAssetOptions options)
        {
            var asset = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
            if (asset == null) return AssetDeleteResult.DidNotDelete;

            var labelList = AssetDatabase.GetLabels(asset);

            if (labelList.Contains(MV_LevelScene.SceneLabelName))
            {
                MV_Logger.Error($"Scene Level wrappers should only be deleted using the <color=#FFFFFF>LDtkVania Level Inspector</color> tool.", asset);
                return AssetDeleteResult.FailedDelete;
            }

            return AssetDeleteResult.DidNotDelete;
        }
    }
}