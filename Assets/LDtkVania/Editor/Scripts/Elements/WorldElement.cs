using System;
using System.Collections.Generic;
using System.Linq;
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
        private TextField _fieldDisplayName;
        private ListView _listAreas;
        private List<MV_Area> _areas;

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

            _listAreas = _containerMain.Q<ListView>("list-areas");
            _listAreas.itemsSource = _world.Areas;
            _listAreas.makeItem = () => new AreaElement();
            _listAreas.bindItem = (e, i) =>
            {
                AreaElement item = e as AreaElement;
                item.Area = _world.Areas[i];
            };
            _listAreas.Q<Button>("unity-list-view__add-button").clickable = new Clickable(CreateNewArea);
            Add(_containerMain);
        }

        private void CreateNewArea()
        {
            HashSet<string> existingGuids = new(_world.Areas.Select(a => a.Iid));
            string newGuid;
            do
            {
                newGuid = Guid.NewGuid().ToString();
            }
            while (existingGuids.Contains(newGuid));

            _world.Areas.Add(new MV_Area(newGuid));
            _listAreas.RefreshItems();
        }
    }
}