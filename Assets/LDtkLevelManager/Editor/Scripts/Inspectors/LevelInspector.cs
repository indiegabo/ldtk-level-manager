using LDtkVania;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace LDtkVaniaEditor
{
    [CustomEditor(typeof(LevelInfo))]
    public class LevelInspector : Editor
    {
        public VisualTreeAsset _inspectorXML;

        public override VisualElement CreateInspectorGUI()
        {
            LevelElement levelsElement = new(target as LevelInfo);
            return levelsElement;
        }
    }
}
