using UnityEditor;
using LDtkLevelManager;
using UnityEngine;
using LDtkUnity;
using System.Collections.Generic;

namespace LDtkLevelManagerEditor
{
    class LDtkLevelManagerLevelsSyncer : AssetPostprocessor
    {
        private static string _projectToProcessPath;
        private static List<ProcessedLevelEntry> _processingSubjectLevels = new();


        public static bool HasProjectToProcess => !string.IsNullOrEmpty(_projectToProcessPath);
        public static void SetProjectToProcess(string path) => _projectToProcessPath = path;
        public static void ClearProjectToProcess() => _projectToProcessPath = null;

        public static bool HasLevelsToProcess => _processingSubjectLevels.Count > 0;
        public static List<ProcessedLevelEntry> ProcessingSubjectLevels => _processingSubjectLevels;
        public static void AddProcessSubjecLevel(ProcessedLevelEntry entry) => _processingSubjectLevels.Add(entry);
        public static void ClearProcessSubjectLevels() => _processingSubjectLevels.Clear();

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
        {
            ProcessProject();
            ProcessLevels();
        }

        private static void ProcessProject()
        {
            if (!HasProjectToProcess) return;
            LDtkIid projectIid = AssetDatabase.LoadAssetAtPath<LDtkIid>(_projectToProcessPath);
            Dictionary<string, Project> projects = GenerateProjectsDictionary();
            if (!projects.TryGetValue(projectIid.Iid, out Project project))
            {
                Debug.LogWarning($"Project not found: {projectIid.Iid}");
                return;
            }

            project.ReSync();

            ClearProjectToProcess();
            ClearProcessSubjectLevels();// No need to process levels since the project is already synced

            EditorUtility.SetDirty(project);
            AssetDatabase.SaveAssetIfDirty(project);
        }

        private static void ProcessLevels()
        {
            if (!HasLevelsToProcess) return;

            Dictionary<string, Project> projects = GenerateProjectsDictionary();

            foreach (ProcessedLevelEntry entry in ProcessingSubjectLevels)
            {
                if (!projects.TryGetValue(entry.projectIid, out Project project))
                {
                    continue;
                }

                LDtkLevelFile levelFile = AssetDatabase.LoadAssetAtPath<LDtkLevelFile>(entry.levelAssetPath);

                if (levelFile == null) continue;
                project.ProcessLevelFile(entry.levelAssetPath, levelFile);
            }

            foreach (Project project in projects.Values)
            {
                project.EvaluateWorldAreas();
                EditorUtility.SetDirty(project);
                AssetDatabase.SaveAssetIfDirty(project);
            }

            ClearProcessSubjectLevels();
        }

        private static Dictionary<string, Project> GenerateProjectsDictionary()
        {
            Dictionary<string, Project> projects = new();
            string[] guids = AssetDatabase.FindAssets($"t:{nameof(Project)}");
            for (int i = 0; i < guids.Length; i++)
            {
                string guid = guids[i];
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Project project = AssetDatabase.LoadAssetAtPath<Project>(path);
                if (project == null || !project.IsInitialized)
                {
                    continue;
                }
                projects.Add(project.LDtkProject.Iid, project);
            }

            return projects;
        }


        [MenuItem("Assets/LDtkLevelManager/Resync Project", true, priority = 80)] // Enable validation
        private static bool ValidateResyncMenuItem()
        {
            // Check if any assets are selected
            if (Selection.assetGUIDs.Length == 0)
                return false; // No assets selected, hide the menu item

            if (Selection.assetGUIDs.Length > 1)
                return false; // More than one asset selected, hide the menu item

            string guid = Selection.assetGUIDs[0];
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            Project project = AssetDatabase.LoadAssetAtPath<Project>(assetPath);
            return project != null;
        }

        [MenuItem("Assets/LDtkLevelManager/Resync Project", false, priority = 80)]
        private static void ResyncProject()
        {

            string guid = Selection.assetGUIDs[0];
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            Project project = AssetDatabase.LoadAssetAtPath<Project>(assetPath);

            if (project == null)
            {
                LDtkLevelManager.Logger.Error($"Could not load project file at {assetPath}");
                return;
            }

            project.ReSync();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.FocusProjectWindow();

            Selection.activeObject = project;
        }
    }
}