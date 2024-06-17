using System;
using System.Collections.Generic;
using LDtkUnity;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace LDtkVaniaEditor
{
    public class MapLevelElement : GraphElement, ICollectibleElement
    {
        private Level _level;
        private bool _pointerIsOver = false;
        private MapView _mapView;

        public Level Level => _level;

        public MapLevelElement(Level level, MapView mapView)
        {
            base.capabilities |= Capabilities.Selectable | Capabilities.Groupable;

            _level = level;
            _mapView = mapView;
            Sprite sprite = Resources.Load<Sprite>("world-tile");

            style.backgroundImage = Background.FromSprite(sprite);
            style.unityBackgroundImageTintColor = new StyleColor(new Color(1, 1, 1, 0.5f));

            Rect rect = new()
            {
                width = level.UnityWorldRect.width * 0.25f,
                height = level.UnityWorldRect.height * 0.25f,
                x = level.UnityWorldRect.x * 0.25f,
                y = level.UnityWorldRect.y * 0.25f
            };

            this.AddManipulator(new MapLevelMouseManipulator(this));

            SetPosition(rect);
        }

        public override void OnSelected()
        {
            EvaluateState();
        }

        public override void OnUnselected()
        {
            EvaluateState();
        }

        public void SetPointerOver(bool pointerIsOver)
        {
            _pointerIsOver = pointerIsOver;
            EvaluateState();
        }

        private void EvaluateState()
        {
            if (_pointerIsOver || selected)
            {
                style.unityBackgroundImageTintColor = new StyleColor(new Color(1, 1, 1, 1));
            }
            else
            {
                style.unityBackgroundImageTintColor = new StyleColor(new Color(1, 1, 1, 0.5f));
            }
        }

        public void CollectElements(HashSet<GraphElement> collectedElementSet, Func<GraphElement, bool> conditionFunc)
        {
            throw new NotImplementedException();
        }
    }

    public class MapLevelMouseManipulator : MouseManipulator
    {
        private MapLevelElement _levelElement;

        public MapLevelMouseManipulator(MapLevelElement levelElement)
        {
            _levelElement = levelElement;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<PointerEnterEvent>(OnPointerEnter);
            target.RegisterCallback<PointerLeaveEvent>(OnPointerLeave);
            target.RegisterCallback<MouseDownEvent>(OnMouseDown);
            target.RegisterCallback<MouseUpEvent>(OnMouseUp);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<MouseDownEvent>(OnMouseDown);
            target.UnregisterCallback<MouseUpEvent>(OnMouseUp);
            target.UnregisterCallback<PointerEnterEvent>(OnPointerEnter);
            target.UnregisterCallback<PointerLeaveEvent>(OnPointerLeave);
        }

        protected void OnMouseDown(MouseDownEvent e)
        {
            if (e.target is not MapLevelElement levelElement) return;
            switch (e.button)
            {
                case 0: // Left Mouse Button
                    break;
                case 1: // Right Mouse Button
                    break;
                case 2: // Middle Mouse Button
                    break;
            }
        }

        protected void OnMouseUp(MouseUpEvent e)
        {
            if (e.target is not MapLevelElement levelElement) return;
            switch (e.button)
            {
                case 0: // Left Mouse Button
                    break;
                case 1: // Right Mouse Button
                    break;
                case 2: // Middle Mouse Button
                    break;
            }
        }

        private void OnPointerEnter(PointerEnterEvent evt)
        {
            if (evt.target is MapLevelElement levelElement)
            {
                levelElement.SetPointerOver(true);
            }
        }

        private void OnPointerLeave(PointerLeaveEvent evt)
        {
            if (evt.target is MapLevelElement levelElement)
            {
                levelElement.SetPointerOver(false);
            }
        }
    }
}