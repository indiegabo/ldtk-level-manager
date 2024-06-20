using System;
using System.Collections.Generic;
using LDtkUnity;
using LDtkLevelManager;
using LDtkLevelManager.Utils;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace LDtkLevelManagerEditor
{
    public class MapView : GraphView
    {
        private Action<List<ISelectable>> _selectionAnalysisAction;
        private Action<MapLevelElement> _levelLoadToggleRequestAction;
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

        public void SetLevelLoadToggleRequestCallback(Action<MapLevelElement> action)
        {
            _levelLoadToggleRequestAction = action;
        }

        public void InitializeWorld(Project project, World world)
        {
            ClearLevels();
            LoadLevels(project, world);
            schedule.Execute(() => FrameWorld());
        }

        public void InitializeWorld(Project project, World world, MapViewTransform existingTransform)
        {
            ClearLevels();
            LoadLevels(project, world);
            UpdateViewTransform(existingTransform.position, existingTransform.scale);
        }

        public void AddLevel(LDtkUnity.Level level, LDtkLevelManager.LevelInfo levelInfo, Rect rect)
        {
            MapLevelElement levelElement = new(this, level, levelInfo, rect);
            levelElement.SetLevelLoadToggleRequestCallback(_levelLoadToggleRequestAction);
            _levelElements.Add(levelElement);
            AddElement(levelElement);
        }

        public void ClearLevels()
        {
            _levelElements.ForEach(levelElement => RemoveElement(levelElement));
            _levelElements.Clear();
        }

        private void LoadLevels(Project project, World world)
        {
            switch (world.WorldLayout)
            {
                case WorldLayout.LinearHorizontal:
                    LoadHorizontalLevels(project, world);
                    break;
                case WorldLayout.LinearVertical:
                    LoadVerticalLevels(project, world);
                    break;
                case WorldLayout.Free:
                case WorldLayout.GridVania:
                    LoadGridVaniaLevels(project, world);
                    break;
            }
        }

        private void LoadGridVaniaLevels(Project project, World world)
        {
            _worldRect = new Rect(0, 0, 0, 0);

            foreach (LDtkUnity.Level level in world.Levels)
            {
                project.TryGetLevel(level.Iid, out LDtkLevelManager.LevelInfo levelInfo);

                Rect levelRect = new()
                {
                    width = level.UnityWorldRect.width * 0.25f,
                    height = level.UnityWorldRect.height * 0.25f,
                    x = level.UnityWorldRect.x * 0.25f,
                    y = level.UnityWorldRect.y * 0.25f
                };

                _worldRect.Expand(levelRect);

                AddLevel(level, levelInfo, levelRect);
            }
        }

        private void LoadHorizontalLevels(Project project, World world)
        {
            _worldRect = new Rect(0, 0, 0, 0);
            float currentPos = 0;

            foreach (LDtkUnity.Level level in world.Levels)
            {

                project.TryGetLevel(level.Iid, out LDtkLevelManager.LevelInfo levelInfo);

                Rect levelRect = new()
                {
                    width = level.UnityWorldRect.width * 0.25f,
                    height = level.UnityWorldRect.height * 0.25f,
                    x = currentPos,
                    y = 0
                };

                _worldRect.Expand(levelRect);
                AddLevel(level, levelInfo, levelRect);
                currentPos = currentPos + levelRect.width + 25f;
            }
        }

        private void LoadVerticalLevels(Project project, World world)
        {
            _worldRect = new Rect(0, 0, 0, 0);
            float currentPos = 0;

            foreach (LDtkUnity.Level level in world.Levels)
            {

                project.TryGetLevel(level.Iid, out LDtkLevelManager.LevelInfo levelInfo);

                Rect levelRect = new()
                {
                    width = level.UnityWorldRect.width * 0.25f,
                    height = level.UnityWorldRect.height * 0.25f,
                    x = 0,
                    y = currentPos
                };

                _worldRect.Expand(levelRect);
                AddLevel(level, levelInfo, levelRect);
                currentPos = currentPos + levelRect.height + 25f;
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