using LDtkVania;
using UnityEngine;
using UnityEngine.UIElements;

namespace LDtkVaniaEditor
{
    public class WorldElement : VisualElement
    {
        private const string TemplateName = "WorldInspector";

        private MV_World _world;
        private TemplateContainer _containerMain;
        private Label _labelName;
        private TextField _fieldDisplayName;

        public WorldElement(MV_World world)
        {
            _world = world;
            _containerMain = Resources.Load<VisualTreeAsset>($"UXML/{TemplateName}").Instantiate();

            _fieldDisplayName = _containerMain.Q<TextField>("field-display-name");
            _fieldDisplayName.SetValueWithoutNotify(world.DisplayName);

            _fieldDisplayName.RegisterValueChangedCallback(evt =>
            {
                _world.DisplayName = evt.newValue;
            });

            Add(_containerMain);
        }
    }
}