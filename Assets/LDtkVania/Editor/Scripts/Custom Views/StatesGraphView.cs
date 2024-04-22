using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace HandyFSM.Editor
{
    public class StatesGraphView : GraphView
    {
        #region Fields

        ContentDragger _contenteDragger;

        #endregion

        #region Getters

        public new class UxmlFactory : UxmlFactory<StatesGraphView, GraphView.UxmlTraits> { }

        #endregion

        #region Constructors

        public StatesGraphView()
        {
            Insert(0, new GridBackground());
            _contenteDragger = new ContentDragger();
            this.AddManipulator(_contenteDragger);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            StyleSheet styleSheet = Resources.Load<StyleSheet>("Styles/handy-fsm-state-visualizer-styles");
            styleSheets.Add(styleSheet);
        }

        #endregion

        #region Nodes

        public void FitAllNodes()
        {
            var rectToFit = CalculateRectToFitAll(contentViewContainer);
            CalculateFrameTransform(rectToFit, layout, 1, out Vector3 frameTranslation, out Vector3 frameScaling);
            Matrix4x4.TRS(frameTranslation, Quaternion.identity, frameScaling);
            UpdateViewTransform(frameTranslation, frameScaling);
        }

        #endregion
    }
}