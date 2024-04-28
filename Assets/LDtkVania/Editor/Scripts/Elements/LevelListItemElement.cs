using UnityEngine;
using UnityEngine.UIElements;
using LDtkVania;
using UnityEditor.UIElements;
using LDtkUnity;
using UnityEditor;
using System.Collections.Generic;

namespace LDtkVaniaEditor
{
    public class LevelListItemElement : VisualElement
    {
        #region Static

        private static readonly HashSet<string> _expandedFoldouts = new();

        #endregion

        #region Fields

        private MV_Level _level;

        LevelElement _levelElement;
        SerializedObject _serialized;
        private Foldout _foldoutMain;

        #endregion

        #region Properties

        public MV_Level Level
        {
            get => _level;
            set
            {
                _level = value;
                SetLevel(_level);
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="LevelListItemElement"/> class.
        /// </summary>
        /// <param name="tree">The visual tree asset to be used for creating the levels element.</param>
        public LevelListItemElement()
        {
            // Create a new foldout with its value set to false.
            _foldoutMain = new Foldout()
            {
                value = false
            };

            // Add the foldout to the current instance of MV_LevelsListElement.
            Add(_foldoutMain);
        }

        #endregion

        #region Define level

        private void SetLevel(MV_Level level)
        {
            if (_levelElement != null)
            {
                _foldoutMain.Remove(_levelElement);
            }

            // Create a new instance of MV_LevelsElement using the provided visual tree asset.
            _levelElement = new LevelElement(level);

            _foldoutMain.text = _level.name;
            _foldoutMain.value = _expandedFoldouts.Contains(level.Iid);
            _serialized = new(_level);
            _levelElement.Bind(_serialized);

            // Add the levels element to the foldout.
            _foldoutMain.Add(_levelElement);

            _foldoutMain.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue)
                {
                    if (!_expandedFoldouts.Contains(level.Iid))
                        _expandedFoldouts.Add(level.Iid);
                }
                else
                {
                    _expandedFoldouts.Remove(level.Iid);
                }
            });
        }

        #endregion
    }
}