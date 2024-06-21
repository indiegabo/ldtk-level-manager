using System;
using LDtkUnity;
using LDtkLevelManager;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace LDtkLevelManagerEditor
{
    public delegate void LoadedStatusChangedEvent(bool isLoaded);
    public class MapLevelElement : GraphElement
    {
        private Action<MapLevelElement> _levelLoadToggleRequestAction;

        private LDtkLevelManager.LevelInfo _levelInfo;
        private LDtkUnity.Level _level;
        private bool _pointerIsOver = false;
        private MapView _mapView;
        private LoadedLevelEntry _loadedLevelEntry;
        private Rect _levelRect;

        private StyleColor _normalColor = new(new Color(1, 1, 1, 0.5f));
        private StyleColor _highlightedColor = new(new Color(1, 1, 1, 1f));
        private StyleColor _loadedColor = new(new Color(0.82f, 0.29f, 0.84f, 1f));

        public LDtkLevelManager.LevelInfo Info => _levelInfo;
        public LDtkUnity.Level Level => _level;
        public Rect LevelRect => _levelRect;

        public bool Loaded => _loadedLevelEntry != null;

        public event LoadedStatusChangedEvent LoadedStatusChanged;

        public MapLevelElement(MapView mapView, LDtkUnity.Level level, LDtkLevelManager.LevelInfo levelInfo, Rect levelRect)
        {
            base.capabilities |= Capabilities.Selectable | Capabilities.Groupable;

            _mapView = mapView;
            _level = level;
            _levelInfo = levelInfo;
            _levelRect = levelRect;

            Sprite sprite = Resources.Load<Sprite>("map-level-tile");
            style.backgroundImage = Background.FromSprite(sprite);
            style.unityBackgroundImageTintColor = new StyleColor(new Color(1, 1, 1, 0.5f));

            this.AddManipulator(new MapLevelMouseManipulator(this));
            SetPosition(levelRect);

            if (LevelEditorSettings.instance.TryGetLoadedLevel(_levelInfo.Iid, out LoadedLevelEntry entry))
            {
                RegisterLoadedEntry(entry);
            }

            AddLevelNameLabel(levelRect.size);
            EvaluateState();
        }

        public void SetLevelLoadToggleRequestCallback(Action<MapLevelElement> action)
        {
            _levelLoadToggleRequestAction = action;
        }

        public void ToggleLoaded()
        {
            _levelLoadToggleRequestAction?.Invoke(this);
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

        private void AddLevelNameLabel(Vector2 levelSize)
        {
            string name = _levelInfo.Name;

            if (levelSize.x < 120)
            {
                name = name[..5] + "...";
            }

            VisualElement container = new();
            container.style.position = Position.Absolute;
            container.style.alignSelf = Align.Center;
            container.style.top = levelSize.y / 2;
            container.style.textOverflow = TextOverflow.Ellipsis;

            Label label = new()
            {
                text = name,
            };

            label.style.height = 0; // This is somewhat a hack so the level element doesn't get blocked by the label upon clicking in it
            label.style.width = levelSize.x;
            label.style.unityTextAlign = TextAnchor.MiddleCenter;
            float maxLabelWidth = levelSize.x * 0.8f; // 80% of level's width
            int fontSize = Mathf.RoundToInt(label.text.Length / maxLabelWidth * 100);
            label.style.fontSize = Mathf.Clamp(fontSize, 12, 30);
            label.style.textOverflow = TextOverflow.Ellipsis;
            label.style.color = Color.white;

            container.Add(label);
            Add(container);
        }

        private void EvaluateState()
        {
            if (Loaded)
            {
                style.unityBackgroundImageTintColor = _loadedColor;
                return;
            }

            if (_pointerIsOver || selected)
            {
                style.unityBackgroundImageTintColor = _highlightedColor;
                return;
            }

            style.unityBackgroundImageTintColor = _normalColor;
        }

        public void RegisterLoadedEntry(LoadedLevelEntry entry)
        {
            _loadedLevelEntry = entry;
            EvaluateState();
            LoadedStatusChanged?.Invoke(true);

            entry.Unloaded += () =>
            {
                _loadedLevelEntry = null;
                EvaluateState();
                LoadedStatusChanged?.Invoke(false);
            };
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
                    _levelElement.ToggleLoaded();
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