using LDtkVania;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace LDtkVaniaEditor
{
    [CustomEditor(typeof(MV_Level))]
    public class MV_LevelInspector : Editor
    {
        public VisualTreeAsset _inspectorXML;

        public override VisualElement CreateInspectorGUI()
        {
            MV_LevelsElement levelsElement = new(_inspectorXML);
            return levelsElement;
        }
    }
}
