using System;
using System.IO;
using System.Linq;
using LDtkVania;
using UnityEditor;
using UnityEngine;

namespace LDtkVaniaEditor
{
    public class SceneDeletionProcessor : AssetModificationProcessor
    {
        public static AssetDeleteResult OnWillDeleteAsset(string path, RemoveAssetOptions options)
        {
            var asset = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
            if (asset == null) return AssetDeleteResult.DidNotDelete;

            var labelList = AssetDatabase.GetLabels(asset);

            if (!labelList.Contains(MV_LevelScene.SceneLabelName))
            {
                return AssetDeleteResult.DidNotDelete;
            }

            if (labelList.Contains($"LDtkSceneDeletion"))
            {
                string assetRealPath = path.Replace("Assets", Application.dataPath);
                string assetMetaPath = assetRealPath + ".meta";
                File.Delete(assetRealPath);
                File.Delete(assetMetaPath);
                return AssetDeleteResult.DidDelete;
            }
            else
            {
                MV_Logger.Error($"Scene Level wrappers should only be deleted using the <color=#FFFFFF>LDtkVania Level Inspector</color> tool.", asset);
                return AssetDeleteResult.FailedDelete;
            }
        }
    }
}