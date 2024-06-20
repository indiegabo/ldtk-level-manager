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
            Project project = AssetDatabase.LoadAssetAtPath<Project>(path);
            if (project == null) return AssetDeleteResult.DidNotDelete;

            project.ClearBeforeDeletion();

            return AssetDeleteResult.DidNotDelete;
        }
    }
}