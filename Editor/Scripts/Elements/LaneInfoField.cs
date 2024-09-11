using LDtkLevelManager;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace LDtkLevelManagerEditor
{
    public class LaneInfoField : VisualElement
    {
        private const string TemplateName = "LaneInfoField_Template";
        public new class UxmlFactory : UxmlFactory<LaneInfoField, UxmlTraits> { }

        private TemplateContainer _containerMain;
        private FloatField _fieldStartingZ;
        private FloatField _fieldDepth;

        private LaneInfo _laneInfo;
        private UnityAction<float, float> _onValueChangedAction;

        public LaneInfoField()
        {
            _containerMain = Resources.Load<VisualTreeAsset>($"UXML/{TemplateName}").Instantiate();

            Add(_containerMain);
        }

        public void Initialize(LaneInfo laneInfo)
        {
            _laneInfo = laneInfo;

            _fieldStartingZ = _containerMain.Q<FloatField>("field-starting-z");
            _fieldStartingZ.SetValueWithoutNotify(laneInfo.StartingZ);
            _fieldStartingZ.RegisterValueChangedCallback(evt => OnValueChanged());

            _fieldDepth = _containerMain.Q<FloatField>("field-depth");
            _fieldDepth.SetValueWithoutNotify(laneInfo.Depth);
            _fieldDepth.RegisterValueChangedCallback(evt => OnValueChanged());
        }

        public void SetOnValueChanged(UnityAction<float, float> onValueChanged)
        {
            _onValueChangedAction = onValueChanged;
        }

        private void OnValueChanged()
        {
            if (_laneInfo != null)
            {
                _laneInfo.StartingZ = _fieldStartingZ.value;
                _laneInfo.Depth = _fieldDepth.value;
            }

            if (_onValueChangedAction == null) return;
            _onValueChangedAction.Invoke(_laneInfo.StartingZ, _laneInfo.Depth);
        }
    }
}