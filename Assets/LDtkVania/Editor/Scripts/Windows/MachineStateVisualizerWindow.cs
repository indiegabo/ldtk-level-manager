using HandyFSM.Registering;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace HandyFSM.Editor
{
    public class MachineStateVisualizerWindow : EditorWindow
    {
        #region Static

        public static MachineStateVisualizerWindow OpenEditorWindow()
        {
            var window = GetWindow<MachineStateVisualizerWindow>();
            window.titleContent = new GUIContent("State Visualizer");
            window.minSize = new Vector2(300, 150);
            window.Show();
            return window;
        }

        #endregion

        #region Fields

        private MachineStateVisualizerWindowData _data;
        private HandyMachine _machine;
        private StateVisualizer _stateVisualizer;
        private VisualElement _stateVisualizerRoot;

        private TemplateContainer _root;
        private VisualElement _body;
        private VisualElement _selectStateMachineContainer;
        private VisualElement _enterPlayModeContainer;

        private ObjectField _machineSelectorField;
        private Button _fromSelectionButton;

        #endregion


        #region Getters

        public ObjectField MachineSelectorField
        {
            get
            {
                _machineSelectorField ??= rootVisualElement.Q<ObjectField>("machine-selector-field");

                return _machineSelectorField;
            }
        }

        public MachineStateVisualizerWindowData Data => MachineStateVisualizerWindowData.instance;

        #endregion

        #region Behaviour

        private void OnEnable()
        {
            VisualTreeAsset tree = Resources.Load<VisualTreeAsset>("UI Documents/MachineStateVisualizerWindowUI");
            _root = tree.CloneTree();

            _root.style.flexGrow = 1;

            _body = _root.Q<VisualElement>("body");

            _selectStateMachineContainer = _root.Q("select-machine-container");
            _enterPlayModeContainer = _root.Q("enter-play-mode-container");

            _stateVisualizer = new StateVisualizer();
            _stateVisualizerRoot = _stateVisualizer.Root;
            _body.Add(_stateVisualizerRoot);

            _machineSelectorField = _root.Q<ObjectField>("machine-selector-field");

            _machineSelectorField.RegisterValueChangedCallback((e) =>
            {
                HandyMachine machine = e.newValue as HandyMachine;
                SetMachine(machine);
            });

            _fromSelectionButton = _root.Q<Button>("from-selection-button");
            _fromSelectionButton.clicked += () =>
            {
                GameObject selectedObject = Selection.activeObject as GameObject;
                if (selectedObject == null) return;
                HandyMachine machine = selectedObject.GetComponent<HandyMachine>();
                if (machine == null) return;
                _machineSelectorField.value = machine;
                SetMachine(machine);
            };

            LoadMachineFromData();
            LoadSession();

            EditorApplication.playModeStateChanged += OnEditorModeChanged;

            rootVisualElement.Add(_root);
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnEditorModeChanged;
            Data.Machine?.StatusChanged.RemoveListener(OnStatusChanged);
            Data.Machine?.StateChanged.RemoveListener(OnStateChanged);
        }

        private void OnDestroy()
        {
            SetMachine(null);
            Data.SetSession(null);
        }

        private void OnEditorModeChanged(PlayModeStateChange mode)
        {
            if (!mode.Equals(PlayModeStateChange.EnteredEditMode)) return;
            LoadMachineFromData();
            LoadSession();
        }

        #endregion

        #region Flow

        private void InitializeSession()
        {
            if (Data.Machine == null)
            {
                Data.SetSession(null);
                return;
            }

            Data.SetSession(new Session(Data.Machine, 100));
            _stateVisualizer.LoadSession(Data.Session);

            if (Data.Machine.CurrentState != null)
            {
                SetState(Data.Machine.CurrentState, Data.Machine.PreviousState);
            }

            Data.Machine.StateChanged.AddListener(OnStateChanged);
        }

        private void EvaluateDisplay()
        {
            if (Data.Machine == null)
            {
                _selectStateMachineContainer.style.display = DisplayStyle.Flex;
                _stateVisualizerRoot.style.display = DisplayStyle.None;
            }
            else
            {
                _selectStateMachineContainer.style.display = DisplayStyle.None;
                _stateVisualizerRoot.style.display = DisplayStyle.Flex;
            }
        }

        #endregion

        #region Machine

        public void SetMachine(HandyMachine machine)
        {
            Data.Machine?.StatusChanged.RemoveListener(OnStatusChanged);

            Data.SetMachine(machine);

            if (Data.Machine != null)
            {
                Data.Machine.StatusChanged.AddListener(OnStatusChanged);
            }

            EvaluateDisplay();
        }

        private void LoadMachineFromData()
        {
            if (Data.Machine != null)
            {
                _machineSelectorField.value = null;
                _machineSelectorField.value = Data.Machine;
                EvaluateDisplay();
                return;
            }

            if (Data.MachineObj == null)
            {
                _machineSelectorField.value = null;
                EvaluateDisplay(); return;
            }

            if (!Data.MachineObj.TryGetComponent<HandyMachine>(out var machine))
            {
                _machineSelectorField.value = null;
                EvaluateDisplay();
                return;
            }

            _machineSelectorField.value = null;
            _machineSelectorField.value = machine;
            SetMachine(machine);
        }

        private void LoadSession()
        {
            if (Data.Machine == null || Data.Session == null) return;
            _stateVisualizer.LoadSession(Data.Session);
        }

        private void OnStatusChanged(MachineStatus status)
        {
            switch (status)
            {
                case MachineStatus.On:
                    InitializeSession();
                    break;
            }
        }

        #endregion

        #region States

        private void SetState(IState state, IState previous)
        {
            _stateVisualizer.RegisterState(state);
        }

        private void OnStateChanged(IState state, IState previous)
        {
            SetState(state, previous);
        }

        #endregion
    }
}