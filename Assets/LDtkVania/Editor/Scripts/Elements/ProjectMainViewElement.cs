
using System.Collections.Generic;
using LDtkUnity;
using LDtkVania;
using UnityEngine;
using UnityEngine.UIElements;

namespace LDtkVaniaEditor
{
    public class ProjectMainViewElement : VisualElement
    {
        #region Fields

        private const string TemplateName = "ProjectInspector_MainView";

        private MV_Project _project;
        private List<MV_World> _worlds;

        private TemplateContainer _containerMain;
        private Button _buttonSyncWorlds;
        private ListView _listWorlds;

        #endregion

        #region Constructors

        public ProjectMainViewElement(MV_Project project)
        {
            _project = project;
            _worlds = _project.GetWorlds();

            _containerMain = Resources.Load<VisualTreeAsset>($"UXML/{TemplateName}").Instantiate();

            _buttonSyncWorlds = _containerMain.Q<Button>("button-sync-worlds");
            _buttonSyncWorlds.clicked += () =>
            {
                World[] worlds = _project.LDtkProject.Worlds;
                _project.SyncWorlds(worlds);
                _worlds = _project.GetWorlds();
            };

            _listWorlds = _containerMain.Q<ListView>("list-worlds");
            _listWorlds.itemsSource = _worlds;
            _listWorlds.makeItem = () => new WorldListItemElement();
            _listWorlds.bindItem = (e, i) =>
            {
                WorldListItemElement item = e as WorldListItemElement;
                item.World = _worlds[i];
            };

            Add(_containerMain);
        }

        #endregion

        #region Worlds

        #endregion

        #region Callbacks

        #endregion
    }
}