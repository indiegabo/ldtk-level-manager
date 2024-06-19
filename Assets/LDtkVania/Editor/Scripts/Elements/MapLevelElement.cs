using LDtkUnity;
using LDtkVania;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace LDtkVaniaEditor
{
    public class MapLevelElement : GraphElement
    {
        private MV_Level _mvLevel;
        private Level _level;
        private bool _pointerIsOver = false;
        private MapView _mapView;
        private bool _loaded = false;

        public MV_Level MVLevel => _mvLevel;
        public Level Level => _level;

        public bool Loaded
        {
            get => _loaded;
            set
            {
                _loaded = value;
                EvaluateState();
            }
        }

        public MapLevelElement(MapView mapView, Level level, MV_Level mvLevel, Rect levelRect)
        {
            base.capabilities |= Capabilities.Selectable | Capabilities.Groupable;

            _mapView = mapView;
            _level = level;
            _mvLevel = mvLevel;

            Sprite sprite = Resources.Load<Sprite>("world-tile");
            style.backgroundImage = Background.FromSprite(sprite);
            style.unityBackgroundImageTintColor = new StyleColor(new Color(1, 1, 1, 0.5f));

            this.AddManipulator(new MapLevelMouseManipulator(this));
            SetPosition(levelRect);
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