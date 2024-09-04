using System;
using System.Linq;
using LDtkLevelManager;
using UnityEditor;

namespace LDtkLevelManagerEditor
{
    public class ProjectDeletionProcessor : AssetModificationProcessor
    {
        public static AssetDeleteResult OnWillDeleteAsset(string path, RemoveAssetOptions options)
        {
            Project project = AssetDatabase.LoadAssetAtPath<Project>(path);
            if (project == null) return AssetDeleteResult.DidNotDelete;

            project.ClearBeforeDeletion();

            return AssetDeleteResult.DidNotDelete;
        }
    }
}