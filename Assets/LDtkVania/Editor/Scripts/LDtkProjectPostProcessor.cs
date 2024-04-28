using LDtkUnity;
using LDtkUnity.Editor;
using UnityEngine;
namespace LDtkVaniaEditor
{
    public class LDtkProjectPostProcessor : LDtkPostprocessor
    {
        protected override void OnPostprocessProject(GameObject root)
        {
            Debug.Log($"Post process LDtk project: {root.name}");
        }

        protected override void OnPostprocessLevel(GameObject root, LdtkJson projectJson)
        {
            LDtkIid lDtkIid = root.GetComponent<LDtkIid>();
            Debug.Log($"Post process LDtk level: {root.name}");
        }
    }
}