using LDtkUnity;
using LDtkUnity.Editor;
using LDtkVania;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
namespace LDtkVaniaEditor
{
    public class LDtkProjectPostProcessor : LDtkPostprocessor
    {
        protected override void OnPostprocessProject(GameObject root)
        {
            // Debug.Log($"Post process LDtk project: {root.name}");
        }

        protected override void OnPostprocessLevel(GameObject root, LdtkJson projectJson)
        {
            if (!TryProject(projectJson, out MV_Project project))
            {
                return;
            }

            LDtkIid lDtkIid = root.GetComponent<LDtkIid>();

            if (!project.TryGetLevel(lDtkIid.Iid, out MV_Level level))
            {
                // CreateLevel(root.name, project);
            }
            else
            {
                // Object asset = AssetDatabase.LoadAssetAtPath<Object>(location.InternalId);
                // level.UpdateInfo(root.GetComponent<LDtkComponentLevel>(), null, root, file);
            }

            // LDtkIid lDtkIid = root.GetComponent<LDtkIid>();
            // LDtkLevelFile file = AssetDatabase.LoadAssetAtPath<LDtkLevelFile>(level.AssetPath);

            // if (!project.TryGetLevel(lDtkIid.Iid, out MV_Level level))
            // {
            //     return;
            // }
            // else
            // {
            //     Object asset = AssetDatabase.LoadAssetAtPath<Object>(location.InternalId);
            //     level.UpdateInfo(root.GetComponent<LDtkComponentLevel>(), null, root, file);
            // }
            // LDtkIid lDtkIid = root.GetComponent<LDtkIid>();
            // Debug.Log($"Post process LDtk level: {root.name}");
        }

        private void CreateLevel(string name, MV_Project project)
        {
            if (!TryLevelFile(name, out LDtkLevelFile file, out string guid))
            {
                MV_Logger.Error($"Could not find level file for iid: {name}");
            }

            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            Object asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);

            Debug.Log($"Creating level: {asset.name}");

            // LDtkLevelFile file = AssetDatabase.LoadAssetAtPath<LDtkLevelFile>(level.AssetPath);
        }

        private bool TryProject(LdtkJson projectJson, out MV_Project project)
        {
            string[] guids = AssetDatabase.FindAssets($"t:{nameof(MV_Project)}");

            for (int i = 0; i < guids.Length; i++)
            {
                string guid = guids[i];
                string path = AssetDatabase.GUIDToAssetPath(guid);
                MV_Project possibleProject = AssetDatabase.LoadAssetAtPath<MV_Project>(path);
                if (possibleProject == null || possibleProject.LDtkProject?.Iid != projectJson.Iid)
                {
                    continue;
                }
                project = possibleProject;
                return true;
            }

            project = null;
            return false;
        }

        private bool TryLevelFile(string name, out LDtkLevelFile level, out string levelGuid)
        {
            Debug.Log($"Try level: {name}");
            string[] guids = AssetDatabase.FindAssets(name);

            for (int i = 0; i < guids.Length; i++)
            {
                string guid = guids[i];
                string path = AssetDatabase.GUIDToAssetPath(guid);
                // LDtkLevelFile possibleLevel = AssetDatabase.LoadAssetAtPath<LDtkLevelFile>(path);

                var test = AssetDatabase.LoadAssetAtPath<Object>(path);

                if (test == null) continue;
                Debug.Log(test.name + " " + path);

                levelGuid = guid;
                level = null;
                return true;
            }

            levelGuid = null;
            level = null;
            return false;
        }
    }
}