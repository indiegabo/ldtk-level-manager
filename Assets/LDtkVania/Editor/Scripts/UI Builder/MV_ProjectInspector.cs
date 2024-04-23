using System.Collections.Generic;
using System.Linq;
using LDtkVania;
using SpriteAnimations.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(MV_Project))]
public class MV_ProjectInspector : Editor
{
    public VisualTreeAsset _inspectorTree;
    public VisualTreeAsset _levelInspectorTree;

    private List<MV_Level> _levels = new();
    private List<MV_Level> _searchableLevels = new();

    private ListView _levelsListView;
    private TextField _nameFilterText;
    private Button _filterButton;

    public override VisualElement CreateInspectorGUI()
    {
        // Create a new VisualElement to be the root of our Inspector UI.
        VisualElement myInspector = new();
        TemplateContainer template = _inspectorTree.Instantiate();

        _nameFilterText = template.Q<TextField>("field-filter-name");
        _filterButton = template.Q<Button>("button-filter");

        _filterButton.clicked += OnFilterButtonClicked;

        _levels = MV_Project.Instance.GetLevels();
        foreach (MV_Level level in _levels)
        {
            _searchableLevels.Add(level);
        }

        _levelsListView = template.Q<ListView>("levels-list");

        _levelsListView.itemsSource = _searchableLevels;
        _levelsListView.makeItem = () => new MV_LevelsListElement(_levelInspectorTree);
        _levelsListView.bindItem = (e, i) =>
        {
            MV_LevelsListElement item = e as MV_LevelsListElement;
            item.Level = _searchableLevels[i];
        };

        myInspector.Add(template);
        // Return the finished Inspector UI.
        return myInspector;
    }

    private void OnFilterButtonClicked()
    {
        string term = _nameFilterText.text.ToLower();

        if (string.IsNullOrEmpty(term))
        {
            _searchableLevels.Clear();
            foreach (MV_Level level in _levels)
            {
                _searchableLevels.Add(level);
            }

            _levelsListView.itemsSource = _searchableLevels;
            _levelsListView.RefreshItems();
            return;
        }

        _searchableLevels = _levels.FindAll(x => x.Name.ToLower().Contains(term));
        _levelsListView.itemsSource = _searchableLevels;

        _levelsListView.RefreshItems();
    }
}
