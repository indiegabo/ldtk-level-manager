using UnityEngine;
using UnityEngine.UIElements;
using LDtkVania;
using UnityEditor.UIElements;
using LDtkUnity;
using UnityEditor;
using System.Collections.Generic;

namespace LDtkVaniaEditor
{
    public class WorldListItemElement : VisualElement
    {
        #region Static

        private static readonly HashSet<string> _expandedFoldouts = new();

        #endregion

        #region Fields

        private MV_World _world;

        WorldElement _worldElement;
        private Foldout _foldoutMain;

        #endregion

        #region Properties

        public MV_World World
        {
            get => _world;
            set
            {
                _world = value;
                SetWorld(_world);
            }
        }

        #endregion

        #region Constructors

        public WorldListItemElement()
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

        private void SetWorld(MV_World world)
        {
            if (_worldElement != null)
            {
                _foldoutMain.Remove(_worldElement);
            }

            // Create a new instance of MV_LevelsElement using the provided visual tree asset.
            _worldElement = new WorldElement(world);

            _foldoutMain.text = world.LDtkName;
            _foldoutMain.value = _expandedFoldouts.Contains(world.Iid);

            // Add the levels element to the foldout.
            _foldoutMain.Add(_worldElement);

            _foldoutMain.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue)
                {
                    if (!_expandedFoldouts.Contains(world.Iid))
                        _expandedFoldouts.Add(world.Iid);
                }
                else
                {
                    _expandedFoldouts.Remove(world.Iid);
                }
            });
        }

        #endregion
    }
}