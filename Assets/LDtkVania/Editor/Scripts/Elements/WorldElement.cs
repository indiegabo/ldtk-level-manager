using LDtkVania;
using UnityEngine;
using UnityEngine.UIElements;

namespace LDtkVaniaEditor
{
    public class WorldElement : VisualElement
    {
        private const string TemplateName = "WorldInspector";

        private TemplateContainer _containerMain;
        private Label _labelName;

        public WorldElement(MV_World world)
        {
            _containerMain = Resources.Load<VisualTreeAsset>($"UXML/{TemplateName}").Instantiate();

            _labelName = _containerMain.Q<Label>("label-name");
            _labelName.text = world.Name;

            Add(_containerMain);
        }
    }
}