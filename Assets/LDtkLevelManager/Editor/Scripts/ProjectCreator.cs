using System.Collections.Generic;
using System.IO;
using System.Linq;
using LDtkUnity;
using LDtkLevelManager;
using UnityEditor;
using UnityEngine;
namespace LDtkLevelManagerEditor
{
    public static class ProjectCreator
    {
        [MenuItem("Assets/Create LDtkLevelManager Project", true, priority = 80)] // Enable validation
        private static bool ValidateCreationMenuItem()
        {
            // Check if any assets are selected
            if (Selection.assetGUIDs.Length == 0)
                return false; // No assets selected, hide the menu item

            if (Selection.assetGUIDs.Length > 1)
                return false; // More than one asset selected, hide the menu item

            string guid = Selection.assetGUIDs[0];
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            LDtkProjectFile projectFile = AssetDatabase.LoadAssetAtPath<LDtkProjectFile>(assetPath);
            return projectFile != null;
        }

        [MenuItem("Assets/Create LDtkLevelManager Project", false, priority = 80)]
        private static void RequestProjectCreation()
        {
            string guid = Selection.assetGUIDs[0];
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            LDtkProjectFile projectFile = AssetDatabase.LoadAssetAtPath<LDtkProjectFile>(assetPath);

            if (projectFile == null)
            {
                LDtkLevelManager.Logger.Error($"Could not load project file at {assetPath}");
                return;
            }

            if (ProjectAlreadyExists(projectFile, out string existingProjectPath))
            {
                LDtkLevelManager.Logger.Error($"Project already exists at {existingProjectPath}");
                return;
            }

            string fileNameToRemove = assetPath.Split("/").Last();
            string fileNameWithoutExtension = fileNameToRemove.Split(".").First();
            string directoryPath = assetPath.Replace(fileNameToRemove, string.Empty);
            string projectPath = Path.Combine(directoryPath, fileNameWithoutExtension + "_LDtkLevelManager.asset");

            Project project = ScriptableObject.CreateInstance<Project>();

            AssetDatabase.CreateAsset(project, projectPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.FocusProjectWindow();

            project.Initialize(projectFile);

            Selection.activeObject = project;
        }

        private static bool ProjectAlreadyExists(LDtkProjectFile projectFile, out string existingProjectPath)
        {
            existingProjectPath = string.Empty;
            List<Project> existingProjects = Project.FindAllProjects();
            int index = existingProjects.FindIndex(p => p.LDtkProjectFile == projectFile);

            if (index >= 0)
            {
                var asset = existingProjects[index];
                existingProjectPath = AssetDatabase.GetAssetPath(asset);
                return true;
            }

            return false;
        }
    }
}