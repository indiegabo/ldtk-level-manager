using System;
using System.Linq;
using LDtkVania;
using UnityEditor;

namespace LDtkVaniaEditor
{
    public class ProjectDeletionProcessor : AssetModificationProcessor
    {
        public static AssetDeleteResult OnWillDeleteAsset(string path, RemoveAssetOptions options)
        {
            MV_Project project = AssetDatabase.LoadAssetAtPath<MV_Project>(path);
            if (project == null) return AssetDeleteResult.DidNotDelete;

            project.ClearBeforeDeletion();

            return AssetDeleteResult.DidNotDelete;
        }
    }
}