using LDtkVania;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace LDtkVaniaEditor
{
    [CustomEditor(typeof(MV_Level))]
    public class MV_LevelInspector : Editor
    {
        public VisualTreeAsset m_InspectorXML;

        public override VisualElement CreateInspectorGUI()
        {
            // Create a new VisualElement to be the root of our Inspector UI.
            VisualElement myInspector = new();

            // Add a simple label.
            myInspector.Add(new Label("This is a custom Inspector"));

            // Load from default reference.
            m_InspectorXML.CloneTree(myInspector);

            // Return the finished Inspector UI.
            return myInspector;
        }
    }
}
