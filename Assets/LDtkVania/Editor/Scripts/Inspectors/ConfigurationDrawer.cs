using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System;

namespace HandyFSM.Editor
{
    [CustomPropertyDrawer(typeof(Configuration), true)]
    public class ConfigurationDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            VisualTreeAsset treeAsset = Resources.Load<VisualTreeAsset>("UI Documents/ConfigurationUI");
            TemplateContainer container = treeAsset.Instantiate();

            VisualElement separator = container.Q<VisualElement>("separator");
            Type targetType = property.serializedObject.targetObject.GetType();
            if (targetType == typeof(HandyMachine))
                separator.style.display = DisplayStyle.None;

            return container;
        }
    }
}
