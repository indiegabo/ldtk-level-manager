using UnityEditor;
using LDtkVania;
using UnityEngine;
using LDtkUnity;
using System.Collections.Generic;

namespace LDtkVaniaEditor
{
    class LDtkVaniaLevelsSyncer : AssetPostprocessor
    {
        private static string _projectToProcessPath;
        private static List<MV_ProcessedLevelEntry> _processingSubjectLevels = new();


        public static bool HasProjectToProcess => !string.IsNullOrEmpty(_projectToProcessPath);
        public static void SetProjectToProcess(string path) => _projectToProcessPath = path;
        public static void ClearProjectToProcess() => _projectToProcessPath = null;

        public static bool HasLevelsToProcess => _processingSubjectLevels.Count > 0;
        public static List<MV_ProcessedLevelEntry> ProcessingSubjectLevels => _processingSubjectLevels;
        public static void AddProcessSubjecLevel(MV_ProcessedLevelEntry entry) => _processingSubjectLevels.Add(entry);
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
            Dictionary<string, MV_Project> projects = GenerateProjectsDictionary();
            if (!projects.TryGetValue(projectIid.Iid, out MV_Project project))
            {
                Debug.LogWarning($"Project not found: {projectIid.Iid}");
                return;
            }

            project.SyncLevels();

            ClearProjectToProcess();
            // No need to process levels since the project is already synced
            ClearProcessSubjectLevels();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void ProcessLevels()
        {
            if (!HasLevelsToProcess) return;

            Dictionary<string, MV_Project> projects = GenerateProjectsDictionary();

            foreach (MV_ProcessedLevelEntry entry in ProcessingSubjectLevels)
            {
                if (!projects.TryGetValue(entry.projectIid, out MV_Project project))
                {
                    continue;
                }

                LDtkLevelFile levelFile = AssetDatabase.LoadAssetAtPath<LDtkLevelFile>(entry.levelAssetPath);

                if (levelFile == null) continue;
                project.ProcessLevelFile(entry.levelAssetPath, levelFile);
            }

            ClearProcessSubjectLevels();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

        }

        private static Dictionary<string, MV_Project> GenerateProjectsDictionary()
        {
            Dictionary<string, MV_Project> projects = new();
            string[] guids = AssetDatabase.FindAssets($"t:{nameof(MV_Project)}");
            for (int i = 0; i < guids.Length; i++)
            {
                string guid = guids[i];
                string path = AssetDatabase.GUIDToAssetPath(guid);
                MV_Project project = AssetDatabase.LoadAssetAtPath<MV_Project>(path);
                if (project == null || !project.HasProjectFile)
                {
                    continue;
                }
                projects.Add(project.LDtkProject.Iid, project);
            }

            return projects;
        }
    }
}