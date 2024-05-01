using System.Collections.Generic;
using LDtkVania;
using UnityEngine;
using UnityEngine.UIElements;

namespace LDtkVaniaEditor
{
    public class AreaElement : VisualElement
    {
        private const string TemplateName = "AreaInspector";

        private MV_Area _area;
        private TemplateContainer _containerMain;

        private TextField _fieldIid;
        private TextField _fieldDisplayName;

        public MV_Area Area
        {
            get => _area;
            set
            {
                _area = value;
                _fieldIid.SetValueWithoutNotify(_area.Iid);
                _fieldDisplayName.SetValueWithoutNotify(_area.DisplayName);
                _fieldDisplayName.RegisterValueChangedCallback(evt =>
                {
                    _area.DisplayName = evt.newValue;
                    Debug.Log(_area.DisplayName);
                });
            }
        }

        public AreaElement()
        {
            _containerMain = Resources.Load<VisualTreeAsset>($"UXML/{TemplateName}").Instantiate();

            _fieldIid = _containerMain.Q<TextField>("field-iid");
            _fieldIid.SetEnabled(false);

            _fieldDisplayName = _containerMain.Q<TextField>("field-display-name");

            Add(_containerMain);
        }
    }
}