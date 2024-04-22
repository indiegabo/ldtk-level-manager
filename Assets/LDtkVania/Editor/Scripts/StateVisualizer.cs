using System;
using System.Collections.Generic;
using System.Linq;
using HandyFSM.Registering;
using UnityEngine;
using UnityEngine.UIElements;

namespace HandyFSM.Editor
{
    public class StateVisualizer
    {
        #region Fields

        private VisualElement _root;
        private StatesGraphView _graphView;

        private IState _state;
        private IState _fromState;

        private Session _session;

        #endregion

        #region Getters

        public VisualElement Root => _root;

        #endregion

        #region Constructors

        public StateVisualizer()
        {
            VisualTreeAsset tree = Resources.Load<VisualTreeAsset>("UI Documents/StateVisualizerUI");
            TemplateContainer template = tree.CloneTree();
            template.style.flexGrow = 1;

            _root = template;
            _graphView = _root.Q<StatesGraphView>();
        }

        #endregion

        #region Flow

        public void LoadSession(Session session)
        {
            if (session == null)
            {
                _session = null;
                return;
            }

            _session = session;

            int recordsCount = _session.Records.Count;
            Debug.Log($"Records count: {recordsCount}");
            if (recordsCount > 0)
            {
                Record record = _session.Records.Last();
                Debug.Log(record.State.Name);
                BuildView(record);
            }
        }

        public void Dismiss()
        {
            _session = null;
        }

        public void RegisterState(IState state)
        {
            Debug.Log(_session);
            Record record = _session.Register(state);
            Debug.Log(record);
            BuildView(record);
        }

        public void BuildView(Record record)
        {
            _graphView.DeleteElements(_graphView.nodes);

            List<Type> types = new();

            Type currentType = record.State.GetType();
            Type stateType = typeof(State);
            Type scriptableStateType = typeof(ScriptableState);

            while (currentType != stateType && currentType != scriptableStateType)
            {
                types.Add(currentType);
                currentType = currentType.BaseType;
            }

            types.Reverse();

            Rect containerRect = _graphView.contentViewContainer.contentRect;

            if (types.Count - 1 <= 0)
            {
                StateNode node = new()
                {
                    title = types[0].Name
                };
                float centerX = containerRect.width / 2;

                node.style.left = centerX;
                _graphView.AddElement(node);
                return;
            }
            int total = types.Count;
            for (int i = 0; i < total; i--)
            {
            }

            _graphView.FitAllNodes();
        }

        #endregion
    }
}