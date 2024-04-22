using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace HandyFSM.Editor
{
    [CustomPropertyDrawer(typeof(TriggerListItem), true)]
    public class TriggerListItemDrawer : PropertyDrawer
    {
        #region GUI

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            VisualTreeAsset treeAsset = Resources.Load<VisualTreeAsset>("UI Documents/TriggerListItemUI");
            TemplateContainer container = treeAsset.Instantiate();

            return container;
        }

        #endregion
    }
}