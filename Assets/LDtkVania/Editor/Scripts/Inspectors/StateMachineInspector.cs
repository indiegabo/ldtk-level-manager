using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace HandyFSM.Editor
{
    [CustomEditor(typeof(HandyMachine), true)]
    public class StateMachineInspector : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var container = new VisualElement();

            // If you're running a recent version of the package, or 2021.2, you can use
            // InspectorElement.FillDefaultInspector(container, serializedObject, this);
            // otherwise, the code below basically does the same thing
            InspectorElement.FillDefaultInspector(container, serializedObject, this);
            return container;
        }
    }
}