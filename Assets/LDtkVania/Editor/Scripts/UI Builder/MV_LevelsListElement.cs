using System;
using UnityEngine;
using UnityEngine.UIElements;
using SpriteAnimations;
using LDtkVania;

namespace SpriteAnimations.Editor
{
    public class MV_LevelsListElement : VisualElement
    {
        #region Fields

        private MV_Level _level;
        private Label _label;

        #endregion

        #region Properties

        public MV_Level Level
        {
            get => _level;
            set
            {
                _level = value;
                _label.text = _level.Name;
            }
        }

        #endregion

        #region Constructors

        public MV_LevelsListElement(VisualTreeAsset tree)
        {
            style.justifyContent = Justify.Center;
            TemplateContainer template = tree.Instantiate();
            _label = template.Q<Label>("label-test");
            Add(template);
        }

        #endregion
    }
}