using System;
using System.Collections.Generic;
using System.Linq;
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
        private List<MapLevelElement> _levelElements = new();
        private Rect _worldRect;

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

        public void InitializeWorld(MV_Project project, World world)
        {
            ClearLevels();

            if (world.WorldLayout != WorldLayout.GridVania)
            {
                Debug.LogWarning("Only grid-based worlds are supported.");
                return;
            }

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

                // if (mvLevel.HasScene && !_loadedScenes.ContainsKey(mvLevel.Iid))
                // {
                //     string path = AssetDatabase.GUIDToAssetPath(mvLevel.Scene.AssetGuid);
                //     Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
                //     _loadedScenes.Add(mvLevel.Iid, scene);
                // }
                // else if (!_loadedObjects.ContainsKey(mvLevel.Iid))
                // {
                //     GameObject obj = Instantiate(mvLevel.Asset) as GameObject;
                //     obj.name = mvLevel.Name;
                //     _loadedObjects.Add(mvLevel.Iid, obj);
                // }
            }
        }

        public void AddLevel(Level level, MV_Level mvLevel, Rect rect)
        {
            MapLevelElement levelElement = new(this, level, mvLevel, rect);
            _levelElements.Add(levelElement);
            AddElement(levelElement);
        }

        public void ClearLevels()
        {
            _levelElements.ForEach(levelElement => RemoveElement(levelElement));
            _levelElements.Clear();
        }

        private void OnViewTransformChanged(GraphView graphView)
        {
            // foreach (var element in graphView.graphElements) // Update all elements
            // {
            //     if (element is TestElement myElement)
            //     {
            //         Debug.Log(element);
            //         // Calculate the desired position (e.g., center of the graph)
            //         Vector2 desiredWorldPosition = graphView.contentViewContainer.WorldToLocal(graphView.viewTransform.position);

            //         // Convert to viewport-relative coordinates
            //         Vector2 viewportPosition = graphView.viewTransform.matrix.inverse.MultiplyPoint(desiredWorldPosition);

            //         // Update the element's position
            //         myElement.SetPosition(new Rect(viewportPosition, myElement.layout.size));
            //     }
            // }
        }

        public void TriggerSelectionAnalysis()
        {
            _selectionAnalysisAction?.Invoke(selection);
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
}