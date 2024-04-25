using System.Collections.Generic;
using LDtkVania;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace LDtkVaniaEditor
{
    [CustomEditor(typeof(MV_Project))]
    public class ProjectInspector : Editor
    {
        public VisualTreeAsset _mainViewUXML;

        public override VisualElement CreateInspectorGUI()
        {
            List<MV_Level> levels = MV_Project.Instance.GetLevels();

            TabViewElement tabViewElement = new();
            ProjectMainViewElement mainViewElement = new();
            ProjectLevelsViewElement levelsViewElement = new(levels);

            tabViewElement.AddTab("Main", mainViewElement, true);
            tabViewElement.AddTab("Levels", levelsViewElement);

            return tabViewElement;
        }
    }
}
