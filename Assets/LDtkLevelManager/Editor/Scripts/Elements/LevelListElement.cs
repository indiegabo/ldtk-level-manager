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

        private Project _project;
        private List<LevelInfo> _levels;

        public LevelsListElement(Project project, List<LevelInfo> levels)
        {
            _project = project;
            _levels = levels;
        }
    }
}