using System;
using System.Collections.Generic;
using System.Linq;
using LDtkUnity;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace LDtkVaniaEditor
{
    public class MapView : GraphView
    {
        private Action<List<ISelectable>> _selectionAnalysisAction;
        private List<MapLevelElement> _levelElements = new();
        private Node _node;

        public new class UxmlFactory : UxmlFactory<MapView, GraphView.UxmlTraits> { }


        public MapView()
        {
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

        public void AddLevel(Level level)
        {
            MapLevelElement levelElement = new(level, this);
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