using System;
using System.Collections.Generic;
using LDtkUnity;
using LDtkVania;
using LDtkVania.Utils;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace LDtkVaniaEditor
{
    public class MapView : GraphView
    {
        private Action<List<ISelectable>> _selectionAnalysisAction;
        private Action<MapLevelElement> _levelLoadRequestAction;
        private List<MapLevelElement> _levelElements = new();
        private Rect _worldRect;

        public List<MapLevelElement> LevelElements => _levelElements;

        public new class UxmlFactory : UxmlFactory<MapView, GraphView.UxmlTraits> { }

        public MapView()
        {
            _worldRect = new Rect(0, 0, 0, 0);
            Insert(0, new GridBackground()
            {
                name = "grid-background",
            });

            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new MapViewRectangleSelector(this));

            StyleSheet styleSheet = Resources.Load<StyleSheet>("Styles/MapView_style");
            styleSheets.Add(styleSheet);

            viewTransformChanged += OnViewTransformChanged;
        }

        public void SetSelectionAnalysisCallback(Action<List<ISelectable>> selectionAnalysisAction)
        {
            _selectionAnalysisAction = selectionAnalysisAction;
        }

        public void SetLevelLoadRequestCallback(Action<MapLevelElement> levelLoadRequestAction)
        {
            _levelLoadRequestAction = levelLoadRequestAction;
        }

        public void InitializeWorld(MV_Project project, World world)
        {
            ClearLevels();

            if (world.WorldLayout != WorldLayout.GridVania)
            {
                Debug.LogWarning("Only grid-based worlds are supported for now.");
                return;
            }

            LoadLevels(project, world);
            schedule.Execute(() => FrameWorld());
        }

        public void InitializeWorld(MV_Project project, World world, MapViewTransform existingTransform)
        {
            ClearLevels();

            if (world.WorldLayout != WorldLayout.GridVania)
            {
                Debug.LogWarning("Only grid-based worlds are supported for now.");
                return;
            }

            LoadLevels(project, world);
            UpdateViewTransform(existingTransform.position, existingTransform.scale);
        }

        public void AddLevel(Level level, MV_Level mvLevel, Rect rect)
        {
            MapLevelElement levelElement = new(this, level, mvLevel, rect);
            levelElement.SetLevelLoadRequestCallback(_levelLoadRequestAction);
            _levelElements.Add(levelElement);
            AddElement(levelElement);
        }

        public void ClearLevels()
        {
            _levelElements.ForEach(levelElement => RemoveElement(levelElement));
            _levelElements.Clear();
        }

        private void LoadLevels(MV_Project project, World world)
        {
            _worldRect = new Rect(0, 0, 0, 0);

            foreach (Level level in world.Levels)
            {
                project.TryGetLevel(level.Iid, out MV_Level mvLevel);

                Rect levelRect = new()
                {
                    width = level.UnityWorldRect.width * 0.25f,
                    height = level.UnityWorldRect.height * 0.25f,
                    x = level.UnityWorldRect.x * 0.25f,
                    y = level.UnityWorldRect.y * 0.25f
                };

                _worldRect.Expand(levelRect);

                AddLevel(level, mvLevel, levelRect);
            }
        }

        private void OnViewTransformChanged(GraphView graphView)
        {
            MapEditorSettings.instance.MapViewTransform = MapViewTransform.From(viewTransform);
        }

        public void TriggerSelectionAnalysis()
        {
            _selectionAnalysisAction?.Invoke(selection);
        }

        private void FrameWorld()
        {
            FrameAll();
            MapViewTransform transform = MapViewTransform.From(viewTransform);
            MapEditorSettings.instance.MapViewTransform = transform;
        }
    }
    public class MapViewRectangleSelector : RectangleSelector
    {
        private readonly MapView _mapView;

        public MapViewRectangleSelector(MapView mapView)
        {
            _mapView = mapView;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            base.RegisterCallbacksOnTarget();
            target.RegisterCallback<MouseUpEvent>(OnMouseUp);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            base.UnregisterCallbacksFromTarget();
            target.UnregisterCallback<MouseUpEvent>(OnMouseUp);
        }

        private void OnMouseUp(MouseUpEvent e)
        {
            if (e.button != (int)MouseButton.LeftMouse) return;
            _mapView.TriggerSelectionAnalysis();
        }
    }

    [System.Serializable]
    public struct MapViewTransform
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;

        public static MapViewTransform From(ITransform iTransform)
        {
            return new MapViewTransform
            {
                position = iTransform.position,
                rotation = iTransform.rotation,
                scale = iTransform.scale
            };
        }
    }
}