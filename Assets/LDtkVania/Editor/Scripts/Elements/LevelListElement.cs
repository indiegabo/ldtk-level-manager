using UnityEngine;
using UnityEngine.UIElements;
using LDtkVania;
using UnityEditor.UIElements;
using UnityEditor;
using System.Collections.Generic;

namespace LDtkVaniaEditor
{
    public class LevelsListElement : VisualElement
    {
        private const string TemplateName = "LevelsListInspector";

        private MV_Project _project;
        private List<MV_Level> _levels;

        public LevelsListElement(MV_Project project, List<MV_Level> levels)
        {
            _project = project;
            _levels = levels;
        }
    }
}