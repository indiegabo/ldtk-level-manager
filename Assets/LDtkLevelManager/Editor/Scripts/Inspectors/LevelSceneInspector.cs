using LDtkLevelManager;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace LDtkLevelManagerEditor
{
    [CustomEditor(typeof(LevelScene))]
    public class LevelSceneInspector : Editor
    {
        public const string TemplateName = "LevelSceneInspector";
        TemplateContainer _containerMain;

        public override VisualElement CreateInspectorGUI()
        {
            _containerMain = Resources.Load<VisualTreeAsset>($"UXML/{TemplateName}").Instantiate();
            return _containerMain;
        }
    }
}
