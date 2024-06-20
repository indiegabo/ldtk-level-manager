#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace LDtkVania.Utils
{
    public static class AddressableExtensions
    {
        public static void SetAddressableGroup(this Object obj, string groupName)
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;

            if (settings)
            {
                var group = settings.FindGroup(groupName);
                if (!group)
                    group = settings.CreateGroup(groupName, false, false, true, null, typeof(ContentUpdateGroupSchema), typeof(BundledAssetGroupSchema));

                var assetpath = AssetDatabase.GetAssetPath(obj);
                var guid = AssetDatabase.AssetPathToGUID(assetpath);

                var e = settings.CreateOrMoveEntry(guid, group, false, false);
                var entriesAdded = new List<AddressableAssetEntry> { e };

                group.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entriesAdded, false, true);
                settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entriesAdded, true, false);
            }
        }

        public static bool TrySetAsAddressable(this Object obj, string address, string groupName, string labelName = null)
        {
            string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(obj));

            if (string.IsNullOrEmpty(guid)) return false;

            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;

            if (string.IsNullOrEmpty(groupName)) return false;

            AddressableAssetGroup group = settings.FindGroup(groupName)
                ?? settings.CreateGroup(groupName, false, true, true, null, typeof(ContentUpdateGroupSchema), typeof(BundledAssetGroupSchema));

            AssetReference assetReference = settings.CreateAssetReference(guid);
            assetReference.SetEditorAsset(obj);
            AddressableAssetEntry entry = settings.CreateOrMoveEntry(guid, group);

            // This will fail if asset is being created on AssetPostprocessor
            if (entry == null) return false;

            entry.SetAddress(address);

            if (!string.IsNullOrEmpty(labelName))
            {
                settings.AddLabel(labelName);
                entry.SetLabel(labelName, true);
            }

            settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entry, true);
            return true;
        }

        public static void UnsetAdressable(this Object obj)
        {
            string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(obj));
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            settings.RemoveAssetEntry(guid);
        }

        public static void UnsetAdressable(string guid)
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            settings.RemoveAssetEntry(guid);
        }
    }
}
#endif