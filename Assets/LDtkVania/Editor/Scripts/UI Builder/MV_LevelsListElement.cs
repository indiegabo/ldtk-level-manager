using UnityEngine;
using UnityEngine.UIElements;
using LDtkVania;
using UnityEditor.UIElements;
using LDtkUnity;
using UnityEditor;

namespace LDtkVaniaEditor
{
    public class MV_LevelsListElement : VisualElement
    {
        #region Fields

        private MV_Level _level;

        MV_LevelsElement _levelsElement;
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
                _foldoutMain.text = _level.Name;
                _serialized = new(_level);
                _levelsElement.Bind(_serialized);
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MV_LevelsListElement"/> class.
        /// </summary>
        /// <param name="tree">The visual tree asset to be used for creating the levels element.</param>
        public MV_LevelsListElement(VisualTreeAsset tree)
        {
            // Create a new instance of MV_LevelsElement using the provided visual tree asset.
            _levelsElement = new MV_LevelsElement(tree);

            // Create a new foldout with its value set to false.
            _foldoutMain = new Foldout()
            {
                value = false
            };

            // Add the levels element to the foldout.
            _foldoutMain.Add(_levelsElement);

            // Add the foldout to the current instance of MV_LevelsListElement.
            Add(_foldoutMain);
        }

        #endregion
    }
}